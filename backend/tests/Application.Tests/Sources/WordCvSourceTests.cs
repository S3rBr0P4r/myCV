using Backend.Domain.Exceptions;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Sources;
using DocumentFormat.OpenXml;
using Moq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        var filePath = CreateValidDocument();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Should().NotBeNull();
        cv.Name.Should().Be("John");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Creative Developer");
        cv.Summary.Should().Be("Building digital experiences.");
        cv.Experiences.Should().HaveCount(2);
        cv.Experiences[0].Period.Should().Be("2024 - PRESENT");
        cv.Experiences[0].Role.Should().Be("Senior Developer");
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Experiences[0].Description.Should().Be("Building things.");
        cv.Experiences[0].Background.Should().Be("bg-1");
        cv.Experiences[1].Period.Should().Be("2021 - 2023");
        cv.Experiences[1].Role.Should().Be("Full Stack Engineer");
        cv.Experiences[1].Company.Should().Be("StartupX");
        cv.Experiences[1].Description.Should().Be("Creating products.");
        cv.Experiences[1].Background.Should().Be("bg-2");
        cv.Skills.Should().BeEquivalentTo(["C#", ".NET", "TypeScript"]);
    }

    [Fact]
    public async Task GetCvAsync_MultiLineSummary_ShouldCollectAllLinesUntilNextLabel()
    {
        var filePath = CreateDocumentWithMultiLineSummary();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Summary.Should().Be("First line of summary.\nSecond line of summary.\nThird line.");
    }

    [Fact]
    public async Task GetCvAsync_MultipleExperienceSections_ShouldMapAll()
    {
        var filePath = CreateValidDocument();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Experiences.Should().HaveCount(2);
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Experiences[1].Company.Should().Be("StartupX");
    }

    [Fact]
    public async Task GetCvAsync_NoPeriodSections_ShouldReturnEmptyExperiences()
    {
        var filePath = CreateDocumentWithHeaderOnly();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Experiences.Should().BeEmpty();
        cv.Name.Should().Be("Jane");
        cv.Skills.Should().BeEquivalentTo(["C#"]);
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
        var filePath = CreateValidDocument();
        var sut = CreateSut(filePath);

        var first = await sut.GetCvAsync();
        var second = await sut.GetCvAsync();

        second.Should().BeSameAs(first);
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
        return new DiscordNotifier(new HttpClient(), options);
    }

    private string CreateValidDocument()
    {
        var path = Path.Combine(_tempDir, "valid.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Name: John");
        AddParagraph(body, "LastName: Doe");
        AddParagraph(body, "Title: Creative Developer");
        AddParagraph(body, "Summary: Building digital experiences.");
        AddParagraph(body, "Period: 2024 - PRESENT");
        AddParagraph(body, "Role: Senior Developer");
        AddParagraph(body, "Company: Acme Corp");
        AddParagraph(body, "Description: Building things.");
        AddParagraph(body, "Background: bg-1");
        AddParagraph(body, "Period: 2021 - 2023");
        AddParagraph(body, "Role: Full Stack Engineer");
        AddParagraph(body, "Company: StartupX");
        AddParagraph(body, "Description: Creating products.");
        AddParagraph(body, "Background: bg-2");
        AddParagraph(body, "Skills: C#, .NET, TypeScript");

        return path;
    }

    private string CreateDocumentWithMultiLineSummary()
    {
        var path = Path.Combine(_tempDir, "multiline_summary.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Name: Alice");
        AddParagraph(body, "LastName: Wonder");
        AddParagraph(body, "Title: Developer");
        AddParagraph(body, "Summary: First line of summary.");
        AddParagraph(body, "Second line of summary.");
        AddParagraph(body, "Third line.");
        AddParagraph(body, "Skills: C#");

        return path;
    }

    private string CreateDocumentWithHeaderOnly()
    {
        var path = Path.Combine(_tempDir, "header_only.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Name: Jane");
        AddParagraph(body, "LastName: Smith");
        AddParagraph(body, "Title: Architect");
        AddParagraph(body, "Summary: Just a summary.");
        AddParagraph(body, "Skills: C#");

        return path;
    }

    private static void AddParagraph(Body body, string text)
    {
        body.AppendChild(new Paragraph(new Run(new Text(text))));
    }
}
