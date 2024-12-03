// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;
using VkNet;
using VkNet.Utils;

namespace SUSUProgramming.MusicDownloader.Music.StreamingServices.Vk
{
    /// <summary>
    /// Represents a media provider for VK.
    /// </summary>
    /// <param name="api">An instance of the <see cref="VkApi"/> to access.</param>
    /// <param name="oauth">An instance of the <see cref="VkOAuthService"/> to authorize.</param>
    /// <param name="apiHelper">An instance of the API service for internal calls.</param>
    internal class VkMediaProvider(VkApi api, VkOAuthService oauth, ApiHelper apiHelper) : IMediaProvider
    {
        /// <inheritdoc/>
        public bool Authorized => api.IsAuthorized;

        /// <inheritdoc/>
        public IOAuthService AuthService => oauth;

        /// <inheritdoc/>
        public string Name => "VK";

        /// <inheritdoc/>
        public async Task<TrackDetails?> DownloadTrackAsync(TrackDetails track, string downloadDirectoryPath)
        {
            var response = await apiHelper.Client.GetAsync(track.TrackUri);
            if (response.IsSuccessStatusCode)
            {
                string filename = track.FormedTrackName + ".mp3";
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    filename = filename.Replace(c, '_');
                }

                string path = Path.Combine(downloadDirectoryPath, filename);
                using (var file = File.Create(path))
                {
                    await response.Content.CopyToAsync(file);
                }

                return MetadataManager.ReadMetadata(path);
            }

            return null;
        }

        /// <summary>
        /// Gets info about the specified <see cref="VkNet.Model.Audio"/> track.
        /// </summary>
        /// <param name="track">Track to get details for.</param>
        /// <returns>Task with info about tracks.</returns>
        public async Task<TrackDetails> GetDetailsAsync(VkNet.Model.Audio track)
        {
            VkCollection<VkNet.Model.Audio>? album = null;
            uint position = 0;
            Uri? cover = null;
            if (track.Album != null)
            {
                album = await api.Audio.GetAsync(new()
                {
                    PlaylistId = track.Album.Id,
                });
                position = (uint)album.IndexOfFirst(x => x.Id == track.Id);
                cover = track.Album.GetBestQualityCover();
            }

            return [
                Tags.Title + track.Title,
                Tags.Album + track.Album?.Title,
                Tags.Performers + TrackNameParser.GetPerformers(track.Artist),
                Tags.Track + position,
                Tags.TrackCount + (uint)(album?.Count ?? 0),
                Tags.Genres + [track.Genre?.ToString() ?? "Unknown"],
                VirtualTags.TrackUri + track.Url,
                cover != null ? await CoverTag.DownloadCoverAsync(cover, apiHelper.Client) : null,
            ];
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<TrackDetails> ListUserRecommendationsAsync()
        {
            var recommendations = await api.Audio.GetRecommendationsAsync();
            foreach (var recommendation in recommendations)
                yield return await GetDetailsAsync(recommendation);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<TrackDetails> ListUserTracksAsync()
        {
            var tracks = await api.Audio.GetAsync(new());
            foreach (var track in tracks)
                yield return await GetDetailsAsync(track);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<TrackDetails> SearchAsync(string query)
        {
            var searchResults = await api.Audio.SearchAsync(new()
            {
                Autocomplete = true,
                Query = query,
            });
            foreach (var track in searchResults)
                yield return await GetDetailsAsync(track);
        }
    }
}