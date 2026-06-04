namespace Backend.Infrastructure.Sources;

internal static class SectionHelper
{
    internal static readonly HashSet<string> SectionHeaders =
    [
        "summary", "experience", "technical skills",
        "education", "certifications & relevant training"
    ];

    internal static bool IsSectionHeader(string line)
    {
        return SectionHeaders.Contains(line.ToLowerInvariant().Trim());
    }

    internal static Dictionary<string, int> BuildSectionMap(List<string> lines)
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

    internal static List<string> GetSectionLines(List<string> lines, Dictionary<string, int> sectionMap, string sectionName)
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

    internal static string ExtractLastName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 1 ? parts[^1] : string.Empty;
    }
}
