using FBC.Application.Options;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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

    public void Login()
    {
        _driver.Navigate().GoToUrl("https://m.facebook.com");
        _driver.FindElement(By.Id("m_login_email")).SendKeys(_options.Email);
        _driver.FindElement(By.Id("m_login_password")).SendKeys(_options.Password);
        _driver.FindElement(By.XPath("//button[@name='login']")).Click();

        var isSavePasswordPresent = WaitForReady(By.XPath("//h3[contains(text(), 'Log In With One Tap')]"));

        if (isSavePasswordPresent)
        {
            _driver.Navigate().GoToUrl("https://m.facebook.com/login/save-device/cancel/?flow=interstitial_nux&nux_source=regular_login");
        }
        
        var isLogged = WaitForReady(By.XPath("//*[@id='MComposer']"));

        if (isLogged)
        {
            _logger.LogInformation("Logged successfully to account");
        }
        else
        {
            _logger.LogError("Could not logged to account");
        }  
    }

    public void ScrapFriendsList(string username)
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

    private bool IsElementPresent(By by)
    {
        try
        {
            _driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool WaitForReady(By by)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        try
        {
            return wait.Until(_ => IsElementPresent(by));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}
