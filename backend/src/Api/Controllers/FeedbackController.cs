using Asp.Versioning;
using Backend.Application.DTOs;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/feedback")]
public sealed class FeedbackController : ControllerBase
{
    private readonly DiscordFeedbackNotifier _notifier;

    public FeedbackController(DiscordFeedbackNotifier notifier)
    {
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Submit([FromBody] FeedbackRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new { error = "Rating must be between 1 and 5." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required." });
        }

        _ = _notifier.SendAsync(request.Country, request.Rating, request.Name.Trim(), request.Comment.Trim());

        return Ok(new { status = "Feedback sent" });
    }
}
