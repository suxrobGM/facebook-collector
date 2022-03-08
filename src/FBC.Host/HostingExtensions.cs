using FBC.Application;
using FBC.EntityFramework;

namespace FBC.Host;

internal static class HostingExtensions
{
    public static IHost ConfigureServices(this IHostBuilder builder)
    {
        builder.ConfigureServices((ctx, services) =>
        {
            
            services.AddApplicationLayer(options =>
            {
                options.Email = "suxrobgm@gmail.com";
                //options.Password = "Suxrobbek0729#";
            });

            services.AddEntityFrameworkLayer(ctx.Configuration);
            services.AddHostedService<Worker>();
        });
        return builder.Build();
    }
}
