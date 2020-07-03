//--------------------
// FILE:   CrawlerResult.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler
{
    /// <summary>
    /// Holds information about the result of a web crawl.
    /// </summary>
    public class CrawlerResult
    {
        /// <summary>
        /// The <see cref="int"/> success code of the crawl.  This indicates
        /// the success or failure of a crawler's execution.
        /// </summary>
        public int SuccessCode { get; set; } = 0;

        /// <summary>
        /// Any <see cref="Exception"/> thrown during crawler execution that
        /// may have caused a failure.
        /// </summary>
        public Exception Error { get; set; } = null;

        /// <summary>
        /// The <see cref="List{KeyValuePair{string, int}}"/> containing the n most common
        /// words found during the web crawl.
        /// </summary>
        public Dictionary<string, int> Words { get; set; } = null;
    }
}
