using System.Net;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Infrastructure.Services;

public sealed class DiscordFeedbackNotifierTests
{
    [Fact]
    public async Task SendAsync_EmptyWebhookUrl_ShouldNotCallHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { FeedbackWebhookUrl = string.Empty });
        var loggerMock = new Mock<ILogger<DiscordFeedbackNotifier>>();
        var notifier = new DiscordFeedbackNotifier(httpClient, options, loggerMock.Object);

        await notifier.SendAsync("ES", 5, "Test User");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_InvalidUrl_ShouldNotCallHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { FeedbackWebhookUrl = "not-a-url" });
        var loggerMock = new Mock<ILogger<DiscordFeedbackNotifier>>();
        var notifier = new DiscordFeedbackNotifier(httpClient, options, loggerMock.Object);

        await notifier.SendAsync("ES", 5, "Test User");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WithValidUrl_ShouldSendEmbed()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var webhookUrl = "https://discord.com/api/webhooks/feedback-test";
        var options = Options.Create(new DiscordOptions { FeedbackWebhookUrl = webhookUrl });
        var loggerMock = new Mock<ILogger<DiscordFeedbackNotifier>>();
        var notifier = new DiscordFeedbackNotifier(httpClient, options, loggerMock.Object);

        await notifier.SendAsync("ES", 4, "John Doe");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().Be(webhookUrl);
        capturedRequest.Method.Should().Be(HttpMethod.Post);
        capturedRequest.Content.Should().NotBeNull();
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("John Doe");
        body.Should().Contain("ES");
        body.Should().Contain("4/5");
    }

    [Fact]
    public async Task SendAsync_ShouldNotHaveCooldown()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { FeedbackWebhookUrl = "https://discord.com/api/webhooks/feedback-test" });
        var loggerMock = new Mock<ILogger<DiscordFeedbackNotifier>>();
        var notifier = new DiscordFeedbackNotifier(httpClient, options, loggerMock.Object);

        await notifier.SendAsync("ES", 5, "First");
        await notifier.SendAsync("US", 3, "Second");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WithComment_ShouldIncludeInEmbed()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { FeedbackWebhookUrl = "https://discord.com/api/webhooks/feedback-test" });
        var loggerMock = new Mock<ILogger<DiscordFeedbackNotifier>>();
        var notifier = new DiscordFeedbackNotifier(httpClient, options, loggerMock.Object);

        await notifier.SendAsync("ES", 5, "Jane", "Great site!");

        capturedRequest.Should().NotBeNull();
        var body = await capturedRequest!.Content!.ReadAsStringAsync();
        body.Should().Contain("Great site!");
    }
}
