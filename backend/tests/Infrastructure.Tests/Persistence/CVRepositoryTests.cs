using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using Backend.Infrastructure.Persistence;
using FluentAssertions;
using Moq;
using Xunit;

namespace Backend.Tests.Infrastructure.Persistence;

public sealed class CVRepositoryTests
{
    private readonly Mock<ICvSource> _sourceMock;
    private readonly CVRepository _repository;

    private static readonly CV EnglishCv = new()
    {
        Name = "John",
        LastName = "Doe",
        Title = "Creative Developer & Architect",
        Summary = "Building digital experiences.",
        Experiences =
        [
            new Experience
            {
                Period = "2024 - PRESENT",
                Role = "Senior Developer",
                Company = "TECH NOIR SYSTEMS",
                Description = "Redefining the web.",
                Background = "bg-placeholder-1"
            },
            new Experience
            {
                Period = "2021 - 2023",
                Role = "Full Stack Engineer",
                Company = "NEON DIGITAL",
                Description = "Creating immersive worlds.",
                Background = "bg-placeholder-2"
            }
        ],
        Skills = ["C#", ".NET 10", "TypeScript", "Clean Architecture"]
    };

    public CVRepositoryTests()
    {
        _sourceMock = new Mock<ICvSource>();
        _repository = new CVRepository(_sourceMock.Object);
    }

    [Fact]
    public async Task GetCVAsync_ShouldReturnCV_WithAllProperties()
    {
        _sourceMock
            .Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(EnglishCv);

        var result = await _repository.GetCVAsync();

        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Title.Should().Be("Creative Developer & Architect");
        result.Experiences.Should().NotBeEmpty();
        result.Skills.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCVAsync_ShouldReturnExperiences_InOrder()
    {
        _sourceMock
            .Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(EnglishCv);

        var result = await _repository.GetCVAsync();

        result.Experiences.Should().HaveCount(2);
        result.Experiences[0].Role.Should().Be("Senior Developer");
        result.Experiences[1].Role.Should().Be("Full Stack Engineer");
    }

    [Fact]
    public async Task GetCVAsync_ShouldReturnExpectedSkills()
    {
        _sourceMock
            .Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(EnglishCv);

        var result = await _repository.GetCVAsync();

        result.Skills.Should().Contain("C#")
            .And.Contain(".NET 10")
            .And.Contain("TypeScript")
            .And.Contain("Clean Architecture");
    }

    [Fact]
    public async Task GetCVAsync_ShouldRespectCancellationToken()
    {
        _sourceMock
            .Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(EnglishCv);

        using var cts = new CancellationTokenSource();
        var act = () => _repository.GetCVAsync(cancellationToken: cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSourceIsNull()
    {
        var act = () => new CVRepository(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }
}
