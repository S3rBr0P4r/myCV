using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Tests.Infrastructure.Sources;

internal static class TestDocumentBuilder
{
    internal static string CreateValidDocument(string tempDir)
    {
        var path = Path.Combine(tempDir, "valid.docx");

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

        return path;
    }

    internal static string CreateLegacyFormatDocument(string tempDir)
    {
        var path = Path.Combine(tempDir, "legacy.docx");

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

        return path;
    }

    internal static string CreateDocumentWithMultiLineSummary(string tempDir)
    {
        var path = Path.Combine(tempDir, "multiline_summary.docx");

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

        return path;
    }

    internal static string CreateDocumentWithNoExperiences(string tempDir)
    {
        var path = Path.Combine(tempDir, "no_experiences.docx");

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

        return path;
    }

    internal static string CreateDocumentWithHierarchicalSkills(string tempDir)
    {
        var path = Path.Combine(tempDir, "hierarchical_skills.docx");

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

        return path;
    }

    internal static void AddParagraph(Body body, string text)
    {
        body.AppendChild(new Paragraph(new Run(new Text(text))));
    }
}
