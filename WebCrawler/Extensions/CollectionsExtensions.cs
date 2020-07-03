//--------------------
// FILE:   DictionaryExtensions.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebCrawler
{
    /// <summary>
    /// Extension methods for collection types.
    /// </summary>
    public static class CollectionsExtensions
    {
        /// <summary>
        /// Converts a <see cref="Dictionary{string, int}"/> to a human readable
        /// <see cref="string"/>.
        /// </summary>
        /// 
        /// <param name="value">The <see cref="Dictionary{string, int}"/> to convert
        /// to a human readable <see cref="string"/>.</param>
        /// 
        /// <returns>The resulting <see cref="string"/>.</returns>
        public static string ToPrettyString(this Dictionary<string, int> value)
        {
            if (value == null || value.Count == 0)
            {
                return $"{{ }}";
            }
            var sorted = value.OrderByDescending(p => p.Value);
            return $"{string.Join(Environment.NewLine, sorted)}";
        }

        /// <summary>
        /// Converts a <see cref="List{string}"/> to a human readable <see cref="string"/>.
        /// </summary>
        /// 
        /// <param name="value">The <see cref="List{string}"/> to convert
        /// to a human readable <see cref="string"/>.</param>
        /// 
        /// <returns>The resulting <see cref="string"/>.</returns>
        public static string ToPrettyString(this List<string> value)
        {
            if (value == null || value.Count == 0)
            {
                return $"{{ }}";
            }

            return string.Join(",", value.ToArray());
        }
    }
}
