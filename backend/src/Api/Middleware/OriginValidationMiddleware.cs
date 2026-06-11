using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Backend.Api.Middleware;

public sealed class OriginValidationMiddleware : IMiddleware
{
    private readonly string? _allowedOrigin;

    public OriginValidationMiddleware(IConfiguration configuration)
    {
        _allowedOrigin = configuration.GetValue<string>("Cors:FrontendUrl");
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/api/v1", StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsGet(context.Request.Method))
        {
            var origin = context.Request.Headers.Origin.FirstOrDefault()
                ?? context.Request.Headers.Referer.FirstOrDefault();

            if (string.IsNullOrEmpty(origin) || !MatchesAllowedOrigin(origin))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await next(context);
    }

    private bool MatchesAllowedOrigin(string origin)
    {
        if (string.IsNullOrEmpty(_allowedOrigin))
        {
            return true;
        }

        try
        {
            var allowedUri = new Uri(_allowedOrigin);
            var originUri = new Uri(origin);

            return string.Equals(allowedUri.Scheme, originUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(allowedUri.Host, originUri.Host, StringComparison.OrdinalIgnoreCase) &&
                   allowedUri.Port == originUri.Port;
        }
        catch
        {
            return false;
        }
    }
}
