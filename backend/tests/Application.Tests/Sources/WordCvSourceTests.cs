using Backend.Domain.Exceptions;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Sources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Backend.Tests.Infrastructure.Sources;

public sealed class WordCvSourceTests : IDisposable
{
    private readonly string _tempDir;

    public WordCvSourceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "mycv_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetCvAsync_ValidDocument_ShouldReturnCVWithAllProperties()
    {
        var filePath = TestDocumentBuilder.CreateValidDocument(_tempDir);
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Should().NotBeNull();
        cv.Name.Should().Be("John Doe");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Creative Developer");
        cv.Summary.Should().Be("Building digital experiences.\nPassionate about code.");
        cv.Experiences.Should().HaveCount(2);
        cv.Experiences[0].Period.Should().Be("2024 - PRESENT");
        cv.Experiences[0].Role.Should().Be("Senior Developer");
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Experiences[0].CompanyUrl.Should().Be("https://acme.com");
        cv.Experiences[0].Location.Should().Be("Barcelona");
        cv.Experiences[0].WorkMode.Should().Be("Remote");
        cv.Experiences[0].Description.Should().Be("Building things.\nTech Stack: C#, .NET");
        cv.Experiences[1].Period.Should().Be("2021 - 2023");
        cv.Experiences[1].Role.Should().Be("Full Stack Engineer");
        cv.Experiences[1].Company.Should().Be("StartupX");
        cv.Experiences[1].CompanyUrl.Should().Be("https://startupx.io");
        cv.Experiences[1].Location.Should().Be("Madrid");
        cv.Experiences[1].WorkMode.Should().Be("Remote");
        cv.Experiences[1].Description.Should().Be("Creating products.\nTech Stack: TypeScript, React");
        cv.SkillCategories.Should().HaveCount(1);
        cv.SkillCategories[0].Name.Should().Be("Languages");
        cv.SkillCategories[0].SubCategories.Should().HaveCount(1);
        cv.SkillCategories[0].SubCategories[0].Name.Should().Be("General");
        cv.SkillCategories[0].SubCategories[0].Items.Should().BeEquivalentTo(["C#", ".NET", "TypeScript"]);
    }

    [Fact]
    public async Task GetCvAsync_MultiLineSummary_ShouldCollectAllLinesUntilNextSection()
    {
        var filePath = TestDocumentBuilder.CreateDocumentWithMultiLineSummary(_tempDir);
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Summary.Should().Be("First line of summary.\nSecond line of summary.\nThird line.");
    }

    [Fact]
    public async Task GetCvAsync_NoExperiences_ShouldReturnEmptyExperiences()
    {
        var filePath = TestDocumentBuilder.CreateDocumentWithNoExperiences(_tempDir);
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Experiences.Should().BeEmpty();
        cv.Name.Should().Be("Jane Smith");
        cv.SkillCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCvAsync_EmptyFilePath_ShouldThrowCvSourceClientException()
    {
        var options = Options.Create(new CvSourceOptions { FilePath = string.Empty });
        var notifier = CreateNoopNotifier();
        var sut = new WordCvSource(options, Mock.Of<ILogger<WordCvSource>>(), notifier);

        var act = () => sut.GetCvAsync();

        await act.Should().ThrowAsync<CvSourceClientException>();
    }

    [Fact]
    public async Task GetCvAsync_FileNotFound_ShouldThrowCvSourceClientException()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.docx");
        var sut = CreateSut(filePath);

        var act = () => sut.GetCvAsync();

        await act.Should().ThrowAsync<CvSourceClientException>();
    }

    [Fact]
    public async Task GetCvAsync_InvalidFile_ShouldThrowCvSourceClientException()
    {
        var filePath = Path.Combine(_tempDir, "invalid.docx");
        await File.WriteAllTextAsync(filePath, "This is not a valid .docx file.");
        var sut = CreateSut(filePath);

        var act = () => sut.GetCvAsync();

        await act.Should().ThrowAsync<CvSourceClientException>();
    }

    [Fact]
    public async Task GetCvAsync_CalledTwice_ShouldReturnCachedResult()
    {
        var filePath = TestDocumentBuilder.CreateValidDocument(_tempDir);
        var sut = CreateSut(filePath);

        var first = await sut.GetCvAsync();
        var second = await sut.GetCvAsync();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task GetCvAsync_HierarchicalSkills_ShouldPreserveStructure()
    {
        var filePath = TestDocumentBuilder.CreateDocumentWithHierarchicalSkills(_tempDir);
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.SkillCategories.Should().HaveCount(2);
        cv.SkillCategories[0].Name.Should().Be("Languages");
        cv.SkillCategories[0].SubCategories.Should().HaveCount(2);
        cv.SkillCategories[0].SubCategories[0].Name.Should().Be("Advanced");
        cv.SkillCategories[0].SubCategories[0].Items.Should().BeEquivalentTo(["C#", ".NET"]);
        cv.SkillCategories[0].SubCategories[1].Name.Should().Be("Working Knowledge");
        cv.SkillCategories[0].SubCategories[1].Items.Should().BeEquivalentTo(["TypeScript", "JavaScript"]);
        cv.SkillCategories[1].Name.Should().Be("Cloud");
        cv.SkillCategories[1].SubCategories.Should().HaveCount(1);
        cv.SkillCategories[1].SubCategories[0].Name.Should().Be("AWS");
        cv.SkillCategories[1].SubCategories[0].Items.Should().BeEquivalentTo(["Lambda", "S3"]);
    }

    [Fact]
    public async Task GetCvAsync_LegacyFormat_ShouldParseCorrectly()
    {
        var filePath = TestDocumentBuilder.CreateLegacyFormatDocument(_tempDir);
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Experiences.Should().HaveCount(2);
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Experiences[0].CompanyUrl.Should().BeEmpty();
        cv.Experiences[0].Location.Should().Be("Barcelona");
        cv.Experiences[0].WorkMode.Should().Be("Remote");
        cv.Experiences[0].Role.Should().Be("Senior Developer");
        cv.Experiences[0].Period.Should().Be("2024 - PRESENT");
        cv.Experiences[0].Description.Should().Be("Building things.\nTech Stack: C#, .NET");
        cv.Experiences[1].Company.Should().Be("StartupX");
        cv.Experiences[1].CompanyUrl.Should().BeEmpty();
        cv.Experiences[1].Location.Should().Be("Madrid");
        cv.Experiences[1].WorkMode.Should().Be("Onsite");
        cv.Experiences[1].Role.Should().Be("Full Stack Engineer");
        cv.Experiences[1].Period.Should().Be("2021 - 2023");
        cv.Experiences[1].Description.Should().Be("Creating products.\nTech Stack: TypeScript, React");
    }

    private static WordCvSource CreateSut(string filePath)
    {
        var options = Options.Create(new CvSourceOptions { FilePath = filePath });
        var notifier = CreateNoopNotifier();
        return new WordCvSource(options, Mock.Of<ILogger<WordCvSource>>(), notifier);
    }

    private static DiscordNotifier CreateNoopNotifier()
    {
        var options = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        return new DiscordNotifier(new HttpClient(), options, Mock.Of<ILogger<DiscordNotifier>>());
    }
}
