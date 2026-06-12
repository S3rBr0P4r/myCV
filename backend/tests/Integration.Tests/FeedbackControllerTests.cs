using System.Net;
using System.Net.Http.Json;
using Backend.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class FeedbackControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FeedbackControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Submit_ValidRequest_ShouldReturn200()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/feedback")
        {
            Content = JsonContent.Create(new FeedbackRequest("ES", 5, "John")),
            Headers = { { "Origin", "http://localhost:5173" } }
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Submit_RatingBelow1_ShouldReturn400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/feedback")
        {
            Content = JsonContent.Create(new FeedbackRequest("ES", 0, "John")),
            Headers = { { "Origin", "http://localhost:5173" } }
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Submit_RatingAbove5_ShouldReturn400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/feedback")
        {
            Content = JsonContent.Create(new FeedbackRequest("ES", 6, "John")),
            Headers = { { "Origin", "http://localhost:5173" } }
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Submit_EmptyName_ShouldReturn400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/feedback")
        {
            Content = JsonContent.Create(new FeedbackRequest("ES", 3, "  ")),
            Headers = { { "Origin", "http://localhost:5173" } }
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
