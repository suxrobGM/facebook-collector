using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using FBC.Application.Models;

namespace FBC.Application.Services.Implementations;

internal sealed class FacebookScraper : IFacebookScraper
{
    private IWebDriver _driver;
    private IJavaScriptExecutor _jsExecutor;
    private string _startupWindow;
    private bool _logged;
    private bool _openedBrowser;
    private readonly ILogger<FacebookScraper> _logger;
    private readonly FacebookScraperOptions _options;

#pragma warning disable CS8618
    public FacebookScraper(
#pragma warning restore CS8618
        FacebookScraperOptions options, 
        ILogger<FacebookScraper> logger)
    {
        _options = options;
        _logger = logger;
    }

    public string FacebookId { get; private set; }

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
        _startupWindow = _driver.CurrentWindowHandle;
        _openedBrowser = true;
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

        _logged = WaitForReady(By.XPath("//input[@name='target' and @type='hidden']"));

        if (_logged)
        {
            var idInput = _driver.FindElement(By.XPath("//input[@name='target' and @type='hidden']"));
            FacebookId = idInput.GetAttribute("value");
            _logger.LogInformation("Logged successfully to account");
        }
        else
        {
            _logger.LogError("Could not logged to account");
        }  
    }

    public void ScrapFriendsList(string facebookId)
    {
        ThrowIfInvalidState();

        _driver.Navigate().GoToUrl($"https://m.facebook.com/{facebookId}/friends");
        var ids = new HashSet<ulong>();
        var friendNodeXPath = "//div[@data-sigil=\"undoable-action\"]";
        var count = 0;

        while (IsElementPresent(By.XPath(friendNodeXPath)))
        {
            var friendNode = _driver.FindElement(By.XPath("//a[@data-sigil='touchable m-add-friend']"));
            var userDataJson = friendNode.GetAttribute("data-store");
            var userData = JsonSerializer.Deserialize<UserData>(userDataJson);

            if (userData != null && userData.Id.HasValue)
            {
                ids.Add(userData.Id.Value);
                _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendNodeXPath)}.remove()");
                count++;
                _logger.LogInformation("{Count}. Added {ID} to list", count, userData.Id.Value);
            }

            if (count % 20 == 0)
            {
                _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                Thread.Sleep(1000);
            }
        }

        _logger.LogInformation("Scraped {Count} IDs from friends list", ids.Count);
    }

    private void ThrowIfInvalidState()
    {
        if (!_openedBrowser)
        {
            throw new InvalidOperationException("Browser is not open, please open browser first by calling method OpenBrowser()");
        }

        if (!_logged)
        {
            throw new InvalidOperationException("User is not logged to Facebook, please login first by calling method Login()");
        }
    }

    private string GetElementByXPathJS(string xpath)
    {
        return $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
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
