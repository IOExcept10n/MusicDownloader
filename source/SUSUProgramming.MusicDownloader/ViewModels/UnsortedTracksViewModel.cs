// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for the unsorted tracks.
    /// </summary>
    /// <param name="library">An instance of the user library to get tracks from.</param>
    /// <param name="settings">App settings to read.</param>
    /// <param name="autoTagger">Tagger instance to use.</param>
    internal partial class UnsortedTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger) : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPathSelected))]
        private int selectedPathIndex;
        [ObservableProperty]
        private ObservableCollection<TrackViewModel> selectedItems = [];

        /// <summary>
        /// Gets the app settings to access.
        /// </summary>
        public AppConfig Settings => settings;

        /// <summary>
        /// Gets the tagger instance to tag tracks.
        /// </summary>
        public AutoTaggingViewModel AutoTagger => autoTagger;

        /// <summary>
        /// Gets the list of unsorted tracks.
        /// </summary>
        public ObservableProjection<TrackDetails, TrackViewModel> UnsortedTracks { get; } = ((ObservableCollection<TrackDetails>)library.Unsorted.Tracks).Project(x => new TrackViewModel(x), (y, x) => y.Model == x);

        /// <summary>
        /// Gets a value indicating whether the path is selected.
        /// </summary>
        [DependsOn(nameof(SelectedPathIndex))]
        public bool IsPathSelected => SelectedPathIndex >= 0 && SelectedPathIndex < settings.TrackedPaths.Count;

        /// <summary>
        /// Gets or sets a value indicating whether the all tracks are selected.
        /// </summary>
        public bool FullSelect
        {
            get => SelectedItems.Count == UnsortedTracks.Count;
            set
            {
                SelectedItems.Clear();
                if (value)
                {
                    foreach (var item in UnsortedTracks)
                    {
                        SelectedItems.Add(item);
                    }
                }
            }
        }
    }
}