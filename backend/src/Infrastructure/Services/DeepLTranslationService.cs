using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using Backend.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

public sealed class DeepLTranslationService : ITranslationService
{
    private const string DeepLApiUrl = "https://api-free.deepl.com/v2/translate";
    private const int MaxFieldLength = 10_000;

    private readonly HttpClient _httpClient;
    private readonly DeepLOptions _options;
    private readonly IMemoryCache _cache;
    private readonly DiscordErrorNotifier _discordNotifier;
    private readonly ILogger<DeepLTranslationService> _logger;

    public DeepLTranslationService(
        HttpClient httpClient,
        IOptions<DeepLOptions> options,
        IMemoryCache cache,
        DiscordErrorNotifier discordNotifier,
        ILogger<DeepLTranslationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _discordNotifier = discordNotifier;
        _logger = logger;
    }

    public async Task<CV?> TranslateAsync(CV source, string targetLanguage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AuthKey))
        {
            _logger.LogDebug("DeepL AuthKey not configured — skipping translation");
            return null;
        }

        var lang = LanguageHelper.NormalizeLanguage(targetLanguage);

        if (string.IsNullOrEmpty(lang) || string.Equals(lang, "EN", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var cacheKey = $"translated_cv_{lang}";

        if (_cache.TryGetValue(cacheKey, out CV? cached) && cached is not null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Returning cached CV translation for language {Lang}", lang);
            }
            return cached;
        }
        try
        {
            var translated = await TranslateCoreAsync(source, lang, cancellationToken);

            if (translated is null)
            {
                return null;
            }

            _cache.Set(cacheKey, translated, TimeSpan.FromMinutes(Math.Max(1, _options.CacheDurationMinutes)));

            return translated;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DeepL translation failed for language {Lang}, falling back to English", lang);

            _ = _discordNotifier.SendAlertAsync("DeepL Translation Failed",
                $"Language: {lang}\nError: {ex.Message}");

            return null;
        }
    }
    private async Task<CV?> TranslateCoreAsync(CV source, string lang, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
        var timeoutToken = timeoutCts.Token;
        var summary = source.Summary;
        var title = source.Title;
        var periods = source.Experiences.Select(e => e.Period).ToList();
        var roles = source.Experiences.Select(e => e.Role).ToList();
        var companies = source.Experiences.Select(e => e.Company).ToList();
        var locations = source.Experiences.Select(e => e.Location).ToList();
        var workModes = source.Experiences.Select(e => e.WorkMode).ToList();
        var descriptions = source.Experiences.Select(e => e.Description).ToList();
        var categoryNames = source.SkillCategories.Select(c => c.Name).ToList();
        var subCategoryNames = source.SkillCategories
            .SelectMany(c => c.SubCategories)
            .Select(s => s.Name)
            .ToList();
        var allTexts = new List<string>();
        if (!string.IsNullOrEmpty(summary))
        {
            allTexts.Add(summary);
        }
        if (!string.IsNullOrEmpty(title))
        {
            allTexts.Add(title);
        }
        allTexts.AddRange(periods.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(roles.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(companies.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(locations.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(workModes.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(descriptions.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(categoryNames.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(subCategoryNames.Where(t => !string.IsNullOrEmpty(t)));
        if (allTexts.Count == 0)
        {
            return source;
        }
        var translatedTexts = await CallDeepLApiAsync(allTexts, lang, timeoutToken);
        if (translatedTexts is null)
        {
            return null;
        }

        return BuildTranslatedCV(source, summary, title, periods, roles, companies,
            locations, workModes, descriptions, categoryNames, subCategoryNames, translatedTexts);
    }

    private async Task<string[]?> CallDeepLApiAsync(List<string> allTexts, string lang, CancellationToken timeoutToken)
    {
        var request = new DeepLRequest
        {
            Text = allTexts.ToArray(),
            TargetLang = lang,
            Context = "The software engineer works in a remote position. The technology stack includes various programming languages and frameworks. Software development, IT skills, engineering."
        };
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, DeepLApiUrl) { Content = JsonContent.Create(request) };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("DeepL-Auth-Key", _options.AuthKey);
        var response = await _httpClient.SendAsync(httpRequest, timeoutToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken: timeoutToken);
        if (result?.Translations is null || result.Translations.Length != allTexts.Count)
        {
            _logger.LogWarning("DeepL returned {Count} translations but expected {Expected}",
                result?.Translations?.Length ?? 0, allTexts.Count);
            return null;
        }
        return result.Translations
            .Select(t => t.Text.Length > MaxFieldLength ? t.Text[..MaxFieldLength] : t.Text)
            .ToArray();
    }
    private static CV BuildTranslatedCV(CV source,
        string? summary, string? title,
        List<string> periods, List<string> roles, List<string> companies,
        List<string> locations, List<string> workModes, List<string> descriptions,
        List<string> categoryNames, List<string> subCategoryNames, string[] translatedTexts)
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
        var translatedDescriptions = descriptions.Select(d => !string.IsNullOrEmpty(d) ? ApplyOverride(translatedTexts[idx++]) : d).ToList();
        var translatedCategoryNames = categoryNames.Select(c => !string.IsNullOrEmpty(c) ? ApplyOverride(translatedTexts[idx++]) : c).ToList();
        var translatedSubCategoryNames = subCategoryNames.Select(s => !string.IsNullOrEmpty(s) ? ApplyOverride(translatedTexts[idx++]) : s).ToList();

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
            source.SkillCategories, translatedCategoryNames, translatedSubCategoryNames);
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
        List<string> translatedCategoryNames, List<string> translatedSubCategoryNames)
    {
        int catIdx = 0;
        int subIdx = 0;
        var result = new List<SkillCategory>();
        foreach (var category in sourceCategories)
        {
            var translatedCatName = !string.IsNullOrEmpty(category.Name) && catIdx < translatedCategoryNames.Count
                ? translatedCategoryNames[catIdx++]
                : category.Name;
            var subCategories = new List<SkillSubCategory>();
            foreach (var sub in category.SubCategories)
            {
                var translatedSubName = !string.IsNullOrEmpty(sub.Name) && subIdx < translatedSubCategoryNames.Count
                    ? translatedSubCategoryNames[subIdx++]
                    : sub.Name;
                subCategories.Add(new SkillSubCategory
                {
                    Name = translatedSubName,
                    Items = sub.Items
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
}
