// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents a base class for the track details search services.
    /// </summary>
    /// <param name="api">An API service that helps with raw API calls.</param>
    /// <param name="url">A base <see cref="System.Uri"/> for the API provider.</param>
    internal abstract class TrackDetailsProvider(ApiHelper api, string url) : MetadataProvider(api, url), IDetailProvider
    {
        /// <inheritdoc/>
        public abstract Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype);
    }
}