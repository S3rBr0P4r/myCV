using Backend.Application.DTOs;
using Backend.Application.Mappings;
using Backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Application.Mappings;

public sealed class CVMappingExtensionsTests
{
    private readonly CV _cv = new()
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
                CompanyUrl = "https://acme.com",
                Location = "Remote",
                WorkMode = "Remote",
                Description = "Building things",
                Background = "acme-bg.jpg"
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
        ContactInfo = new ContactInfo
        {
            Email = "john@example.com",
            Phone = "+123456789",
            Location = "NYC",
            WillingnessToTravel = "Yes"
        }
    };

    private static CVDto InvokeToDto(CV cv) => cv.ToDto();

    [Fact]
    public void ToDto_ShouldMapAllCvProperties()
    {
        var dto = InvokeToDto(_cv);

        dto.Name.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.Title.Should().Be("Developer");
        dto.Summary.Should().Be("A skilled developer");
    }

    [Fact]
    public void ToDto_ShouldMapExperiences()
    {
        var dto = InvokeToDto(_cv);

        dto.Experiences.Should().HaveCount(1);
        var exp = dto.Experiences[0];
        exp.Period.Should().Be("2024 - Present");
        exp.Role.Should().Be("Senior Dev");
        exp.Company.Should().Be("Acme");
        exp.CompanyUrl.Should().Be("https://acme.com");
        exp.Location.Should().Be("Remote");
        exp.WorkMode.Should().Be("Remote");
        exp.Description.Should().Be("Building things");
        exp.Background.Should().Be("acme-bg.jpg");
    }

    [Fact]
    public void ToDto_ShouldMapSkillCategories()
    {
        var dto = InvokeToDto(_cv);

        dto.SkillCategories.Should().HaveCount(1);
        var cat = dto.SkillCategories[0];
        cat.Name.Should().Be("Languages");
        cat.SubCategories.Should().HaveCount(1);
        cat.SubCategories[0].Name.Should().Be(".NET");
        cat.SubCategories[0].Items.Should().Equal("C#", ".NET");
    }

    [Fact]
    public void ToDto_ShouldMapContactInfo()
    {
        var dto = InvokeToDto(_cv);

        dto.ContactInfo.Should().NotBeNull();
        dto.ContactInfo!.Email.Should().Be("john@example.com");
        dto.ContactInfo.Phone.Should().Be("+123456789");
        dto.ContactInfo.Location.Should().Be("NYC");
        dto.ContactInfo.WillingnessToTravel.Should().Be("Yes");
    }

    [Fact]
    public void ToDto_ShouldSetLinkedInAndGitHubToNull()
    {
        var dto = InvokeToDto(_cv);

        dto.LinkedInUrl.Should().BeNull();
        dto.GitHubUrl.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldHandleEmptyExperiences()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences = [],
            SkillCategories = _cv.SkillCategories,
            ContactInfo = _cv.ContactInfo
        };
        var dto = InvokeToDto(cv);

        dto.Experiences.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldHandleEmptySkillCategories()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences = _cv.Experiences,
            SkillCategories = [],
            ContactInfo = _cv.ContactInfo
        };
        var dto = InvokeToDto(cv);

        dto.SkillCategories.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldHandleNullContactInfo()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences = _cv.Experiences,
            SkillCategories = _cv.SkillCategories,
            ContactInfo = null
        };
        var dto = InvokeToDto(cv);

        dto.ContactInfo.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldHandleExperienceWithMinimalData()
    {
        var cv = new CV
        {
            Name = "Test",
            LastName = "User",
            Title = "T",
            Summary = "S",
            Experiences =
            [
                new Experience { Period = "2020", Role = "Dev", Company = "Co", Description = "Work" }
            ],
            SkillCategories = []
        };
        var dto = InvokeToDto(cv);

        dto.Experiences.Should().HaveCount(1);
        dto.Experiences[0].CompanyUrl.Should().BeEmpty();
        dto.Experiences[0].Location.Should().BeEmpty();
        dto.Experiences[0].WorkMode.Should().BeEmpty();
        dto.Experiences[0].Background.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldHandleSkillCategoryWithNoSubCategories()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences = _cv.Experiences,
            SkillCategories =
            [
                new SkillCategory { Name = "Empty", SubCategories = new List<SkillSubCategory>().AsReadOnly() }
            ],
            ContactInfo = _cv.ContactInfo
        };
        var dto = InvokeToDto(cv);

        dto.SkillCategories.Should().HaveCount(1);
        dto.SkillCategories[0].SubCategories.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldHandleSubCategoryWithNoItems()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences = _cv.Experiences,
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "Empty",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = "EmptyCat", Items = new List<string>().AsReadOnly() }
                    }.AsReadOnly()
                }
            ],
            ContactInfo = _cv.ContactInfo
        };
        var dto = InvokeToDto(cv);

        dto.SkillCategories[0].SubCategories[0].Items.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldHandleMultipleExperiences()
    {
        var cv = new CV
        {
            Name = _cv.Name,
            LastName = _cv.LastName,
            Title = _cv.Title,
            Summary = _cv.Summary,
            Experiences =
            [
                new Experience { Period = "2020", Role = "Dev", Company = "A", Description = "X" },
                new Experience { Period = "2021", Role = "Sr Dev", Company = "B", Description = "Y" },
            ],
            SkillCategories = _cv.SkillCategories,
            ContactInfo = _cv.ContactInfo
        };
        var dto = InvokeToDto(cv);

        dto.Experiences.Should().HaveCount(2);
        dto.Experiences[0].Company.Should().Be("A");
        dto.Experiences[1].Company.Should().Be("B");
    }
}
