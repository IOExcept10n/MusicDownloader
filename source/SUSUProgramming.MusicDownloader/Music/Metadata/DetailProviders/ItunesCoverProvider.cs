// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a cover provider based on the iTunes Search API.
    /// </summary>
    /// <param name="api">An instance of the API helper to use.</param>
    /// <param name="logger">The logger to use for logging operations.</param>
    internal class ItunesCoverProvider(ApiHelper api, ILogger<ItunesCoverProvider> logger) : CoverSearchProvider(api, "https://itunes.apple.com/", logger)
    {
        /// <inheritdoc/>
        protected override Uri BuildSearchUri(string title, string artist) =>
            new(BaseUrl, $"search?term={Uri.EscapeDataString(artist + " " + title)}&entity=song&limit=5");

        /// <inheritdoc/>
        protected override Uri? ParseCover(string json, string title, string artist) =>
            CoverSearchParsers.ParseItunes(json, title, artist);
    }
}
