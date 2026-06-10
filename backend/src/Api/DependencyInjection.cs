using Asp.Versioning;
using Backend.Api.Middleware;
using Microsoft.OpenApi.Models;

namespace Backend.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddScoped<GlobalExceptionHandler>();
        services.AddScoped<SecurityHeadersMiddleware>();
        services.AddScoped<HealthCheckMiddleware>();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MyCV API",
                Version = "v1",
                Description = "API for serving CV/resume data"
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                var frontendUrl = configuration.GetValue<string>("Cors:FrontendUrl");
                if (!string.IsNullOrEmpty(frontendUrl))
                {
                    allowedOrigins = [.. allowedOrigins, frontendUrl];
                }
                policy.WithOrigins(allowedOrigins)
                      .AllowCredentials()
                      .WithHeaders("Accept", "Accept-Language", "Content-Type", "Authorization")
                      .WithMethods("GET", "OPTIONS");
            });
        });

        return services;
    }
}
