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
    private readonly GetCVHandler _handler;

    public GetCVHandlerTests()
    {
        _repositoryMock = new Mock<ICVRepository>();
        _handler = new GetCVHandler(_repositoryMock.Object);
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
            Skills = ["C#", ".NET"]
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
        result.CV.Skills.Should().HaveCount(2);
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
                Skills = []
            });

        await _handler.HandleAsync(new GetCVQuery());

        _repositoryMock.Verify(
            r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRepositoryIsNull()
    {
        var act = () => new GetCVHandler(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cvRepository");
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
            Skills = ["Architecture", "C#", "Azure"]
        };

        _repositoryMock
            .Setup(r => r.GetCVAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cv);

        var result = await _handler.HandleAsync(new GetCVQuery());

        result.CV.Experiences.Should().HaveCount(2);
        result.CV.Experiences[0].Company.Should().Be("StartupX");
        result.CV.Experiences[1].Company.Should().Be("BigCorp");
        result.CV.Skills.Should().Contain("Azure");
    }
}
