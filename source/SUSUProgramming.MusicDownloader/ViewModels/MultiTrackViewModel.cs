// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia.Media.Imaging;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal class MultiTrackViewModel : ViewModelBase
    {
        private static readonly string[] PropertyNames = typeof(MultiTrackViewModel).GetProperties().Select(x => x.Name).ToArray();
        private readonly DelayedNotifier notifier;

        public MultiTrackViewModel(ObservableCollection<TrackViewModel> tracks)
        {
            SelectedTracks = tracks;
            SelectedTracks.CollectionChanged += OnSelectedTracksUpdated;
            notifier = new(NotifyReset, 50);
        }

        public string? CommonAlbum { get => GetValue(x => x.Album); set => UpdateValue((x, y) => x.Album = y, value); }

        public string[]? CommonAlbumArtists { get => GetValue(x => x.AlbumArtists); set => UpdateValue((x, y) => x.AlbumArtists = y, value); }

        public string? CommonAlbumArtistsString { get => GetValue(x => x.AlbumArtistsString); set => UpdateValue((x, y) => x.AlbumArtistsString = y, value); }

        public string? CommonComment { get => GetValue(x => x.Comment); set => UpdateValue((x, y) => x.Comment = y, value); }

        public Uri? CommonCoverUrl { get => GetValue(x => x.CoverUrl); set => UpdateValue((x, y) => x.CoverUrl = y, value); }

        public string? CommonDescription { get => GetValue(x => x.Description); set => UpdateValue((x, y) => x.Description = y, value); }

        public uint? CommonDisc { get => GetValue(x => x.Disc); set => UpdateValue((x, y) => x.Disc = y ?? 0, value); }

        public uint? CommonDiscCount { get => GetValue(x => x.DiscCount); set => UpdateValue((x, y) => x.DiscCount = y ?? 0, value); }

        public string[]? CommonGenres { get => GetValue(x => x.Genres); set => UpdateValue((x, y) => x.Genres = y, value); }

        public string? CommonGenresString { get => GetValue(x => x.GenresString); set => UpdateValue((x, y) => x.GenresString = y, value); }

        public string? CommonLyrics { get => GetValue(x => x.Lyrics); set => UpdateValue((x, y) => x.Lyrics = y, value); }

        public string[]? CommonPerformers { get => GetValue(x => x.Performers); set => UpdateValue((x, y) => x.Performers = y, value); }

        public string[]? CommonPerformersRole { get => GetValue(x => x.PerformersRole); set => UpdateValue((x, y) => x.PerformersRole = y, value); }

        public string? CommonPerformersString { get => GetValue(x => x.PerformersString); set => UpdateValue((x, y) => x.PerformersString = y, value); }

        public string? CommonSubtitle { get => GetValue(x => x.Subtitle); set => UpdateValue((x, y) => x.Subtitle = y, value); }

        public string? CommonTitle { get => GetValue(x => x.Title); set => UpdateValue((x, y) => x.Title = y, value); }

        public uint? CommonTrack { get => GetValue(x => x.Track); set => UpdateValue((x, y) => x.Track = y ?? 0, value); }

        public uint? CommonTrackCount { get => GetValue(x => x.TrackCount); set => UpdateValue((x, y) => x.TrackCount = y ?? 0, value); }

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

        public uint? CommonYear { get => GetValue(x => x.Year); set => UpdateValue((x, y) => x.Year = y ?? 0, value); }

        public ObservableCollection<TrackViewModel> SelectedTracks { get; }

        public void Save()
        {
            foreach (var track in SelectedTracks)
            {
                track.Save();
            }
        }

        public void ResetState()
        {
            foreach (var track in SelectedTracks)
            {
                track.ResetState();
            }
        }

        private T? GetValue<T>(Func<TrackViewModel, T> getter) => SelectedTracks.Select(getter).Distinct().SingleNoExcept();

        private void OnSelectedTracksUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            notifier.NotifyUpdate();
        }

        private void NotifyReset()
        {
            foreach (var property in PropertyNames)
            {
                OnPropertyChanged(property);
            }
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