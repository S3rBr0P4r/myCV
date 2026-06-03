using System.Net;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class SwaggerEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SwaggerEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_ShouldReturn200()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("openapi");
        body.Should().Contain("MyCV API");
    }

    [Fact]
    public async Task SwaggerUI_ShouldReturn200()
    {
        var response = await _client.GetAsync("/swagger");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("swagger");
    }
}
