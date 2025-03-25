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
        private static readonly ILogger Logger = App.Services.GetRequiredService<ILogger>();

        /// <summary>
        /// Loads track metadata from the specified file.
        /// </summary>
        /// <param name="filePath">File path to load track data from.</param>
        /// <returns>A set of tags with info about loaded track.</returns>
        public static TrackDetails ReadMetadata(string filePath)
        {
            Logger.LogInformation("Reading metadata from file: {FilePath}", filePath);
            using TagLib.File file = TagLib.File.Create(filePath);

            // Skip cover because it takes too much memory
            var tags = Tags.AllTags.Where(x => x is not CoverTag);
            Logger.LogDebug("Found {TagCount} tags to read", tags.Count());
            foreach (var tag in tags)
            {
                Logger.LogTrace("Reading tag: {TagName}", tag.Name);
                tag.ReadFrom(file);
            }

            TrackDetails details = [.. tags, VirtualTags.TrackFilePath + filePath, VirtualTags.HasCover + (file.Tag.Pictures.Length > 0)];
            Logger.LogDebug("Created track details with {TagCount} tags", details.Count);

            if (TryRestoreDetails(details))
            {
                Logger.LogDebug("Successfully restored additional details from filename");
            }
            else
            {
                Logger.LogDebug("No additional details found in filename");
            }

            TrackNameParser.FixPerformers(details);
            Logger.LogInformation("Successfully read metadata from file: {FilePath}", filePath);
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
                Logger.LogError(FileNotFoundMessage);
                ThrowFileNotFoundException(FileNotFoundMessage);
            }

            try
            {
                Logger.LogInformation("Saving metadata to file: {FilePath}", details.FilePath);
                TagLib.File file = TagLib.File.Create(details.FilePath);
                Logger.LogDebug("Applying {TagCount} tags to file", details.Count);
                foreach (var tag in details)
                {
                    Logger.LogTrace("Applying tag: {TagName}", tag.Name);
                    tag.Apply(file);
                }

                file.Save();
                Logger.LogInformation("Successfully saved metadata to file: {FilePath}", details.FilePath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save metadata to file: {FilePath}", details.FilePath);
                throw;
            }
        }

        /// <summary>
        /// Tries to restore track cover by loading it from the track file.
        /// </summary>
        /// <param name="track">Track to load cover from.</param>
        public static void RestoreCover(TrackDetails track)
        {
            if (track.FilePath == null)
            {
                Logger.LogWarning("Cannot restore cover: track has no file path");
                return;
            }

            Logger.LogDebug("Attempting to restore cover for track: {FilePath}", track.FilePath);

            // We should be sure that cover exists even without this fake tag.
            track.Remove(nameof(VirtualTags.HasCover));
            if (track.HasCover)
            {
                Logger.LogDebug("Track already has a cover, skipping restoration");
                return;
            }

            using TagLib.File file = TagLib.File.Create(track.FilePath);
            CoverTag cover = new();
            cover.ReadFrom(file);
            track.Add(cover);

            // Restore virtual tag too.
            bool hasCover = file.Tag.Pictures.Length > 0;
            track.Add(VirtualTags.HasCover + hasCover);
            Logger.LogDebug("Cover restoration complete. Has cover: {HasCover}", hasCover);
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
                Logger.LogDebug("Attempting to restore details from filename: {FilePath}", filePath);
                string name = Path.GetFileNameWithoutExtension(filePath);
                var match = TrackNumberRegex.Match(name);
                if (match.Success)
                {
                    // Because match is from beginning of the string, we can iterate from its length to replace one
                    name = name[match.Length..];
                    int number = int.Parse(match.Groups["number"].Value);
                    details.Add(VirtualTags.IncrementalNumber + number);
                    Logger.LogTrace("Found track number: {TrackNumber}", number);
                }

                // Why here? I need incremental number for each track.
                if (details.HasArtists && details.HasTitle)
                {
                    Logger.LogDebug("Track already has required details (artists and title)");
                    return true;
                }

                if (TrackNameParser.TryParseName(name, out var performers, out var title, out var subtitle))
                {
                    Logger.LogDebug(
                        "Successfully parsed filename into: Artists: {Artists}, Title: {Title}, Subtitle: {Subtitle}",
                        string.Join(", ", performers ?? []),
                        title,
                        subtitle);
                    details.Add(Tags.Performers + performers);
                    if (performers is [string x, ..])
                    {
                        details.Add(Tags.AlbumArtists + [x]);
                        Logger.LogTrace("Set album artist to first performer: {Artist}", x);
                    }

                    details.Add(Tags.Title + title);
                    details.Add(Tags.Subtitle + subtitle);
                    return true;
                }
                else
                {
                    Logger.LogDebug("Could not parse filename into track details");
                }
            }
            else
            {
                Logger.LogDebug("Cannot restore details: track has no file path");
            }

            return false;
        }

        [DoesNotReturn]
        private static void ThrowFileNotFoundException(string message) => throw new FileNotFoundException(message);

        [GeneratedRegex(@"^(?<number>\d+).\s*", RegexOptions.Compiled)]
        private static partial Regex GetTrackNumberRegex();
    }
}