// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents a base class for the lyrics search.
    /// </summary>
    /// <param name="api">API service instance that helps with raw API calls.</param>
    /// <param name="url">Base <see cref="System.Uri"/> of the provider API.</param>
    internal abstract class LyricsProvider(ApiHelper api, string url) : MetadataProvider(api, url), ILyricsProvider
    {
        /// <inheritdoc/>
        public abstract Task<string?> SearchLyricsAsync(TrackDetails details);
    }
}