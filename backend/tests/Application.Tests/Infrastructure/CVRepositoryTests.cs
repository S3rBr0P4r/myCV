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

    public CVRepositoryTests()
    {
        _sourceMock = new Mock<ICvSource>();
        _repository = new CVRepository(_sourceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSourceIsNull()
    {
        var act = () => new CVRepository(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public async Task GetCVAsync_ShouldReturnCvFromSource()
    {
        var expected = new CV
        {
            Name = "Test",
            LastName = "User",
            Title = "Dev",
            Summary = "S",
            Experiences = [],
            SkillCategories = []
        };
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _repository.GetCVAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetCVAsync_ShouldCallSource_ExactlyOnce()
    {
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CV
            {
                Name = "T",
                LastName = "U",
                Title = "D",
                Summary = "S",
                Experiences = [],
                SkillCategories = []
            });

        await _repository.GetCVAsync();

        _sourceMock.Verify(s => s.GetCvAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCVAsync_ShouldPassCultureParameter()
    {
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CV
            {
                Name = "T",
                LastName = "U",
                Title = "D",
                Summary = "S",
                Experiences = [],
                SkillCategories = []
            });

        await _repository.GetCVAsync("es");

        _sourceMock.Verify(s => s.GetCvAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCVAsync_ShouldPassCancellationToken()
    {
        var cts = new CancellationTokenSource();
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CV
            {
                Name = "T",
                LastName = "U",
                Title = "D",
                Summary = "S",
                Experiences = [],
                SkillCategories = []
            });

        await _repository.GetCVAsync(cancellationToken: cts.Token);

        _sourceMock.Verify(s => s.GetCvAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetCVAsync_ShouldPropagateSourceException()
    {
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Source error"));

        var act = () => _repository.GetCVAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Source error");
    }

    [Fact]
    public async Task GetCVAsync_ShouldReturnCvWhenCultureIsNull()
    {
        var expected = new CV
        {
            Name = "T",
            LastName = "U",
            Title = "D",
            Summary = "S",
            Experiences = [],
            SkillCategories = []
        };
        _sourceMock.Setup(s => s.GetCvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _repository.GetCVAsync(null);

        result.Should().BeSameAs(expected);
    }
}
