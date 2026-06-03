using Backend.Application.UseCases.GetCV;
using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Backend.Tests.Application.UseCases;

public sealed class GetCVHandlerTests
{
    private readonly Mock<ICVRepository> _repositoryMock;
    private readonly Mock<ITranslationService> _translationMock;
    private readonly GetCVHandler _handler;

    public GetCVHandlerTests()
    {
        _repositoryMock = new Mock<ICVRepository>();
        _translationMock = new Mock<ITranslationService>();
        _translationMock
            .Setup(t => t.TranslateAsync(It.IsAny<CV>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CV?)null);
        _handler = new GetCVHandler(_repositoryMock.Object, _translationMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCVDto_WhenRepositoryReturnsData()
    {
        var cv = new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Developer",
            Summary = "A great developer",
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
            Education = [],
            Certifications = []
        };

        _repositoryMock
            .Setup(r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cv);

        var result = await _handler.HandleAsync(new GetCVQuery());

        result.Should().NotBeNull();
        result.CV.Name.Should().Be("John");
        result.CV.LastName.Should().Be("Doe");
        result.CV.Experiences.Should().HaveCount(1);
        result.CV.Experiences[0].Role.Should().Be("Senior Dev");
        result.CV.SkillCategories.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepository_ExactlyOnce()
    {
        _repositoryMock
            .Setup(r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CV
            {
                Name = "Test",
                LastName = "User",
                Title = "Title",
                Summary = "Summary",
                Experiences = [],
                SkillCategories = [],
                Education = [],
                Certifications = []
            });

        await _handler.HandleAsync(new GetCVQuery());

        _repositoryMock.Verify(
            r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRepositoryIsNull()
    {
        var act = () => new GetCVHandler(null!, Mock.Of<ITranslationService>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cvRepository");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTranslationServiceIsNull()
    {
        var act = () => new GetCVHandler(Mock.Of<ICVRepository>(), null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("translationService");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenQueryIsNull()
    {
        var act = () => _handler.HandleAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapExperiences_Correctly()
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
                    Period = "2020 - 2022",
                    Role = "Backend Developer",
                    Company = "StartupX",
                    Description = "Microservices"
                },
                new Experience
                {
                    Period = "2022 - Present",
                    Role = "Tech Lead",
                    Company = "BigCorp",
                    Description = "Platform engineering"
                }
            ],
            SkillCategories = [],
            Education = [],
            Certifications = []
        };

        _repositoryMock
            .Setup(r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cv);

        var result = await _handler.HandleAsync(new GetCVQuery());

        result.CV.Experiences.Should().HaveCount(2);
        result.CV.Experiences[0].Company.Should().Be("StartupX");
        result.CV.Experiences[1].Company.Should().Be("BigCorp");
        result.CV.SkillCategories.Should().BeEmpty();
    }
}
