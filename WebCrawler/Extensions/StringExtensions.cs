//--------------------
// FILE:   StringExtensions.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebCrawler
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Strips a <see cref="string"/> of all non alpha-numeric values and "-".
        /// </summary>
        /// 
        /// <param name="value">The <see cref="string"/> to clean.</param>
        /// <param name="trim"><see cref="bool"/> flag indicating if the resulting
        /// <see cref="string"/> should be trimmed of whitespace.</param>
        /// 
        /// <returns>The cleaned <see cref="string"/> with only alpha-numeric values and "-".</returns>
        public static string Clean(this string value, bool trim = false)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "";
            }

            // remove terminating characters

            value = value.Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("\r", " ")
                .Replace("-", " ")
                .Replace("'s", "");

            if (trim)
            {
                value = value.Trim();
            }

            var arr = value.ToCharArray();
            arr     = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))));

            return new string(arr);
        }

        /// <summary>
        /// Tokenizes a string into a <see cref="List{string}"/> separated by a delimeter.
        /// </summary>
        /// 
        /// <param name="value">The <see cref="string"/> to tokenize.</param>
        /// <param name="separator">The <see cref="string"/> token separator.</param>
        /// 
        /// <returns>The resulting <see cref="List{string}"/> of strings from the input <see cref="string"/>,
        /// separated by the specified <see cref="string"/> separator.</returns>
        public static List<string> TokenizeToList(this string value, string separator = " ")
        {
            if (String.IsNullOrEmpty(value))
            {
                return new List<string>();
            }

            if (String.IsNullOrEmpty(separator))
            {
                return new List<string> { value };
            }

            return value.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        }

        /// <summary>
        /// Determines if a <see cref="string"/> word is valid.  A valid word
        /// does not contain numbers or symbols and is greater than length 1
        /// (exception is "a", "I",).
        /// </summary>
        /// 
        /// <param name="value">The <see cref="string"/> to check for validity.</param>
        /// 
        /// <returns><see cref="bool"/> is valid.</returns>
        public static bool IsValidWord(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var c in value)
            {
                if (!char.IsLetter(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
