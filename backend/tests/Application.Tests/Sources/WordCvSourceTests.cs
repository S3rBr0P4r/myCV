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
        cv.Education.Should().HaveCount(1);
        cv.Education[0].Degree.Should().Be("Bachelor in CS");
        cv.Education[0].Institution.Should().Be("University");
    }

    [Fact]
    public async Task GetCvAsync_MultiLineSummary_ShouldCollectAllLinesUntilNextSection()
    {
        var filePath = CreateDocumentWithMultiLineSummary();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Summary.Should().Be("First line of summary.\nSecond line of summary.\nThird line.");
    }

    [Fact]
    public async Task GetCvAsync_NoExperiences_ShouldReturnEmptyExperiences()
    {
        var filePath = CreateDocumentWithNoExperiences();
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
        var filePath = CreateValidDocument();
        var sut = CreateSut(filePath);

        var first = await sut.GetCvAsync();
        var second = await sut.GetCvAsync();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task GetCvAsync_HierarchicalSkills_ShouldPreserveStructure()
    {
        var filePath = CreateDocumentWithHierarchicalSkills();
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
    public async Task GetCvAsync_Certifications_ShouldParseByCategory()
    {
        var filePath = CreateDocumentWithCertifications();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Certifications.Should().HaveCount(2);
        cv.Certifications[0].Category.Should().Be("Agile");
        cv.Certifications[0].Title.Should().Be("PSM I");
        cv.Certifications[0].Issuer.Should().Be("Scrum.org");
        cv.Certifications[1].Category.Should().Be("Cloud");
        cv.Certifications[1].Title.Should().Be("AWS Practitioner");
        cv.Certifications[1].Issuer.Should().Be("Amazon");
    }

    [Fact]
    public async Task GetCvAsync_LegacyFormat_ShouldParseCorrectly()
    {
        var filePath = CreateLegacyFormatDocument();
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

    private string CreateLegacyFormatDocument()
    {
        var path = Path.Combine(_tempDir, "legacy.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "John Doe");
        AddParagraph(body, "Creative Developer");
        AddParagraph(body, "s3rbr0p4r@example.com");
        AddParagraph(body, "+34 123 456 789");
        AddParagraph(body, "Remote");
        AddParagraph(body, "Willingness to travel");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Building digital experiences.");
        AddParagraph(body, "Passionate about code.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "Acme Corp | Barcelona (Remote)");
        AddParagraph(body, "Senior Developer | 2024 - PRESENT");
        AddParagraph(body, "Building things.");
        AddParagraph(body, "Tech Stack: C#, .NET");
        AddParagraph(body, "StartupX | Madrid (Onsite)");
        AddParagraph(body, "Full Stack Engineer | 2021 - 2023");
        AddParagraph(body, "Creating products.");
        AddParagraph(body, "Tech Stack: TypeScript, React");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "Languages");
        AddParagraph(body, "C#, .NET, TypeScript");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "Bachelor in CS | University");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");

        return path;
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

    private string CreateValidDocument()
    {
        var path = Path.Combine(_tempDir, "valid.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "John Doe");
        AddParagraph(body, "Creative Developer");
        AddParagraph(body, "s3rbr0p4r@example.com");
        AddParagraph(body, "+34 123 456 789");
        AddParagraph(body, "Remote");
        AddParagraph(body, "Willingness to travel");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Building digital experiences.");
        AddParagraph(body, "Passionate about code.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "Acme Corp | https://acme.com");
        AddParagraph(body, "Barcelona | Remote");
        AddParagraph(body, "Senior Developer | 2024 - PRESENT");
        AddParagraph(body, "Building things.");
        AddParagraph(body, "Tech Stack: C#, .NET");
        AddParagraph(body, "StartupX | https://startupx.io");
        AddParagraph(body, "Madrid | Remote");
        AddParagraph(body, "Full Stack Engineer | 2021 - 2023");
        AddParagraph(body, "Creating products.");
        AddParagraph(body, "Tech Stack: TypeScript, React");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "Languages");
        AddParagraph(body, "C#, .NET, TypeScript");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "Bachelor in CS | University");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");

        return path;
    }

    private string CreateDocumentWithMultiLineSummary()
    {
        var path = Path.Combine(_tempDir, "multiline_summary.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Alice Wonder");
        AddParagraph(body, "Developer");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "First line of summary.");
        AddParagraph(body, "Second line of summary.");
        AddParagraph(body, "Third line.");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");

        return path;
    }

    private string CreateDocumentWithNoExperiences()
    {
        var path = Path.Combine(_tempDir, "no_experiences.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Jane Smith");
        AddParagraph(body, "Architect");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Just a summary.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");

        return path;
    }

    private string CreateDocumentWithHierarchicalSkills()
    {
        var path = Path.Combine(_tempDir, "hierarchical_skills.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Test User");
        AddParagraph(body, "Developer");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Summary text.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "Languages");
        AddParagraph(body, "Advanced");
        AddParagraph(body, "C#, .NET");
        AddParagraph(body, "Working Knowledge");
        AddParagraph(body, "TypeScript, JavaScript");
        AddParagraph(body, "Cloud");
        AddParagraph(body, "AWS");
        AddParagraph(body, "Lambda, S3");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");

        return path;
    }

    private string CreateDocumentWithCertifications()
    {
        var path = Path.Combine(_tempDir, "certifications.docx");

        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "Test User");
        AddParagraph(body, "Developer");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Summary.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "EDUCATION");
        AddParagraph(body, "CERTIFICATIONS & RELEVANT TRAINING");
        AddParagraph(body, "Agile");
        AddParagraph(body, "PSM I | Scrum.org");
        AddParagraph(body, "Cloud");
        AddParagraph(body, "AWS Practitioner | Amazon");

        return path;
    }

    private static void AddParagraph(Body body, string text)
    {
        body.AppendChild(new Paragraph(new Run(new Text(text))));
    }
}
