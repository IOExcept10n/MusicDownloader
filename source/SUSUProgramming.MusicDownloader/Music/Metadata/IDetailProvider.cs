// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents an interface for the provider for the track details.
    /// </summary>
    internal interface IDetailProvider
    {
        /// <summary>
        /// Gets info about track, such as title, artists etc, asynchronously.
        /// </summary>
        /// <param name="prototype">Prototype track to get info for.</param>
        /// <returns>Details found for this prototype or <see langword="null"/> if no details found.</returns>
        Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype);
    }
}