using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AngleSharp.Html.Parser;
using ScrapySharp.Network;

namespace FBC.Host;

/*public class FacebookScrapper
{
    private IWebDriver _driver;
    private IJavaScriptExecutor _jsExecutor;
    private string _startupWindow;
    private readonly string _email;
    private readonly string _password;
    private readonly HtmlDocument _htmlDoc;
    private readonly ScrapingBrowser _browser;
    private readonly HtmlParser _parser;

    public FacebookScrapper(string email, string password)
    {
        _email = email;
        _password = password;
        _htmlDoc = new HtmlDocument();
        _browser = new ScrapingBrowser()
        {
            Encoding = Encoding.UTF8,
            UserAgent = new FakeUserAgent("Chrome", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36")
        };
        _parser = new HtmlParser();
    }

    /// <summary>
    /// Basic method, will use Chrome browser for simulating and interactions
    /// </summary>
    public void OpenChrome()
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

    /// <summary>
    /// Authorize to Chrome and hideless browser
    /// </summary>
    public async Task LoginToMobileFacebookAsync()
    {
        _driver.Navigate().GoToUrl("https://m.facebook.com");
        _driver.FindElement(By.Id("m_login_email")).SendKeys(_email);
        _driver.FindElement(By.Id("m_login_password")).SendKeys(_password);
        _driver.FindElement(By.XPath("//button[@name='login']")).Click();
        await LoginToHidelessBrowserAsync();
        Thread.Sleep(3000);
        _driver.Navigate().GoToUrl("https://m.facebook.com/login/save-device/cancel/?flow=interstitial_nux&nux_source=regular_login");
        Console.WriteLine("Logged to account");
    }

    /// <summary>
    /// Scrap user friends by using chrome browser.
    /// Slow algorithm but safe method
    /// </summary>
    /// <param name="targetUsername">Target facebook username</param>
    public void ScrapFriends(string targetUsername = "suxrobgm")
    {
        _driver.Navigate().GoToUrl($"https://m.facebook.com/{targetUsername}/friends");
        _startupWindow = _driver.CurrentWindowHandle;
        Thread.Sleep(1000);

        int count = 0;
        int friendsCount = 0;
        bool isMyFriend = targetUsername == "suxrobgm" ? true : false;

        _htmlDoc.LoadHtml(_driver.PageSource);
        var friendsCountNode = _htmlDoc.DocumentNode.SelectSingleNode("//div[@data-sigil='marea']/a/.//span[contains(text(), 'друзей')]");

        if (friendsCountNode != null)
            friendsCount = int.Parse(friendsCountNode.InnerText.Split().First());

        if (friendsCount <= 0)
        {
            Console.WriteLine($"Target user: {targetUsername} have not any friends or target account permission restricted");
            return;
        }
        else
        {
            Console.WriteLine($"Target user: {targetUsername} have {friendsCount} friends");
        }

        while (true)
        {
            string friendContainerXP = "//div[@data-sigil=\"undoable-action\"]";
            IWebElement friendDivElement = null;

            if (IsElementPresent(By.XPath(friendContainerXP)))
            {
                friendDivElement = _driver.FindElement(By.XPath(friendContainerXP));
            }
            else if (IsElementPresent(By.XPath("//div[@class=\"_55wp _7om2 _5pxa\"]")))
            {
                friendDivElement = _driver.FindElement(By.XPath("//div[@class=\"_55wp _7om2 _5pxa\"]"));
                friendContainerXP = "//div[@class=\"_55wp _7om2 _5pxa\"]";
            }
            else
            {
                Console.WriteLine($"\nFinished! scrapped {targetUsername} friends");
                return;
            }

            string friendLink = friendDivElement.FindElement(By.XPath(".//div/a")).GetAttribute("href");

            if (friendLink == null)
            {
                _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                if (count % 20 == 0)
                {
                    _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                    Thread.Sleep(1000);
                }
                continue;
            }
            string username = friendLink.Split('/').Last();

            Console.WriteLine();
            Console.Write($"{++count}. ");

            using (var db = new DatabaseContext())
            {
                if (UserExistsInDb((i => i.UserName == username), db))
                {
                    _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                    if (count % 20 == 0)
                    {
                        _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                        Thread.Sleep(1000);
                    }
                    continue;
                }
            }

            try
            {
                ScrapUserBrowser(friendLink, isMyFriend, () => { _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()"); });
            }
            catch (Exception ex)
            {
                File.WriteAllText($"{count}.txt", $"{friendLink} \n{ex.Message} \n{targetUsername}");
                _driver.Close();
                _driver.SwitchTo().Window(_startupWindow);
                _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                continue;
            }

            if (count % 20 == 0)
                _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
        }
    }

    /// <summary>
    /// Scrap user friends by using combination of hideless and chrome browser.
    /// Very fast algorithm using multithread parallel methods but safeless
    /// </summary>
    /// <param name="targetUsername">Target facebook username</param>
    public void ScrapFriendsParallel(string targetUsername = "suxrobgm")
    {
        _driver.Navigate().GoToUrl($"https://m.facebook.com/{targetUsername}/friends");
        _startupWindow = _driver.CurrentWindowHandle;
        Thread.Sleep(1000);

        int count = 0;
        int friendsCount = 0;
        bool isMyFriend = targetUsername == "suxrobgm" ? true : false;
        var friendsLinkList = new List<string>();
        var friendsCountNode = _driver.FindElement(By.XPath("//div[@data-sigil='marea']/a/.//span[contains(text(), 'друзей')]"));

        if (friendsCountNode != null)
            friendsCount = int.Parse(friendsCountNode.Text.Split().First());

        if (friendsCount <= 0)
        {
            Console.WriteLine($"Target user: {targetUsername} have not any friends or target account permission restricted");
            return;
        }
        else
        {
            Console.WriteLine($"Target user: {targetUsername} have {friendsCount} friends");
        }

        while (true)
        {
            string friendContainerXP = "//div[@data-sigil=\"undoable-action\"]";
            IWebElement friendDivElement = null;

            if (IsElementPresent(By.XPath(friendContainerXP)))
            {
                friendDivElement = _driver.FindElement(By.XPath(friendContainerXP));
            }
            else if (IsElementPresent(By.XPath("//div[@class=\"_55wp _7om2 _5pxa\"]")))
            {
                friendDivElement = _driver.FindElement(By.XPath("//div[@class=\"_55wp _7om2 _5pxa\"]"));
                friendContainerXP = "//div[@class=\"_55wp _7om2 _5pxa\"]";
            }
            else
            {
                Console.WriteLine($"\nAdded {targetUsername} friends to list now will start parrallel scraping...\n");
                break;
            }

            string friendLink = friendDivElement.FindElement(By.XPath(".//div/a")).GetAttribute("href");

            if (friendLink == null)
            {
                _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                if (count % 20 == 0)
                {
                    _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                    Thread.Sleep(1000);
                }
                continue;
            }
            string username = friendLink.Split('/').Last();

            Console.WriteLine();
            Console.Write($"{++count}. ");

            using (var db = new DatabaseContext())
            {
                if (UserExistsInDb((i => i.UserName == username), db))
                {
                    _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                    if (count % 20 == 0)
                    {
                        _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                        Thread.Sleep(1000);
                    }
                    continue;
                }
            }

            // TEST
            if (friendLink.Contains("profile.php"))
            {
                Console.Write("link without username");
                _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");
                continue;
            }

            friendsLinkList.Add(friendLink);
            _jsExecutor.ExecuteScript($"{GetElementByXPathJS(friendContainerXP)}.remove()");

            if (count % 20 == 0)
            {
                _jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2)");
                Thread.Sleep(1000);
            }

            // TEST
            //if (count == 1000)
            //    return;
        }

        Parallel.ForEach(friendsLinkList, (profileLink) =>
        {
            ScrapUserHidelessBrowser(profileLink, true);
            Console.WriteLine();
        });
    }

    /// <summary>
    /// Scrap user by using browser (usually Chrome browser).
    /// Slow algorithm but safe method
    /// </summary>
    /// <param name="profileLink">Full link to user profile starts with 'https://m.facebook.com/'</param>
    /// <param name="isInFriendList">If user has in your friend list</param>
    /// <param name="callback">Callback function after completing this method</param>
    public void ScrapUserBrowser(string profileLink, bool isInFriendList = false, Action callback = null)
    {
        _startupWindow = _driver.CurrentWindowHandle;
        OpenNewTab(profileLink);
        WaitForReady(By.Id("timelineContextList"));
        _htmlDoc.LoadHtml(_driver.PageSource);

        var actionBarButtonNodes = _htmlDoc.DocumentNode.SelectNodes("//*[@data-sigil='hq-profile-logging-action-bar-button flyout-causal']");
        string jsonData;

        if (actionBarButtonNodes.Count >= 2)
            jsonData = _htmlDoc.DocumentNode.SelectNodes("//*[@data-sigil='hq-profile-logging-action-bar-button flyout-causal']")[1].Attributes["data-store"].DeEntitizeValue;
        else
            jsonData = _htmlDoc.DocumentNode.SelectNodes("//*[@data-sigil='hq-profile-logging-action-bar-button flyout-causal']")[0].Attributes["data-store"].DeEntitizeValue;

        string profileId = JObject.Parse(jsonData)["hq-profile-logging"]["profile_id"].ToString();
        string sessionToken = JObject.Parse(jsonData)["hq-profile-logging"]["profile_session_token"].ToString();

        using (var db = new DatabaseContext())
        {
            if (UserExistsInDb((i => i.Id == profileId), db) || profileId == "100004966276119")
            {
                _driver.Close();
                _driver.SwitchTo().Window(_startupWindow);
                callback?.Invoke();
                return;
            }

            var membershipNode = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='profile_intro_card']/.//span[contains(text(), 'На Facebook с')]");
            var bioNode = _htmlDoc.GetElementbyId("bio");
            string userFullName = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='cover-name-root']/h3").InnerText;
            string username = profileLink.Split('/').Last();
            string headerImgSrc = "null";
            string profileImgSrc = "null";
            string profileBio = "null";

            if (IsElementPresent(By.XPath("//div[@data-sigil='cover-photo']/a/i")))
                headerImgSrc = _driver.FindElement(By.XPath("//div[@data-sigil='cover-photo']/a/i")).GetCssValue("background").Split("url(")[1].Split("\"")[1];

            if (IsElementPresent(By.XPath("//i[@class='img profpic']")))
                profileImgSrc = _driver.FindElement(By.XPath("//i[@class='img profpic']")).GetCssValue("background").Split("url(")[1].Split("\"")[1];

            if (bioNode != null)
                profileBio = bioNode.InnerText;

            var user = new User()
            {
                Id = profileId,
                UserName = username,
                ProfilePhotoSrc = profileImgSrc,
                HeaderPhotoSrc = headerImgSrc,
                Bio = profileBio,
                IsMyFriend = isInFriendList
            };
            user.ParseFullName(userFullName);

            if (membershipNode != null)
                user.MemberSince = DateTime.Parse(membershipNode.InnerText.Split(" с ")[1]);

            string aboutUrl = "about";

            if (profileLink.Contains("profile.php"))
            {
                aboutUrl = $"?v=info&Ist={sessionToken}&id={sessionToken.Split(':')[1]}";
                _driver.Url = $"https://m.facebook.com/profile.php{aboutUrl}";
            }
            else
            {
                _driver.Navigate().GoToUrl($"{profileLink.Trim()}/{aboutUrl}");
            }

            _htmlDoc.LoadHtml(_driver.PageSource);

            var contactInfo = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='contact-info']/div");
            var basicInfo = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='basic-info']/div");
            var educations = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='education']/div");
            var works = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='work']/div");
            var livedCities = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='living']/div");
            var skills = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='skills']/div");
            var quote = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='quote']/div");
            var relationship = _htmlDoc.DocumentNode.SelectSingleNode("//*[@id='relationship']/div");

            if (basicInfo != null)
            {
                foreach (var item in basicInfo.ChildNodes)
                {
                    if (item.Attributes["title"].Value == "Дата рождения")
                    {
                        string birthdayNode = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;
                        if (DateTime.TryParse(birthdayNode, out DateTime birthday))
                            user.Birthday = birthday;
                    }

                    if (item.Attributes["title"].Value == "Пол")
                        user.Gender = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "Языки")
                        user.Languages = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "Религиозные взгляды")
                        user.ReligiousView = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;
                }
            }

            if (contactInfo != null)
            {
                var contactNumbersList = new List<string>();
                var webSitesList = new List<string>();
                foreach (var item in contactInfo.ChildNodes)
                {
                    if (item.Attributes["title"].Value == "Мобильный")
                        contactNumbersList.Add(item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText);

                    if (item.Attributes["title"].Value == "OK")
                        user.OK = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "Twitter")
                        user.Twitter = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "VK")
                        user.VK = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "Instagram")
                        user.Instagram = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "YouTube")
                        user.YouTube = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "LinkedIn")
                        user.LinkedIn = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "GitHub")
                        user.GitHub = item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText;

                    if (item.Attributes["title"].Value == "Веб-сайты")
                        webSitesList.Add(item.SelectSingleNode(".//div[@class='lr']/div[contains(@class, ' r')]").InnerText);

                }
                user.ContactNumbers = string.Join(',', contactNumbersList);
                user.WebSites = string.Join(',', webSitesList);
            }

            if (educations != null)
            {
                var institutions = new HashSet<UserEducation>(new UserInstitutionComparer());
                foreach (var item in educations.ChildNodes)
                {
                    var node = item.SelectSingleNode(".//div/span/a");
                    string link = node.Attributes["href"].Value;
                    string name = node.InnerText;
                    string institutionType = item.SelectNodes(".//div/span")[1].InnerText;

                    var institution = new Education()
                    {
                        Name = name,
                        Link = link
                    };

                    if (institutionType != "Средняя школа")
                        institution.IsHigherEducation = true;

                    if (db.Educations.Where(i => i.Link == institution.Link).FirstOrDefault() != null)
                        institution = db.Educations.Where(i => i.Link == institution.Link).FirstOrDefault();
                    else
                        institution = db.Educations.Add(institution).Entity;

                    var userInstitution = new UserEducation() { Institution = institution };
                    institutions.Add(userInstitution);
                }
                user.Institutions.AddRange(institutions);
            }

            if (works != null)
            {
                foreach (var item in works.ChildNodes)
                {
                    var node = item.SelectSingleNode(".//span/a");
                    string link = node.Attributes["href"].Value;
                    string name = node.InnerText;
                    string position = "null";
                    string workingPeriod = "null";

                    var company = new Company()
                    {
                        Name = name,
                        Link = link
                    };

                    if (db.Companies.Where(i => i.Link == company.Link).FirstOrDefault() != null)
                        company = db.Companies.Where(i => i.Link == company.Link).FirstOrDefault();
                    else
                        company = db.Companies.Add(company).Entity;

                    var work = new Employee() { Company = company };

                    if (item.SelectNodes(".//span").Count > 2)
                    {
                        position = item.SelectNodes(".//span")[1].InnerText;
                        workingPeriod = item.SelectNodes(".//span")[2].InnerText;
                        work.Position = position;

                        if (workingPeriod.Contains("по настоящее время"))
                        {
                            work.IsCurrentlyWorking = true;
                            string startDateString = workingPeriod.Substring(3, (workingPeriod.IndexOf("по ") - 3));

                            if (DateTime.TryParse(startDateString, out DateTime startWorkDate))
                                work.StartWorkDate = startWorkDate;
                        }
                        else if (workingPeriod == "Я работаю здесь в настоящее время")
                        {
                            work.IsCurrentlyWorking = true;
                        }
                        else
                        {
                            string[] dates = null;
                            if (workingPeriod.Contains('-'))
                                dates = workingPeriod.Split('-');
                            else if (workingPeriod.Contains('—'))
                                dates = workingPeriod.Split('—');

                            if (dates != null && dates.Length >= 2)
                            {
                                if (DateTime.TryParse(dates[0], out DateTime startWorkDate))
                                    work.StartWorkDate = startWorkDate;
                                if (DateTime.TryParse(dates[1], out DateTime endWorkDate))
                                    work.EndWorkDate = endWorkDate;
                            }
                        }
                    }

                    user.Works.Add(work);
                }
            }

            if (livedCities != null)
            {
                var livedCitiesList = new List<string>();
                foreach (var item in livedCities.ChildNodes)
                {
                    var nodes = item.SelectNodes(".//header/h4");
                    string cityName = nodes[0].InnerText;
                    string livingType = nodes[1].InnerText;

                    if (livingType == "Родной город")
                        user.Hometown = cityName;
                    else
                        livedCitiesList.Add(cityName);
                }
                user.LivedCities = string.Join(" * ", livedCitiesList);
            }

            if (skills != null)
                user.Skills = skills.InnerText;

            if (quote != null)
                user.Quote = quote.InnerText;

            if (relationship != null)
                user.MaritalStatus = relationship.InnerText;

            db.Users.Add(user);
            db.SaveChanges();
            Console.Write(user);
        }

        _driver.Close();
        _driver.SwitchTo().Window(_startupWindow);
        callback?.Invoke();
    }

    /// <summary>
    /// Scrap user by using browser (usually Chrome browser).
    /// Fast algorithm but safeless method 
    /// </summary>
    /// <param name="profileLink">Full link to user profile starts with 'https://m.facebook.com/'</param>
    /// <param name="isInFriendList">If user has in your friend list</param>
    /// <param name="callback">Callback function, will invoke after completing this method</param>
    public void ScrapUserHidelessBrowser(string profileLink, bool isInFriendList = false, Action callback = null)
    {
        var aboutPage = _browser.NavigateToPage(new Uri($"{profileLink}/about/"));
        var document = _parser.ParseDocument(aboutPage.Content);
        var profileId = document.Body.QuerySelectorAll("a").Where(i => i.GetAttribute("href").Contains("owner_id")).First().GetAttribute("href").Split("owner_id=")[1];

        using (var db = new DatabaseContext())
        {
            if (UserExistsInDb((i => i.Id == profileId), db) || profileId == "100004966276119")
                return;

            string username = profileLink.Split('/').Last();
            string profileBio = document.Body.QuerySelector("div.co.cp.cq.cr")?.TextContent;
            string userFullName = document.Body.QuerySelector("span > strong")?.TextContent;
            string headerImgSrc = document.Body.QuerySelector("#profile_cover_photo_container > a > img")?.GetAttribute("src");
            string profileImgSrc = document.Body.QuerySelector("img.cb.r")?.GetAttribute("src");

            var user = new User()
            {
                Id = profileId,
                UserName = username,
                ProfilePhotoSrc = profileImgSrc,
                HeaderPhotoSrc = headerImgSrc,
                Bio = profileBio,
                IsMyFriend = isInFriendList
            };
            user.ParseFullName(userFullName);

            var basicInfo = document.Body.QuerySelector("#basic-info > div > div:nth-child(2)");
            var contactInfo = document.Body.QuerySelector("#contact-info > div > div:nth-child(2)");
            var educations = document.Body.QuerySelector("#education > div > div:nth-child(2)");
            var works = document.Body.QuerySelector("#work > div > div:nth-child(2)");
            var livedCities = document.Body.QuerySelector("#living > div > div:nth-child(2)");
            var skills = document.Body.QuerySelector("#skills > div > div:nth-child(2)");
            var quote = document.Body.QuerySelector("#quote > div > div:nth-child(2)");
            var relationship = document.Body.QuerySelector("#relationship > div > div:nth-child(2)");

            var contactNumbersList = new List<string>();
            var webSitesList = new List<string>();
            var livedCitiesList = new List<string>();
            var institutions = new HashSet<UserEducation>(new UserInstitutionComparer());

            if (basicInfo != null)
            {
                foreach (var item in basicInfo.Children)
                {
                    if (item.Attributes["title"].Value == "Дата рождения")
                    {
                        string birthdayNode = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;
                        if (DateTime.TryParse(birthdayNode, out DateTime birthday))
                            user.Birthday = birthday;
                    }

                    if (item.Attributes["title"].Value == "Пол")
                        user.Gender = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "Языки")
                        user.Languages = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "Религиозные взгляды")
                        user.ReligiousView = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;
                }
            }

            if (contactInfo != null)
            {
                foreach (var item in contactInfo.Children)
                {
                    if (item.Attributes["title"].Value == "Мобильный")
                        contactNumbersList.Add(item.QuerySelector("td[valign='top']:nth-child(2)").TextContent);

                    if (item.Attributes["title"].Value == "OK")
                        user.OK = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "Twitter")
                        user.Twitter = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "VK")
                        user.VK = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "Instagram")
                        user.Instagram = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "YouTube")
                        user.YouTube = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "LinkedIn")
                        user.LinkedIn = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "GitHub")
                        user.GitHub = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;

                    if (item.Attributes["title"].Value == "Веб-сайты")
                        webSitesList.Add(item.QuerySelector("td[valign='top']:nth-child(2)").TextContent);
                }
            }

            if (educations != null)
            {
                foreach (var item in educations.Children)
                {
                    var nodes = item.QuerySelectorAll("div > span");
                    var eduNode = nodes.Select(i => i.QuerySelector("a")).First();
                    string link = eduNode.Attributes["href"].Value;
                    string name = eduNode.TextContent;
                    string institutionType = nodes[1].TextContent;

                    var institution = new Education()
                    {
                        Name = name,
                        Link = link
                    };

                    if (institutionType != "Средняя школа")
                        institution.IsHigherEducation = true;

                    if (db.Educations.Where(i => i.Link == institution.Link).FirstOrDefault() != null)
                        institution = db.Educations.Where(i => i.Link == institution.Link).FirstOrDefault();
                    else
                        institution = db.Educations.Add(institution).Entity;

                    var userInstitution = new UserEducation() { Institution = institution };
                    institutions.Add(userInstitution);
                }
            }

            if (works != null)
            {
                foreach (var item in works.Children)
                {
                    var node = item.QuerySelector("span > a");
                    string link = node.Attributes["href"].Value;
                    string name = node.TextContent;
                    string position = "null";
                    string workingPeriod = "null";

                    var company = new Company()
                    {
                        Name = name,
                        Link = link
                    };

                    if (db.Companies.Where(i => i.Link == company.Link).FirstOrDefault() != null)
                        company = db.Companies.Where(i => i.Link == company.Link).FirstOrDefault();
                    else
                        company = db.Companies.Add(company).Entity;

                    var work = new Employee() { Company = company };

                    if (item.QuerySelectorAll("span").Length > 2)
                    {
                        position = item.QuerySelectorAll("span")[1]?.TextContent;
                        workingPeriod = item.QuerySelectorAll("span")[2]?.TextContent;
                        work.Position = position;

                        if (workingPeriod.Contains("по настоящее время"))
                        {
                            work.IsCurrentlyWorking = true;
                            string startDateString = workingPeriod.Substring(3, (workingPeriod.IndexOf("по ") - 3));

                            if (DateTime.TryParse(startDateString, out DateTime startWorkDate))
                                work.StartWorkDate = startWorkDate;
                        }
                        else if (workingPeriod == "Я работаю здесь в настоящее время")
                        {
                            work.IsCurrentlyWorking = true;
                        }
                        else
                        {
                            string[] dates = null;
                            if (workingPeriod.Contains('-'))
                                dates = workingPeriod.Split('-');
                            else if (workingPeriod.Contains('—'))
                                dates = workingPeriod.Split('—');

                            if (dates != null && dates.Length >= 2)
                            {
                                if (DateTime.TryParse(dates[0], out DateTime startWorkDate))
                                    work.StartWorkDate = startWorkDate;
                                if (DateTime.TryParse(dates[1], out DateTime endWorkDate))
                                    work.EndWorkDate = endWorkDate;
                            }
                        }
                    }

                    user.Works.Add(work);
                }
            }

            if (livedCities != null)
            {
                foreach (var item in livedCities.Children)
                {
                    string cityName = item.QuerySelector("td[valign='top']:nth-child(2)").TextContent;
                    string livingType = item.QuerySelector("td[valign='top']:nth-child(1)").TextContent;

                    if (livingType == "Родной город")
                        user.Hometown = cityName;
                    else
                        livedCitiesList.Add(cityName);
                }
            }

            user.Skills = skills?.TextContent;
            user.Quote = quote?.TextContent;
            user.MaritalStatus = relationship?.TextContent;

            if (contactNumbersList.Count > 0)
                user.ContactNumbers = string.Join(',', contactNumbersList);

            if (webSitesList.Count > 0)
                user.WebSites = string.Join(',', webSitesList);

            if (livedCitiesList.Count > 0)
                user.LivedCities = string.Join(" * ", livedCitiesList);

            if (institutions.Count > 0)
                user.Institutions.AddRange(institutions);

            db.Users.Add(user);
            db.SaveChanges();
            Console.Write(user);
        }

        callback?.Invoke();
    }

    private async Task LoginToHidelessBrowserAsync()
    {
        await Task.Run(() =>
        {
            var loginPage = _browser.NavigateToPage(new Uri("https://m.facebook.com/"));
            var form = loginPage.FindFormById("login_form");
            form["email"] = _email;
            form["pass"] = _password;
            form.Submit();
            //var aboutPage = _browser.NavigateToPage(new Uri("https://m.facebook.com/bobur.sobirov.7/about"));

            //File.WriteAllText("_html4.html", aboutPage.Content);
            //ScrapUser_2("https://m.facebook.com/realgulomov");
        });
    }
    private bool UserExistsInDb(Func<User, bool> predicate, DatabaseContext context)
    {
        var user = context.Users.Where(predicate).FirstOrDefault();
        return user != null;
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
    private string GetImgBase64(string imgElement)
    {
        string imgBase64string = _jsExecutor.ExecuteScript(@$"
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
    private string GetElementByXPathJS(string xpath)
    {
        return $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
    }
    private void OpenNewTab(string url)
    {
        var windowHandles = _driver.WindowHandles;
        _jsExecutor.ExecuteScript(string.Format("window.open('{0}', '_blank');", url));
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
}*/
