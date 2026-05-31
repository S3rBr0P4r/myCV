using Asp.Versioning;
using Backend.Application.UseCases.GetCV;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cv")]
public sealed class CvController : ControllerBase
{
    private readonly GetCVHandler _handler;

    public CvController(GetCVHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    [HttpGet]
    public async Task<IActionResult> GetCV(CancellationToken cancellationToken)
    {
        var culture = HttpContext.Request.Headers.AcceptLanguage.FirstOrDefault()?.Split(',')[0]?.Trim();
        var query = new GetCVQuery(culture);
        var result = await _handler.HandleAsync(query, cancellationToken);
        return Ok(result.CV);
    }
}
