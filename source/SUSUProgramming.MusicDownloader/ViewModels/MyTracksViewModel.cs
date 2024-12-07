// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for user's tracks list.
    /// </summary>
    /// <param name="library">Library instance to retrieve tracks.</param>
    /// <param name="settings">Settings instance to use.</param>
    /// <param name="autoTagger">Auto tagger instance to reference.</param>
    internal partial class MyTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger) : ViewModelBase
    {
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
        public ObservableProjection<TrackDetails, TrackViewModel> Tracks { get; } = library.AllTracks.Project(x => new TrackViewModel(x), (y, x) => y.Model == x);
    }
}