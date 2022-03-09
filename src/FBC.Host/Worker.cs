using FBC.Application.Services;

namespace FBC.Host;

public class Worker : BackgroundService
{
    private readonly IFacebookScraper _facebookScraper;

    public Worker(IFacebookScraper facebookScraper)
    {
        _facebookScraper = facebookScraper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _facebookScraper.OpenBrowser();
        _facebookScraper.Login();
        await Task.CompletedTask;
    }
}