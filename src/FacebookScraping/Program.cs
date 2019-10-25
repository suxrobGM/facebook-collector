using System;
using System.IO;

namespace FacebookScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            var fbScrapper = new FacebookScrapper("email", "password");
            fbScrapper.OpenChrome();
            fbScrapper.LoginToMobileFacebookAsync().Wait();
            fbScrapper.ScrapFriends();           
            //fbScrapper.ScrapFriendsParallel();

            //var captchaDownloader = new CaptchaDownloader();
            //captchaDownloader.DownloadMailRuCaptcha();

            Console.WriteLine("\nEND");
            Console.ReadKey();
        }
    }
}
