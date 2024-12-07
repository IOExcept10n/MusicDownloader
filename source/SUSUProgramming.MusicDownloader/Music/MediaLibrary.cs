// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
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
        private readonly Dictionary<TrackDetails, string> tracksNameMapping = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibrary"/> class.
        /// </summary>
        /// <param name="settings">The application configuration settings containing paths for unsorted and tracked tracks.</param>
        public MediaLibrary(AppConfig settings)
        {
            Unsorted = new(settings.UnsortedTracksPath);
            trackProviders = settings.TrackedPaths.Project(x => new DirectoryTracksProvider(x), (y, x) => y.Path == x);
            tracks = trackProviders.Project(x => x.Tracks);
            AllTracks = tracks.MultiProject(x => x);
            ((INotifyCollectionChanged)AllTracks).CollectionChanged += OnTracksChanged;
            ((INotifyCollectionChanged)Unsorted.Tracks).CollectionChanged += OnTracksChanged;
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
            return trackNamesSet.Contains(trackName);
        }

        /// <summary>
        /// Deletes a track from the media library and the file system.
        /// </summary>
        /// <param name="track">The track to delete.</param>
        public void DeleteTrack(TrackDetails track)
        {
            if (track.FilePath is not string path)
                return;

            File.Delete(path);
            if (Unsorted.DirectoryContainsPath(path))
                Unsorted.RemoveTrack(path);
            else
                trackProviders.FirstOrDefault(x => x.DirectoryContainsPath(path))?.RemoveTrack(path);
        }

        /// <summary>
        /// Moves a track to a new path, updating its location in the media library.
        /// </summary>
        /// <param name="track">The track to move.</param>
        /// <param name="newPath">The new path for the track.</param>
        public void MoveTrack(TrackDetails track, string newPath)
        {
            if (track.FilePath == null)
                return;

            string oldPath = track.FilePath;
            if (Path.GetDirectoryName(oldPath) == Path.GetDirectoryName(newPath))
                return;

            DirectoryTracksProvider oldProvider, newProvider;

            if (Unsorted.DirectoryContainsPath(oldPath))
            {
                oldProvider = Unsorted;
                newProvider = trackProviders.First(x => x.DirectoryContainsPath(newPath));
            }
            else if (Unsorted.DirectoryContainsPath(newPath))
            {
                oldProvider = trackProviders.First(x => x.DirectoryContainsPath(oldPath));
                newProvider = Unsorted;
            }
            else
            {
                ThrowHelper.ThrowInvalidOperationException("Couldn't determine movement type. Track should be moved between tracked locations.");
                return;
            }

            string newFileName = $"{++newProvider.LastIncrementalNumber:D3}. {track.FormedArtistString} - {track.FormedTitle}.mp3";
            foreach (char c in Path.GetInvalidFileNameChars())
                newFileName = newFileName.Replace(c, '_');
            newFileName = Path.Combine(newPath, newFileName);
            File.Move(oldPath, newFileName);
            track.SetTag(nameof(TrackDetails.FilePath), newFileName);
            oldProvider.RemoveTrack(oldPath);
            newProvider.AddTrack(track);
        }

        /// <summary>
        /// Scans for tracks in the unsorted directory and all tracked directories asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ScanAsync()
        {
            await Unsorted.ScanAsync();
            foreach (var provider in trackProviders)
            {
                await provider.ScanAsync();
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
            }
        }

        private void OnTracksChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TrackDetails track in e.NewItems!)
                    {
                        tracksNameMapping[track] = track.FormedTrackName;
                        trackNamesSet.Add(track.FormedTrackName);
                        track.PropertyChanged += OnTrackPropertyChanged;
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TrackDetails track in e.OldItems!)
                    {
                        tracksNameMapping.Remove(track);
                        trackNamesSet.Remove(track.FormedTrackName);
                        track.PropertyChanged -= OnTrackPropertyChanged;
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (TrackDetails track in ((ObservableCollection<TrackDetails>?)sender)!)
                    {
                        tracksNameMapping.Remove(track);
                        trackNamesSet.Remove(track.FormedTrackName);
                        track.PropertyChanged -= OnTrackPropertyChanged;
                    }

                    break;
            }
        }
    }
}