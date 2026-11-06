// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Parses MusicBrainz recording search responses.
    /// </summary>
    internal static class MusicBrainzParser
    {
        /// <summary>
        /// Finds the recording that is really the requested track and reads its data from the earliest release.
        /// </summary>
        /// <param name="json">Response body of the recording search.</param>
        /// <param name="title">Title of the track to look for.</param>
        /// <param name="artist">Artist string of the track to look for.</param>
        /// <returns>The match or <see langword="null"/> if there is none or the response is not valid.</returns>
        public static MusicBrainzMatch? Parse(string json, string title, string artist)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty("recordings", out var recordings) || recordings.ValueKind != JsonValueKind.Array)
                    return null;

                foreach (var recording in recordings.EnumerateArray())
                {
                    string? recordingTitle = GetString(recording, "title");
                    string[] artists = GetArtists(recording);
                    if (recordingTitle != null && TrackTitleMatcher.IsSameTrack(title, artist, recordingTitle, string.Join(", ", artists)))
                        return Build(recording, recordingTitle, artists);
                }
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            {
                // Not a valid response: treated as "nothing found".
            }

            return null;
        }

        private static MusicBrainzMatch Build(JsonElement recording, string title, string[] artists)
        {
            var release = PickEarliestRelease(recording);
            if (release is not { } chosen)
                return new MusicBrainzMatch(title, artists, null, 0, 0, 0, null);

            uint track = 0, trackCount = 0;
            if (chosen.TryGetProperty("media", out var media) && media.ValueKind == JsonValueKind.Array && media.GetArrayLength() > 0)
            {
                var medium = media[0];
                if (medium.TryGetProperty("track-count", out var count) && count.TryGetUInt32(out uint parsedCount))
                    trackCount = parsedCount;
                if (medium.TryGetProperty("track", out var tracks) && tracks.ValueKind == JsonValueKind.Array && tracks.GetArrayLength() > 0)
                    uint.TryParse(GetString(tracks[0], "number"), out track);
            }

            return new MusicBrainzMatch(title, artists, GetString(chosen, "title"), ParseYear(GetString(chosen, "date")), track, trackCount, GetString(chosen, "id"));
        }

        private static JsonElement? PickEarliestRelease(JsonElement recording)
        {
            if (!recording.TryGetProperty("releases", out var releases) || releases.ValueKind != JsonValueKind.Array || releases.GetArrayLength() == 0)
                return null;

            JsonElement? best = null;
            uint bestYear = uint.MaxValue;
            foreach (var release in releases.EnumerateArray())
            {
                uint year = ParseYear(GetString(release, "date"));
                if (year != 0 && year < bestYear)
                {
                    best = release;
                    bestYear = year;
                }
            }

            return best ?? releases[0];
        }

        private static string[] GetArtists(JsonElement recording)
        {
            if (!recording.TryGetProperty("artist-credit", out var credits) || credits.ValueKind != JsonValueKind.Array)
                return [];

            List<string> names = [];
            foreach (var credit in credits.EnumerateArray())
            {
                string? name = GetString(credit, "name");
                if (name == null && credit.TryGetProperty("artist", out var inner))
                    name = GetString(inner, "name");
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }

            return [.. names];
        }

        private static uint ParseYear(string? date) =>
            date is { Length: >= 4 } && uint.TryParse(date.AsSpan(0, 4), out uint year) ? year : 0;

        private static string? GetString(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
