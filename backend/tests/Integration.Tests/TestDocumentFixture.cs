using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Tests.Integration;

internal static class TestDocumentFixture
{
    public static Task CreateValidDocumentAsync(string filePath)
    {
        using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        AddParagraph(body, "John Doe");
        AddParagraph(body, "Software Engineer");
        AddParagraph(body, "john@acme.com");
        AddParagraph(body, "+34 123 456 789");
        AddParagraph(body, "SUMMARY");
        AddParagraph(body, "Experienced developer.");
        AddParagraph(body, "EXPERIENCE");
        AddParagraph(body, "Acme Corp | https://acme.com");
        AddParagraph(body, "San Francisco, CA | Remote");
        AddParagraph(body, "Senior Dev | Jan 2020 - Present");
        AddParagraph(body, "- Built APIs");
        AddParagraph(body, "- Led teams");
        AddParagraph(body, "TECHNICAL SKILLS");
        AddParagraph(body, "Languages");
        AddParagraph(body, "C#, TypeScript");

        mainPart.Document.Save();
        return Task.CompletedTask;
    }

    private static void AddParagraph(Body body, string text)
    {
        var para = body.AppendChild(new Paragraph());
        para.AppendChild(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }
}
