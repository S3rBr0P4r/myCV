using Backend.Domain.Interfaces;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Sources;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cvSourceOptions = configuration.GetSection(CvSourceOptions.SectionName).Get<CvSourceOptions>() ?? new();

        services.Configure<CvSourceOptions>(configuration.GetSection(CvSourceOptions.SectionName));
        services.Configure<DiscordOptions>(configuration.GetSection(DiscordOptions.SectionName));

        services.AddHttpClient<DiscordNotifier>();

        services.AddScoped<ICvSource, WordCvSource>();

        services.AddScoped<ICVRepository, CVRepository>();
        return services;
    }
}
