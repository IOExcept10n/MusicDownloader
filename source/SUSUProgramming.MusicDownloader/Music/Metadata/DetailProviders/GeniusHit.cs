// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a single song found by the Genius search.
    /// </summary>
    /// <param name="Id">Genius song identifier.</param>
    /// <param name="Title">Song title.</param>
    /// <param name="ArtistNames">Artists of the song as a single string.</param>
    /// <param name="Url">Address of the song page.</param>
    internal readonly record struct GeniusHit(int Id, string Title, string ArtistNames, string Url);
}
