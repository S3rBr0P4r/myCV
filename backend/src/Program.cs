using System.Globalization;
using Backend.Api;
using Backend.Api.Middleware;
using Backend.Application;
using Backend.Infrastructure;
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
        .AddInfrastructure()
        .AddApiServices(builder.Configuration);

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
        app.UseHttpsRedirection();
    }
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
