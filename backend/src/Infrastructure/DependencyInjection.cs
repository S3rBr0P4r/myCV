using Backend.Domain.Interfaces;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Sources;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICvSource, EnglishCvSource>();
        services.AddScoped<ICVRepository, CVRepository>();
        return services;
    }
}
