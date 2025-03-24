// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for user's tracks list.
    /// </summary>
    internal partial class MyTracksViewModel : ViewModelBase
    {
        private readonly MediaLibrary library;
        private readonly AppConfig settings;
        private readonly AutoTaggingViewModel autoTagger;
        private readonly DelayedNotifier filterNotifier;

        [ObservableProperty]
        private ObservableCollection<TrackViewModel> selectedItems = [];

        private bool fullSelect;
        private string searchTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyTracksViewModel"/> class.
        /// </summary>
        /// <param name="library">Library instance to retrieve tracks.</param>
        /// <param name="settings">Settings instance to use.</param>
        /// <param name="autoTagger">Auto tagger instance to reference.</param>
        public MyTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger)
        {
            this.library = library;
            this.settings = settings;
            this.autoTagger = autoTagger;
            filterNotifier = new(() => OnPropertyChanged(nameof(FilteredTracks)), 500);
            Tracks = library.AllTracks.Project(x => new TrackViewModel(x), (y, x) => y.Model == x);
            Tracks.CollectionChanged += (s, e) => filterNotifier.NotifyUpdate();
        }

        /// <summary>
        /// Occurs when user selection changed.
        /// </summary>
        public event EventHandler<bool>? SelectionChanged;

        /// <summary>
        /// Gets app settings for UI access.
        /// </summary>
        public AppConfig Settings => settings;

        /// <summary>
        /// Gets auto tagger instance to use.
        /// </summary>
        public AutoTaggingViewModel AutoTagger => autoTagger;

        /// <summary>
        /// Gets set of user's tracks.
        /// </summary>
        public ObservableProjection<TrackDetails, TrackViewModel> Tracks { get; }

        /// <summary>
        /// Gets the list of tracks filtered by search query.
        /// </summary>
        public IEnumerable<TrackViewModel> FilteredTracks => Tracks.Where(x => x.Model.FormedTrackName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));

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
        /// Gets or sets a value indicating whether the all tracks are selected.
        /// </summary>
        public bool FullSelect
        {
            get => SelectedItems.Count == Tracks.Count;
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