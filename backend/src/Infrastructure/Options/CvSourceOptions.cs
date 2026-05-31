namespace Backend.Infrastructure.Options;

public sealed class CvSourceOptions
{
    public const string SectionName = "CvSource";

    public string FilePath { get; init; } = string.Empty;
}
