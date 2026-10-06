using Backend.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Application.Services;

public sealed class DescriptionSegmenterTests
{
    [Fact]
    public void CollectTranslatable_MultiLineWithTechStackMarkers_ReturnsOnlyTranslatableSegments()
    {
        var description = "Line one.\n**Tech Stack:**\n**Core**: C#, ASP.NET\n**Tooling**: Docker";

        var texts = DescriptionSegmenter.CollectTranslatable(description);

        texts.Should().Equal(
            Seg("Line one.", false),
            Seg("Tech Stack:", true),
            Seg("Core", true),
            Seg("Tooling", true));
    }

    [Fact]
    public void CollectTranslatable_InlineTechStack_SkipsListAfterMarker()
    {
        var description = "Built the platform.\nTech Stack: C#, WinForms";

        var texts = DescriptionSegmenter.CollectTranslatable(description);

        texts.Should().Equal(Seg("Built the platform.", false), Seg("Tech Stack", true));
    }

    [Fact]
    public void CollectTranslatable_NarrativeLineStartingWithCoreWord_IsFullyTranslatable()
    {
        var description = "Core business logic was refactored: earlier.";

        var texts = DescriptionSegmenter.CollectTranslatable(description);

        texts.Should().Equal(Seg(description, false));
    }

    [Fact]
    public void CollectTranslatable_EmptyLines_AreSkipped()
    {
        var description = "First.\n\nSecond.";

        var texts = DescriptionSegmenter.CollectTranslatable(description);

        texts.Should().Equal(Seg("First.", false), Seg("Second.", false));
    }

    [Fact]
    public void Rebuild_AppliesTranslationsAndKeepsProtectedPayloadsVerbatim()
    {
        var description = "Line one.\n**Tech Stack:**\n**Core**: C#, ASP.NET\n**Tooling**: Docker";

        var result = DescriptionSegmenter.Rebuild(description, text => $"[{text}]");

        result.Should().Be("[Line one.]\n**[Tech Stack:]**\n**[Core]**: C#, ASP.NET\n**[Tooling]**: Docker");
    }

    [Fact]
    public void Rebuild_InlineTechStack_KeepsListVerbatimAfterTranslatedLabel()
    {
        var description = "Built the platform.\nTech Stack: C#, WinForms";

        var result = DescriptionSegmenter.Rebuild(description, text => $"[{text}]");

        result.Should().Be("[Built the platform.]\n[Tech Stack]: C#, WinForms");
    }

    [Fact]
    public void Rebuild_EmptyDescription_ReturnsEmpty()
    {
        var result = DescriptionSegmenter.Rebuild("", text => $"[{text}]");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Rebuild_EmptySegments_DoNotInvokeTranslation()
    {
        var result = DescriptionSegmenter.Rebuild("First.\n\nSecond.", text => $"[{text}]");

        result.Should().Be("[First.]\n\n[Second.]");
    }

    private static DescriptionSegmenter.TranslatableSegment Seg(string text, bool isLabel)
    {
        return new DescriptionSegmenter.TranslatableSegment(text, isLabel);
    }
}
