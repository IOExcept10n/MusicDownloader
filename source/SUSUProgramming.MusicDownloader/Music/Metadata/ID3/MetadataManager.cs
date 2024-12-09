// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Represents a static class for loading track metadata.
    /// </summary>
    internal static partial class MetadataManager
    {
        /// <summary>
        /// Defines a message for the cases when loaded file not found.
        /// </summary>
        private const string FileNotFoundMessage = "Couldn't save metadata because details doesn't contain file path metadata. Consider adding FilePathTag with path to a file to save metadata for.";

        private static readonly Regex TrackNumberRegex = GetTrackNumberRegex();

        /// <summary>
        /// Loads track metadata from the specified file.
        /// </summary>
        /// <param name="filePath">File path to load track data from.</param>
        /// <returns>A set of tags with info about loaded track.</returns>
        public static TrackDetails ReadMetadata(string filePath)
        {
            using TagLib.File file = TagLib.File.Create(filePath);

            // Skip cover because it takes too much memory
            var tags = Tags.AllTags.Where(x => x is not CoverTag);
            foreach (var tag in tags)
                tag.ReadFrom(file);
            TrackDetails details = [.. tags, VirtualTags.TrackFilePath + filePath, VirtualTags.HasCover + (file.Tag.Pictures.Length > 0)];
            TryRestoreDetails(details);
            TrackNameParser.FixPerformers(details);
            return details;
        }

        /// <summary>
        /// Saves track metadata to its bound file.
        /// </summary>
        /// <param name="details">Details about the track to save data for.</param>
        public static void SaveMetadata(TrackDetails details)
        {
            if (string.IsNullOrEmpty(details.FilePath))
            {
                ThrowFileNotFoundException(FileNotFoundMessage);
            }

            try
            {
                TagLib.File file = TagLib.File.Create(details.FilePath);
                foreach (var tag in details)
                    tag.Apply(file);
                file.Save();
            }
            catch (Exception ex)
            {
                App.Services.GetRequiredService<ILogger>().LogError("Couldn't save metadata because file cannot be accessed. Exception details: {ex}.", ex.Message);
            }
        }

        /// <summary>
        /// Tries to restore track cover by loading it from the track file.
        /// </summary>
        /// <param name="track">Track to load cover from.</param>
        public static void RestoreCover(TrackDetails track)
        {
            if (track.FilePath == null)
                return;

            // We should be sure that cover exists even without this fake tag.
            track.Remove(nameof(VirtualTags.HasCover));
            if (track.HasCover)
                return;
            using TagLib.File file = TagLib.File.Create(track.FilePath);
            CoverTag cover = new();
            cover.ReadFrom(file);
            track.Add(cover);

            // Restore virtual tag too.
            track.Add(VirtualTags.HasCover + (file.Tag.Pictures.Length > 0));
        }

        /// <summary>
        /// Tries to restore main details about the specified track from its file name.
        /// </summary>
        /// <param name="details">Details about the track.</param>
        /// <returns><see langword="true"/> if the details were restored successfully; otherwise <see langword="false"/>.</returns>
        public static bool TryRestoreDetails(TrackDetails details)
        {
            var filePath = details.FilePath;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                var match = TrackNumberRegex.Match(name);
                if (match.Success)
                {
                    // Because match is from beginning of the string, we can iterate from its length to replace one
                    name = name[match.Length..];
                    int number = int.Parse(match.Groups["number"].Value);
                    details.Add(VirtualTags.IncrementalNumber + number);
                }

                // Why here? I need incremental number for each track.
                if (details.HasArtists && details.HasTitle)
                {
                    return true;
                }

                if (TrackNameParser.TryParseName(name, out var performers, out var title, out var subtitle))
                {
                    details.Add(Tags.Performers + performers);
                    if (performers is [string x, ..])
                        details.Add(Tags.AlbumArtists + [x]);
                    details.Add(Tags.Title + title);
                    details.Add(Tags.Subtitle + subtitle);
                    return true;
                }
            }

            return false;
        }

        [DoesNotReturn]
        private static void ThrowFileNotFoundException(string message) => throw new FileNotFoundException(message);

        [GeneratedRegex(@"^(?<number>\d+).\s*", RegexOptions.Compiled)]
        private static partial Regex GetTrackNumberRegex();
    }
}