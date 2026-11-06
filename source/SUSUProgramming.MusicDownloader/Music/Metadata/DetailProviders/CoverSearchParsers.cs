// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Text.Json;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Parses cover search responses of the iTunes Search and Deezer APIs.
    /// </summary>
    internal static class CoverSearchParsers
    {
        /// <summary>
        /// Finds the matching song in an iTunes Search response and returns its enlarged artwork address.
        /// </summary>
        /// <param name="json">Response body.</param>
        /// <param name="title">Title of the track to look for.</param>
        /// <param name="artist">Artist string of the track to look for.</param>
        /// <returns>The artwork address or <see langword="null"/> if there is no matching song or the response is not valid.</returns>
        public static Uri? ParseItunes(string json, string title, string artist) =>
            FindCover(json, "results", title, artist, item => GetString(item, "trackName"), item => GetString(item, "artistName"), item =>
            {
                string? artwork = GetString(item, "artworkUrl100");
                return artwork?.Replace("100x100bb", "1000x1000bb", StringComparison.Ordinal);
            });

        /// <summary>
        /// Finds the matching track in a Deezer search response and returns its extra large album cover address.
        /// </summary>
        /// <param name="json">Response body.</param>
        /// <param name="title">Title of the track to look for.</param>
        /// <param name="artist">Artist string of the track to look for.</param>
        /// <returns>The cover address or <see langword="null"/> if there is no matching track or the response is not valid.</returns>
        public static Uri? ParseDeezer(string json, string title, string artist) =>
            FindCover(
                json,
                "data",
                title,
                artist,
                item => GetString(item, "title"),
                item => item.TryGetProperty("artist", out var a) ? GetString(a, "name") : null,
                item => item.TryGetProperty("album", out var a) ? GetString(a, "cover_xl") : null);

        private static Uri? FindCover(
            string json,
            string listName,
            string title,
            string artist,
            Func<JsonElement, string?> getTitle,
            Func<JsonElement, string?> getArtist,
            Func<JsonElement, string?> getCover)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty(listName, out var items) || items.ValueKind != JsonValueKind.Array)
                    return null;

                foreach (var item in items.EnumerateArray())
                {
                    string? itemTitle = getTitle(item);
                    string? itemArtist = getArtist(item);
                    if (itemTitle == null || itemArtist == null || !TrackTitleMatcher.IsSameTrack(title, artist, itemTitle, itemArtist))
                        continue;

                    string? cover = getCover(item);
                    if (Uri.TryCreate(cover, UriKind.Absolute, out var uri))
                        return uri;
                }
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            {
                // Not a valid response: treated as "nothing found".
            }

            return null;
        }

        private static string? GetString(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
