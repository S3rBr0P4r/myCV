namespace Backend.Infrastructure.Options;

public sealed class DeepLOptions
{
    public const string SectionName = "DeepL";

    public string? AuthKey { get; init; }

    public int CacheDurationMinutes { get; init; } = 1440;
}
