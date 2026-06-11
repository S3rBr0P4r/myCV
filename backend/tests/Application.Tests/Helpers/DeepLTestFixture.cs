using System.Net;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Backend.Tests.Helpers;

internal static class DeepLTestFixture
{
    internal const string DefaultResponseJson = """{"translations":[{"detected_source_language":"EN","text":"Translated summary"},{"detected_source_language":"EN","text":"Translated title"},{"detected_source_language":"EN","text":"Translated period 1"},{"detected_source_language":"EN","text":"Translated role 1"},{"detected_source_language":"EN","text":"Translated desc 1"},{"detected_source_language":"EN","text":"Translated skill 1"},{"detected_source_language":"EN","text":"Translated skill 2"}]}""";

    internal static (Mock<HttpMessageHandler> Handler, HttpClient Client) CreateHandlerPair(
        string responseJson, Action<HttpRequestMessage>? capture = null)
    {
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capture?.Invoke(request);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson)
                };
            });
        return (mock, new HttpClient(mock.Object));
    }

    internal static (Mock<HttpMessageHandler> Handler, HttpClient Client) CreateHandlerThatThrows(Exception exception)
    {
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
        return (mock, new HttpClient(mock.Object));
    }

    internal static (Mock<HttpMessageHandler> Handler, HttpClient Client) CreateHandlerThatReturnsStatus(
        HttpStatusCode statusCode)
    {
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));
        return (mock, new HttpClient(mock.Object));
    }

    internal static DeepLTranslationService CreateSut(HttpClient client)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new DeepLOptions
        {
            AuthKey = "test-key-12345",
            CacheDurationMinutes = 1440
        });
        var discordOptions = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        return new DeepLTranslationService(
            client, options, cache,
            new DiscordNotifier(new HttpClient(), discordOptions, Mock.Of<ILogger<DiscordNotifier>>()),
            Mock.Of<ILogger<DeepLTranslationService>>());
    }
}
