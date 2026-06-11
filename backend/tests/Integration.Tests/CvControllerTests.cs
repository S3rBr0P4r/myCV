using System.Net;
using System.Net.Http.Json;
using Backend.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class CvControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CvControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCv_ShouldReturn200_WithCVDto()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cv = await response.Content.ReadFromJsonAsync<CVDto>();
        cv.Should().NotBeNull();
        cv!.Name.Should().Be("John Doe");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Software Engineer");
        cv.Summary.Should().Be("Experienced developer.");
    }

    [Fact]
    public async Task GetCv_ShouldReturnExperiences()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        var response = await _client.SendAsync(request);
        var cv = await response.Content.ReadFromJsonAsync<CVDto>();

        cv!.Experiences.Should().HaveCount(1);
        var exp = cv.Experiences[0];
        exp.Company.Should().Be("Acme Corp");
        exp.CompanyUrl.Should().Be("https://acme.com");
        exp.Location.Should().Be("San Francisco, CA");
        exp.WorkMode.Should().Be("Remote");
        exp.Role.Should().Be("Senior Dev");
        exp.Period.Should().Be("Jan 2020 - Present");
        exp.Description.Should().Contain("Built APIs");
    }

    [Fact]
    public async Task GetCv_ShouldReturnSkillCategories()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        var response = await _client.SendAsync(request);
        var cv = await response.Content.ReadFromJsonAsync<CVDto>();

        cv!.SkillCategories.Should().HaveCount(1);
        cv.SkillCategories[0].Name.Should().Be("Languages");
        cv.SkillCategories[0].SubCategories.Should().HaveCount(1);
        cv.SkillCategories[0].SubCategories[0].Items.Should().Contain("C#");
    }

    [Fact]
    public async Task GetCv_WithAcceptLanguageSpanish_ShouldReturnUntranslatedWhenNoAuthKey()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.AcceptLanguage.ParseAdd("es");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cv = await response.Content.ReadFromJsonAsync<CVDto>();
        cv.Should().NotBeNull();
        cv!.Name.Should().Be("John Doe");
        cv.Summary.Should().Be("Experienced developer.");
    }

    [Fact]
    public async Task GetCv_WithAcceptLanguageEnglish_ShouldReturnUntranslated()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cv");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.AcceptLanguage.ParseAdd("en");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cv = await response.Content.ReadFromJsonAsync<CVDto>();
        cv.Should().NotBeNull();
        cv!.Summary.Should().Be("Experienced developer.");
    }
}
