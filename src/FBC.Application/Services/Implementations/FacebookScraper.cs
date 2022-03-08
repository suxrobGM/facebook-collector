using FBC.Application.Options;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FBC.Application.Services.Implementations;

internal sealed class FacebookScraper : IFacebookScraper
{
    private IWebDriver _driver;
    private IJavaScriptExecutor _jsExecutor;
    private readonly ILogger<FacebookScraper> _logger;
    private readonly FacebookScraperOptions _options;

    public FacebookScraper(
        FacebookScraperOptions options, 
        ILogger<FacebookScraper> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task LoginAsync()
    {
        _driver.Navigate().GoToUrl("https://m.facebook.com");
        _driver.FindElement(By.Id("m_login_email")).SendKeys(_options.Email);
        _driver.FindElement(By.Id("m_login_password")).SendKeys(_options.Password);
        _driver.FindElement(By.XPath("//button[@name='login']")).Click();
        await Task.Delay(3000);
        _driver.Navigate().GoToUrl("https://m.facebook.com/login/save-device/cancel/?flow=interstitial_nux&nux_source=regular_login");
        _logger.LogInformation("Logged successfully to account");
    }

    public Task ScrapFriendsListAsync()
    {
        throw new NotImplementedException();
    }

    public void OpenBrowser()
    {
        var options = new ChromeOptions();
        options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
        options.AddArgument("--disable-notifications");
        //options.AddArgument("headless");

        var driverService = ChromeDriverService.CreateDefaultService();
        //driverService.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(driverService, options);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        _jsExecutor = (IJavaScriptExecutor)_driver;
    }
}
