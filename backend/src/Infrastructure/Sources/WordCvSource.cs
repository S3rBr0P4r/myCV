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
    private readonly ILogger<WordCvSource> _logger;
    private readonly DiscordNotifier _discordNotifier;
    private CV? _cv;
    private bool _parsed;
    private readonly object _parseLock = new();

    public WordCvSource(
        IOptions<CvSourceOptions> options,
        ILogger<WordCvSource> logger,
        DiscordNotifier discordNotifier)
    {
        _filePath = options.Value.FilePath;
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

        if (!File.Exists(_filePath))
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

        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Length > MaxFileSize)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("CV Word document exceeds maximum size ({Size} bytes > {Max} bytes)", fileInfo.Length, MaxFileSize);
            }

            _discordNotifier.SendAlertAsync(
                "CV File Too Large",
                $"The CV Word document at `{_filePath}` exceeds the maximum file size of {MaxFileSize / 1024 / 1024} MB.");
            throw new CvSourceClientException();
        }

        try
        {
            var cv = ParseDocument();
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

    private CV ParseDocument()
    {
        string name = string.Empty;
        string lastName = string.Empty;
        string title = string.Empty;
        var summaryLines = new List<string>();
        var experiences = new List<Experience>();
        var skills = new List<string>();
        bool collectingSummary = false;

        string expPeriod = string.Empty;
        string expRole = string.Empty;
        string expCompany = string.Empty;
        string expDescription = string.Empty;
        string expBackground = string.Empty;
        bool hasCurrentExperience = false;

        using var document = WordprocessingDocument.Open(_filePath, false);
        var body = document.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            throw new CvSourceClientException();
        }

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var text = paragraph.InnerText.Trim();

            if (text.Length == 0)
            {
                continue;
            }

            if (TryMatchLabel(text, "Name: ", out var value))
            {
                name = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "LastName: ", out value))
            {
                lastName = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Title: ", out value))
            {
                title = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Summary: ", out value))
            {
                summaryLines.Add(value);
                collectingSummary = true;
            }
            else if (TryMatchLabel(text, "Period: ", out value))
            {
                if (hasCurrentExperience)
                {
                    experiences.Add(new Experience
                    {
                        Period = expPeriod,
                        Role = expRole,
                        Company = expCompany,
                        Description = expDescription,
                        Background = expBackground
                    });
                }

                expPeriod = value;
                expRole = string.Empty;
                expCompany = string.Empty;
                expDescription = string.Empty;
                expBackground = string.Empty;
                hasCurrentExperience = true;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Role: ", out value))
            {
                expRole = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Company: ", out value))
            {
                expCompany = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Description: ", out value))
            {
                expDescription = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Background: ", out value))
            {
                expBackground = value;
                collectingSummary = false;
            }
            else if (TryMatchLabel(text, "Skills: ", out value))
            {
                skills = value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                collectingSummary = false;
            }
            else if (collectingSummary)
            {
                summaryLines.Add(text);
            }
        }

        if (hasCurrentExperience)
        {
            experiences.Add(new Experience
            {
                Period = expPeriod,
                Role = expRole,
                Company = expCompany,
                Description = expDescription,
                Background = expBackground
            });
        }

        return new CV
        {
            Name = name,
            LastName = lastName,
            Title = title,
            Summary = string.Join("\n", summaryLines),
            Experiences = experiences.AsReadOnly(),
            Skills = skills.AsReadOnly()
        };
    }

    private static bool TryMatchLabel(string text, string label, out string value)
    {
        if (text.StartsWith(label, StringComparison.OrdinalIgnoreCase))
        {
            value = text[label.Length..];
            return true;
        }

        value = string.Empty;
        return false;
    }
}
