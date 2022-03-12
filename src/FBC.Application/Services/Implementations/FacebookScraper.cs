using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using FBC.Application.Models;

namespace FBC.Application.Services.Implementations;

internal sealed class FacebookScraper : IFacebookScraper
{
    private IWebDriver _mainDriver;
    private IWebDriver _helperDriver;
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
        if (string.IsNullOrEmpty(options.Email))
        {
            throw new ArgumentException("Email is an empty string");
        }

        if (string.IsNullOrEmpty(options.Password))
        {
            throw new ArgumentException("Password is an empty string");
        }

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

        _mainDriver = new ChromeDriver(driverService, options);
        //_helperDriver = new ChromeDriver(driverService, options);
        _mainDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        //_helperDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        _openedBrowser = true;
    }

    public async Task LoginAsync()
    {
        var loggedMainBrowser = await LoginInternalAsync(_mainDriver);
        //var loggedHelperBrowser = await LoginInternalAsync(_helperDriver);
        _logged = loggedMainBrowser && true;

        if (_logged)
        {
            _logger.LogInformation("Logged successfully to account");
        }
        else
        {
            _logger.LogError("Could not logged to account");
        }
    }

    public async Task ScrapFriendsListAsync(string facebookId)
    {
        ThrowIfInvalidState();

        var jsExecutor = (IJavaScriptExecutor)_mainDriver;
        var ids = new ConcurrentDictionary<ulong, bool>();
        var friendNodeXPath = "//div[@data-sigil=\"undoable-action\"]";
        var count = 0;

        _mainDriver.Navigate().GoToUrl($"https://m.facebook.com/{facebookId}/friends");

        while (true)
        {
            try
            {
                var presentFriendNode = IsElementPresent(_mainDriver, By.XPath(friendNodeXPath));
                if (count % 20 == 0 || !presentFriendNode)
                {
                    ScrollUntilElementPresent(_mainDriver, By.XPath(friendNodeXPath));
                }
            }
            catch (TimeoutException)
            {
                break;
            }
            

            var friendNode = _mainDriver.FindElement(By.XPath("//a[@data-sigil='touchable m-add-friend']"));
            var userDataJson = friendNode.GetAttribute("data-store");
            var userData = JsonSerializer.Deserialize<UserData>(userDataJson);

            if (userData != null && userData.Id.HasValue)
            {
                ids.TryAdd(userData.Id.Value, false);
                jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendNodeXPath)}.remove()");
                count++;
                _logger.LogInformation("{Count}. Added {ID} to list", count, userData.Id.Value);
            }
        }

        _logger.LogInformation("Scraped {Count} IDs from friends list", ids.Count);
        await Task.CompletedTask;
    }

    //private Task ScrapUsers(ConcurrentDictionary<ulong, bool> idsList)
    //{
    //    _driver.SwitchTo().
    //}

    private Task<bool> LoginInternalAsync(IWebDriver driver)
    {
        return Task.Run(() =>
        {
            driver.Navigate().GoToUrl("https://m.facebook.com");
            driver.FindElement(By.Id("m_login_email")).SendKeys(_options.Email);
            driver.FindElement(By.Id("m_login_password")).SendKeys(_options.Password);
            driver.FindElement(By.XPath("//button[@name='login']")).Click();

            var isSavePasswordPresent = WaitForReady(driver, By.XPath("//h3[contains(text(), 'Log In With One Tap')]"));

            if (isSavePasswordPresent)
            {
                driver.Navigate().GoToUrl("https://m.facebook.com/login/save-device/cancel/?flow=interstitial_nux&nux_source=regular_login");
            }

            var logged = WaitForReady(driver, By.XPath("//input[@name='target' and @type='hidden']"));

            if (logged && !string.IsNullOrEmpty(FacebookId))
            {
                var idInput = _mainDriver.FindElement(By.XPath("//input[@name='target' and @type='hidden']"));
                FacebookId = idInput.GetAttribute("value");
            }
            return logged;
        });
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

    private void ScrollUntilElementPresent(IWebDriver driver, By by, int retry = 10)
    {
        var jsExecutor = (IJavaScriptExecutor)driver;
        var retryCount = 0;

        do
        {
            jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            retryCount++;
            Thread.Sleep(1000);

        } while (!IsElementPresent(driver, by) && retryCount <= retry);

        if (retryCount > retry)
        {
            throw new TimeoutException();
        }
    }

    private bool IsElementPresent(IWebDriver driver, By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool WaitForReady(IWebDriver driver, By by)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        try
        {
            return wait.Until(_ => IsElementPresent(driver, by));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}
