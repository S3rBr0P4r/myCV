using System.Net;
using System.Text.Json;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Backend.Tests.Helpers;

namespace Backend.Tests.Application.Services;

public sealed class DeepLTranslationServiceTests
{
    [Fact]
    public async Task TranslateAsync_AuthKeyEmpty_ShouldReturnNull()
    {
        var client = new HttpClient();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new DeepLOptions { AuthKey = string.Empty });
        var discordOptions = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        var sut = new DeepLTranslationService(
            client, options, cache,
            new DiscordNotifier(new HttpClient(), discordOptions, Mock.Of<ILogger<DiscordNotifier>>()),
            Mock.Of<ILogger<DeepLTranslationService>>());

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_TargetLanguageEN_ShouldReturnNull()
    {
        var (_, client) = DeepLTestFixture.CreateHandlerPair(DeepLTestFixture.DefaultResponseJson);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "EN");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_TargetLanguageEmpty_ShouldReturnNull()
    {
        var (_, client) = DeepLTestFixture.CreateHandlerPair(DeepLTestFixture.DefaultResponseJson);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_CacheHit_ShouldReturnCachedResult()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var original = CVTestDataFactory.CreateSampleCV();
        cache.Set("translated_cv_ES", original, TimeSpan.FromMinutes(1440));

        var (handlerMock, client) = DeepLTestFixture.CreateHandlerPair(DeepLTestFixture.DefaultResponseJson);
        var options = Options.Create(new DeepLOptions
        {
            AuthKey = "test-key-12345",
            CacheDurationMinutes = 1440
        });
        var discordOptions = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        var sut = new DeepLTranslationService(
            client, options, cache,
            new DiscordNotifier(new HttpClient(), discordOptions, Mock.Of<ILogger<DiscordNotifier>>()),
            Mock.Of<ILogger<DeepLTranslationService>>());

        var result = await sut.TranslateAsync(original, "ES");

        result.Should().NotBeNull();
        result.Should().BeSameAs(original);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task TranslateAsync_ApiSuccess_ShouldReturnTranslatedCV()
    {
        var (_, client) = DeepLTestFixture.CreateHandlerPair(DeepLTestFixture.DefaultResponseJson);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().NotBeNull();
        result!.Name.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Title.Should().Be("Translated title");
        result.Summary.Should().Be("Translated summary");
        result.Experiences.Should().HaveCount(1);
        result.Experiences[0].Period.Should().Be("Translated period 1");
        result.Experiences[0].Role.Should().Be("Translated role 1");
        result.Experiences[0].Company.Should().Be("Acme");
        result.Experiences[0].Description.Should().Be("Translated desc 1");
        result.Experiences[0].Background.Should().Be(string.Empty);
        result.SkillCategories.Should().HaveCount(1);
        result.SkillCategories[0].Name.Should().Be("Translated languages");
        result.SkillCategories[0].SubCategories.Should().HaveCount(1);
        result.SkillCategories[0].SubCategories[0].Name.Should().Be("Translated dotnet");
        result.SkillCategories[0].SubCategories[0].Items.Should()
            .BeEquivalentTo(["C#", ".NET"]);
    }

    [Fact]
    public async Task TranslateAsync_ApiThrows_ShouldReturnNull()
    {
        var (_, client) = DeepLTestFixture.CreateHandlerThatThrows(new HttpRequestException("Network error"));
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_ApiReturnsErrorStatusCode_ShouldReturnNull()
    {
        var (_, client) = DeepLTestFixture.CreateHandlerThatReturnsStatus(HttpStatusCode.BadRequest);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_WrongTranslationCount_ShouldReturnNull()
    {
        var singleTranslation = """{"translations":[{"detected_source_language":"EN","text":"Only one"}]}""";
        var (_, client) = DeepLTestFixture.CreateHandlerPair(singleTranslation);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_LanguageCodeNormalized_ShouldUseTwoLetterCode()
    {
        HttpRequestMessage? capturedRequest = null;
        var spanishResponse = JsonSerializer.Serialize(new
        {
            translations = new[]
            {
                new { detected_source_language = "EN", text = "Resumen" },
                new { detected_source_language = "EN", text = "Título" },
                new { detected_source_language = "EN", text = "Periodo 1" },
                new { detected_source_language = "EN", text = "Rol 1" },
                new { detected_source_language = "EN", text = "Empresa 1" },
                new { detected_source_language = "EN", text = "Descripción 1" },
                new { detected_source_language = "EN", text = "Lenguajes" },
                new { detected_source_language = "EN", text = "PuntoNET" }
            }
        });

        var (_, client) = DeepLTestFixture.CreateHandlerPair(
            spanishResponse, req => capturedRequest = req);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "es-ES");

        result.Should().NotBeNull();
        result!.Summary.Should().Be("Resumen");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("api-free.deepl.com");
    }

    [Fact]
    public async Task TranslateAsync_ApiCalledWithCorrectAuthHeader_ShouldPassAuthKey()
    {
        HttpRequestMessage? capturedRequest = null;
        var (_, client) = DeepLTestFixture.CreateHandlerPair(
            DeepLTestFixture.DefaultResponseJson, req => capturedRequest = req);
        var sut = DeepLTestFixture.CreateSut(client);

        var result = await sut.TranslateAsync(CVTestDataFactory.CreateSampleCV(), "ES");

        result.Should().NotBeNull();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Parameter.Should().Be("test-key-12345");
    }
}
