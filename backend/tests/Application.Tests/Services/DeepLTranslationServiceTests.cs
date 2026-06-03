using System.Net;
using System.Text.Json;
using Backend.Domain.Entities;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Application.Services;

public sealed class DeepLTranslationServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IOptions<DeepLOptions> _options;
    private readonly ILogger<DeepLTranslationService> _logger;
    private readonly DeepLTranslationService _sut;

    public DeepLTranslationServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    translations = new[]
                    {
                        new { detected_source_language = "EN", text = "Translated summary" },
                        new { detected_source_language = "EN", text = "Translated title" },
                        new { detected_source_language = "EN", text = "Translated period 1" },
                        new { detected_source_language = "EN", text = "Translated role 1" },
                        new { detected_source_language = "EN", text = "Translated desc 1" },
                        new { detected_source_language = "EN", text = "Translated skill 1" },
                        new { detected_source_language = "EN", text = "Translated skill 2" }
                    }
                }))
            });

        _httpClient = new HttpClient(_handlerMock.Object);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = Options.Create(new DeepLOptions
        {
            AuthKey = "test-key-12345",
            CacheDurationMinutes = 1440
        });
        _logger = Mock.Of<ILogger<DeepLTranslationService>>();
        _sut = new DeepLTranslationService(_httpClient, _options, _cache, _logger);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        (_cache as IDisposable)?.Dispose();
    }

    private static CV CreateSampleCV()
    {
        return new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Developer",
            Summary = "A skilled developer",
            Experiences =
            [
                new Experience
                {
                    Period = "2024 - Present",
                    Role = "Senior Dev",
                    Company = "Acme",
                    Description = "Building things"
                }
            ],
            SkillCategories =
            [
                new SkillCategory
                {
                    Name = "Languages",
                    SubCategories = new List<SkillSubCategory>
                    {
                        new SkillSubCategory { Name = ".NET", Items = new List<string> { "C#", ".NET" }.AsReadOnly() }
                    }.AsReadOnly()
                }
            ],

        };
    }

    [Fact]
    public async Task TranslateAsync_AuthKeyEmpty_ShouldReturnNull()
    {
        var options = Options.Create(new DeepLOptions
        {
            AuthKey = string.Empty,
            CacheDurationMinutes = 1440
        });
        var sut = new DeepLTranslationService(_httpClient, options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_TargetLanguageEN_ShouldReturnNull()
    {
        var result = await _sut.TranslateAsync(CreateSampleCV(), "EN");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_TargetLanguageEmpty_ShouldReturnNull()
    {
        var result = await _sut.TranslateAsync(CreateSampleCV(), string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_CacheHit_ShouldReturnCachedResult()
    {
        var original = CreateSampleCV();
        _cache.Set("translated_cv_ES", original, TimeSpan.FromMinutes(1440));
        var sut = new DeepLTranslationService(_httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(original, "ES");

        result.Should().NotBeNull();
        result.Should().BeSameAs(original);
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task TranslateAsync_ApiSuccess_ShouldReturnTranslatedCV()
    {
        var result = await _sut.TranslateAsync(CreateSampleCV(), "ES");

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
        result.SkillCategories[0].SubCategories.Should().HaveCount(1);
        result.SkillCategories[0].SubCategories[0].Items.Should()
            .BeEquivalentTo(["Translated skill 1", "Translated skill 2"]);
    }

    [Fact]
    public async Task TranslateAsync_ApiThrows_ShouldReturnNull()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new DeepLTranslationService(httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_ApiReturnsErrorStatusCode_ShouldReturnNull()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new DeepLTranslationService(httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_WrongTranslationCount_ShouldReturnNull()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    translations = new[]
                    {
                        new { detected_source_language = "EN", text = "Only one" }
                    }
                }))
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new DeepLTranslationService(httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "ES");

        result.Should().BeNull();
    }

    [Fact]
    public async Task TranslateAsync_LanguageCodeNormalized_ShouldUseTwoLetterCode()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
            {
                capturedRequest = request;
                var body = request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
                body.Should().Contain("target_lang\":\"ES");

                var responseContent = JsonSerializer.Serialize(new
                {
                    translations = new[]
                    {
                        new { detected_source_language = "EN", text = "Resumen" },
                        new { detected_source_language = "EN", text = "Título" },
                        new { detected_source_language = "EN", text = "Periodo 1" },
                        new { detected_source_language = "EN", text = "Rol 1" },
                        new { detected_source_language = "EN", text = "Descripción 1" },
                        new { detected_source_language = "EN", text = "Habilidad 1" },
                        new { detected_source_language = "EN", text = "Habilidad 2" }
                    }
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseContent)
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new DeepLTranslationService(httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "es-ES");

        result.Should().NotBeNull();
        result!.Summary.Should().Be("Resumen");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("api-free.deepl.com");
    }

    [Fact]
    public async Task TranslateAsync_ApiCalledWithCorrectAuthHeader_ShouldPassAuthKey()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
            {
                capturedRequest = request;

                var responseContent = JsonSerializer.Serialize(new
                {
                    translations = new[]
                    {
                        new { detected_source_language = "EN", text = "Resumen" },
                        new { detected_source_language = "EN", text = "Título" },
                        new { detected_source_language = "EN", text = "Periodo 1" },
                        new { detected_source_language = "EN", text = "Rol 1" },
                        new { detected_source_language = "EN", text = "Descripción 1" },
                        new { detected_source_language = "EN", text = "Habilidad 1" },
                        new { detected_source_language = "EN", text = "Habilidad 2" }
                    }
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseContent)
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new DeepLTranslationService(httpClient, _options, _cache, _logger);

        var result = await sut.TranslateAsync(CreateSampleCV(), "ES");

        result.Should().NotBeNull();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Parameter.Should().Be("test-key-12345");
    }
}
