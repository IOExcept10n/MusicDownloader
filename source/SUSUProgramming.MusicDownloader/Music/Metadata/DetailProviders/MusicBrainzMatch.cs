// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents the data MusicBrainz knows about a track.
    /// </summary>
    /// <param name="Title">Recording title.</param>
    /// <param name="Artists">Credited artists.</param>
    /// <param name="Album">Title of the earliest release or <see langword="null"/>.</param>
    /// <param name="Year">Year of the earliest release or <c>0</c> if unknown.</param>
    /// <param name="Track">Track number on that release or <c>0</c> if unknown.</param>
    /// <param name="TrackCount">Number of tracks on that release or <c>0</c> if unknown.</param>
    /// <param name="ReleaseId">Identifier of that release, used to look for a cover.</param>
    internal record MusicBrainzMatch(string Title, string[] Artists, string? Album, uint Year, uint Track, uint TrackCount, string? ReleaseId);
}
