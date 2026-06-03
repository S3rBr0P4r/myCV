namespace Backend.Application.DTOs;

public sealed record SkillCategoryDto(
    string Name,
    IReadOnlyList<SkillSubCategoryDto> SubCategories);
