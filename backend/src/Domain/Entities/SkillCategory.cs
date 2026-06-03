namespace Backend.Domain.Entities;

public sealed class SkillCategory
{
    public required string Name { get; init; }
    public required IReadOnlyList<SkillSubCategory> SubCategories { get; init; }
}
