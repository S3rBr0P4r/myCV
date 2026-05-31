using Backend.Application.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Mappings;

public static class CVMappingExtensions
{
    public static CVDto ToDto(this CV cv)
    {
        return new CVDto(
            Name: cv.Name,
            LastName: cv.LastName,
            Title: cv.Title,
            Summary: cv.Summary,
            Experiences: cv.Experiences.Select(e => e.ToDto()).ToList(),
            Skills: cv.Skills);
    }

    public static ExperienceDto ToDto(this Experience experience)
    {
        return new ExperienceDto(
            Period: experience.Period,
            Role: experience.Role,
            Company: experience.Company,
            Description: experience.Description,
            Background: experience.Background);
    }
}
