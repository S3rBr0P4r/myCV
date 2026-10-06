using Backend.Domain.Entities;

namespace Backend.Infrastructure.Services;

public static class TranslatedCVBuilder
{
    public static CV Build(
        CV source,
        string? summary, string? title,
        IReadOnlyList<string> periods, IReadOnlyList<string> roles, IReadOnlyList<string> companies,
        IReadOnlyList<string> locations, IReadOnlyList<string> workModes, IReadOnlyList<string> descriptions,
        IReadOnlyList<string> categoryNames, IReadOnlyList<string> subCategoryNames, IReadOnlyList<string> skillItems,
        string[] translatedTexts)
    {
        int idx = 0;
        string? translatedSummary = !string.IsNullOrEmpty(summary) ? ApplyOverride(translatedTexts[idx++]) : null;
        string? translatedTitle = !string.IsNullOrEmpty(title) ? ApplyOverride(translatedTexts[idx++]) : null;

        var translatedPeriods = periods.Select(p => !string.IsNullOrEmpty(p) ? ApplyOverride(translatedTexts[idx++]) : p).ToList();
        var translatedRoles = roles.Select(r => !string.IsNullOrEmpty(r) ? ApplyOverride(translatedTexts[idx++]) : r).ToList();
        var translatedCompanies = companies.Select(c =>
        {
            if (string.IsNullOrEmpty(c))
            {
                return c;
            }
            var translated = translatedTexts[idx++];
            return c.Contains('(') ? ApplyOverride(translated) : c;
        }).ToList();
        var translatedLocations = locations.Select(l => !string.IsNullOrEmpty(l) ? ApplyOverride(translatedTexts[idx++]) : l).ToList();
        var translatedWorkModes = workModes.Select(w => !string.IsNullOrEmpty(w) ? ApplyOverride(translatedTexts[idx++]) : w).ToList();
        var translatedDescriptions = descriptions
            .Select(d => DescriptionSegmenter.Rebuild(d, _ => ApplyOverride(translatedTexts[idx++]).Trim()))
            .ToList();
        var translatedCategoryNames = categoryNames.Select(c => !string.IsNullOrEmpty(c) ? ApplyOverride(translatedTexts[idx++]) : c).ToList();
        var translatedSubCategoryNames = subCategoryNames.Select(s => !string.IsNullOrEmpty(s) ? ApplyOverride(translatedTexts[idx++]) : s).ToList();
        var translatedSkillItems = skillItems.Select(s => !string.IsNullOrEmpty(s) ? ApplyOverride(translatedTexts[idx++]) : s).ToList();

        var translatedExperiences = source.Experiences.Select((e, i) => new Experience
        {
            Period = translatedPeriods[i],
            Role = translatedRoles[i],
            Company = translatedCompanies[i],
            Location = translatedLocations[i],
            WorkMode = translatedWorkModes[i],
            Description = translatedDescriptions[i],
            Background = e.Background
        }).ToList();
        var translatedCategories = RebuildSkillCategories(
            source.SkillCategories, translatedCategoryNames, translatedSubCategoryNames, translatedSkillItems);
        return new CV
        {
            Name = source.Name,
            LastName = source.LastName,
            Title = translatedTitle ?? source.Title,
            Summary = translatedSummary ?? source.Summary,
            ContactInfo = source.ContactInfo,
            Experiences = translatedExperiences.AsReadOnly(),
            SkillCategories = translatedCategories.AsReadOnly()
        };
    }

    private static string ApplyOverride(string translated)
    {
        var result = translated
            .Replace("Pila tecnológica", "Stack tecnológico", StringComparison.OrdinalIgnoreCase)
            .Replace("A distancia", "Remoto", StringComparison.OrdinalIgnoreCase);
        return result;
    }

    private static List<SkillCategory> RebuildSkillCategories(
        IReadOnlyList<SkillCategory> sourceCategories,
        List<string> translatedCategoryNames, List<string> translatedSubCategoryNames,
        List<string> translatedSkillItems)
    {
        int catIdx = 0;
        int subIdx = 0;
        int itemIdx = 0;
        var result = new List<SkillCategory>();
        foreach (var category in sourceCategories)
        {
            var translatedCatName = catIdx < translatedCategoryNames.Count
                ? translatedCategoryNames[catIdx++]
                : category.Name;
            var subCategories = new List<SkillSubCategory>();
            foreach (var sub in category.SubCategories)
            {
                var translatedSubName = subIdx < translatedSubCategoryNames.Count
                    ? translatedSubCategoryNames[subIdx++]
                    : sub.Name;
                subCategories.Add(new SkillSubCategory
                {
                    Name = translatedSubName,
                    Items = TranslateItems(sub.Items, translatedSkillItems, ref itemIdx)
                });
            }

            result.Add(new SkillCategory
            {
                Name = translatedCatName,
                SubCategories = subCategories.AsReadOnly()
            });
        }

        return result;
    }

    private static List<string> TranslateItems(
        IReadOnlyList<string> sourceItems, List<string> translatedSkillItems, ref int itemIdx)
    {
        var items = new List<string>();
        foreach (var item in sourceItems)
        {
            items.Add(itemIdx < translatedSkillItems.Count
                ? translatedSkillItems[itemIdx++]
                : item);
        }
        return items;
    }
}
