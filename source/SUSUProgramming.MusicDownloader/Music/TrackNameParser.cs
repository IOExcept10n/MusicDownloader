// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Provides methods for parsing track names and managing performer information.
    /// This class is responsible for extracting performers, titles, and subtitles from a formatted track name.
    /// </summary>
    internal class TrackNameParser
    {
        /// <summary>
        /// A list of performers that contain slashes, retrieved from the application configuration.
        /// </summary>
        private static readonly List<string> SlashContainedPerformersList =
            App.Services.GetRequiredService<IConfiguration>()
                .GetSection(nameof(SlashContainedPerformersList))
                .Get<List<string>>() ?? new List<string>();

        /// <summary>
        /// A list of tuples containing formatted performer names and their original names,
        /// created from the <see cref="SlashContainedPerformersList"/>.
        /// </summary>
        private static readonly List<(string, string)> SlashContainedReplacement =
            (from performer in SlashContainedPerformersList
             let parts = performer.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
             select (string.Join(", ", parts), performer)).ToList();

        /// <summary>
        /// Fixes the performers in the provided track details by replacing any slashed performer names
        /// with their formatted versions and updates the track's tags accordingly.
        /// </summary>
        /// <param name="track">The track details to fix performers for.</param>
        public static void FixPerformers(TrackDetails track)
        {
            var artists = track.FormedArtistString;
            foreach (var artist in SlashContainedReplacement)
                artists = artists.Replace(artist.Item1, artist.Item2);

            track.SetTag(nameof(Tags.Performers), artists.Split(", "));
            track.SetTag(nameof(Tags.AlbumArtists), artists.Split(", "));
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
            return formedPerformers
                .Split(new[] { "feat. ", "ft." }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(x => x.Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .ToArray();
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
            performers = Array.Empty<string>();
            title = string.Empty;
            subtitle = string.Empty;

            if (string.IsNullOrWhiteSpace(formedTrackName))
                return false;

            var parts = formedTrackName.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Only title
            if (parts.Length == 1)
            {
                title = parts[0];
                performers = Array.Empty<string>();
                subtitle = string.Empty;
                return !string.IsNullOrWhiteSpace(title);
            }

            // Title and performers
            if (parts.Length == 2)
            {
                performers = GetPerformers(parts[0]);
                title = ParseTitle(parts[1], out subtitle);
                return performers.Length > 0 && !string.IsNullOrWhiteSpace(title);
            }

            // Title, performers and subtitle
            if (parts.Length >= 3)
            {
                performers = GetPerformers(parts[0]);
                title = ParseTitle(parts[1], out subtitle);
                subtitle = parts[2] ?? subtitle; // Corrected index to 2 for subtitle
                return performers.Length > 0 && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(subtitle);
            }

            return false;
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
            subtitle = string.Empty;
            int bracketPos = titleString.LastIndexOf('(');
            if (bracketPos == -1)
            {
                bracketPos = titleString.LastIndexOf('[');
                if (bracketPos == -1)
                    return titleString;
            }

            int bracketEndPos = titleString.LastIndexOf(']');
            if (bracketEndPos == -1)
                bracketEndPos = titleString.LastIndexOf(')');

            // If brackets start but do not end, it's a part of a title.
            if (bracketEndPos == -1)
                return titleString;

            // Before bracket should be whitespace.
            if (!char.IsWhiteSpace(titleString[bracketPos - 1]))
            {
                return titleString;
            }

            subtitle = titleString[(bracketPos + 1)..bracketEndPos].Trim();
            return titleString[..bracketPos].Trim();
        }
    }
}