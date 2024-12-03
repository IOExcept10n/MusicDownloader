// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents a basic abstraction for the track tag info.
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// Occurs when the value has updated.
        /// </summary>
        event EventHandler? ValueUpdated;

        /// <summary>
        /// Gets a value indicating whether the tag has valid and significant value.
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the value of the tag.
        /// </summary>
        object? Value { get; set; }

        /// <summary>
        /// Applies tag changes to the specified <see cref="TagLib.File"/>.
        /// </summary>
        /// <param name="file">File instance to write tag data to.</param>
        void Apply(TagLib.File file);

        /// <summary>
        /// Creates a shallow copy of the tag.
        /// </summary>
        /// <returns>New instance of the tag with the same data as in prototype.</returns>
        ITag Clone();

        /// <summary>
        /// Reads tag data from the specified <see cref="TagLib.File"/>.
        /// </summary>
        /// <param name="file">File instance to read tag data from.</param>
        void ReadFrom(TagLib.File file);
    }
}