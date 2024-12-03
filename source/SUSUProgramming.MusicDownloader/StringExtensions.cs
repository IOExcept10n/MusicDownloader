// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Text;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;

namespace SUSUProgramming.MusicDownloader
{
    /// <summary>
    /// Provides extension methods for the <see cref="string"/> class to enhance string manipulation capabilities.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Compares two strings using a specified similarity metric.
        /// If no metric is provided, the Jaro-Winkler similarity metric is used by default.
        /// </summary>
        /// <param name="str1">The first string to compare.</param>
        /// <param name="str2">The second string to compare.</param>
        /// <param name="metric">An optional similarity metric to use for comparison. If null, Jaro-Winkler is used.</param>
        /// <returns>A double representing the similarity score between the two strings.</returns>
        public static double CompareStrings(this string str1, string str2, INormalizedStringSimilarity? metric = null)
        {
            metric ??= new JaroWinkler();
            return metric.Similarity(str1, str2);
        }

        /// <summary>
        /// Capitalizes the first letter of each sentence in the string.
        /// Uses a fast method for shorter strings and a slower method for longer strings.
        /// </summary>
        /// <param name="str">The string to capitalize.</param>
        /// <returns>A new string with the first letter of each sentence capitalized.</returns>
        public static string Capitalize(this string str)
        {
            if (str.Length < Environment.SystemPageSize / 2)
                return CapitalizeFast(str);
            return CapitalizeSlow(str);
        }

        /// <summary>
        /// Converts the string to Pascal case, where the first letter of each word is capitalized.
        /// Uses a fast method for shorter strings and a slower method for longer strings.
        /// </summary>
        /// <param name="str">The string to convert to Pascal case.</param>
        /// <returns>A new string in Pascal case.</returns>
        public static string ToPascalCase(this string str)
        {
            if (str.Length < Environment.SystemPageSize / 2)
                return ToPascalFast(str);
            return ToPascalSlow(str);
        }

        /// <summary>
        /// A fast implementation of the Capitalize method for shorter strings.
        /// </summary>
        /// <param name="str">The string to capitalize.</param>
        /// <returns>A new string with the first letter of each sentence capitalized.</returns>
        private static string CapitalizeFast(string str)
        {
            Span<char> span = stackalloc char[str.Length];
            bool capitalize = false;
            int i = 0;
            foreach (char c in str)
            {
                if (capitalize && char.IsLetter(c))
                {
                    span[i] = char.ToUpper(c);
                    capitalize = false;
                }
                else
                {
                    span[i] = c;
                }

                if (c == '.')
                    capitalize = true;
                i++;
            }

            return new string(span);
        }

        /// <summary>
        /// A slower implementation of the Capitalize method for longer strings.
        /// </summary>
        /// <param name="str">The string to capitalize.</param>
        /// <returns>A new string with the first letter of each sentence capitalized.</returns>
        private static string CapitalizeSlow(string str)
        {
            StringBuilder result = new();
            bool capitalize = false;
            foreach (char c in str)
            {
                if (capitalize && char.IsLetter(c))
                {
                    result.Append(char.ToUpper(c));
                    capitalize = false;
                }
                else
                {
                    result.Append(c);
                }

                if (c == '.')
                    capitalize = true;
            }

            return result.ToString();
        }

        /// <summary>
        /// A fast implementation of the ToPascalCase method for shorter strings.
        /// </summary>
        /// <param name="str">The string to convert to Pascal case.</param>
        /// <returns>A new string in Pascal case.</returns>
        private static string ToPascalFast(string str)
        {
            Span<char> span = stackalloc char[str.Length];
            bool capitalize = true;
            int i = 0;
            foreach (char c in str)
            {
                if (capitalize && char.IsLetter(c))
                {
                    span[i] = char.ToUpper(c);
                    capitalize = false;
                }
                else
                {
                    span[i] = c;
                }

                if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
                    capitalize = true;
                i++;
            }

            return new string(span);
        }

        /// <summary>
        /// A slower implementation of the ToPascalCase method for longer strings.
        /// </summary>
        /// <param name="str">The string to convert to Pascal case.</param>
        /// <returns>A new string in Pascal case.</returns>
        private static string ToPascalSlow(string str)
        {
            StringBuilder result = new();
            bool capitalize = true;
            foreach (char c in str)
            {
                if (capitalize && char.IsLetter(c))
                {
                    result.Append(char.ToUpper(c));
                    capitalize = false;
                }
                else
                {
                    result.Append(c);
                }

                if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
                    capitalize = true;
            }

            return result.ToString();
        }
    }
}