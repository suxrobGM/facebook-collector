using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScrapySharp.Network;

namespace FB.Collector.Host;

public class CaptchaDownloader
{
    private IWebDriver _driver;

    public CaptchaDownloader()
    {
        _driver = new ChromeDriver();
    }

    public void DownloadMailRuCaptcha(int captchaCount = 10000)
    {
        _driver.Navigate().GoToUrl("https://account.mail.ru/signup/simple");
        Thread.Sleep(1000);

        string startupWindow = _driver.CurrentWindowHandle;

        _driver.FindElement(By.Name("firstname")).SendKeys("Test");
        _driver.FindElement(By.Name("lastname")).SendKeys("Test");

        var arrowElements = _driver.FindElements(By.ClassName("b-dropdown__arrow"));
        var selectElements = _driver.FindElements(By.XPath("//a[@data-bem='b-dropdown__list__params']"));
        var daysSelectElements = selectElements.Take(30);
        var monthSelectElements = selectElements.Skip(30).Take(12);
        var yearSelectElements = selectElements.Skip(42);

        arrowElements[0].Click();
        daysSelectElements.ElementAt(3).Click();
        arrowElements[2].Click();
        monthSelectElements.ElementAt(3).Click();
        arrowElements[4].Click();
        yearSelectElements.ElementAt(10).Click();

        _driver.FindElement(By.XPath("//div[contains(@class,'b-radiogroup__item')]")).Click();
        _driver.FindElement(By.XPath("//*[@data-blockid='email_name']")).SendKeys("Test979250728");
        _driver.FindElement(By.Name("password")).SendKeys("Suxrobbek0729");
        var passwordRetry = _driver.FindElement(By.Name("password_retry"));
        passwordRetry.SendKeys("Suxrobbek0729");
        Thread.Sleep(2000);
        passwordRetry.SendKeys(Keys.Enter);
        Thread.Sleep(1000);

        int count = 1276;
        while (true)
        {
            string captchaSrc = _driver.FindElement(By.XPath("//img[contains(@class,'js-captcha-img')]")).GetAttribute("src");
            _driver.FindElement(By.XPath("//a[contains(@class,'js-captcha-reload')]")).Click();
            OpenNewTab(captchaSrc);
            WaitForReady(By.TagName("img"));
            File.WriteAllBytes($"captcha/{count}.png", Convert.FromBase64String(GetImgBase64("document.getElementsByTagName('img')[0]")));
            _driver.Close();
            _driver.SwitchTo().Window(startupWindow);
            count++;
        }
    }

    private string GetImgBase64(string imgElement)
    {
        var jsExecutor = (IJavaScriptExecutor)_driver;
        string imgBase64string = jsExecutor.ExecuteScript(@$"
                                var c = document.createElement('canvas');
                                var ctx = c.getContext('2d');
                                var img = {imgElement};
                                c.height=img.height;
                                c.width=img.width;
                                ctx.drawImage(img, 0, 0,img.width, img.height);
                                var base64String = c.toDataURL();
                                return base64String;                                                       
                            ") as string;
        return imgBase64string.Split(',').Last();
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
    private void OpenNewTab(string url)
    {
        var windowHandles = _driver.WindowHandles;
        var jsExecutor = (IJavaScriptExecutor)_driver;
        jsExecutor.ExecuteScript(string.Format("window.open('{0}', '_blank');", url));
        var newWindowHandles = _driver.WindowHandles;
        var openedWindowHandle = newWindowHandles.Except(windowHandles).Single();
        _driver.SwitchTo().Window(openedWindowHandle);
    }
    private void WaitForReady(By by)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        wait.Until(driver =>
        {
            //bool isAjaxFinished = (bool)((IJavaScriptExecutor)driver).ExecuteScript("return jQuery.active == 0");
            return IsElementPresent(by);
        });
    }
}
