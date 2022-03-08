namespace FBC.Application.Services;

public interface IFacebookScraper
{
    void OpenBrowser();
    Task LoginAsync();
    Task ScrapFriendsListAsync();
}
