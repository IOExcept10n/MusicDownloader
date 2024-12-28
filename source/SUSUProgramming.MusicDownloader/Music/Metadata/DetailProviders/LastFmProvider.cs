// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using Microsoft.Extensions.Configuration;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a track details provider for Last.FM.
    /// </summary>
    /// <param name="config">Initial app configuration to get access tokens and API keys.</param>
    /// <param name="api">API service instance.</param>
    internal class LastFmProvider(IConfiguration config, ApiHelper api) : TrackDetailsProvider(api, "https://last.fm/api/")
    {
        private readonly LastfmClient client = new(config.GetSection("Providers:LastFM")["ApiKey"], config.GetSection("Providers:LastFM")["SharedSecret"], api.Client);

        /// <inheritdoc/>
        public override async Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype)
        {
            string? title = prototype.FormedTitle;
            if (title == TrackDetails.UnknownTitle)
                return null;
            string artist = prototype.FormedArtistString;
            if (artist == TrackDetails.UnknownArtist)
                return null;
            var info = await client.Track.GetInfoAsync(title, artist);
            if (info.Success)
            {
                var track = info.Content;
                DateTimeOffset? releaseDate = null;
                uint? trackNumber = null, trackCount = null;
                Uri? image = null;
                if (artist != null && track.AlbumName != null)
                {
                    try
                    {
                        var albumInfo = await client.Album.GetInfoAsync(artist, track.AlbumName, true);
                        if (albumInfo.Success)
                        {
                            var album = albumInfo.Content;
                            image = album.Images.Largest;
                            releaseDate = album.ReleaseDateUtc;
                            trackNumber = (uint)(album.Tracks.IndexOfFirst(x => x.Name == title) + 1);
                            trackCount = (uint)album.Tracks.CountOrDefault();
                        }
                    }
                    catch
                    {
                    }
                }

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
                return scanResult;
            }

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
                    return null;
                string? title = details.FormedTitle;
                if (title == TrackDetails.UnknownTitle)
                    return null;
                string artist = details.FormedArtistString;
                if (artist == TrackDetails.UnknownArtist)
                    return null;
                var info = await client.Track.GetInfoAsync(title, artist);
                if (info.Success)
                {
                    var track = info.Content;
                    return track.PlayCount;
                }

                return -1;
            }
            catch
            {
                return null;
            }
        }
    }
}