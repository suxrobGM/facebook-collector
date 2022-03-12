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
        await _facebookScraper.LoginAsync();
        await _facebookScraper.ScrapFriendsListAsync(_facebookScraper.FacebookId);
        await Task.CompletedTask;
    }
}