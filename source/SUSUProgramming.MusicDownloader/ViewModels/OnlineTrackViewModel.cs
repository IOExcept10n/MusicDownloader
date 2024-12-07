// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Avalonia.Media;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for the online accessible track for visualization.
    /// </summary>
    internal partial class OnlineTrackViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineTrackViewModel"/> class.
        /// </summary>
        /// <param name="track">An instance of the track to use.</param>
        /// <param name="library">Local user library to check.</param>
        /// <param name="settings">Settings instance to check for blacklist.</param>
        public OnlineTrackViewModel(TrackDetails track, MediaLibrary library, AppConfig settings)
        {
            TrackLoadingState state = TrackLoadingState.None;
            if (settings.BlacklistedTrackNames.Contains(track.FormedTrackName))
            {
                state = TrackLoadingState.Ignored;
            }
            else if (library.ContainsTrack(track.FormedTrackName))
            {
                state = TrackLoadingState.Exists;
            }

            track.Add(VirtualTags.LoadingState + state);
            track.PropertyChanged += OnTrackUpdated;
            track.CollectionChanged += OnTrackTagsAdded;
            Model = track;
        }

        /// <summary>
        /// Gets track artist to visualize.
        /// </summary>
        public string Artist => Model.FormedArtistString;

        /// <summary>
        /// Gets a value indicating whether the track can be downloaded.
        /// </summary>
        public bool CanDownload => Model.ContainsKey(nameof(Uri));

        /// <summary>
        /// Gets track model instance.
        /// </summary>
        public TrackDetails Model { get; }

        /// <summary>
        /// Gets the color of the download indicator.
        /// </summary>
        public SolidColorBrush SelectionColor => new(Model.TryGetTag<TrackLoadingState>(nameof(VirtualTags.LoadingState), out var tag) switch
        {
            _ when tag is TrackLoadingState.Exists => Colors.LightGreen,
            _ when tag is TrackLoadingState.Loading => Colors.LightCoral,
            _ when tag is TrackLoadingState.Ignored => Colors.Gray,
            { } => Colors.Transparent,
        });

        /// <summary>
        /// Gets track title to visualize.
        /// </summary>
        public string Title => Model.FormedTitle;

        private void OnTrackTagsAdded(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectionColor));
        }

        private void OnTrackUpdated(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(nameof(SelectionColor));
        }
    }
}