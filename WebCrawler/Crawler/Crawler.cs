//--------------------
// FILE:   Crawler.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using Priority_Queue;
using Polly;
using Polly.Retry;
using Serilog;

namespace WebCrawler
{
    /// <summary>
    /// Describes a Crawler that crawls a specified URI for unique
    /// words and their counts.
    /// </summary>
    public class Crawler : IDisposable
    {
        /// <summary>
        /// The <see cref="string"/> key value indicating whether a node has been
        /// visited.
        /// </summary>
        public const string VISITED_KEY = "node-visited";

        /// <summary>
        /// The <see cref="HttpClient"/> used to make calls network calls.
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// The <see cref="HttpClientHandler"/> for the <see cref="HttpClient"/>.
        /// </summary>
        private readonly HttpClientHandler handler;

        /// <summary>
        /// The <see cref="RetryPolicy"/> used to retry Http requests upon failure.
        /// </summary>
        private AsyncPolicy retryPolicy;

        /// <summary>
        /// The <see cref="SimplePriorityQueue{string}"/> that contains the top n occurrences.
        /// </summary>
        private SimplePriorityQueue<string> priorityQueue;

        /// <summary>
        /// The <see cref="Dictionary{string, int}"/> that maps <see cref="string"/> words to 
        /// <see cref="int"/> number of occurrences.
        /// </summary>
        private Dictionary<string, int> wordDict;

        /// <summary>
        /// The <see cref="System.Collections.Generic.HashSet{string}"/> containing all <see cref="string"/>
        /// words to exclude from the crawl.
        /// </summary>
        private System.Collections.Generic.HashSet<string> excludedWords;

        /// <summary>
        /// The <see cref="int"/> number of words to return.
        /// </summary>
        private readonly int wordLimit;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// 
        /// <param name="wordLimit">The <see cref="int"/> limit for the number of
        /// words to return.  Word limit must be positive.</param>
        /// <param name="excludedWords">The <see cref="List{string}"/> containing words to exclude
        /// from the crawl.</param>
        /// <param name="baseAddress">The <see cref="string"/> base address to set in <see cref="client"/>.
        /// THIS IS USED PRIMARILY FOR TESTING. THERE IS NOT NEED TO SET THIS VALUE 
        /// EXPLICITLY OUTSIDE OF TEST ENVIRONMENT.</param>
        public Crawler(
            int wordLimit              = 10,
            List<string> excludedWords = null, 
            string baseAddress         = null)
        {
            // initialize maps and sets

            this.wordLimit     = (wordLimit < 0) ? 10 : wordLimit;
            this.priorityQueue = new SimplePriorityQueue<string>();
            this.wordDict      = new Dictionary<string, int>();
            this.excludedWords = (excludedWords == null) ? new HashSet<string>() : excludedWords.ToHashSet<string>();

            // configure HttpClient

            if (string.IsNullOrEmpty(baseAddress))
            {
                baseAddress = HttpSettings.BaseAddress;
            }

            this.handler            = new HttpClientHandler();
            this.client             = new HttpClient(handler, false);
            this.client.BaseAddress = new Uri(baseAddress);

            // configure retry policy

            this.retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(HttpSettings.MaxRetryAttempts, i => HttpSettings.RetryInterval);
        }

        /// <summary>
        /// Gets <see cref="wordLimit"/>.
        /// </summary>
        public int WordLimit => wordLimit;

        /// <summary>
        /// Gets most frequent words from scraper.
        /// </summary>
        public Dictionary<string, int> MostFrequent => GetMostFrequentWords();

        /// <summary>
        /// Gets <see cref="wordDict"/>.
        /// </summary>
        public Dictionary<string, int> WordDict => wordDict;

        /// <summary>
        /// Gets <see cref="excludedWords"/>.
        /// </summary>
        public HashSet<string> ExcludedWords => excludedWords;

        /// <summary>
        /// Gets the <see cref="string"/> base address of <see cref="client"/>.
        /// </summary>
        public string BaseAddress => client.BaseAddress.AbsoluteUri;

        /// <summary>
        /// Crawls the specified web page collecting counts of each unique
        /// <see cref="string"/> word in the History section.
        /// </summary>
        /// 
        /// <param name="rootNode">The <see cref="HtmlNode"/> root node to start crawl at.
        /// USED PRIMARILY FOR TESTING.  THERE IS NOT NEED TO SET THIS VALUE EXPLICITLY
        /// OUTSIDE OF TEST ENVIRONMENT.</param>
        /// <param name="endName">The <see cref="string"/> name of the <see cref="HtmlNode"/>
        /// to terminate on.  USED PRIMARILY FOR TESTING.  THERE IS NOT NEED TO SET THIS
        /// VALUE EXPLICITLY OUTSIDE OF TEST ENVIRONMENT.
        /// 
        /// <returns>A <see cref="Task{CrawlerResult}"/> representing the async operation where
        /// the result is the <see cref="CrawlerResult"/> of the crawler execution. A
        /// 0 success code indicates success.  A negative success code indicates a failure.</returns>
        public async Task<CrawlerResult> CrawlAsync(HtmlNode rootNode = null, string endName = "h2")
        {
            CrawlerResult result  = new CrawlerResult();
            string html           = null;

            // execute async request to crawl base address

            await retryPolicy.ExecuteAsync(async () =>
            {
                // get the page content

                HttpResponseMessage response = null;

                try
                {
                    response = await client.GetAsync("");
                }
                catch (Exception e)
                {
                    result.SuccessCode = -1;
                    result.Error       = new Exception($"{e.Message}\n{e.StackTrace}");

                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    result.SuccessCode = -1;
                    result.Error       = new Exception(response.ReasonPhrase);

                    return; 
                }

                // read the string content 

                try
                {
                    html = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    result.SuccessCode = -1;
                    result.Error       = e;
                }
            });

            if (result.Error != null)
            {
                return result;
            }

            // load the HTML document
            // and count the words

            var document = new HtmlDocument();
            document.LoadHtml(html);

            try
            {
                await CountWordsAsync(document, rootNode, endName);
            }
            catch (Exception e)
            {
                result.SuccessCode = -1;
                result.Error       = e;
            }

            if (result.Error != null)
            {
                return result;
            }

            result.Words = GetMostFrequentWords();

            return result;
        }

        /// <summary>
        /// Traverses the input <see cref="HtmlDocument"/> counting all unique
        /// word occurrences in the History section.
        /// </summary>
        /// 
        /// <param name="document">The <see cref="HtmlDocument"/> to crawl.</param>
        /// <param name="rootNode">The <see cref="HtmlNode"/> root node to start crawl at.
        /// USED PRIMARILY FOR TESTING.</param>
        /// <param name="endName">The <see cref="string"/> name of the <see cref="HtmlNode"/>
        /// to terminate on. USED PRIMARILY FOR TESTING.</param>
        /// 
        /// <returns>The <see cref="Task{int}"/> success code.</returns>
        private async Task<int> CountWordsAsync(
            HtmlDocument document, 
            HtmlNode     rootNode = null, 
            string       endName  = "h2")
        {
            // get the root node of History Section

            if (rootNode == null)
            {
                rootNode = document.GetElementbyId("History").ParentNode;
            }

            // traverse every sibling and their descendents until we 
            // reach the end of the History section

            do
            {
                TraverseDescendents(rootNode);
                rootNode = rootNode.NextSibling;
            } while (rootNode != null && rootNode.Name != endName);
            
            return await Task.FromResult<int>(0);
        }

        /// <summary>
        /// Does a DFS traversal on a <see cref="HtmlNode"/>'s children and scrapes
        /// the element's text.
        /// </summary>
        /// 
        /// <param name="node">The <see cref="HtmlNode"/> to traverse.</param>
        private void TraverseDescendents(HtmlNode node)
        {
            var stack = new Stack<HtmlNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                // check if it is a text node

                var curr = stack.Pop();
                if (curr.NodeType == HtmlNodeType.Text)
                {
                    ParseText(curr.InnerText);
                    continue;
                }

                // if the node is crawlable

                if (curr.ShouldCrawl())
                {
                    // add visited and traverse
                    // its children

                    curr.AddClass(VISITED_KEY);
                    foreach (var child in curr.ChildNodes)
                    {
                        switch (child.NodeType)
                        {
                            case HtmlNodeType.Comment:
                                continue;
                            case HtmlNodeType.Document:
                                throw new Exception("Document cannot be child of HtmlNode");
                            case HtmlNodeType.Text:
                                ParseText(child.InnerText);
                                continue;
                            case HtmlNodeType.Element:
                                if (!child.ContainsClass(VISITED_KEY) && child.ShouldCrawl())
                                {
                                    stack.Push(child);
                                }
                                continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a <see cref="string"/> text block and adds the 
        /// the valid words to <see cref="wordDict"/>.
        /// </summary>
        /// 
        /// <param name="text">The <see cref="string"/> text block to parse.</param>
        private void ParseText(string text)
        {
            // clean text
            // split by spaces

            var words = text.Clean().TokenizeToList();
            foreach (var word in words)
            {
                // check if excluded and 
                // a valid word

                if (excludedWords.Contains(word) || !word.IsValidWord())
                {
                    continue;
                }

                PersistWord(word);
            }
        }

        /// <summary>
        /// Responsible for actually persisting the word counts in memory.
        /// </summary>
        /// 
        /// <param name="word">The <see cref="string"/> word to persist.</param>
        private void PersistWord(string word)
        {
            if (!wordDict.ContainsKey(word))
            {
                wordDict[word] = 0;
            }

            wordDict[word] += 1;
            var priority   = wordDict[word];

            if (priorityQueue.Contains(word))
            {
                if (!priorityQueue.TryUpdatePriority(word, priority))
                {
                    throw new Exception($"Could not update priority for item: {word}.");
                }
            }
            else
            {
                if (priorityQueue.TryFirst(out var first))
                {
                    if (priority >= wordDict[first])
                    {
                        priorityQueue.Enqueue(word, priority);
                    }
                }
                else
                {
                    priorityQueue.Enqueue(word, priority);
                }

                if (priorityQueue.Count > wordLimit)
                {
                    if (priorityQueue.TryDequeue(out var head))
                    {
                        Log.Logger.Debug($"Successfully removed item {head} with priority {wordDict[head]} from pirority queue.");
                    }
                    else
                    {
                        Log.Logger.Debug($"Cannot remove head from empty priority queue.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the most frequent words from <see cref="priorityQueue"/>.
        /// </summary>
        /// 
        /// <returns>A <see cref="Dictionary{string, int}"/> containing the most frequently
        /// seen words.</returns>
        public Dictionary<string, int> GetMostFrequentWords()
        {
            var items = new Dictionary<string, int>(capacity: wordLimit);
            foreach (var item in priorityQueue)
            {
                items[item] = wordDict[item];
            }

            return items;
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
