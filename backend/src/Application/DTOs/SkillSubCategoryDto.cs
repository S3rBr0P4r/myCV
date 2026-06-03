namespace Backend.Application.DTOs;

public sealed record SkillSubCategoryDto(
    string Name,
    IReadOnlyList<string> Items);
