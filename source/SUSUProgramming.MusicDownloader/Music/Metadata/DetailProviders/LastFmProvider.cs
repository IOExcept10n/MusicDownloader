// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a track details provider for Last.FM.
    /// </summary>
    /// <param name="tokens">Tokens storage instance to get access tokens and API keys.</param>
    /// <param name="api">API service instance.</param>
    /// <param name="logger">The logger to use for logging operations.</param>
    internal class LastFmProvider(TokenStorage tokens, ApiHelper api, ILogger<LastFmProvider> logger) : TrackDetailsProvider(api, "https://last.fm/api/")
    {
        private readonly LastfmClient client = new(tokens.GetToken("LastFM"), tokens.GetSharedSecret("LastFM"), api.Client);
        private readonly ConcurrentDictionary<(string Artist, string Name), int?> listenCountCache = [];
        private readonly ILogger<LastFmProvider> logger = logger;

        /// <inheritdoc/>
        public override async Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype)
        {
            string? title = prototype.FormedTitle;
            if (title == TrackDetails.UnknownTitle)
            {
                logger.LogDebug("Track has unknown title, skipping Last.FM search");
                return null;
            }

            string artist = prototype.FormedArtistString;
            if (artist == TrackDetails.UnknownArtist)
            {
                logger.LogDebug("Track has unknown artist, skipping Last.FM search");
                return null;
            }

            logger.LogInformation("Searching Last.FM for track: {Title} by {Artist}", title, artist);
            var info = await client.Track.GetInfoAsync(title, artist);
            if (info.Success)
            {
                logger.LogDebug("Found track on Last.FM");
                var track = info.Content;
                DateTimeOffset? releaseDate = null;
                uint? trackNumber = null, trackCount = null;
                Uri? image = null;
                if (artist != null && track.AlbumName != null)
                {
                    try
                    {
                        logger.LogDebug("Fetching album info for: {AlbumName}", track.AlbumName);
                        var albumInfo = await client.Album.GetInfoAsync(artist, track.AlbumName, true);
                        if (albumInfo.Success)
                        {
                            var album = albumInfo.Content;
                            image = album.Images.Largest;
                            releaseDate = album.ReleaseDateUtc;
                            trackNumber = (uint)(album.Tracks.IndexOfFirst(x => x.Name == title) + 1);
                            trackCount = (uint)album.Tracks.CountOrDefault();
                            logger.LogDebug(
                                "Found album info: Release date: {ReleaseDate}, Track number: {TrackNumber}, Total tracks: {TrackCount}",
                                releaseDate,
                                trackNumber,
                                trackCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to fetch album info for: {AlbumName}", track.AlbumName);
                    }
                }

                logger.LogDebug("Creating track details with {TagCount} tags", 8);
                TrackDetails scanResult = [
                    Tags.Performers + TrackNameParser.GetPerformers(track.ArtistName),
                    Tags.Title + track.Name,
                    Tags.Album + track.AlbumName,
                    Tags.Genres + [.. (from tag in track.TopTags select tag.Name.ToPascalCase()).Take(3)],
                    Tags.Track + trackNumber.GetValueOrDefault(),
                    Tags.TrackCount + trackCount.GetValueOrDefault(),
                    Tags.Year + (uint)(releaseDate?.Year).GetValueOrDefault(),
                    image != null ? await CoverTag.DownloadCoverAsync(image, HttpClient) : null,
                ];
                logger.LogInformation("Successfully retrieved track details from Last.FM");
                return scanResult;
            }

            logger.LogWarning("Track not found on Last.FM: {Title} by {Artist}", title, artist);
            return null;
        }

        /// <summary>
        /// Gets count of track listeners if available.
        /// </summary>
        /// <param name="details">Track details to search for listeners count.</param>
        /// <returns>Count of listeners, <see langword="-1"/> if track is not found or <see langword="null"/> if API is unavailable.</returns>
        public async Task<int?> GetListensCountAsync(TrackDetails details)
        {
            try
            {
                if (client.Auth.ApiKey == null)
                {
                    logger.LogWarning("Last.FM API key is not set");
                    return null;
                }

                string? title = details.FormedTitle;
                if (title == TrackDetails.UnknownTitle)
                {
                    logger.LogDebug("Track has unknown title, cannot get listener count");
                    return null;
                }

                string artist = details.FormedArtistString;
                if (artist == TrackDetails.UnknownArtist)
                {
                    logger.LogDebug("Track has unknown artist, cannot get listener count");
                    return null;
                }

                if (listenCountCache.TryGetValue((artist, title), out int? listens))
                {
                    logger.LogTrace("Using cached listener count for: {Title} by {Artist}", title, artist);
                    return listens;
                }

                logger.LogDebug("Fetching listener count for: {Title} by {Artist}", title, artist);
                var info = await client.Track.GetInfoAsync(title, artist);
                if (info.Success)
                {
                    var track = info.Content;
                    var count = track.PlayCount;
                    listenCountCache[(artist, title)] = count;
                    logger.LogDebug("Found {ListenerCount} listeners for: {Title} by {Artist}", count, title, artist);
                    return count;
                }

                logger.LogWarning("Track not found on Last.FM: {Title} by {Artist}", title, artist);
                listenCountCache[(artist, title)] = null;
                return -1;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting listener count from Last.FM");
                return null;
            }
        }
    }
}