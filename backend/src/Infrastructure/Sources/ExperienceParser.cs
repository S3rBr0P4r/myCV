using Backend.Domain.Entities;

namespace Backend.Infrastructure.Sources;

internal static class ExperienceParser
{
    private static readonly HashSet<string> KnownWorkModes =
        ["Remote", "Onsite", "Hybrid", "Both", "Remote/Hybrid", "Hybrid/Remote", "On-site", "On Site"];

    private static bool IsKnownWorkMode(string value) =>
        KnownWorkModes.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    private static readonly string[] PipeSeparator = [" | "];

    private static bool TryParseExperienceFormat(
        string line, out string role, out string company, out string location, out string workMode)
    {
        role = string.Empty;
        company = string.Empty;
        location = string.Empty;
        workMode = string.Empty;

        // Expected format: "Role | Company | WorkMode (Location)" e.g., "Senior Backend Engineer | Docplanner | Remote (Barcelona-based)"
        var parts = line.Split(PipeSeparator, StringSplitOptions.None);
        if (parts.Length != 3)
        {
            return false;
        }

        role = parts[0].Trim();
        company = parts[1].Trim();

        var locationPart = parts[2].Trim();
        var parenStart = locationPart.LastIndexOf('(');
        var parenEnd = locationPart.LastIndexOf(')');
        if (parenStart >= 0 && parenEnd > parenStart)
        {
            var beforeParen = locationPart[..parenStart].Trim();
            var insideParen = locationPart[(parenStart + 1)..parenEnd].Trim();

            // Document format is "WorkMode (Location)" e.g., "Remote (Barcelona-based)"
            if (IsKnownWorkMode(beforeParen))
            {
                workMode = beforeParen;
                location = insideParen;
            }
            else
            {
                // Default: assume before paren is work mode, inside is location
                workMode = beforeParen;
                location = insideParen;
            }
        }
        else
        {
            // No parentheses - cannot parse
            return false;
        }

        return !string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(company);
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
            var line = sectionLines[idx];

            if (TryParseExperienceFormat(line, out var role, out var company, out var location, out var workMode))
            {
                idx++;
                if (idx >= sectionLines.Count)
                {
                    break;
                }

                // Next line should be the period
                var period = sectionLines[idx].Trim();
                idx++;

                experiences.Add(new Experience
                {
                    Period = period,
                    Role = role,
                    Company = company,
                    CompanyUrl = string.Empty,
                    Location = location,
                    WorkMode = workMode,
                    Description = CollectDescription(sectionLines, ref idx),
                    Background = string.Empty
                });
                continue;
            }

            // Unrecognized line - skip
            idx++;
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
                // Check if this is a new experience entry
                if (line.Split(" | ", StringSplitOptions.None).Length == 3)
                {
                    break;
                }
            }

            lines.Add(line);
            idx++;
        }

        return string.Join("\n", lines);
    }
}