namespace Backend.Domain.Entities;

public sealed class SkillSubCategory
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}
