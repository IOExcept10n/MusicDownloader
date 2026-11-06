// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Compares track titles and artists coming from different sources.
    /// </summary>
    internal static partial class TrackTitleMatcher
    {
        private static readonly Regex DecorationRegex = GetDecorationRegex();
        private static readonly Regex NonWordRegex = GetNonWordRegex();
        private static readonly Regex ArtistSeparatorRegex = GetArtistSeparatorRegex();

        /// <summary>
        /// Brings a title to a comparable form: lower case, no featuring/remaster/live suffixes in brackets, no punctuation.
        /// </summary>
        /// <param name="s">String to normalize.</param>
        /// <returns>Normalized string.</returns>
        public static string Normalize(string s)
        {
            string withoutDecorations = DecorationRegex.Replace(s.ToLowerInvariant(), " ");
            string words = NonWordRegex.Replace(withoutDecorations, " ");
            return string.Join(' ', words.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Determines whether two descriptions refer to the same track.
        /// </summary>
        /// <param name="titleA">Title from the first source.</param>
        /// <param name="artistA">Artist string from the first source.</param>
        /// <param name="titleB">Title from the second source.</param>
        /// <param name="artistB">Artist string from the second source.</param>
        /// <returns><see langword="true"/> if titles are equal after normalization and the artists share at least one name.</returns>
        public static bool IsSameTrack(string titleA, string artistA, string titleB, string artistB)
        {
            string normalizedTitle = Normalize(titleA);
            if (normalizedTitle.Length == 0 || normalizedTitle != Normalize(titleB))
                return false;

            return SplitArtists(artistA).Intersect(SplitArtists(artistB)).Any();
        }

        /// <summary>
        /// Finds the position of the title in the list ignoring case and decorations.
        /// </summary>
        /// <param name="titles">Titles to search in.</param>
        /// <param name="title">Title to find.</param>
        /// <returns>Zero-based index of the first match or <c>-1</c> if there is none.</returns>
        public static int IndexOfTitle(IEnumerable<string> titles, string title)
        {
            string normalized = Normalize(title);
            int index = 0;
            foreach (string candidate in titles)
            {
                if (Normalize(candidate) == normalized)
                    return index;
                index++;
            }

            return -1;
        }

        private static IEnumerable<string> SplitArtists(string artists) =>
            ArtistSeparatorRegex.Split(artists.ToLowerInvariant()).Select(Normalize).Where(x => x.Length > 0);

        [GeneratedRegex(@"[\(\[][^\)\]]*\b(feat|ft|featuring|with|remaster|remastered|live)\b[^\)\]]*[\)\]]")]
        private static partial Regex GetDecorationRegex();

        [GeneratedRegex(@"[^\p{L}\p{N}]+")]
        private static partial Regex GetNonWordRegex();

        [GeneratedRegex(@"[,;/&]|\b(?:feat|ft)\b\.?")]
        private static partial Regex GetArtistSeparatorRegex();
    }
}
