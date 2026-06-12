using Backend.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Infrastructure.Services;

public sealed class LanguageHelperTests
{
    [Fact]
    public void NormalizeLanguage_Null_ShouldReturnNull()
    {
        LanguageHelper.NormalizeLanguage(null).Should().BeNull();
    }

    [Fact]
    public void NormalizeLanguage_EmptyString_ShouldReturnNull()
    {
        LanguageHelper.NormalizeLanguage(string.Empty).Should().BeNull();
    }

    [Fact]
    public void NormalizeLanguage_Whitespace_ShouldReturnNull()
    {
        LanguageHelper.NormalizeLanguage("   ").Should().BeNull();
    }

    [Fact]
    public void NormalizeLanguage_TwoLetterCode_ShouldReturnUppercased()
    {
        LanguageHelper.NormalizeLanguage("es").Should().Be("ES");
    }

    [Fact]
    public void NormalizeLanguage_FullLocale_ShouldReturnTwoLetterCode()
    {
        LanguageHelper.NormalizeLanguage("es-ES").Should().Be("ES");
    }

    [Fact]
    public void NormalizeLanguage_AcceptLanguageHeader_ShouldReturnFirstLanguage()
    {
        LanguageHelper.NormalizeLanguage("es-ES,en;q=0.9").Should().Be("ES");
    }

    [Fact]
    public void NormalizeLanguage_MultipleLocalesWithSemicolon_ShouldReturnFirst()
    {
        LanguageHelper.NormalizeLanguage("fr-FR;q=0.8, es-ES;q=0.5").Should().Be("FR");
    }

    [Fact]
    public void NormalizeLanguage_MultipleCulturesInHeader_ShouldUseFirst()
    {
        LanguageHelper.NormalizeLanguage("fr-FR,en;q=0.9").Should().Be("FR");
    }

    [Fact]
    public void NormalizeLanguage_AlreadyUppercased_ShouldReturnAsIs()
    {
        LanguageHelper.NormalizeLanguage("DE").Should().Be("DE");
    }
}
