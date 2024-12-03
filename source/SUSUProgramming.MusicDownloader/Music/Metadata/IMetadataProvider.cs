// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents a base interface for the track metadata providers.
    /// </summary>
    internal interface IMetadataProvider
    {
        /// <summary>
        /// Gets the base <see cref="Uri"/> of the API provider.
        /// </summary>
        public Uri BaseUrl { get; }
    }
}