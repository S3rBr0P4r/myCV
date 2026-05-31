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
            Skills = ["C#"]
        };

        cv.Name.Should().Be("John");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Developer");
        cv.Summary.Should().Be("A great developer");
        cv.Experiences.Should().BeEmpty();
        cv.Skills.Should().ContainSingle().Which.Should().Be("C#");
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
            Skills = ["Architecture", "C#"]
        };

        cv.Experiences.Should().HaveCount(1);
        cv.Experiences[0].Role.Should().Be("Tech Lead");
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Skills.Should().HaveCount(2);
    }
}
