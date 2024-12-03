// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Logging;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.LyricsProviders
{
    /// <summary>
    /// Represents a lyrics provider instance for the LrcLib API.
    /// </summary>
    /// <param name="api">An instance of the service for API handling.</param>
    internal class LrcLibProvider(ApiHelper api) : LyricsProvider(api, "https://lrclib.net")
    {
        private const string Endpoint = "api/search";

        private const string TrackNameQueryKey = "track_name";
        private const string ArtistNameQueryKey = "artist_name";

        /// <inheritdoc/>
        public override async Task<string?> SearchLyricsAsync(TrackDetails details)
        {
            try
            {
                var apiCall = Api.BuildRequest(BaseUrl)
                                  .WithEndpoint(Endpoint)
                                  .WithParams(TrackNameQueryKey, details.FormedTitle);
                if (details.HasArtists)
                    apiCall.WithParams(ArtistNameQueryKey, details.FormedArtistString);
                var response = await apiCall.CallAsync();
                var tracks = from dynamic track in response
                             let trackName = $"{track.artistName} - {track.trackName}"
                             orderby details.FormedTrackName.CompareStrings(trackName) descending
                             let lyrics = (track.syncedLyrics ?? track.plainLyrics)?.ToString()?.Trim()
                             where !string.IsNullOrEmpty(lyrics)
                             select new
                             {
                                 track.artistName,
                                 track.trackName,
                                 lyrics,
                             };
                var topTrack = tracks.FirstOrDefault();
                if (topTrack?.lyrics != null)
                    return topTrack.lyrics;
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Warning, Category)
                    .GetValueOrDefault()
                    .Log(this, "Can't load the lyrics from the following provider. Unhandled exception: {exception}", ex);
            }

            return null;
        }
    }
}