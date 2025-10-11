// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Persistence;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Represents a media library that manages a collection of audio tracks.
    /// It provides functionality to scan for tracks, move tracks between directories,
    /// delete tracks, and maintain a mapping of track names for quick access.
    /// </summary>
    internal class MediaLibrary
    {
        private readonly ReadOnlyCollection<DirectoryTracksProvider> trackProviders;
        private readonly ObservableProjection<DirectoryTracksProvider, IReadOnlyCollection<TrackDetails>> tracks;
        private readonly Dictionary<TrackDetails, (string Artist, string Title)> tracksNameMapping = [];
        private readonly ILogger<MediaLibrary> logger;
        private readonly ITrackStatsRepository trackStatsRepo;
        private readonly DelayedNotifier n;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibrary"/> class.
        /// </summary>
        /// <param name="settings">The application configuration settings containing paths for unsorted and tracked tracks.</param>
        /// <param name="trackContext">A database for tracks stats.</param>
        /// <param name="logger">The logger to use for logging operations.</param>
        /// <param name="dtpLogger">The logger to use with directory track providers instances.</param>
        public MediaLibrary(AppConfig settings, ITrackStatsRepository trackStatsRepo, ILogger<MediaLibrary> logger, ILogger<DirectoryTracksProvider> dtpLogger)
        {
            this.logger = logger;
            this.trackStatsRepo = trackStatsRepo;
            logger.LogInformation("Initializing MediaLibrary with {TrackedPathCount} tracked paths", settings.TrackedPaths.Count);

            Unsorted = new(settings.UnsortedTracksPath, dtpLogger);
            trackProviders = settings.TrackedPaths.Project(x => new DirectoryTracksProvider(x, dtpLogger), (y, x) => y.Path == x);
            tracks = trackProviders.Project(x => x.Tracks);
            AllTracks = tracks.MultiProject(x => x);

            ((INotifyCollectionChanged)AllTracks).CollectionChanged += OnTracksChanged;
            ((INotifyCollectionChanged)Unsorted.Tracks).CollectionChanged += OnTracksChanged;

            n = new(trackStatsRepo.SaveAsync, 5000);

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
        /// <param name="artist">The artist to check.</param>
        /// <param name="title">The title of the track to check.</param>
        /// <returns>True if the track exists; otherwise, false.</returns>
        public bool ContainsTrack(string artist, string title)
        {
            var stats = trackStatsRepo.GetByArtistAndTitleAsync(artist, title).GetAwaiter().GetResult();
            bool exists = stats?.IsDownloaded == true;
            logger.LogDebug("Checking if track exists: {Artist} - {TrackName} - {Result}", artist, title, exists);
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
                Dispatcher.UIThread.Post(
                    async () =>
                    {
                        (string oldArtist, string oldName) = tracksNameMapping[track];

                        var info = await trackStatsRepo.GetOrCreateByArtistAndTitleAsync(oldArtist, oldName);

                        string? oldPath = info.TrackPath;

                        info.TrackPath = null;

                        n.NotifyUpdate();

                        var newInfo = await trackStatsRepo.GetOrCreateByArtistAndTitleAsync(track.FormedArtistString, track.FormedTitle);

                        info.TrackPath = oldPath;

                        n.NotifyUpdate();

                        tracksNameMapping[track] = (track.FormedArtistString, track.FormedTitle);
                        logger.LogDebug("Track name updated from {OldName} to {NewName}", oldName, track.FormedTrackName);
                    },
                    DispatcherPriority.Background);
            }
        }

        private void OnTracksChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            logger.LogDebug("Tracks collection changed: {Action}", e.Action);
            Dispatcher.UIThread.Post(
                async () =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (TrackDetails track in e.NewItems!)
                            {
                                tracksNameMapping[track] = (track.FormedArtistString, track.FormedTitle);

                                var stats = await trackStatsRepo.GetOrCreateByArtistAndTitleAsync(track.FormedArtistString, track.FormedTitle);

                                stats.TrackPath = track.FilePath;

                                
                                track.PropertyChanged += OnTrackPropertyChanged;
                                logger.LogDebug("Added track: {TrackName}", track.FormedTrackName);
                            }

                            break;

                        case NotifyCollectionChangedAction.Remove:
                            foreach (TrackDetails track in e.OldItems!)
                            {
                                tracksNameMapping.Remove(track);
                                var stats = await trackStatsRepo.GetOrCreateByArtistAndTitleAsync(track.FormedArtistString, track.FormedTitle);
                                stats.TrackPath = null;
                                track.PropertyChanged -= OnTrackPropertyChanged;
                                logger.LogDebug("Removed track: {TrackName}", track.FormedTrackName);
                            }

                            break;

                        case NotifyCollectionChangedAction.Reset:
                            foreach (TrackDetails track in ((ObservableCollection<TrackDetails>?)sender)!)
                            {
                                tracksNameMapping.Remove(track);
                                var stats = await trackStatsRepo.GetOrCreateByArtistAndTitleAsync(track.FormedArtistString, track.FormedTitle);
                                stats.TrackPath = null;
                                track.PropertyChanged -= OnTrackPropertyChanged;
                            }

                            logger.LogDebug("Tracks collection reset");
                            break;
                    }

                    n.NotifyUpdate();
                },
                DispatcherPriority.Background);
        }
    }
}