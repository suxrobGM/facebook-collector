using FBC.Application;
using FBC.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FBC.Host;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddApplicationLayer(options =>
        {
            options.Email = "suxrobgm@gmail.com";
            options.Password = "Suxrobbek0729#";
        });
        var serviceProvider = services.BuildServiceProvider();
        var scraper = serviceProvider.GetRequiredService<IFacebookScraper>();
        scraper.OpenBrowser();
        await scraper.LoginAsync();

        Console.WriteLine("\nEND");
        Console.ReadKey();
    }
}
