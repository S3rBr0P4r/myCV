using System.Text.Json.Serialization;

namespace Backend.Infrastructure.Services;

internal sealed class DeepLRequest
{
    [JsonPropertyName("text")]
    public string[] Text { get; init; } = [];

    [JsonPropertyName("target_lang")]
    public string TargetLang { get; init; } = string.Empty;
}

internal sealed class DeepLResponse
{
    [JsonPropertyName("translations")]
    public DeepLTranslation[] Translations { get; init; } = [];
}

internal sealed class DeepLTranslation
{
    [JsonPropertyName("detected_source_language")]
    public string DetectedSourceLanguage { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
