// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Music.StreamingServices
{
    /// <summary>
    /// Defines a state for the track loading.
    /// </summary>
    public enum TrackLoadingState
    {
        /// <summary>
        /// Track hasn't been loaded yet.
        /// </summary>
        None,

        /// <summary>
        /// Track already exists in local library.
        /// </summary>
        Exists,

        /// <summary>
        /// Track name is marked as ignored.
        /// </summary>
        Ignored,
    }

    /// <summary>
    /// Represents an asynchronous track details loader.
    /// </summary>
    /// <param name="provider">An instance of the <see cref="IMediaProvider"/> to get tracks from.</param>
    internal class TracksAsyncLoader(IMediaProvider provider)
    {
        /// <summary>
        /// Gets the current loaded tracks list.
        /// </summary>
        public ObservableCollection<TrackDetails> LoadedTracks { get; } = [];

        /// <summary>
        /// Scans for the tracks with the specified scanning selector.
        /// </summary>
        /// <param name="categorySelector">Asynchronous method that selects tracks from <see cref="IMediaProvider"/>.</param>
        /// <param name="token">Token to cancel the search.</param>
        /// <returns>A task instance for this operation.</returns>
        public async Task ScanAsync(Func<IMediaProvider, IAsyncEnumerable<TrackDetails>> categorySelector, CancellationToken token = default)
        {
            LoadedTracks.Clear();
            await foreach (var track in categorySelector(provider))
            {
                token.ThrowIfCancellationRequested();
                LoadedTracks.Add(track);
            }
        }
    }
}