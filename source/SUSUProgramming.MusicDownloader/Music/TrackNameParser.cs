// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Provides methods for parsing track names and managing performer information.
    /// This class is responsible for extracting performers, titles, and subtitles from a formatted track name.
    /// </summary>
    internal partial class TrackNameParser
    {
        private static readonly Regex TrackNumberRegex = GetNumberedTrackFilenameRegex();

        /// <summary>
        /// A list of performers that contain slashes, retrieved from the application configuration.
        /// </summary>
        private static readonly string[] SlashContainedPerformersList = App.Services?.GetService<AppConfig>()?.SlashContainedPerformersList ?? [];

        /// <summary>
        /// A list of tuples containing formatted performer names and their original names,
        /// created from the <see cref="SlashContainedPerformersList"/>.
        /// </summary>
        private static readonly List<(string, string)> SlashContainedReplacement =
            [.. from performer in SlashContainedPerformersList
             let parts = performer.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
             select (string.Join(", ", parts), performer)];

        private static ILogger<TrackNameParser>? logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackNameParser"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging operations.</param>
        public TrackNameParser(ILogger<TrackNameParser>? logger = null)
        {
            TrackNameParser.logger = logger;
        }

        /// <summary>
        /// Fixes the performers in the provided track details by replacing any slashed performer names
        /// with their formatted versions and updates the track's tags accordingly.
        /// </summary>
        /// <param name="track">The track details to fix performers for.</param>
        public static void FixPerformers(TrackDetails track)
        {
            ArgumentNullException.ThrowIfNull(track);

            logger?.LogDebug("Fixing performers for track: {TrackName}", track.FormedTrackName);
            var artists = track.FormedArtistString;
            foreach (var artist in SlashContainedReplacement)
            {
                if (artists.Contains(artist.Item1))
                {
                    logger?.LogTrace("Replacing formatted artist '{FormattedArtist}' with original '{OriginalArtist}'", artist.Item1, artist.Item2);
                    artists = artists.Replace(artist.Item1, artist.Item2);
                }
            }

            var performers = artists.Split(", ");
            logger?.LogDebug("Setting performers: {Performers}", string.Join(", ", performers));
            track.SetTag(nameof(Tags.Performers), performers);
            track.SetTag(nameof(Tags.AlbumArtists), performers);
            logger?.LogDebug("Fixed performers for track: {TrackName}", track.FormedTrackName);
        }

        /// <summary>
        /// Splits a string of performers into an array of individual performer names.
        /// Handles cases for featured artists denoted by "feat." or "ft." and separates by commas or semicolons.
        /// </summary>
        /// <param name="formedPerformers">The string containing the formatted performers.</param>
        /// <returns>
        /// An array of performer names extracted from the input string.
        /// </returns>
        public static string[] GetPerformers(string formedPerformers)
        {
            if (string.IsNullOrEmpty(formedPerformers))
                return [];

            logger?.LogDebug("Getting performers from string: {Performers}", formedPerformers);
            var result = formedPerformers.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
            logger?.LogDebug("Found {Count} performers", result.Length);
            return result;
        }

        /// <summary>
        /// Attempts to parse a formatted track name into performers, title, and subtitle.
        /// </summary>
        /// <param name="formedTrackName">The formatted track name to parse.</param>
        /// <param name="performers">
        /// When this method returns <see langword="true"/>, contains the performers extracted from the track name.
        /// Otherwise, <c>null</c>.
        /// </param>
        /// <param name="title">
        /// When this method returns <see langword="true"/>, contains the title extracted from the track name.
        /// Otherwise, <c>null</c>.
        /// </param>
        /// <param name="subtitle">
        /// When this method returns <see langword="true"/>, contains the subtitle extracted from the track name.
        /// Otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the parsing was successful; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryParseName(
            string formedTrackName,
            [NotNullWhen(true)] out string[]? performers,
            [NotNullWhen(true)] out string? title,
            [NotNullWhen(true)] out string? subtitle)
        {
            if (string.IsNullOrEmpty(formedTrackName))
            {
                performers = null;
                title = null;
                subtitle = null;
                return false;
            }

            logger?.LogDebug("Attempting to parse track name: {FormedTrackName}", formedTrackName);
            formedTrackName = TrackNumberRegex.Replace(formedTrackName, string.Empty);
            var parts = formedTrackName.Split([" - "], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                logger?.LogDebug("Failed to parse track name: {FormedTrackName}", formedTrackName);
                performers = null;
                title = parts.Length > 0 ? parts[0] : null;
                subtitle = null;
                return parts.Length > 0;
            }

            performers = GetPerformers(parts[0]);
            title = ParseTitle(parts[1], out subtitle);

            logger?.LogDebug("Successfully parsed track name: {FormedTrackName}", formedTrackName);
            return true;
        }

        /// <summary>
        /// Parses the title string to extract the title and subtitle.
        /// </summary>
        /// <param name="titleString">The title string to parse.</param>
        /// <param name="subtitle">
        /// When this method returns, contains the extracted subtitle if present; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// The parsed title string.
        /// </returns>
        private static string ParseTitle(string titleString, out string subtitle)
        {
            logger?.LogTrace("Parsing title string: {TitleString}", titleString);
            subtitle = string.Empty;
            int bracketPos = titleString.LastIndexOf('(');
            if (bracketPos == -1)
            {
                bracketPos = titleString.LastIndexOf('[');
                if (bracketPos == -1)
                {
                    logger?.LogTrace("No brackets found in title string");
                    return titleString;
                }
            }

            int bracketEndPos = titleString.LastIndexOf(']');
            if (bracketEndPos == -1)
                bracketEndPos = titleString.LastIndexOf(')');

            // If brackets start but do not end, it's a part of a title.
            if (bracketEndPos == -1)
            {
                logger?.LogTrace("Found opening bracket but no closing bracket");
                return titleString;
            }

            // Before bracket should be whitespace.
            if (!char.IsWhiteSpace(titleString[bracketPos - 1]))
            {
                logger?.LogTrace("No whitespace before bracket, treating as part of title");
                return titleString;
            }

            subtitle = titleString[(bracketPos + 1)..bracketEndPos].Trim();
            var title = titleString[..bracketPos].Trim();
            logger?.LogTrace("Parsed title: {Title}, Subtitle: {Subtitle}", title, subtitle);
            return title;
        }

        [GeneratedRegex(@"^\d+\.?\s*", RegexOptions.Compiled)]
        private static partial Regex GetNumberedTrackFilenameRegex();
    }
}