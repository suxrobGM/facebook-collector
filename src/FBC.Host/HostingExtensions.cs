using FBC.Application;
using FBC.EntityFramework;

namespace FBC.Host;

internal static class HostingExtensions
{
    public static IHost ConfigureServices(this IHostBuilder builder)
    {
        builder.ConfigureServices((ctx, services) =>
        {
            services.AddApplicationLayer(ctx.Configuration);
            services.AddEntityFrameworkLayer(ctx.Configuration);
            services.AddHostedService<Worker>();
        });
        return builder.Build();
    }
}
