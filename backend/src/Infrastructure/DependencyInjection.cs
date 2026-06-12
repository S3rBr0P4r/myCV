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
        services.Configure<CvSourceOptions>(configuration.GetSection(CvSourceOptions.SectionName));
        services.Configure<DiscordOptions>(configuration.GetSection(DiscordOptions.SectionName));
        services.Configure<DeepLOptions>(configuration.GetSection(DeepLOptions.SectionName));

        services.AddHttpClient<DiscordErrorNotifier>();
        services.AddHttpClient<DiscordFeedbackNotifier>();
        services.AddHttpClient<DeepLTranslationService>();

        services.AddMemoryCache();

        services.AddSingleton<ICvSource, WordCvSource>();
        services.AddScoped<ITranslationService, DeepLTranslationService>();

        services.AddScoped<ICVRepository, CVRepository>();
        return services;
    }
}
