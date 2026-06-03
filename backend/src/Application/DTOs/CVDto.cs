namespace Backend.Application.DTOs;

public sealed record ExperienceDto(
    string Period,
    string Role,
    string Company,
    string CompanyUrl,
    string Location,
    string WorkMode,
    string Description,
    string Background);

public sealed record CVDto(
    string Name,
    string LastName,
    string Title,
    string Summary,
    ContactInfoDto? ContactInfo,
    IReadOnlyList<ExperienceDto> Experiences,
    IReadOnlyList<SkillCategoryDto> SkillCategories,
    IReadOnlyList<EducationDto> Education,
    IReadOnlyList<CertificationDto> Certifications);
