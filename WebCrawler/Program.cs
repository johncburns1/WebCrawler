//--------------------
// FILE:   Program.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace WebCrawler
{
    class Program
    {
        /// <summary>
        /// Main method for web crawler.
        /// <para>
        /// Argument 1 is log level.
        /// Argument 2 is word limit.
        /// Argument 3 is list of words separated by commas to be excluded in the crawler.
        /// There cannot be spaces between the words unless the list is in " ".
        /// </para>
        /// </summary>
        /// 
        /// <param name="args">
        /// </param>
        static void Main(string[] args)
        {
            // get the command line args

            int wordLimit              = 10;
            List<string> excludedWords = new List<string>();
            string level               = "Information";

            if (args.Length > 0)
            {
                if (args.Length >= 1)
                {
                    var arg = args[0].Trim();
                    level   = arg;
                }
                if (args.Length >= 2)
                {
                    var arg = args[1].Trim();
                    if (!Int32.TryParse(arg, out wordLimit)) 
                    {
                        Log.Logger.Debug($"Could not set word limit to desired configuration {arg}.");
                    }
                }
                if (args.Length >= 3)
                {
                    var arg       = args[2].Trim().Replace(" ", ",");
                    excludedWords = arg.TokenizeToList(",");
                }
            }

            // configure the logger

            LogHelper.ConfigureLogger(level);

            Log.Logger.Information($"Starting Crawler with configurations: Log_Level={level}, Word_Limit={wordLimit}, Excluded_Words={excludedWords.ToPrettyString()}");

            // start the crawler

            CrawlerResult result = new CrawlerResult();
            using (var crawler = new Crawler(wordLimit, excludedWords))
            {
                Task.Run(async () =>
                {
                    result = await crawler.CrawlAsync();
                }).Wait();
            }            

            if (result.Error != null)
            {
                Log.Logger.Error($"Crawl failed with Error: {result.Error.Message}\n{result.Error.StackTrace}");
                return;
            }

            Log.Logger.Information($"\n***MOST FREQUENT*** \n{result.Words.ToPrettyString()}");
        }
    }
}
