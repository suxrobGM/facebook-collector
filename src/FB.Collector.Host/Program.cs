using System;
using System.IO;

namespace FB.Collector.Host;
class Program
{
    static void Main(string[] args)
    {
        var fbScrapper = new FacebookScrapper("email", "password");
        fbScrapper.OpenChrome();
        fbScrapper.LoginToMobileFacebookAsync().Wait();
        fbScrapper.ScrapFriends();

        Console.WriteLine("\nEND");
        Console.ReadKey();
    }
}
