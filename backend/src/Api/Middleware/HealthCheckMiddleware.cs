namespace Backend.Api.Middleware;

public sealed class HealthCheckMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == "/health")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Healthy");
            return;
        }

        await next(context);
    }
}
