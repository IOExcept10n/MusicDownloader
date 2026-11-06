// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Linq;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Defines which tag values are considered real data and which are placeholders.
    /// </summary>
    internal static class TagValuePolicy
    {
        private static readonly string[] Placeholders =
        [
            TrackDetails.UnknownTitle,
            TrackDetails.UnknownArtist,
            "Unknown",
            "Unknown Album",
            "Unknown Title",
            "Unknown Genre",
        ];

        /// <summary>
        /// Determines whether the tag carries a meaningful value.
        /// </summary>
        /// <param name="tag">Tag to check.</param>
        /// <param name="fileNameWithoutExtension">Name of the track file without extension: a title equal to it is treated as derived from the name.</param>
        /// <returns><see langword="true"/> if the value is real data; otherwise <see langword="false"/>.</returns>
        public static bool IsMeaningful(ITag tag, string? fileNameWithoutExtension = null)
        {
            ArgumentNullException.ThrowIfNull(tag);
            if (!tag.HasValue)
                return false;

            return tag.Value switch
            {
                string s => IsMeaningfulString(s) && !(tag.Name == Tags.Title.Name && IsFileName(s, fileNameWithoutExtension)),
                string[] array => array.Any(IsMeaningfulString),
                _ => true,
            };
        }

        private static bool IsMeaningfulString(string? value) =>
            !string.IsNullOrWhiteSpace(value) && !Placeholders.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

        private static bool IsFileName(string title, string? fileName) =>
            fileName != null && string.Equals(title.Trim(), fileName.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
