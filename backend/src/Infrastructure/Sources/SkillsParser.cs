using Backend.Domain.Entities;

namespace Backend.Infrastructure.Sources;

internal static class SkillsParser
{
    internal static List<SkillCategory> ParseSkills(List<string> lines, Dictionary<string, int> sectionMap)
    {
        // New format: "certifications & relevant training" section with bold category headers
        var sectionName = sectionMap.ContainsKey("certifications & relevant training") ? "certifications & relevant training" : null;

        if (sectionName is null)
        {
            return [];
        }

        var sectionLines = SectionHelper.GetSectionLines(lines, sectionMap, sectionName);
        if (sectionLines.Count == 0)
        {
            return [];
        }

        var categories = new List<(string Name, List<(string SubName, List<string> Items)> Subs)>();
        string? currentCategory = null;
        string? currentSub = null;
        var currentItems = new List<string>();

        for (int i = 0; i < sectionLines.Count; i++)
        {
            var line = sectionLines[i];

            if (SectionHelper.IsSectionHeader(line))
            {
                break;
            }

            var trimmed = line.Trim();

            // Detect category headers (bold lines: **Category**)
            var isCategoryHeader = trimmed.StartsWith("**", StringComparison.Ordinal)
                && trimmed.EndsWith("**", StringComparison.Ordinal)
                && !trimmed.Contains(',')
                && !trimmed.Contains(':');

            if (isCategoryHeader)
            {
                FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);
                FlushCategory(ref categories, currentCategory);
                currentCategory = trimmed.Trim('*');
                currentSub = null;
                currentItems = [];
                continue;
            }

            if (currentCategory is null)
            {
                // Skip lines before first category
                continue;
            }

            // Handle subsection headers ending with ":"
            if (trimmed.EndsWith(':'))
            {
                FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);
                currentSub = trimmed[..^1].Trim();
                continue;
            }

            // Handle subcategory with items on same line: "Subcategory: item1, item2, item3"
            var colonIdx = trimmed.IndexOf(": ", StringComparison.Ordinal);
            if (colonIdx > 0 && trimmed.IndexOf(',', colonIdx) > 0)
            {
                var subCategory = trimmed[..colonIdx].Trim();
                var itemsText = trimmed[(colonIdx + 2)..].Trim();
                FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);
                currentSub = subCategory;
                currentItems.AddRange(
                    itemsText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                continue;
            }

            if (line.Contains(','))
            {
                currentSub ??= "General";
                currentItems.AddRange(
                    line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                continue;
            }

            // Skill items on separate lines, possibly with **Language**: Level format
            var skillText = ExtractSkillText(line);
            if (!string.IsNullOrEmpty(skillText))
            {
                currentSub ??= "General";
                currentItems.Add(skillText);
                continue;
            }

            // Empty or unrecognized line - skip
        }

        FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);
        FlushCategory(ref categories, currentCategory);

        return BuildCategories(categories);
    }

    private static List<SkillCategory> BuildCategories(List<(string Name, List<(string SubName, List<string> Items)> Subs)> categories)
    {
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

    private static string ExtractSkillText(string line)
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return string.Empty;
        }

        // Handle **Language**: Level format
        if (trimmed.StartsWith("**", StringComparison.Ordinal) && trimmed.Contains("**:", StringComparison.Ordinal))
        {
            var colonIdx = trimmed.IndexOf("**:", StringComparison.Ordinal);
            var skillName = trimmed[..colonIdx].Trim('*').Trim();
            var level = trimmed[(colonIdx + 3)..].Trim();
            return $"{skillName}: {level}";
        }

        // Skip if it's just formatting markers or section-like
        if (trimmed.StartsWith("**", StringComparison.Ordinal) && trimmed.EndsWith("**", StringComparison.Ordinal) && trimmed.Count(c => c == '*') == 4)
        {
            return string.Empty;
        }

        return trimmed;
    }

    private static void FlushSub(
        ref List<(string Name, List<(string SubName, List<string> Items)> Subs)> categories,
        ref string? currentSub, ref List<string> currentItems, string? currentCategory)
    {
        if (currentSub is null || currentCategory is null)
        {
            return;
        }

        AddSubItem(ref categories, currentCategory, currentSub, currentItems);
        currentSub = null;
        currentItems = [];
    }

    private static void FlushCategory(
        ref List<(string Name, List<(string SubName, List<string> Items)> Subs)> categories,
        string? currentCategory)
    {
        if (currentCategory is null || categories.Any(c => c.Name == currentCategory))
        {
            return;
        }

        AddCategory(ref categories, currentCategory);
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

    internal static List<string> SplitRespectingParentheses(string text)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')')
            {
                depth--;
            }
            else if (text[i] == ',' && depth == 0)
            {
                var part = text[start..i].Trim();
                if (part.Length > 0)
                {
                    result.Add(part);
                }

                start = i + 1;
            }
        }

        var lastPart = text[start..].Trim();
        if (lastPart.Length > 0)
        {
            result.Add(lastPart);
        }

        return result;
    }
}