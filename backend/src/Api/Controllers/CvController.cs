using Asp.Versioning;
using Backend.Application.DTOs;
using Backend.Application.UseCases.GetCV;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cv")]
public sealed class CvController : ControllerBase
{
    private readonly GetCVHandler _handler;
    private readonly IConfiguration _configuration;

    public CvController(GetCVHandler handler, IConfiguration configuration)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Language")]
    [ProducesResponseType<CVDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCV(CancellationToken cancellationToken)
    {
        var culture = HttpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        var query = new GetCVQuery(culture);
        var result = await _handler.HandleAsync(query, cancellationToken);

        var dto = result.CV with
        {
            LinkedInUrl = _configuration["SocialLinks:LinkedIn"] ?? "",
            GitHubUrl = _configuration["SocialLinks:GitHub"] ?? ""
        };

        return Ok(dto);
    }
}
