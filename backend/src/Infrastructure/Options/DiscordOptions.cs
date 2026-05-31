namespace Backend.Infrastructure.Options;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";

    public string WebhookUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 5;
}
