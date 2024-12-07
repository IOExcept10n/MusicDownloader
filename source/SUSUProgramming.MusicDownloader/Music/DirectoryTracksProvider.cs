// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Represents a provider for the track details for the specified directory.
    /// </summary>
    internal class DirectoryTracksProvider : IDisposable
    {
        private readonly FileSystemWatcher directoryWatcher = new();
        private readonly DirectoryInfo trackedDirectory;
        private readonly Dictionary<string, TrackDetails> tracks = [];
        private readonly ObservableCollection<TrackDetails> tracksObservable = [];
        private int? lastInc = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryTracksProvider"/> class for the specified directory.
        /// </summary>
        /// <param name="trackedPath">Path to track.</param>
        public DirectoryTracksProvider(string trackedPath)
        {
            trackedDirectory = new DirectoryInfo(trackedPath);
            if (!trackedDirectory.Exists)
                Directory.CreateDirectory(trackedPath);
            directoryWatcher = new FileSystemWatcher(trackedPath);
            directoryWatcher.Error += OnWatchingError;
            directoryWatcher.Created += OnFileUpdate;
            directoryWatcher.Changed += OnFileUpdate;
            directoryWatcher.Deleted += OnFileUpdate;
            directoryWatcher.Renamed += OnFileRename;
            directoryWatcher.Filter = "*.mp3";
            directoryWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Gets or sets the last incremental number among the tracks in the directory.
        /// </summary>
        public int LastIncrementalNumber
        {
            get => lastInc ?? Tracks.Count;
            set => lastInc = value;
        }

        /// <summary>
        /// Gets the path of the tracked directory.
        /// </summary>
        public string Path => trackedDirectory.FullName;

        /// <summary>
        /// Gets the list of tracks in the specified directory.
        /// </summary>
        public IReadOnlyCollection<TrackDetails> Tracks => tracksObservable;

        /// <summary>
        /// Gets a value indicating whether the specified path is inside tracked directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns><see langword="true"/> if the path is inside directory tracked by this instance of the <see cref="DirectoryTracksProvider"/>; otherwise <see langword="false"/>.</returns>
        public bool DirectoryContainsPath(string path)
        {
            return path.Contains(trackedDirectory.FullName);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            directoryWatcher.Dispose();
        }

        /// <summary>
        /// Clears all cache and scans directory for tracks from scratch.
        /// </summary>
        /// <returns>Task to wait.</returns>
        public async Task ScanAsync()
        {
            tracks.Clear();
            tracksObservable.Clear();
            var settings = App.Services.GetRequiredService<AppConfig>();
            foreach (var file in Directory.EnumerateFiles(trackedDirectory.FullName, "*.mp3", SearchOption.AllDirectories))
            {
                if (settings.BlacklistedPaths.Any(file.Contains))
                    continue;
                TrackDetails track = await Task.Run(() => MetadataManager.ReadMetadata(file));
                if (track.TryGetTag(nameof(VirtualTags.IncrementalNumber), out int inc))
                {
                    lastInc ??= inc;
                    lastInc = Math.Max(lastInc.Value, inc);
                }

                tracks.Add(file, track);
                tracksObservable.Add(track);
            }
        }

        /// <summary>
        /// Adds single track info to the specified directory manually.
        /// </summary>
        /// <param name="newTrack">Track to add.</param>
        internal void AddTrack(TrackDetails newTrack)
        {
            if (newTrack.FilePath == null)
                return;
            tracks[newTrack.FilePath] = newTrack;
            tracksObservable.Add(newTrack);
        }

        /// <summary>
        /// Removes the track by its path manually.
        /// </summary>
        /// <param name="oldPath">Path to remove.</param>
        internal void RemoveTrack(string oldPath)
        {
            if (tracks.Remove(oldPath, out var value))
            {
                if (value.TryGetTag(nameof(VirtualTags.IncrementalNumber), out int inc) && inc == lastInc)
                {
                    lastInc--;
                }

                tracksObservable.Remove(value);
            }
        }

        private void OnFileRename(object sender, RenamedEventArgs e)
        {
            var track = tracks[e.OldFullPath];
            if (track.TryGetTag<string>(nameof(TrackDetails.FilePath), out _))
            {
                track.SetTag(nameof(TrackDetails.FilePath), e.FullPath);
            }
            else
            {
                track.Add(VirtualTags.TrackFilePath + e.FullPath);
            }
        }

        private void OnFileUpdate(object sender, FileSystemEventArgs e)
        {
            // HACK: for now, when downloading track into scanned location, it tries to read data when track is not ready yet. Let's fix it later.
            /*
            //switch (e.ChangeType)
            //{
            //    case WatcherChangeTypes.Created:
            //        TrackDetails track = MetadataManager.ReadMetadata(e.FullPath);
            //        tracks.Add(e.FullPath, track);
            //        tracksObservable.Add(track);
            //        return;
            //    case WatcherChangeTypes.Deleted:
            //        track = tracks[e.FullPath];
            //        tracks.Remove(e.FullPath);
            //        tracksObservable.Remove(track);
            //        return;
            //}
            */
        }

        private void OnWatchingError(object sender, ErrorEventArgs e) => App.Services
            .GetService<ILogger<DirectoryTracksProvider>>()?
            .LogWarning("Unexpected error when watching unsorted tracks directory, stopping tracking. Details: {ex}", e.GetException().Message);
    }
}