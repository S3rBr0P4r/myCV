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
            ContactInfo: cv.ContactInfo?.ToDto(),
            Experiences: cv.Experiences.Select(e => e.ToDto()).ToList(),
            SkillCategories: cv.SkillCategories.Select(sc => sc.ToDto()).ToList(),
            Education: cv.Education.Select(e => e.ToDto()).ToList(),
            Certifications: cv.Certifications.Select(c => c.ToDto()).ToList());
    }

    public static ExperienceDto ToDto(this Experience experience)
    {
        return new ExperienceDto(
            Period: experience.Period,
            Role: experience.Role,
            Company: experience.Company,
            CompanyUrl: experience.CompanyUrl,
            Location: experience.Location,
            WorkMode: experience.WorkMode,
            Description: experience.Description,
            Background: experience.Background);
    }

    public static ContactInfoDto ToDto(this ContactInfo contact)
    {
        return new ContactInfoDto(
            Email: contact.Email,
            Phone: contact.Phone,
            Location: contact.Location,
            WillingnessToTravel: contact.WillingnessToTravel);
    }

    public static SkillCategoryDto ToDto(this SkillCategory category)
    {
        return new SkillCategoryDto(
            Name: category.Name,
            SubCategories: category.SubCategories.Select(sc => sc.ToDto()).ToList());
    }

    public static SkillSubCategoryDto ToDto(this SkillSubCategory subCategory)
    {
        return new SkillSubCategoryDto(
            Name: subCategory.Name,
            Items: subCategory.Items.ToList());
    }

    public static EducationDto ToDto(this Education education)
    {
        return new EducationDto(
            Degree: education.Degree,
            Institution: education.Institution,
            Notes: education.Notes);
    }

    public static CertificationDto ToDto(this Certification certification)
    {
        return new CertificationDto(
            Category: certification.Category,
            Title: certification.Title,
            Issuer: certification.Issuer);
    }
}
