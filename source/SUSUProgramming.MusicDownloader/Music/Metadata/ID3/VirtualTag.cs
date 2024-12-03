// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using TagLib;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents a tag that applies program-bound metadata to a track.
    /// Metadata represented by virtual tags is not saved into a file.
    /// </summary>
    /// <param name="Name">Name of the tag.</param>
    public abstract record class VirtualTag(string Name) : ITag
    {
        /// <inheritdoc/>
        public event EventHandler? ValueUpdated;

        /// <inheritdoc/>
        public object? Value { get => ValueAccessor; set => ValueAccessor = value; }

        /// <inheritdoc/>
        public abstract bool HasValue { get; }

        /// <summary>
        /// Gets or sets property that can be used for access interface value.
        /// </summary>
        protected abstract object? ValueAccessor { get; set; }

        /// <inheritdoc/>
        public void Apply(File file)
        {
        }

        /// <inheritdoc/>
        ITag ITag.Clone() => (VirtualTag)MemberwiseClone();

        /// <inheritdoc/>
        public void ReadFrom(File file)
        {
        }

        /// <summary>
        /// Invokes <see cref="ValueUpdated"/> event.
        /// </summary>
        public void OnValueUpdated() => ValueUpdated?.Invoke(this, EventArgs.Empty);
    }
}