// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for user tracks library.
    /// </summary>
    [Scoped]
    internal partial class LibraryViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TrackViewModel> selectedTracks = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryViewModel"/> class.
        /// </summary>
        /// <param name="settings">Settings instance to use.</param>
        public LibraryViewModel(AppConfig settings)
        {
            Settings = settings;
            EditingModel = new(selectedTracks);
        }

        /// <summary>
        /// Gets the instance of tracks set to edit.
        /// </summary>
        public MultiTrackViewModel EditingModel { get; }

        /// <summary>
        /// Gets the instance of settings to use.
        /// </summary>
        public AppConfig Settings { get; }
    }
}