// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Diagnostics;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Opens web searches in the default browser for things the app could not find on its own (lyrics, covers).
    /// </summary>
    internal static class WebSearchLauncher
    {
        /// <summary>
        /// Builds a web search address for the lyrics of the track.
        /// </summary>
        /// <param name="artist">Artist string of the track.</param>
        /// <param name="title">Title of the track.</param>
        /// <returns>Search address.</returns>
        public static Uri LyricsQuery(string artist, string title) =>
            new($"https://www.google.com/search?q={Uri.EscapeDataString($"{artist} {title} текст песни")}");

        /// <summary>
        /// Builds an image search address for the cover of the track.
        /// </summary>
        /// <param name="artist">Artist string of the track.</param>
        /// <param name="title">Title of the track.</param>
        /// <returns>Search address.</returns>
        public static Uri CoverQuery(string artist, string title) =>
            new($"https://www.google.com/search?tbm=isch&q={Uri.EscapeDataString($"{artist} {title} обложка")}");

        /// <summary>
        /// Opens the address in the default browser. Only web addresses are allowed.
        /// </summary>
        /// <param name="uri">Address to open.</param>
        /// <exception cref="ArgumentException">The address is not an http or https one.</exception>
        public static void Open(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Only http and https addresses can be opened.", nameof(uri));

            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
    }
}
