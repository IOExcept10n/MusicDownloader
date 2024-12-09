// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for the editing of multiple tracks set.
    /// </summary>
    internal class MultiTrackViewModel : ViewModelBase
    {
        private static readonly string[] PropertyNames = typeof(MultiTrackViewModel).GetProperties().Select(x => x.Name).ToArray();
        private readonly DelayedNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTrackViewModel"/> class.
        /// </summary>
        /// <param name="tracks">Tracks set to follow to edit.</param>
        public MultiTrackViewModel(ObservableCollection<TrackViewModel> tracks)
        {
            SelectedTracks = tracks;
            SelectedTracks.CollectionChanged += OnSelectedTracksUpdated;
            notifier = new(NotifyReset, 50);
        }

        /// <summary>
        /// Gets or sets the album name.
        /// </summary>
        public string? CommonAlbum
        {
            get => GetValue(x => x.Album);
            set => UpdateValue((x, y) => x.Album = y, value);
        }

        /// <summary>
        /// Gets or sets the array of album artists.
        /// </summary>
        public string[]? CommonAlbumArtists
        {
            get => GetValue(x => x.AlbumArtists);
            set => UpdateValue((x, y) => x.AlbumArtists = y, value);
        }

        /// <summary>
        /// Gets or sets the album artists as a single string.
        /// </summary>
        public string? CommonAlbumArtistsString
        {
            get => GetValue(x => x.AlbumArtistsString);
            set => UpdateValue((x, y) => x.AlbumArtistsString = y, value);
        }

        /// <summary>
        /// Gets or sets the comment associated with the album or track.
        /// </summary>
        public string? CommonComment
        {
            get => GetValue(x => x.Comment);
            set => UpdateValue((x, y) => x.Comment = y, value);
        }

        /// <summary>
        /// Gets or sets the URL of the album cover.
        /// </summary>
        public Uri? CommonCoverUrl
        {
            get => GetValue(x => x.CoverUrl);
            set => UpdateValue((x, y) => x.CoverUrl = y, value);
        }

        /// <summary>
        /// Gets or sets the description of the album or track.
        /// </summary>
        public string? CommonDescription
        {
            get => GetValue(x => x.Description);
            set => UpdateValue((x, y) => x.Description = y, value);
        }

        /// <summary>
        /// Gets or sets the disc number of the album.
        /// </summary>
        public uint? CommonDisc
        {
            get => GetValue(x => x.Disc);
            set => UpdateValue((x, y) => x.Disc = y ?? 0, value);
        }

        /// <summary>
        /// Gets or sets the total number of discs in the album.
        /// </summary>
        public uint? CommonDiscCount
        {
            get => GetValue(x => x.DiscCount);
            set => UpdateValue((x, y) => x.DiscCount = y ?? 0, value);
        }

        /// <summary>
        /// Gets or sets the array of genres associated with the album or track.
        /// </summary>
        public string[]? CommonGenres
        {
            get => GetValue(x => x.Genres);
            set => UpdateValue((x, y) => x.Genres = y, value);
        }

        /// <summary>
        /// Gets or sets the genres as a single string.
        /// </summary>
        public string? CommonGenresString
        {
            get => GetValue(x => x.GenresString);
            set => UpdateValue((x, y) => x.GenresString = y, value);
        }

        /// <summary>
        /// Gets or sets the lyrics of the track.
        /// </summary>
        public string? CommonLyrics
        {
            get => GetValue(x => x.Lyrics);
            set => UpdateValue((x, y) => x.Lyrics = y, value);
        }

        /// <summary>
        /// Gets or sets the array of performers associated with the album or track.
        /// </summary>
        public string[]? CommonPerformers
        {
            get => GetValue(x => x.Performers);
            set => UpdateValue((x, y) => x.Performers = y, value);
        }

        /// <summary>
        /// Gets or sets the array of roles for the performers.
        /// </summary>
        public string[]? CommonPerformersRole
        {
            get => GetValue(x => x.PerformersRole);
            set => UpdateValue((x, y) => x.PerformersRole = y, value);
        }

        /// <summary>
        /// Gets or sets the performers as a single string.
        /// </summary>
        public string? CommonPerformersString
        {
            get => GetValue(x => x.PerformersString);
            set => UpdateValue((x, y) => x.PerformersString = y, value);
        }

        /// <summary>
        /// Gets or sets the subtitle of the album or track.
        /// </summary>
        public string? CommonSubtitle
        {
            get => GetValue(x => x.Subtitle);
            set => UpdateValue((x, y) => x.Subtitle = y, value);
        }

        /// <summary>
        /// Gets or sets the title of the album or track.
        /// </summary>
        public string? CommonTitle
        {
            get => GetValue(x => x.Title);
            set => UpdateValue((x, y) => x.Title = y, value);
        }

        /// <summary>
        /// Gets or sets the track number of the album.
        /// </summary>
        public uint? CommonTrack
        {
            get => GetValue(x => x.Track);
            set => UpdateValue((x, y) => x.Track = y ?? 0, value);
        }

        /// <summary>
        /// Gets or sets the total number of tracks in the album.
        /// </summary>
        public uint? CommonTrackCount
        {
            get => GetValue(x => x.TrackCount);
            set => UpdateValue((x, y) => x.TrackCount = y ?? 0, value);
        }

        /// <summary>
        /// Gets the cover image of the currently selected track.
        /// If only one track is selected, it restores the cover from the metadata manager and returns the track's cover image.
        /// Otherwise, it returns null.
        /// </summary>
        public Bitmap? CommonTrackCover
        {
            get
            {
                if (SelectedTracks.Count == 1)
                {
                    var track = SelectedTracks[0];
                    MetadataManager.RestoreCover(track.Model);
                    return track.TrackCover;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the year of the album or track.
        /// If the value is null, it defaults to 0 when updating.
        /// </summary>
        public uint? CommonYear
        {
            get => GetValue(x => x.Year);
            set => UpdateValue((x, y) => x.Year = y ?? 0, value);
        }

        /// <summary>
        /// Gets the collection of currently selected tracks.
        /// </summary>
        public ObservableCollection<TrackViewModel> SelectedTracks { get; }

        /// <summary>
        /// Resets the state of all currently selected tracks.
        /// This method calls the <see cref="ResetState"/> method on each track in the <see cref="SelectedTracks"/> collection.
        /// </summary>
        public void ResetState()
        {
            foreach (var track in SelectedTracks)
            {
                track.ResetState();
            }
        }

        /// <summary>
        /// Saves the current state of all currently selected tracks.
        /// This method calls the <see cref="Save"/> method on each track in the <see cref="SelectedTracks"/> collection.
        /// </summary>
        public void Save()
        {
            foreach (var track in SelectedTracks)
            {
                track.Save();
            }
        }

        /// <summary>
        /// Deletes cover from all selected tracks.
        /// </summary>
        public void DeleteCover()
        {
            foreach (var track in SelectedTracks)
            {
                track.CoverUrl = null;
            }

            OnPropertyChanged(nameof(CommonTrackCover));
        }

        /// <summary>
        /// Sets cover from the clipboard.
        /// </summary>
        /// <returns>Task to wait for.</returns>
        public async Task SetCoverFromClipboardAsync()
        {
            var clipboard = App.GetClipboard();
            if (clipboard == null)
                return;
            string? text = await clipboard.GetTextAsync();
            if (text != null)
            {
                if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
                {
                    await SetCoverFromFileAsync(new(text));
                }
            }
            else
            {
                var file = ((await clipboard.GetDataAsync("Files") as IEnumerable<IStorageItem>) ?? []).FirstOrDefault();
                if (file != null)
                {
                    await SetCoverFromFileAsync(file.Path);
                }
            }

            OnPropertyChanged(nameof(CommonTrackCover));
        }

        /// <summary>
        /// Sets new cover from the file.
        /// </summary>
        /// <param name="path">Path to the file or <see cref="Uri"/> to the online resource to download.</param>
        /// <returns>Task to wait for.</returns>
        public async Task SetCoverFromFileAsync(Uri path)
        {
            foreach (var track in SelectedTracks)
            {
                await track.SetNewCoverAsync(path);
            }

            OnPropertyChanged(nameof(CommonTrackCover));
        }

        private T? GetValue<T>(Func<TrackViewModel, T> getter) => SelectedTracks.Select(getter).Distinct().SingleNoExcept();

        private void NotifyReset()
        {
            foreach (var property in PropertyNames)
            {
                OnPropertyChanged(property);
            }
        }

        private void OnSelectedTracksUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            notifier.NotifyUpdate();
        }

        private void UpdateValue<T>(Action<TrackViewModel, T> setter, T value, [CallerMemberName] string? callingProperty = null)
        {
            if (value == null)
                return;
            foreach (var track in SelectedTracks)
                setter(track, value);
            OnPropertyChanged(callingProperty);
        }
    }
}