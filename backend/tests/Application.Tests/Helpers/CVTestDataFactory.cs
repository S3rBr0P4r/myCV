using Backend.Domain.Entities;

namespace Backend.Tests.Helpers;

internal static class CVTestDataFactory
{
    internal static CV CreateSampleCV()
    {
        return new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Developer",
            Summary = "A skilled developer",
            Experiences =
            [
                new Experience
                {
                    Period = "2024 - Present",
                    Role = "Senior Dev",
                    Company = "Acme",
                    Description = "Building things"
                }
            ],
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "Languages",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = ".NET", Items = new List<string> { "C#", ".NET" }.AsReadOnly() }
                    }.AsReadOnly()
                }
            ],
        };
    }
}
