using System.Net;
using Backend.Domain.Exceptions;

namespace Backend.Api.Middleware;

public sealed class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {EntityName} with key {Key}", ex.EntityName, ex.Key);
            await WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, "Resource Not Found", ex.Message);
        }
        catch (CvSourceException ex)
        {
            _logger.LogError(ex, "CV source error: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "CV Source Error", ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Domain Error", ex.Message);
        }
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error",
                "An unexpected error occurred. Please try again later.");
        }
#pragma warning restore CA1031
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.Value
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
