namespace FBC.Application.Services;

public interface IFacebookScraper
{
    string FacebookId { get; }
    void OpenBrowser();
    void Login();
    void ScrapFriendsList(string facebookId);
}
