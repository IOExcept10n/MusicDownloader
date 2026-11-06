// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a base class for providers that only look for a cover by artist and title, so they work for singles without an album.
    /// </summary>
    /// <param name="api">An instance of the API helper to use.</param>
    /// <param name="url">Base address of the service.</param>
    /// <param name="logger">The logger to use for logging operations.</param>
    internal abstract class CoverSearchProvider(ApiHelper api, string url, ILogger logger) : TrackDetailsProvider(api, url)
    {
        /// <inheritdoc/>
        public override async Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype)
        {
            string title = prototype.FormedTitle;
            string artist = prototype.FormedArtistString;
            if (title == TrackDetails.UnknownTitle || artist == TrackDetails.UnknownArtist)
                return null;

            try
            {
                string json = await HttpClient.GetStringAsync(BuildSearchUri(Clean(title), Clean(artist)));
                var coverUri = ParseCover(json, title, artist);
                if (coverUri == null)
                {
                    logger.LogDebug("{Provider} found no cover for {Title} by {Artist}", GetType().Name, title, artist);
                    return null;
                }

                var cover = await ID3.CoverTag.DownloadCoverAsync(coverUri, HttpClient);
                return cover == null ? null : [cover];
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Provider} lookup failed for {Title} by {Artist}", GetType().Name, title, artist);
                return null;
            }
        }

        /// <summary>
        /// Builds the search request address.
        /// </summary>
        /// <param name="title">Track title without quotes.</param>
        /// <param name="artist">Artist string without quotes.</param>
        /// <returns>Address to request.</returns>
        protected abstract Uri BuildSearchUri(string title, string artist);

        /// <summary>
        /// Reads the cover address of the matching track from the response.
        /// </summary>
        /// <param name="json">Response body.</param>
        /// <param name="title">Title of the track to look for.</param>
        /// <param name="artist">Artist string of the track to look for.</param>
        /// <returns>The cover address or <see langword="null"/>.</returns>
        protected abstract Uri? ParseCover(string json, string title, string artist);

        private static string Clean(string value) => value.Replace('"', ' ');
    }
}
