using System.Globalization;
using System.Threading.RateLimiting;
using Backend.Api;
using Backend.Api.Middleware;
using Backend.Application;
using Backend.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MyCV API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices(builder.Configuration)
        .AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Api", opts =>
            {
                opts.PermitLimit = 100;
                opts.Window = TimeSpan.FromMinutes(1);
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 0;
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        })
        .AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyCV API v1");
        });
    }

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/health")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Healthy");
            return;
        }
        await next();
    });

    app.UseResponseCompression();

    app.Use(async (context, next) =>
    {
        Log.Information("{Method} {Path}{QueryString} — start",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);
        await next();
    });

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "{RequestMethod} {RequestPath}{QueryString} — ended {StatusText} in {Elapsed:0.0}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? "");
            var statusText = Enum.IsDefined(typeof(System.Net.HttpStatusCode), httpContext.Response.StatusCode)
                ? ((System.Net.HttpStatusCode)httpContext.Response.StatusCode).ToString()
                : httpContext.Response.StatusCode.ToString(CultureInfo.InvariantCulture);
            diagnosticContext.Set("StatusText", statusText);
        };
        options.GetLevel = (httpContext, elapsed, ex) =>
            ex != null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
    });
    app.UseMiddleware<GlobalExceptionHandler>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        await next();
    });

    app.UseRateLimiter();
    app.UseCors("AllowFrontend");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
