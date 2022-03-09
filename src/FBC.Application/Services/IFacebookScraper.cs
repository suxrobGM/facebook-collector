namespace FBC.Application.Services;

public interface IFacebookScraper
{
    void OpenBrowser();
    void Login();
    void ScrapFriendsList(string username);
}
