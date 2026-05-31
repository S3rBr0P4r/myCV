namespace Backend.Domain.Entities;

public sealed class Experience
{
    public required string Period { get; init; }
    public required string Role { get; init; }
    public required string Company { get; init; }
    public required string Description { get; init; }
    public string Background { get; init; } = string.Empty;
}

public sealed class CV
{
    public required string Name { get; init; }
    public required string LastName { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<Experience> Experiences { get; init; }
    public required IReadOnlyList<string> Skills { get; init; }
}
