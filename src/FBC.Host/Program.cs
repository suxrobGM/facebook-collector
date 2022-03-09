using Serilog;
using FBC.Host;


class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddCommandLine(args);
            });
        
            builder.UseSerilog((ctx, lc) => lc
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            var app = builder
                .ConfigureServices();

            await app.RunAsync();
        }
        catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException")
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}