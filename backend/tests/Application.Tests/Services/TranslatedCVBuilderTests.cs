using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Xunit;
using Backend.Tests.Helpers;

namespace Backend.Tests.Application.Services;

public sealed class TranslatedCVBuilderTests
{
    [Fact]
    public void Build_WithTranslatedTexts_ShouldReplaceSkillItemsInOrder()
    {
        var source = CreateSource(["One", "Two"]);
        var translatedTexts = new[]
        {
            "Summary2", "Title2", "2024-2", "Rol", "Empresa", "Desc2", "Cat2", "Sub2", "One2", "Two2"
        };

        var result = BuildResult(source, translatedTexts);

        result.Title.Should().Be("Title2");
        result.Summary.Should().Be("Summary2");
        result.Experiences[0].Period.Should().Be("2024-2");
        result.Experiences[0].Role.Should().Be("Rol");
        result.Experiences[0].Company.Should().Be("Acme");
        result.Experiences[0].Description.Should().Be("Desc2");
        result.SkillCategories[0].Name.Should().Be("Cat2");
        result.SkillCategories[0].SubCategories[0].Name.Should().Be("Sub2");
        result.SkillCategories[0].SubCategories[0].Items.Should().Equal("One2", "Two2");
    }

    [Fact]
    public void Build_EmptyItemInMiddle_ShouldKeepAlignmentWithSourceOrder()
    {
        var source = CreateSource(["", "Two"]);
        var translatedTexts = new[]
        {
            "Summary2", "Title2", "2024-2", "Rol", "Empresa", "Desc2", "Cat2", "Sub2", "Two2"
        };

        var result = BuildResult(source, translatedTexts);

        result.SkillCategories[0].SubCategories[0].Items.Should().Equal("", "Two2");
    }

    [Fact]
    public void Build_EmptyNamesInMiddle_ShouldKeepAlignmentWithSourceOrder()
    {
        var source = CreateMultiCategorySource();
        var translatedTexts = new[]
        {
            "Summary2", "Title2", "2024-2", "Rol", "Empresa", "Desc2",
            "A2", "B2", "Sa2", "Sb2", "a2", "b2", "c2"
        };

        var result = BuildResult(source, translatedTexts);

        result.SkillCategories.Select(c => c.Name).Should().Equal("A2", "", "B2");
        var subNames = result.SkillCategories.SelectMany(c => c.SubCategories).Select(s => s.Name);
        subNames.Should().Equal("Sa2", "", "Sb2");
        var items = result.SkillCategories
            .SelectMany(c => c.SubCategories)
            .SelectMany(s => s.Items);
        items.Should().Equal("a2", "b2", "c2");
    }

    [Fact]
    public void Build_TranslatedLabelFundamental_ShouldBecomeNucleo()
    {
        var source = CreateSource(["One"], "**Core**: C#");
        var translatedTexts = new[]
        {
            "Summary2", "Title2", "2024-2", "Rol", "Empresa", "Fundamental", "Cat2", "Sub2", "One2", "Two2"
        };

        var result = BuildResult(source, translatedTexts);

        result.Experiences[0].Description.Should().Be("**Núcleo**: C#");
    }

    [Fact]
    public void Build_FundamentalInsideNarrative_IsNotOverridden()
    {
        var source = CreateSource(["One"], "Established a fundamental baseline.");
        var translatedTexts = new[]
        {
            "Summary2", "Title2", "2024-2", "Rol", "Empresa",
            "Se estableció una base fundamental.", "Cat2", "Sub2", "One2", "Two2"
        };

        var result = BuildResult(source, translatedTexts);

        result.Experiences[0].Description.Should().Be("Se estableció una base fundamental.");
    }

    private static CV CreateSource(IReadOnlyList<string> items, string description = "Desc")
    {
        return new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Title",
            Summary = "Summary",
            Experiences =
            [
                new Experience
                {
                    Period = "2024",
                    Role = "Dev",
                    Company = "Acme",
                    Description = description
                }
            ],
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "Cat",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = "Sub", Items = items }
                    }.AsReadOnly()
                }
            ],
        };
    }

    private static CV CreateMultiCategorySource()
    {
        return new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Title",
            Summary = "Summary",
            Experiences =
            [
                new Experience
                {
                    Period = "2024",
                    Role = "Dev",
                    Company = "Acme",
                    Description = "Desc"
                }
            ],
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "A",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = "Sa", Items = new List<string> { "a" }.AsReadOnly() }
                    }.AsReadOnly()
                },
                new SkillCategory
                {
                    Name = "",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = "", Items = new List<string> { "b" }.AsReadOnly() }
                    }.AsReadOnly()
                },
                new SkillCategory
                {
                    Name = "B",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = "Sb", Items = new List<string> { "c" }.AsReadOnly() }
                    }.AsReadOnly()
                }
            ],
        };
    }

    private static CV BuildResult(CV source, string[] translatedTexts)
    {
        return TranslatedCVBuilder.Build(
            source, source.Summary, source.Title,
            source.Experiences.Select(e => e.Period).ToList(),
            source.Experiences.Select(e => e.Role).ToList(),
            source.Experiences.Select(e => e.Company).ToList(),
            source.Experiences.Select(e => e.Location).ToList(),
            source.Experiences.Select(e => e.WorkMode).ToList(),
            source.Experiences.Select(e => e.Description).ToList(),
            source.SkillCategories.Select(c => c.Name).ToList(),
            source.SkillCategories.SelectMany(c => c.SubCategories).Select(s => s.Name).ToList(),
            source.SkillCategories.SelectMany(c => c.SubCategories).SelectMany(s => s.Items).ToList(),
            translatedTexts);
    }
}
