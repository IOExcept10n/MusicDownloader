// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Represents a media library that manages a collection of audio tracks.
    /// It provides functionality to scan for tracks, move tracks between directories,
    /// delete tracks, and maintain a mapping of track names for quick access.
    /// </summary>
    internal class MediaLibrary
    {
        private readonly HashSet<string> trackNamesSet = new(StringComparer.OrdinalIgnoreCase);
        private readonly ReadOnlyCollection<DirectoryTracksProvider> trackProviders;
        private readonly ObservableProjection<DirectoryTracksProvider, IReadOnlyCollection<TrackDetails>> tracks;
        private readonly Dictionary<TrackDetails, string> tracksNameMapping = [];
        private readonly ILogger<MediaLibrary> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibrary"/> class.
        /// </summary>
        /// <param name="settings">The application configuration settings containing paths for unsorted and tracked tracks.</param>
        /// <param name="logger">The logger to use for logging operations.</param>
        /// <param name="dtpLogger">The logger to use with directory track providers instances.</param>
        public MediaLibrary(AppConfig settings, ILogger<MediaLibrary> logger, ILogger<DirectoryTracksProvider> dtpLogger)
        {
            this.logger = logger;
            logger.LogInformation("Initializing MediaLibrary with {TrackedPathCount} tracked paths", settings.TrackedPaths.Count);

            Unsorted = new(settings.UnsortedTracksPath, dtpLogger);
            trackProviders = settings.TrackedPaths.Project(x => new DirectoryTracksProvider(x, dtpLogger), (y, x) => y.Path == x);
            tracks = trackProviders.Project(x => x.Tracks);
            AllTracks = tracks.MultiProject(x => x);

            ((INotifyCollectionChanged)AllTracks).CollectionChanged += OnTracksChanged;
            ((INotifyCollectionChanged)Unsorted.Tracks).CollectionChanged += OnTracksChanged;

            logger.LogInformation("MediaLibrary initialized successfully");
        }

        /// <summary>
        /// Gets a read-only collection of all tracks in the media library.
        /// </summary>
        public ReadOnlyCollection<TrackDetails> AllTracks { get; }

        /// <summary>
        /// Gets the provider for unsorted tracks.
        /// </summary>
        public DirectoryTracksProvider Unsorted { get; }

        /// <summary>
        /// Checks if a track with the specified name exists in the media library.
        /// </summary>
        /// <param name="trackName">The name of the track to check.</param>
        /// <returns>True if the track exists; otherwise, false.</returns>
        public bool ContainsTrack(string trackName)
        {
            bool exists = trackNamesSet.Contains(trackName);
            logger.LogDebug("Checking if track exists: {TrackName} - {Result}", trackName, exists);
            return exists;
        }

        /// <summary>
        /// Deletes a track from the media library and the file system.
        /// </summary>
        /// <param name="track">The track to delete.</param>
        public void DeleteTrack(TrackDetails track)
        {
            if (track.FilePath is not string path)
            {
                logger.LogWarning("Attempted to delete track with null file path");
                return;
            }

            logger.LogInformation("Deleting track: {FilePath}", path);
            try
            {
                File.Delete(path);
                if (Unsorted.DirectoryContainsPath(path))
                {
                    logger.LogDebug("Removing track from unsorted directory");
                    Unsorted.RemoveTrack(path);
                }
                else
                {
                    var provider = trackProviders.FirstOrDefault(x => x.DirectoryContainsPath(path));
                    if (provider != null)
                    {
                        logger.LogDebug("Removing track from provider: {ProviderPath}", provider.Path);
                        provider.RemoveTrack(path);
                    }
                    else
                    {
                        logger.LogWarning("No provider found for track path: {FilePath}", path);
                    }
                }

                logger.LogInformation("Track deleted successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting track: {FilePath}", path);
                throw;
            }
        }

        /// <summary>
        /// Moves a track to a new path, updating its location in the media library.
        /// </summary>
        /// <param name="track">The track to move.</param>
        /// <param name="newPath">The new path for the track.</param>
        public void MoveTrack(TrackDetails track, string newPath)
        {
            if (track.FilePath == null)
            {
                logger.LogWarning("Attempted to move track with null file path");
                return;
            }

            string oldPath = track.FilePath;
            if (Path.GetDirectoryName(oldPath) == Path.GetDirectoryName(newPath))
            {
                logger.LogDebug("Track is already in the target directory: {FilePath}", oldPath);
                return;
            }

            logger.LogInformation("Moving track from {OldPath} to {NewPath}", oldPath, newPath);
            DirectoryTracksProvider oldProvider, newProvider;

            if (Unsorted.DirectoryContainsPath(oldPath))
            {
                oldProvider = Unsorted;
                newProvider = trackProviders.First(x => x.DirectoryContainsPath(newPath));
                logger.LogDebug("Moving from unsorted to provider: {ProviderPath}", newProvider.Path);
            }
            else if (Unsorted.DirectoryContainsPath(newPath))
            {
                oldProvider = trackProviders.First(x => x.DirectoryContainsPath(oldPath));
                newProvider = Unsorted;
                logger.LogDebug("Moving from provider {ProviderPath} to unsorted", oldProvider.Path);
            }
            else
            {
                logger.LogError("Invalid move operation - track must be moved between tracked locations");
                return;
            }

            try
            {
                string newFileName = $"{++newProvider.LastIncrementalNumber:D3}. {track.FormedArtistString} - {track.FormedTitle}.mp3";
                foreach (char c in Path.GetInvalidFileNameChars())
                    newFileName = newFileName.Replace(c, '_');
                newFileName = Path.Combine(newPath, newFileName);

                logger.LogDebug("Moving file to: {NewFileName}", newFileName);
                File.Move(oldPath, newFileName);
                track.SetTag(nameof(TrackDetails.FilePath), newFileName);
                oldProvider.RemoveTrack(oldPath);
                newProvider.AddTrack(track);
                logger.LogInformation("Track moved successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error moving track from {OldPath} to {NewPath}", oldPath, newPath);
                throw;
            }
        }

        /// <summary>
        /// Scans for tracks in the unsorted directory and all tracked directories asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ScanAsync()
        {
            try
            {
                logger.LogInformation("Starting full media library scan");
                await Unsorted.ScanAsync();
                foreach (var provider in trackProviders)
                {
                    logger.LogDebug("Scanning provider: {ProviderPath}", provider.Path);
                    await provider.ScanAsync();
                }

                logger.LogInformation("Media library scan completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scanning process has been interrupted because of the unexpected error: ");
            }
        }

        private void OnTrackPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrackDetails.FormedTrackName))
            {
                if (sender is not TrackDetails track)
                    return;

                string oldName = tracksNameMapping[track];
                trackNamesSet.Remove(oldName);
                trackNamesSet.Add(track.FormedTrackName);
                tracksNameMapping[track] = track.FormedTrackName;
                logger.LogDebug("Track name updated from {OldName} to {NewName}", oldName, track.FormedTrackName);
            }
        }

        private void OnTracksChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            logger.LogDebug("Tracks collection changed: {Action}", e.Action);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TrackDetails track in e.NewItems!)
                    {
                        tracksNameMapping[track] = track.FormedTrackName;
                        trackNamesSet.Add(track.FormedTrackName);
                        track.PropertyChanged += OnTrackPropertyChanged;
                        logger.LogDebug("Added track: {TrackName}", track.FormedTrackName);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TrackDetails track in e.OldItems!)
                    {
                        tracksNameMapping.Remove(track);
                        trackNamesSet.Remove(track.FormedTrackName);
                        track.PropertyChanged -= OnTrackPropertyChanged;
                        logger.LogDebug("Removed track: {TrackName}", track.FormedTrackName);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (TrackDetails track in ((ObservableCollection<TrackDetails>?)sender)!)
                    {
                        tracksNameMapping.Remove(track);
                        trackNamesSet.Remove(track.FormedTrackName);
                        track.PropertyChanged -= OnTrackPropertyChanged;
                    }

                    logger.LogDebug("Tracks collection reset");
                    break;
            }
        }
    }
}