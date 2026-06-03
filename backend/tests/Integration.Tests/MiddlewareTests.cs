using System.Net;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class MiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MiddlewareTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        var response = await _client.GetAsync("/api/v1/cv");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task PermissionsPolicyHeader_ShouldRestrictFeatures()
    {
        var response = await _client.GetAsync("/api/v1/cv");

        response.Headers.Should().ContainKey("Permissions-Policy");
        var policy = response.Headers.GetValues("Permissions-Policy").First();
        policy.Should().Contain("camera=()");
        policy.Should().Contain("microphone=()");
        policy.Should().Contain("geolocation=()");
        policy.Should().Contain("interest-cohort=()");
    }

    [Fact]
    public async Task CorsPreflight_ShouldAllowConfiguredOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:5173");
    }

    [Fact]
    public async Task CorsPreflight_ShouldRejectDisallowedOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/cv");
        request.Headers.Add("Origin", "https://evil.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        response.Headers.Should().NotContainKey("Access-Control-Allow-Origin");
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }

    [Fact]
    public async Task ResponseCompression_ShouldBeEnabled()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.AcceptEncoding.ParseAdd("gzip");

        var response = await _client.SendAsync(request);

        response.Content.Headers.ContentEncoding.Should().Contain("gzip");
    }
}
