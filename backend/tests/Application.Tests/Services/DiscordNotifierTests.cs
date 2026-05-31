using System.Net;
using System.Reflection;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Infrastructure.Services;

public sealed class DiscordNotifierTests
{
    public DiscordNotifierTests()
    {
        var field = typeof(DiscordNotifier).GetField("_alertSent", BindingFlags.Static | BindingFlags.NonPublic);
        field!.SetValue(null, false);
    }
    [Fact]
    public async Task SendAlertAsync_EmptyWebhookUrl_ShouldNotCallHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("title", "message");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAlertAsync_CalledTwice_ShouldSendOnlyOnce()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("First", "First message");
        await notifier.SendAlertAsync("Second", "Second message");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAlertAsync_WithValidUrl_ShouldSendEmbed()
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
        var webhookUrl = "https://discord.com/api/webhooks/test";
        var options = Options.Create(new DiscordOptions { WebhookUrl = webhookUrl });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("Test Title", "Test description");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().Be(webhookUrl);
        capturedRequest.Method.Should().Be(HttpMethod.Post);
        capturedRequest.Content.Should().NotBeNull();
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("Test Title");
        body.Should().Contain("Test description");
    }
}
