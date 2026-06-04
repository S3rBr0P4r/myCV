using Backend.Domain.Entities;

namespace Backend.Infrastructure.Sources;

internal static class ExperienceParser
{
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

    private static bool TryResolveNewFormatLocation(
        string nextLine, string companyAfterPipe,
        out string companyUrl, out string location, out string workMode)
    {
        var nextPipeIdx = nextLine.IndexOf(" | ", StringComparison.Ordinal);
        if (nextPipeIdx >= 0 && IsKnownWorkMode(nextLine[(nextPipeIdx + 3)..].Trim()))
        {
            companyUrl = companyAfterPipe;
            location = nextLine[..nextPipeIdx].Trim();
            workMode = nextLine[(nextPipeIdx + 3)..].Trim();
            return true;
        }

        companyUrl = string.Empty;
        location = string.Empty;
        workMode = string.Empty;
        return false;
    }

    private static bool TryParseRoleLine(string line, out string role, out string period)
    {
        var pipeIdx = line.IndexOf(" | ", StringComparison.Ordinal);
        if (pipeIdx < 0)
        {
            role = string.Empty;
            period = string.Empty;
            return false;
        }

        role = line[..pipeIdx].Trim();
        period = line[(pipeIdx + 3)..].Trim();
        return true;
    }

    internal static List<Experience> ParseExperiences(List<string> lines, Dictionary<string, int> sectionMap)
    {
        if (!sectionMap.TryGetValue("experience", out var start))
        {
            return [];
        }

        var sectionLines = SectionHelper.GetSectionLines(lines, sectionMap, "experience");
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

            if (TryResolveNewFormatLocation(sectionLines[idx], afterPipe, out companyUrl, out location, out workMode))
            {
                idx++;
            }
            else
            {
                companyUrl = string.Empty;
                (location, workMode) = ParseLegacyLocation(afterPipe);
            }

            if (idx >= sectionLines.Count)
            {
                break;
            }

            if (!TryParseRoleLine(sectionLines[idx], out var role, out var period))
            {
                idx++;
                continue;
            }
            idx++;

            experiences.Add(new Experience
            {
                Period = period,
                Role = role,
                Company = companyName,
                CompanyUrl = companyUrl,
                Location = location,
                WorkMode = workMode,
                Description = CollectDescription(sectionLines, ref idx),
                Background = string.Empty
            });
        }

        return experiences;
    }

    private static string CollectDescription(List<string> sectionLines, ref int idx)
    {
        var lines = new List<string>();
        while (idx < sectionLines.Count)
        {
            var line = sectionLines[idx];
            if (line.Contains(" | ", StringComparison.Ordinal))
            {
                break;
            }

            lines.Add(line);
            idx++;
        }

        return string.Join("\n", lines);
    }
}
