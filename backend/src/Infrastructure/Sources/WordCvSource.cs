using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Sources;

public sealed class WordCvSource : ICvSource
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly string _filePath;
    private readonly string _allowedDirectory;
    private readonly ILogger<WordCvSource> _logger;
    private readonly DiscordNotifier _discordNotifier;
    private CV? _cv;
    private bool _parsed;
    private readonly object _parseLock = new();

    private static readonly HashSet<string> SectionHeaders =
    [
        "summary", "experience", "technical skills",
        "education", "certifications & relevant training"
    ];

    public WordCvSource(
        IOptions<CvSourceOptions> options,
        ILogger<WordCvSource> logger,
        DiscordNotifier discordNotifier)
    {
        _filePath = options.Value.FilePath ?? string.Empty;
        _allowedDirectory = options.Value.AllowedDirectory ?? string.Empty;
        _logger = logger;
        _discordNotifier = discordNotifier;
    }

    public Task<CV> GetCvAsync(CancellationToken cancellationToken = default)
    {
        if (_parsed)
        {
            return Task.FromResult(_cv!);
        }

        lock (_parseLock)
        {
            if (_parsed)
            {
                return Task.FromResult(_cv!);
            }

            Parse();
        }

        return Task.FromResult(_cv!);
    }

    private void Parse()
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CvSource:FilePath is not configured");
            }

            _discordNotifier.SendAlertAsync(
                "CV Source Error",
                "CvSource:FilePath is not configured. The CV endpoint will return 500.");
            throw new CvSourceClientException();
        }

        if (_filePath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CV Source file path is a UNC path; rejecting for security: {FilePath}", _filePath);
            }

            _discordNotifier.SendAlertAsync(
                "CV Source Security",
                $"CV Source file path is a UNC path. Rejected for security.");
            throw new CvSourceClientException();
        }

        var fullPath = Path.GetFullPath(_filePath);
        if (!string.IsNullOrEmpty(_allowedDirectory) && !fullPath.StartsWith(_allowedDirectory, StringComparison.Ordinal))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CV Source file path is outside the allowed directory");
            }

            _discordNotifier.SendAlertAsync(
                "CV Source Security",
                "CV Source file path is outside the allowed directory.");
            throw new CvSourceClientException();
        }

        if (!File.Exists(fullPath))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CV Word document not found at {FilePath}", _filePath);
            }

            _discordNotifier.SendAlertAsync(
                "CV File Not Found",
                $"The CV Word document was not found at:\n`{_filePath}`\n\nThe CV endpoint will return 500.");
            throw new CvSourceClientException();
        }

        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length > MaxFileSize)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CV Word document exceeds maximum size ({Size} bytes > {Max} bytes)", fileInfo.Length, MaxFileSize);
            }

            _discordNotifier.SendAlertAsync(
                "CV File Too Large",
                $"The CV Word document exceeds the maximum file size of {MaxFileSize / 1024 / 1024} MB.");
            throw new CvSourceClientException();
        }

        try
        {
            var cv = ParseDocument(fullPath);
            _cv = cv;
            _parsed = true;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully parsed CV from {FilePath}", _filePath);
            }
        }
        catch (CvSourceClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Failed to parse CV Word document at {FilePath}", _filePath);
            }

            _discordNotifier.SendAlertAsync(
                "CV Parse Error",
                $"Failed to parse the CV Word document at:\n`{_filePath}`\n\n**Error:** {ex.Message}\n\nThe CV endpoint will return 500.");
            throw new CvSourceClientException();
        }
    }

    private static CV ParseDocument(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            throw new CvSourceClientException();
        }

        var paragraphs = body.Elements<Paragraph>().ToList();
        var lines = paragraphs
            .Select(GetFormattedText)
            .Where(t => t.Length > 0)
            .ToList();

        if (lines.Count < 3)
        {
            throw new CvSourceClientException();
        }

        var name = lines[0];
        var lastName = ExtractLastName(name);
        var title = lines[1];

        var contactInfo = ParseContactInfo(lines, 2);

        var sectionMap = BuildSectionMap(lines);
        var summary = string.Join("\n", GetSectionLines(lines, sectionMap, "summary"));
        var experiences = ParseExperiences(lines, sectionMap);
        var skillCategories = ParseSkills(lines, sectionMap);

        return new CV
        {
            Name = name,
            LastName = lastName,
            Title = title,
            Summary = summary,
            ContactInfo = contactInfo,
            Experiences = experiences.AsReadOnly(),
            SkillCategories = skillCategories.AsReadOnly()
        };
    }

    private static string GetFormattedText(Paragraph paragraph)
    {
        var raw = paragraph.InnerText.Trim();
        if (raw.Length == 0)
        {
            return raw;
        }

        var lower = raw.ToLowerInvariant();
        if (SectionHeaders.Contains(lower))
        {
            return raw;
        }

        var parts = paragraph.Elements<Run>()
            .Select(r =>
            {
                var text = r.InnerText;
                if (text.Length == 0)
                {
                    return text;
                }

                var isBold = r.RunProperties?.Bold is not null;
                return isBold ? $"**{text}**" : text;
            });

        return string.Concat(parts).Trim();
    }

    private static string ExtractLastName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 1 ? parts[^1] : string.Empty;
    }

    private static ContactInfo? ParseContactInfo(List<string> lines, int startIndex)
    {
        if (startIndex >= lines.Count)
        {
            return null;
        }

        string email = string.Empty;
        string phone = string.Empty;
        string location = string.Empty;
        string willingness = string.Empty;

        for (int i = startIndex; i < lines.Count; i++)
        {
            var line = lines[i];
            if (IsSectionHeader(line))
            {
                break;
            }

            var value = StripIconPrefix(line);

            if (value.Contains('@'))
            {
                email = value;
            }
            else if (value.StartsWith('+') || value.Any(char.IsDigit))
            {
                phone = value;
            }
            else if (value.Contains("Remote", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("Hybrid", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("Onsite", StringComparison.OrdinalIgnoreCase))
            {
                location = value;
            }
            else if (value.Contains("Willingness", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("travel", StringComparison.OrdinalIgnoreCase))
            {
                willingness = value;
            }
        }

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone) &&
            string.IsNullOrEmpty(location) && string.IsNullOrEmpty(willingness))
        {
            return null;
        }

        return new ContactInfo
        {
            Email = email,
            Phone = phone,
            Location = location,
            WillingnessToTravel = willingness
        };
    }

    private static string StripIconPrefix(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        if (!char.IsLetterOrDigit(text[0]) && text[0] != ' ')
        {
            var afterIcon = text[1..].TrimStart();
            return afterIcon.Length > 0 ? afterIcon : text;
        }

        return text;
    }

    private static bool IsSectionHeader(string line)
    {
        return SectionHeaders.Contains(line.ToLowerInvariant().Trim());
    }

    private static Dictionary<string, int> BuildSectionMap(List<string> lines)
    {
        var map = new Dictionary<string, int>();

        for (int i = 0; i < lines.Count; i++)
        {
            var key = lines[i].ToLowerInvariant().Trim();
            if (SectionHeaders.Contains(key) && !map.ContainsKey(key))
            {
                map[key] = i;
            }
        }

        return map;
    }

    private static List<string> GetSectionLines(List<string> lines, Dictionary<string, int> sectionMap, string sectionName)
    {
        if (!sectionMap.TryGetValue(sectionName, out var start))
        {
            return [];
        }

        var result = new List<string>();

        for (int i = start + 1; i < lines.Count; i++)
        {
            var key = lines[i].ToLowerInvariant().Trim();
            if (SectionHeaders.Contains(key))
            {
                break;
            }

            result.Add(lines[i]);
        }

        return result;
    }

    private static readonly HashSet<string> KnownWorkModes =
        ["Remote", "Onsite", "Hybrid", "Both", "Remote/Hybrid", "Hybrid/Remote", "On-site", "On Site"];

    private static bool IsKnownWorkMode(string value) =>
        KnownWorkModes.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    private static (string location, string workMode) ParseLegacyLocation(string raw)
    {
        var parenStart = raw.LastIndexOf('(');
        var parenEnd = raw.LastIndexOf(')');
        if (parenStart >= 0 && parenEnd > parenStart)
        {
            return (raw[..parenStart].Trim(), raw[(parenStart + 1)..parenEnd].Trim());
        }
        return (raw.Trim(), string.Empty);
    }

    private static List<Experience> ParseExperiences(List<string> lines, Dictionary<string, int> sectionMap)
    {
        if (!sectionMap.TryGetValue("experience", out var start))
        {
            return [];
        }

        var sectionLines = GetSectionLines(lines, sectionMap, "experience");
        var experiences = new List<Experience>();
        int idx = 0;

        while (idx < sectionLines.Count)
        {
            var companyLine = sectionLines[idx];
            var pipeIdx = companyLine.IndexOf(" | ", StringComparison.Ordinal);
            if (pipeIdx < 0)
            {
                idx++;
                continue;
            }

            var companyName = companyLine[..pipeIdx].Trim();
            var afterPipe = companyLine[(pipeIdx + 3)..].Trim();
            idx++;

            if (idx >= sectionLines.Count)
            {
                break;
            }

            string companyUrl;
            string location;
            string workMode;

            var nextLine = sectionLines[idx];
            var nextPipeIdx = nextLine.IndexOf(" | ", StringComparison.Ordinal);

            if (nextPipeIdx >= 0 && IsKnownWorkMode(nextLine[(nextPipeIdx + 3)..].Trim()))
            {
                // New format: Company | URL
                //             Location | WorkMode
                //             Role | Period
                companyUrl = afterPipe;
                location = nextLine[..nextPipeIdx].Trim();
                workMode = nextLine[(nextPipeIdx + 3)..].Trim();
                idx++;
            }
            else
            {
                // Old format: Company | Location (WorkMode)
                //             Role | Period
                companyUrl = string.Empty;
                (location, workMode) = ParseLegacyLocation(afterPipe);
            }

            if (idx >= sectionLines.Count)
            {
                break;
            }

            var roleLine = sectionLines[idx];
            var rolePipeIdx = roleLine.IndexOf(" | ", StringComparison.Ordinal);
            if (rolePipeIdx < 0)
            {
                idx++;
                continue;
            }

            var role = roleLine[..rolePipeIdx].Trim();
            var period = roleLine[(rolePipeIdx + 3)..].Trim();
            idx++;

            var descriptionLines = new List<string>();
            while (idx < sectionLines.Count)
            {
                var nextDescLine = sectionLines[idx];

                if (nextDescLine.Contains(" | ", StringComparison.Ordinal))
                {
                    break;
                }

                descriptionLines.Add(nextDescLine);
                idx++;
            }

            experiences.Add(new Experience
            {
                Period = period,
                Role = role,
                Company = companyName,
                CompanyUrl = companyUrl,
                Location = location,
                WorkMode = workMode,
                Description = string.Join("\n", descriptionLines),
                Background = string.Empty
            });
        }

        return experiences;
    }

    private static List<SkillCategory> ParseSkills(List<string> lines, Dictionary<string, int> sectionMap)
    {
        if (!sectionMap.TryGetValue("technical skills", out var start))
        {
            return [];
        }

        var sectionLines = GetSectionLines(lines, sectionMap, "technical skills");
        if (sectionLines.Count == 0)
        {
            return [];
        }

        var categories = new List<(string Name, List<(string SubName, List<string> Items)> Subs)>();
        string? currentCategory = null;
        string? currentSub = null;
        var currentItems = new List<string>();

        void FlushSub()
        {
            if (currentSub is not null)
            {
                AddSubItem(ref categories, currentCategory!, currentSub, currentItems);
                currentSub = null;
                currentItems = [];
            }
        }

        void FlushCategory()
        {
            if (currentCategory is not null && !categories.Any(c => c.Name == currentCategory))
            {
                AddCategory(ref categories, currentCategory);
            }
        }

        for (int i = 0; i < sectionLines.Count; i++)
        {
            var line = sectionLines[i];

            if (IsSectionHeader(line))
            {
                break;
            }

            if (currentCategory is null)
            {
                currentCategory = line;
                continue;
            }

            if (line.Contains(','))
            {
                currentSub ??= "General";
                currentItems.AddRange(
                    line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                continue;
            }

            FlushSub();

            if (NextLineHasItems(sectionLines, i))
            {
                currentSub = line;
            }
            else
            {
                FlushCategory();
                currentCategory = line;
            }
        }

        FlushSub();
        FlushCategory();

        return categories.Select(c => new SkillCategory
        {
            Name = c.Name,
            SubCategories = c.Subs.Select(s => new SkillSubCategory
            {
                Name = s.SubName,
                Items = s.Items.AsReadOnly()
            }).ToList().AsReadOnly()
        }).ToList();
    }

    private static bool NextLineHasItems(List<string> lines, int currentIndex)
    {
        for (int i = currentIndex + 1; i < lines.Count; i++)
        {
            var next = lines[i];

            if (IsSectionHeader(next))
            {
                return false;
            }

            if (next.Contains(','))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(next))
            {
                return false;
            }
        }

        return false;
    }

    private static void AddCategory(
        ref List<(string Name, List<(string SubName, List<string> Items)> Subs)> categories,
        string name)
    {
        categories.Add((name, []));
    }

    private static void AddSubItem(
        ref List<(string Name, List<(string SubName, List<string> Items)> Subs)> categories,
        string categoryName, string subName, List<string> items)
    {
        var catIdx = categories.FindIndex(c => c.Name == categoryName);
        if (catIdx < 0)
        {
            categories.Add((categoryName, []));
            catIdx = categories.Count - 1;
        }

        var cat = categories[catIdx];
        var updatedSubs = cat.Subs.Append((subName, new List<string>(items))).ToList();
        categories[catIdx] = (cat.Name, updatedSubs);
    }

}
