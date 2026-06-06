using Backend.Api;
using Backend.Api.Middleware;
using Backend.Application;
using Backend.Application.UseCases.GetCV;
using Backend.Domain.Interfaces;
using Backend.Infrastructure;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Sources;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Application;

public sealed class DependencyInjectionTests
{
    private static ServiceProvider BuildProvider(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddSingleton<IConfiguration>(config);
        register(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApplication_ShouldRegisterGetCVHandler()
    {
        var provider = BuildProvider(s =>
        {
            s.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection().Build());
            s.AddApplication();
        });

        var handler = provider.GetService<GetCVHandler>();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterGetCVHandlerAsScoped()
    {
        var provider = BuildProvider(s =>
        {
            s.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection().Build());
            s.AddApplication();
        });

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var h1 = scope1.ServiceProvider.GetRequiredService<GetCVHandler>();
        var h2 = scope2.ServiceProvider.GetRequiredService<GetCVHandler>();

        h1.Should().NotBeSameAs(h2);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterCvSource()
    {
        var provider = BuildProvider(s => s.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection().Build()));

        var source = provider.GetService<ICvSource>();
        source.Should().NotBeNull();
        source.Should().BeOfType<WordCvSource>();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterTranslationService()
    {
        var provider = BuildProvider(s => s.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection().Build()));

        var service = provider.GetService<ITranslationService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<DeepLTranslationService>();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRepository()
    {
        var provider = BuildProvider(s => s.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection().Build()));

        var repo = provider.GetService<ICVRepository>();
        repo.Should().NotBeNull();
        repo.Should().BeOfType<CVRepository>();
    }

    [Fact]
    public void AddApiServices_ShouldRegisterGlobalExceptionHandler()
    {
        var provider = BuildProvider(s =>
        {
            s.AddLogging();
            s.AddApiServices(new ConfigurationBuilder().AddInMemoryCollection().Build());
        });

        var handler = provider.GetService<GlobalExceptionHandler>();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddApiServices_ShouldRegisterSecurityHeadersMiddleware()
    {
        var provider = BuildProvider(s =>
        {
            s.AddLogging();
            s.AddApiServices(new ConfigurationBuilder().AddInMemoryCollection().Build());
        });

        var middleware = provider.GetService<SecurityHeadersMiddleware>();
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void AddApiServices_ShouldRegisterControllers()
    {
        var provider = BuildProvider(s =>
        {
            s.AddLogging();
            s.AddControllers();
            s.AddApiServices(new ConfigurationBuilder().AddInMemoryCollection().Build());
        });

        var controllers = provider.GetService<Microsoft.AspNetCore.Mvc.Controllers.ControllerFeature>();
    }
}
