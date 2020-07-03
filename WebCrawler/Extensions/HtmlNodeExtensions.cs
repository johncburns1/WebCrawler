//--------------------
// FILE:   HtmlNodeExtensions.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Text;

using HtmlAgilityPack;

namespace WebCrawler
{
    /// <summary>
    /// Extension methods for <see cref="HtmlNode"/>.
    /// </summary>
    public static class HtmlNodeExtensions
    {
        /// <summary>
        /// Determines if an <see cref="HtmlNode"/> should be crawled.
        /// </summary>
        /// 
        /// <param name="node">The <see cref="HtmlNode"/> to check for validity.</param>
        /// 
        /// <returns><see cref="bool"/> is valid.</returns>
        public static bool ShouldCrawl(this HtmlNode node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.NodeType == HtmlNodeType.Comment)
            {
                return false;
            }

            if ((node.Name == "sup")
                || (node.Name == "div" && node.GetAttributeValue("class", "").Equals("thumb tleft"))
                || (node.Name == "div" && node.GetAttributeValue("class", "").Equals("thumb tright")))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the input <see cref="HtmlNode"/> contains a specified
        /// class attribute.
        /// </summary>
        /// 
        /// <param name="node">The <see cref="HtmlNode"/> to check for class.</param>
        /// <param name="classStr">The <see cref="string"/> class to check for.</param>
        /// 
        /// <returns><see cref="bool"/> the class exists in the <see cref="HtmlNode"/> attributes.</returns>
        public static bool ContainsClass(this HtmlNode node, string classStr)
        {
            if (node == null)
            {
                return false;
            }

            return node.GetAttributeValue("class", "").Contains(classStr);
        }
    }
}
