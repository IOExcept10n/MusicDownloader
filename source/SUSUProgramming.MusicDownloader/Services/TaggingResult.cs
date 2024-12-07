// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a result of the tagging for the specified track.
    /// </summary>
    /// <param name="Details">Details about the tagged track.</param>
    /// <param name="Conflicts">List of conflicted tags.</param>
    internal record TaggingResult(TrackDetails Details, ConflictsCollection Conflicts);
}