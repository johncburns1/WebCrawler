//--------------------
// FILE:   HttpHelper.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler
{
    /// <summary>
    /// Defines static helper methods for configuration and execution
    /// of the http client.
    /// </summary>
    public static class HttpSettings
    {
        /// <summary>
        /// The <see cref="int"/> number of retries for http client calls.
        /// </summary>
        public static readonly int MaxRetryAttempts = 3;

        /// <summary>
        /// The <see cref="TimeSpan"/> in seconds between retry attempts.
        /// </summary>
        public static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The <see cref="string"/> URI to crawl.
        /// </summary>
        public static readonly string BaseAddress = "https://en.wikipedia.org/wiki/Microsoft";
    }
}
