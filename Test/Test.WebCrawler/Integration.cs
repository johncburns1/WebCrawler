//--------------------
// FILE:   Integration.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using NuGet.Frameworks;
using Polly;
using WebCrawler;
using Xunit;

namespace Test.WebCrawler
{
    public class Integration
    {
        /// <summary>
        /// The <see cref="HttpClient"/> used to make calls network calls.
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// The <see cref="RetryPolicy"/> used to retry Http requests upon failure.
        /// </summary>
        private AsyncPolicy retryPolicy;

        /// <summary>
        /// The <see cref="HttpClientHandler"/> for the <see cref="HttpClient"/>.
        /// </summary>
        private readonly HttpClientHandler handler;

        public Integration()
        {
            // configure HttpClient

            this.handler            = new HttpClientHandler();
            this.client             = new HttpClient(handler, false);
            this.client.BaseAddress = new Uri("https://www.crawler-test.com/content/word_count_100_words");

            // configure retry policy

            this.retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void TestInput()
        {
            using (var crawler = new Crawler())
            {
                Assert.Equal(10, crawler.WordLimit);
                Assert.Empty(crawler.WordDict);
                Assert.Empty(crawler.ExcludedWords);
                Assert.Empty(crawler.MostFrequent);
            }

            using (var crawler = new Crawler(5, new List<string> {"Microsoft", "the", "lobster"}))
            {
                Assert.Equal(5, crawler.WordLimit);
                Assert.Equal(3, crawler.ExcludedWords.Count);
                Assert.Equal(new HashSet<string> { "Microsoft", "the", "lobster" }, crawler.ExcludedWords);
            }

            using (var crawler = new Crawler(-1))
            {
                Assert.Equal(10, crawler.WordLimit);
                Assert.Empty(crawler.WordDict);
                Assert.Empty(crawler.ExcludedWords);
                Assert.Empty(crawler.MostFrequent);
            }
        }

        [Fact]
        public async void TestBadAddress()
        {
            using (var crawler = new Crawler(baseAddress: "https://lost/fdsjaklfdjsalfdsajlkfjdlsaffdkla;sfjkld"))
            {
                Assert.Equal("https://lost/fdsjaklfdjsalfdsajlkfjdlsaffdkla;sfjkld", crawler.BaseAddress);
                var result = await crawler.CrawlAsync();
                Assert.NotNull(result.Error);
            }
        }

        [Fact]
        public async void TestCrawlerAgainstActivePage()
        {
            string html = null;

            // execute async request to crawl base address

            await retryPolicy.ExecuteAsync(async () =>
            {
                // get the page content

                var response = await client.GetAsync("");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"FAILED WITH STATUS CODE: {response.StatusCode}");
                }

                // read the string content 

                html = await response.Content.ReadAsStringAsync();
            });

            // load the HTML document
            // and count the words

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var endName  = "noscript";
            var rootNode = document.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("row"))
                .First();
            
            using (var crawler = new Crawler(baseAddress: "https://www.crawler-test.com/content/word_count_100_words"))
            {
                var result = await crawler.CrawlAsync(rootNode, endName);

                Assert.NotEmpty(crawler.WordDict);
                Assert.NotEmpty(crawler.MostFrequent);
                Assert.Null(result.Error);
                Assert.NotNull(result.Words);
                Assert.Equal(5, result.Words.Values.Max());

                var sum = 0;
                foreach (var v in crawler.WordDict.Values)
                {
                    sum += v;
                }

                var key = "";
                foreach (var i in crawler.WordDict) 
                {
                    if (i.Value == result.Words.Values.Max())
                    {
                        key = i.Key;
                    }
                }

                Assert.Equal("he", key);
                Assert.Equal(100, sum);
            }
        }

        [Fact]
        public async void TestCrawlerExcludedWords()
        {
            string html = null;

            // execute async request to crawl base address

            await retryPolicy.ExecuteAsync(async () =>
            {
                // get the page content

                var response = await client.GetAsync("");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"FAILED WITH STATUS CODE: {response.StatusCode}");
                }

                // read the string content 

                html = await response.Content.ReadAsStringAsync();
            });

            // load the HTML document
            // and count the words

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var endName = "noscript";
            var rootNode = document.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("row"))
                .First();

            using (var crawler = new Crawler(excludedWords: new List<string> { "he", "a" }, baseAddress: "https://www.crawler-test.com/content/word_count_100_words"))
            {
                var result = await crawler.CrawlAsync(rootNode, endName);

                Assert.NotEmpty(crawler.WordDict);
                Assert.NotEmpty(crawler.MostFrequent);
                Assert.Null(result.Error);
                Assert.NotNull(result.Words);
                Assert.Equal(4, result.Words.Values.Max());

                var sum = 0;
                foreach (var v in crawler.WordDict.Values)
                {
                    sum += v;
                }

                var key = "";
                foreach (var i in crawler.WordDict)
                {
                    if (i.Value == result.Words.Values.Max())
                    {
                        key = i.Key;
                    }
                }

                Assert.Equal("his", key);
                Assert.Equal(91, sum);
            }
        }
    }
}
