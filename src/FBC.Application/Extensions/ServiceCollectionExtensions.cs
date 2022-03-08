using Microsoft.Extensions.DependencyInjection;
using FBC.Application.Options;
using FBC.Application.Services;
using FBC.Application.Services.Implementations;

namespace FBC.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        Action<FacebookScraperOptions> configureOptions)
    {
        var options = new FacebookScraperOptions();
        configureOptions(options);
        return AddApplicationLayer(services, options);
    }

    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        FacebookScraperOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IDataService, DataService>();
        services.AddScoped<IFacebookScraper, FacebookScraper>();
        return services;
    }
}