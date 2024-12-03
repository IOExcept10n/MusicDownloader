// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents an interface for the provider that searches track lyrics.
    /// </summary>
    internal interface ILyricsProvider
    {
        /// <summary>
        /// Searches lyrics for the specified track.
        /// </summary>
        /// <param name="details">Details about the track to search lyrics for.</param>
        /// <returns>Lyrics text of <see langword="null"/> if no lyrics found.</returns>
        Task<string?> SearchLyricsAsync(TrackDetails details);
    }
}