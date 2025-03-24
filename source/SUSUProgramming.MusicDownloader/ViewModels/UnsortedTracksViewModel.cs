// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    internal partial class UnsortedTracksViewModel : ViewModelBase
    {
        private readonly MediaLibrary library;
        private readonly AppConfig settings;
        private readonly AutoTaggingViewModel autoTagger;
        private readonly DelayedNotifier filterNotifier;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPathSelected))]
        private int selectedPathIndex;
        [ObservableProperty]
        private ObservableCollection<TrackViewModel> selectedItems = [];
        private bool fullSelect;
        private string searchTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsortedTracksViewModel"/> class.
        /// </summary>
        /// <param name="library">An instance of the user library to get tracks from.</param>
        /// <param name="settings">App settings to read.</param>
        /// <param name="autoTagger">Tagger instance to use.</param>
        public UnsortedTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger)
        {
            this.library = library;
            this.settings = settings;
            this.autoTagger = autoTagger;
            filterNotifier = new(() => OnPropertyChanged(nameof(FilteredTracks)), 500);
            UnsortedTracks = ((ObservableCollection<TrackDetails>)library.Unsorted.Tracks).Project(x => new TrackViewModel(x), (y, x) => y.Model == x);
            UnsortedTracks.CollectionChanged += (s, e) => filterNotifier.NotifyUpdate();
        }

        /// <summary>
        /// Occurs when user selection changed.
        /// </summary>
        public event EventHandler<bool>? SelectionChanged;

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
        public ObservableProjection<TrackDetails, TrackViewModel> UnsortedTracks { get; }

        /// <summary>
        /// Gets the list of tracks filtered by search query.
        /// </summary>
        public IEnumerable<TrackViewModel> FilteredTracks => UnsortedTracks.Where(x => x.Model.FormedTrackName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets or sets the query to search tracks with.
        /// </summary>
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (SetProperty(ref searchTerm, value))
                {
                    OnPropertyChanged(nameof(FilteredTracks));
                }
            }
        }

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
                if (fullSelect != value)
                {
                    fullSelect = value;
                    SelectionChanged?.Invoke(this, fullSelect);
                    OnPropertyChanged();
                }
            }
        }
    }
}