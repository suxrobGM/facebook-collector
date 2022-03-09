using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FBC.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration, 
        string emailSection = "FB:Email",
        string passwordSection = "FB:Password")
    {
        var options = new FacebookScraperOptions
        {
            Email = configuration[emailSection],
            Password = configuration[passwordSection]
        };
        return AddApplicationLayer(services, options);
    }

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
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<IFacebookScraper, FacebookScraper>();
        return services;
    }
}