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
        private readonly ILogger<DirectoryTracksProvider> logger;
        private int? lastInc = null;
        private bool isDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryTracksProvider"/> class for the specified directory.
        /// </summary>
        /// <param name="trackedPath">Path to track.</param>
        /// <param name="logger">The logger to use for logging operations.</param>
        public DirectoryTracksProvider(string trackedPath, ILogger<DirectoryTracksProvider> logger)
        {
            this.logger = logger;
            trackedDirectory = new DirectoryInfo(trackedPath);
            if (!trackedDirectory.Exists)
            {
                logger.LogInformation("Creating directory: {DirectoryPath}", trackedPath);
                Directory.CreateDirectory(trackedPath);
            }

            logger.LogInformation("Initializing directory watcher for: {DirectoryPath}", trackedPath);
            directoryWatcher = new FileSystemWatcher(trackedPath);
            directoryWatcher.Error += OnWatchingError;
            directoryWatcher.Created += OnFileUpdate;
            directoryWatcher.Changed += OnFileUpdate;
            directoryWatcher.Deleted += OnFileUpdate;
            directoryWatcher.Renamed += OnFileRename;
            directoryWatcher.Filter = "*.mp3";
            directoryWatcher.EnableRaisingEvents = true;
            logger.LogDebug("Directory watcher initialized and started");
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
            bool contains = path.Contains(trackedDirectory.FullName);
            logger.LogDebug("Checking if path {Path} is in tracked directory {TrackedDirectory}: {Result}", path, trackedDirectory.FullName, contains);
            return contains;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            logger.LogDebug("Disposing directory watcher for: {DirectoryPath}", trackedDirectory.FullName);
            directoryWatcher.Dispose();
        }

        /// <summary>
        /// Clears all cache and scans directory for tracks from scratch.
        /// </summary>
        /// <returns>Task to wait.</returns>
        public async Task ScanAsync()
        {
            if (!isDirty)
                return;
            logger.LogInformation("Starting directory scan for: {DirectoryPath}", trackedDirectory.FullName);
            tracks.Clear();
            tracksObservable.Clear();
            var settings = App.Services.GetRequiredService<AppConfig>();
            int scannedFiles = 0;
            int skippedFiles = 0;

            foreach (var file in Directory.EnumerateFiles(trackedDirectory.FullName, "*.mp3", SearchOption.AllDirectories))
            {
                if (settings.BlacklistedPaths.Any(file.Contains))
                {
                    logger.LogDebug("Skipping blacklisted file: {FilePath}", file);
                    skippedFiles++;
                    continue;
                }

                try
                {
                    logger.LogDebug("Reading metadata for file: {FilePath}", file);
                    TrackDetails track = await Task.Run(() => MetadataManager.ReadMetadata(file));
                    if (track.TryGetTag(nameof(VirtualTags.IncrementalNumber), out int inc))
                    {
                        lastInc ??= inc;
                        lastInc = Math.Max(lastInc.Value, inc);
                    }

                    tracks.Add(file, track);
                    tracksObservable.Add(track);
                    scannedFiles++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error reading metadata for file: {FilePath}", file);
                }
            }

            logger.LogInformation("Directory scan completed. Scanned: {ScannedCount}, Skipped: {SkippedCount}", scannedFiles, skippedFiles);
            isDirty = true;
        }

        /// <summary>
        /// Adds single track info to the specified directory manually.
        /// </summary>
        /// <param name="newTrack">Track to add.</param>
        internal void AddTrack(TrackDetails newTrack)
        {
            if (newTrack.FilePath == null)
            {
                logger.LogWarning("Attempted to add track with null file path");
                return;
            }

            logger.LogDebug("Adding track to directory: {FilePath}", newTrack.FilePath);
            tracks[newTrack.FilePath] = newTrack;
            tracksObservable.Add(newTrack);
            logger.LogDebug("Track added successfully");
        }

        /// <summary>
        /// Removes the track by its path manually.
        /// </summary>
        /// <param name="oldPath">Path to remove.</param>
        internal void RemoveTrack(string oldPath)
        {
            logger.LogDebug("Removing track from directory: {FilePath}", oldPath);
            if (tracks.Remove(oldPath, out var value))
            {
                if (value.TryGetTag(nameof(VirtualTags.IncrementalNumber), out int inc) && inc == lastInc)
                {
                    lastInc--;
                    logger.LogDebug("Updated last incremental number to: {NewValue}", lastInc);
                }

                tracksObservable.Remove(value);
                logger.LogDebug("Track removed successfully");
            }
            else
            {
                logger.LogWarning("Track not found in directory: {FilePath}", oldPath);
            }
        }

        private void OnFileRename(object sender, RenamedEventArgs e)
        {
            logger.LogInformation("File renamed from {OldPath} to {NewPath}", e.OldFullPath, e.FullPath);
            var track = tracks[e.OldFullPath];
            if (track.TryGetTag<string>(nameof(TrackDetails.FilePath), out _))
            {
                track.SetTag(nameof(TrackDetails.FilePath), e.FullPath);
                logger.LogDebug("Updated track file path tag");
            }
            else
            {
                track.Add(VirtualTags.TrackFilePath + e.FullPath);
                logger.LogDebug("Added new track file path tag");
            }
        }

        private void OnFileUpdate(object sender, FileSystemEventArgs e)
        {
            logger.LogDebug("File system event detected: {ChangeType} for {FilePath}", e.ChangeType, e.FullPath);
            isDirty = true;

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

        private void OnWatchingError(object sender, ErrorEventArgs e)
        {
            logger.LogError(e.GetException(), "Unexpected error when watching directory {DirectoryPath}", trackedDirectory.FullName);
        }
    }
}