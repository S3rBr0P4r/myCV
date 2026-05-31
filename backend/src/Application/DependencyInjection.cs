using Backend.Application.UseCases.GetCV;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetCVHandler>();
        return services;
    }
}
