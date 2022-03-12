namespace FBC.Application.Services;

public interface IFacebookScraper
{
    string FacebookId { get; }
    void OpenBrowser();
    Task LoginAsync();
    Task ScrapFriendsListAsync(string facebookId);
}
