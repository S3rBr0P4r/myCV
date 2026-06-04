using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Infrastructure.Sources;

internal static class CvDocumentReader
{
    internal static CV Read(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var mainPart = document.MainDocumentPart;
        var body = mainPart?.Document?.Body;

        if (body is null)
        {
            throw new CvSourceClientException();
        }

        var hyperlinkRels = mainPart!.HyperlinkRelationships
            .ToDictionary(r => r.Id, r => r.Uri, StringComparer.Ordinal);

        var lines = new List<string>();
        Table? skillsTable = null;

        foreach (var element in body.ChildElements)
        {
            if (element is Paragraph paragraph)
            {
                var text = TextFormatter.GetFormattedText(paragraph, hyperlinkRels);
                if (text.Length > 0)
                {
                    lines.Add(text);
                }
            }
            else if (element is Table table)
            {
                if (!HasSectionHeader(lines))
                {
                    CollectTableRows(table, hyperlinkRels, lines);
                }
                else
                {
                    skillsTable = table;
                }
            }
        }

        if (lines.Count < 3)
        {
            throw new CvSourceClientException();
        }

        return BuildCV(lines, skillsTable, hyperlinkRels);
    }

    private static bool HasSectionHeader(List<string> lines)
    {
        return lines.Any(l => SectionHelper.SectionHeaders.Contains(l.ToLowerInvariant().Trim()));
    }

    private static void CollectTableRows(Table table, Dictionary<string, Uri> hyperlinkRels, List<string> lines)
    {
        foreach (var row in table.Elements<TableRow>())
        {
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = string.Join(" ", cell.Elements<Paragraph>()
                    .Select(p => TextFormatter.GetFormattedText(p, hyperlinkRels))
                    .Where(t => t.Length > 0));
                if (cellText.Length > 0)
                {
                    lines.Add(cellText);
                }
            }
        }
    }

    private static CV BuildCV(List<string> lines, Table? skillsTable, Dictionary<string, Uri> hyperlinkRels)
    {
        var name = lines[0];
        var lastName = SectionHelper.ExtractLastName(name);
        var title = lines[1];

        var contactInfo = ContactParser.ParseContactInfo(lines, 2);

        var sectionMap = SectionHelper.BuildSectionMap(lines);
        var summary = string.Join("\n", SectionHelper.GetSectionLines(lines, sectionMap, "summary"));
        var experiences = ExperienceParser.ParseExperiences(lines, sectionMap);
        var skillCategories = skillsTable is not null
            ? SkillsParser.ParseSkillsFromTable(skillsTable)
            : SkillsParser.ParseSkills(lines, sectionMap);

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
}
