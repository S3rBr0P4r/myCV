using System.Net;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class CvErrorTests
{
    [Fact]
    public async Task GetCv_OnMissingDocx_ShouldReturn500()
    {
        await using var factory = new CustomWebApplicationFactory();
        factory.SkipDocxCreation();
        await factory.InitializeAsync();

        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
