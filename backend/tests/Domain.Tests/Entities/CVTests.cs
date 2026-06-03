using Backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Domain.Entities;

public sealed class CVTests
{
    [Fact]
    public void CV_ShouldBeCreated_WithRequiredProperties()
    {
        var cv = new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Developer",
            Summary = "A great developer",
            Experiences = [],
            SkillCategories = [],
            Education = [],
            Certifications = []
        };

        cv.Name.Should().Be("John");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Developer");
        cv.Summary.Should().Be("A great developer");
        cv.Experiences.Should().BeEmpty();
        cv.SkillCategories.Should().BeEmpty();
    }

    [Fact]
    public void CV_ShouldContainExperiences()
    {
        var cv = new CV
        {
            Name = "Jane",
            LastName = "Smith",
            Title = "Architect",
            Summary = "Senior architect",
            Experiences =
            [
                new Experience
                {
                    Period = "2020 - Present",
                    Role = "Tech Lead",
                    Company = "Acme Corp",
                    Description = "Leading the platform team"
                }
            ],
            SkillCategories = [],
            Education = [],
            Certifications = []
        };

        cv.Experiences.Should().HaveCount(1);
        cv.Experiences[0].Role.Should().Be("Tech Lead");
        cv.Experiences[0].Company.Should().Be("Acme Corp");
    }

    [Fact]
    public void CV_ShouldSupportHierarchicalSkills()
    {
        var cv = new CV
        {
            Name = "Jane",
            LastName = "Smith",
            Title = "Architect",
            Summary = "Senior architect",
            Experiences = [],
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "Languages",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory
                        {
                            Name = "Advanced",
                            Items = new List<string> { "C#", "TypeScript" }.AsReadOnly()
                        }
                    }.AsReadOnly()
                }
            ],
            Education = [],
            Certifications = []
        };

        cv.SkillCategories.Should().HaveCount(1);
        cv.SkillCategories[0].Name.Should().Be("Languages");
        cv.SkillCategories[0].SubCategories.Should().HaveCount(1);
        cv.SkillCategories[0].SubCategories[0].Items.Should().Contain("C#");
    }
}
