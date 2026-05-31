namespace Backend.Application.DTOs;

public sealed record ExperienceDto(
    string Period,
    string Role,
    string Company,
    string Description,
    string Background);

public sealed record CVDto(
    string Name,
    string LastName,
    string Title,
    string Summary,
    IReadOnlyList<ExperienceDto> Experiences,
    IReadOnlyList<string> Skills);
