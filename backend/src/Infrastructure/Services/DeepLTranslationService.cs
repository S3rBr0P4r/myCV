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
    private const string TranslationContext =
        "The software engineer works in a remote position. The technology stack includes various programming languages and frameworks. Software development, IT skills, engineering.";

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
        var descriptionTexts = descriptions
            .SelectMany(DescriptionSegmenter.CollectTranslatable)
            .Where(t => !string.IsNullOrEmpty(t.Text))
            .ToList();
        var categoryNames = source.SkillCategories.Select(c => c.Name).ToList();
        var subCategoryNames = source.SkillCategories
            .SelectMany(c => c.SubCategories)
            .Select(s => s.Name)
            .ToList();
        var skillItems = source.SkillCategories
            .SelectMany(c => c.SubCategories)
            .SelectMany(s => s.Items)
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
        allTexts.AddRange(descriptionTexts.Select(AddFirstPersonSubject));
        allTexts.AddRange(categoryNames.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(subCategoryNames.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(skillItems.Where(t => !string.IsNullOrEmpty(t)));
        if (allTexts.Count == 0)
        {
            return source;
        }
        var translatedTexts = await CallDeepLApiAsync(allTexts, lang, timeoutToken);
        if (translatedTexts is null)
        {
            return null;
        }

        return TranslatedCVBuilder.Build(source, summary, title, periods, roles, companies,
            locations, workModes, descriptions, categoryNames, subCategoryNames, skillItems, translatedTexts);
    }

    private static string AddFirstPersonSubject(DescriptionSegmenter.TranslatableSegment segment)
    {
        return segment.IsLabel ? segment.Text : "I " + segment.Text;
    }

    private async Task<string[]?> CallDeepLApiAsync(List<string> allTexts, string lang, CancellationToken timeoutToken)
    {
        var initial = await SendTranslationsAsync(allTexts, lang, sourceLang: null, timeoutToken);
        if (initial is null)
        {
            return null;
        }

        var stuckIndexes = FindUntranslatedIndexes(allTexts, initial, lang);
        if (stuckIndexes.Count > 0)
        {
            await RetryStuckTranslationsAsync(allTexts, initial, stuckIndexes, lang, timeoutToken);
        }

        return initial
            .Select(t => t.Text.Length > MaxFieldLength ? t.Text[..MaxFieldLength] : t.Text)
            .ToArray();
    }

    private async Task<DeepLTranslation[]?> SendTranslationsAsync(
        List<string> texts, string lang, string? sourceLang, CancellationToken timeoutToken)
    {
        var request = new DeepLRequest
        {
            Text = texts.ToArray(),
            TargetLang = lang,
            SourceLang = sourceLang,
            Context = TranslationContext
        };
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, DeepLApiUrl) { Content = JsonContent.Create(request) };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("DeepL-Auth-Key", _options.AuthKey);
        var response = await _httpClient.SendAsync(httpRequest, timeoutToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken: timeoutToken);
        if (result?.Translations is null || result.Translations.Length != texts.Count)
        {
            _logger.LogWarning("DeepL returned {Count} translations but expected {Expected}",
                result?.Translations?.Length ?? 0, texts.Count);
            return null;
        }
        return result.Translations;
    }

    private static List<int> FindUntranslatedIndexes(
        List<string> sourceTexts, DeepLTranslation[] translations, string lang)
    {
        var indexes = new List<int>();
        for (var i = 0; i < translations.Length; i++)
        {
            if (IsPassthrough(sourceTexts[i], translations[i], lang))
            {
                indexes.Add(i);
            }
        }
        return indexes;
    }

    private static bool IsPassthrough(string sourceText, DeepLTranslation translation, string targetLang)
    {
        return string.Equals(translation.Text, sourceText, StringComparison.Ordinal)
            && string.Equals(translation.DetectedSourceLanguage, targetLang, StringComparison.OrdinalIgnoreCase);
    }

    private async Task RetryStuckTranslationsAsync(
        List<string> allTexts,
        DeepLTranslation[] translations,
        List<int> stuckIndexes,
        string lang,
        CancellationToken timeoutToken)
    {
        var retryTexts = stuckIndexes.Select(i => allTexts[i]).ToList();
        DeepLTranslation[]? retried;
        try
        {
            retried = await SendTranslationsAsync(retryTexts, lang, "EN", timeoutToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DeepL forced-source retry failed, keeping initial translations");
            return;
        }

        if (retried is null)
        {
            _logger.LogWarning("DeepL forced-source retry returned unexpected count, keeping initial translations");
            return;
        }

        for (var i = 0; i < stuckIndexes.Count; i++)
        {
            translations[stuckIndexes[i]] = retried[i];
        }
    }
}
