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
        var response = await client.GetAsync("/api/v1/cv");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
