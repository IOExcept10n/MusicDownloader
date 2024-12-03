// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using VkNet.Model;

namespace SUSUProgramming.MusicDownloader.Music.StreamingServices.Vk
{
    /// <summary>
    /// Represents a class for some extension methods for the VK API.
    /// </summary>
    internal static class VkExtensions
    {
        /// <summary>
        /// Gets an <see cref="Uri"/> for the cover with the best possible quality for the specified track.
        /// </summary>
        /// <param name="album">Album to get info for.</param>
        /// <returns>Uri for the best quality image or <see langword="null"/>.</returns>
        public static Uri? GetBestQualityCover(this AudioAlbum album)
        {
            string url = album.Thumb.Photo600 ??
                         album.Thumb.Photo300 ??
                         album.Thumb.Photo270 ??
                         album.Thumb.Photo135 ??
                         album.Thumb.Photo68 ??
                         album.Thumb.Photo34;
            return string.IsNullOrWhiteSpace(url) ? null : new Uri(url);
        }
    }
}