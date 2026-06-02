using System.Globalization;
using System.Text.Json.Serialization;
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
    private readonly ILogger<DeepLTranslationService> _logger;

    public DeepLTranslationService(
        HttpClient httpClient,
        IOptions<DeepLOptions> options,
        IMemoryCache cache,
        ILogger<DeepLTranslationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CV?> TranslateAsync(CV source, string targetLanguage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AuthKey))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeepL AuthKey not configured — skipping translation");
            }
            return null;
        }

        var lang = NormalizeLanguage(targetLanguage);

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
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(ex, "DeepL translation failed for language {Lang}, falling back to English", lang);
            }
            return null;
        }
    }

    private async Task<CV?> TranslateCoreAsync(CV source, string lang, CancellationToken cancellationToken)
    {
        var summary = source.Summary;
        var title = source.Title;
        var periods = source.Experiences.Select(e => e.Period).ToList();
        var roles = source.Experiences.Select(e => e.Role).ToList();
        var descriptions = source.Experiences.Select(e => e.Description).ToList();
        var skills = source.Skills.ToList();

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
        allTexts.AddRange(descriptions.Where(t => !string.IsNullOrEmpty(t)));
        allTexts.AddRange(skills.Where(t => !string.IsNullOrEmpty(t)));

        if (allTexts.Count == 0)
        {
            return source;
        }

        var segments = allTexts.Select(t => new { text = t }).ToList();

        var request = new DeepLRequest
        {
            Text = segments.Select(s => s.text).ToArray(),
            TargetLang = lang
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, DeepLApiUrl)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("DeepL-Auth-Key", _options.AuthKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken: cancellationToken);

        if (result?.Translations is null || result.Translations.Length != allTexts.Count)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    "DeepL returned {Count} translations but expected {Expected}", result?.Translations?.Length ?? 0, allTexts.Count);
            }
            return null;
        }

        var translatedTexts = result.Translations.Select(t => t.Text.Length > MaxFieldLength ? t.Text[..MaxFieldLength] : t.Text).ToArray();
        int idx = 0;

        string? translatedSummary = null;
        if (!string.IsNullOrEmpty(summary))
        {
            translatedSummary = translatedTexts[idx++];
        }

        string? translatedTitle = null;
        if (!string.IsNullOrEmpty(title))
        {
            translatedTitle = translatedTexts[idx++];
        }

        var translatedPeriods = periods.Select(p => !string.IsNullOrEmpty(p) ? translatedTexts[idx++] : p).ToList();
        var translatedRoles = roles.Select(r => !string.IsNullOrEmpty(r) ? translatedTexts[idx++] : r).ToList();
        var translatedDescriptions = descriptions.Select(d => !string.IsNullOrEmpty(d) ? translatedTexts[idx++] : d).ToList();
        var translatedSkills = skills.Select(s => !string.IsNullOrEmpty(s) ? translatedTexts[idx++] : s).ToList();

        var translatedExperiences = source.Experiences.Select((e, i) => new Experience
        {
            Period = translatedPeriods[i],
            Role = translatedRoles[i],
            Company = e.Company,
            Description = translatedDescriptions[i],
            Background = e.Background
        }).ToList();

        return new CV
        {
            Name = source.Name,
            LastName = source.LastName,
            Title = translatedTitle ?? source.Title,
            Summary = translatedSummary ?? source.Summary,
            Experiences = translatedExperiences.AsReadOnly(),
            Skills = translatedSkills.AsReadOnly()
        };
    }

    private static string? NormalizeLanguage(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        var primary = culture.Split(',')[0].Trim().Split(';')[0].Trim();

        try
        {
            var ci = CultureInfo.GetCultureInfo(primary);
            var lang = ci.TwoLetterISOLanguageName.ToUpperInvariant();
            return lang;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    private sealed class DeepLRequest
    {
        [JsonPropertyName("text")]
        public string[] Text { get; init; } = [];

        [JsonPropertyName("target_lang")]
        public string TargetLang { get; init; } = string.Empty;
    }

    private sealed class DeepLResponse
    {
        [JsonPropertyName("translations")]
        public DeepLTranslation[] Translations { get; init; } = [];
    }

    private sealed class DeepLTranslation
    {
        [JsonPropertyName("detected_source_language")]
        public string DetectedSourceLanguage { get; init; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;
    }
}
