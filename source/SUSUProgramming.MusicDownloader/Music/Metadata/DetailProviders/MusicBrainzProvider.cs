// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a track details provider for MusicBrainz with covers from the Cover Art Archive.
    /// </summary>
    /// <param name="api">An instance of the API helper to use.</param>
    /// <param name="logger">The logger to use for logging operations.</param>
    internal class MusicBrainzProvider(ApiHelper api, ILogger<MusicBrainzProvider> logger) : TrackDetailsProvider(api, "https://musicbrainz.org/ws/2/")
    {
        private const string UserAgent = "MusicDownloader/1.0 ( https://github.com/IOExcept10n/MusicDownloader )";

        // MusicBrainz asks clients to make at most one request per second.
        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static DateTime lastRequestUtc = DateTime.MinValue;

        /// <inheritdoc/>
        public override async Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype)
        {
            string title = prototype.FormedTitle;
            string artist = prototype.FormedArtistString;
            if (title == TrackDetails.UnknownTitle || artist == TrackDetails.UnknownArtist)
            {
                logger.LogDebug("Track has unknown title or artist, skipping MusicBrainz search");
                return null;
            }

            try
            {
                string query = $"recording:\"{Clean(title)}\" AND artist:\"{Clean(artist)}\"";
                Uri uri = new(BaseUrl, $"recording?query={Uri.EscapeDataString(query)}&fmt=json&limit=5");
                string? json = await GetJsonAsync(uri);
                var match = json == null ? null : MusicBrainzParser.Parse(json, title, artist);
                if (match == null)
                {
                    logger.LogDebug("Track not found on MusicBrainz: {Title} by {Artist}", title, artist);
                    return null;
                }

                CoverTag? cover = null;
                if (match.ReleaseId != null)
                    cover = await CoverTag.DownloadCoverAsync(new($"https://coverartarchive.org/release/{match.ReleaseId}/front-1200"), HttpClient);

                return [
                    Tags.Title + match.Title,
                    Tags.Performers + match.Artists,
                    Tags.Album + match.Album,
                    Tags.Year + match.Year,
                    Tags.Track + match.Track,
                    Tags.TrackCount + match.TrackCount,
                    cover,
                ];
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MusicBrainz lookup failed for {Title} by {Artist}", title, artist);
                return null;
            }
        }

        private static string Clean(string value) => value.Replace('"', ' ');

        private async Task<string?> GetJsonAsync(Uri uri)
        {
            await Gate.WaitAsync();
            try
            {
                var wait = TimeSpan.FromSeconds(1) - (DateTime.UtcNow - lastRequestUtc);
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait);

                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.UserAgent.ParseAdd(UserAgent);
                using var response = await HttpClient.SendAsync(request);
                lastRequestUtc = DateTime.UtcNow;
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("MusicBrainz returned {StatusCode}", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            finally
            {
                Gate.Release();
            }
        }
    }
}
