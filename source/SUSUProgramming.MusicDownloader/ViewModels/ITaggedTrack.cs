// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents the part of a track view model that the tagging results need, so they can be tested without the UI.
    /// </summary>
    internal interface ITaggedTrack
    {
        /// <summary>
        /// Gets the tags of the track.
        /// </summary>
        TrackDetails Model { get; }

        /// <summary>
        /// Writes the tags to the track file.
        /// </summary>
        void Save();
    }
}
