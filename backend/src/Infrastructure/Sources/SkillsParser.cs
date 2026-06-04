using Backend.Domain.Entities;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Infrastructure.Sources;

internal static class SkillsParser
{
    internal static List<SkillCategory> ParseSkills(List<string> lines, Dictionary<string, int> sectionMap)
    {
        if (!sectionMap.TryGetValue("technical skills", out var start))
        {
            return [];
        }

        var sectionLines = SectionHelper.GetSectionLines(lines, sectionMap, "technical skills");
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

            FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);

            if (NextLineHasItems(sectionLines, i))
            {
                currentSub = line;
            }
            else
            {
                FlushCategory(ref categories, currentCategory);
                currentCategory = line;
            }
        }

        FlushSub(ref categories, ref currentSub, ref currentItems, currentCategory);
        FlushCategory(ref categories, currentCategory);

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

    internal static List<SkillCategory> ParseSkillsFromTable(Table table)
    {
        var categories = new List<(string Name, List<(string SubName, List<string> Items)> Subs)>();
        string? currentCategory = null;

        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count < 2)
            {
                continue;
            }

            var nameCell = cells[0].InnerText.Trim();
            var itemsCell = cells[1].InnerText.Trim();

            if (nameCell.Length == 0)
            {
                continue;
            }

            if (itemsCell.Length == 0)
            {
                currentCategory = nameCell;
                if (!categories.Any(c => c.Name == currentCategory))
                {
                    categories.Add((currentCategory, []));
                }
            }
            else if (currentCategory is not null)
            {
                var items = SplitRespectingParentheses(itemsCell);
                var catIdx = categories.FindIndex(c => c.Name == currentCategory);
                if (catIdx >= 0)
                {
                    var cat = categories[catIdx];
                    var updatedSubs = cat.Subs.Append((nameCell, items)).ToList();
                    categories[catIdx] = (cat.Name, updatedSubs);
                }
            }
        }

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

            if (SectionHelper.IsSectionHeader(next))
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
