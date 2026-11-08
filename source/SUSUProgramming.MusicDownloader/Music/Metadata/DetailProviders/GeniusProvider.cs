// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders
{
    /// <summary>
    /// Represents a metadata provider for Genius.com.
    /// </summary>
    /// <param name="api">An instance of the API helper to use.</param>
    /// <param name="tokens">An instance of the tokens storage to get api token from.</param>
    internal partial class GeniusProvider(ApiHelper api, TokenStorage tokens) : TrackDetailsProvider(api, "https://api.genius.com/")
    {
        private static readonly Regex LyricsDivRegex = GetLyricsDivRegex();
        private static readonly Regex TagRegex = GetTagRegex();
        private static readonly Regex MultilineRegex = GetMultilineRegex();
        private static readonly Regex MultiSpaceRegex = GetMultiSpaceRegex();

        private readonly string? token = tokens.GetToken("Genius");

        /// <inheritdoc/>
        public override async Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype)
        {
            if (token == null)
                return null;
            dynamic response = await Api.Request(this)
                                    .WithEndpoint("search")
                                    .WithParams("q", prototype.FormedTrackName)
                                    .WithParams("access_token", token)
                                    .CallAsync();
            var hits = response.response?.hits;
            if (hits is not null && Enumerable.Any(hits))
            {
                var candidates = new List<GeniusHit>();
                foreach (dynamic hit in hits!)
                {
                    try
                    {
                        dynamic found = hit!.result;
                        candidates.Add(new GeniusHit((int)found.id, (string)found.title, (string)found.artist_names, (string)found.url));
                    }
                    catch
                    {
                        // A malformed hit is just skipped.
                    }
                }

                // Genius often puts a different song first, so the hit must actually match the track.
                if (PickMatchingHit(candidates, prototype.FormedTitle, prototype.FormedArtistString) is { } picked)
                {
                    int id = picked.Id;
                    string url = picked.Url;
                    string? lyrics = null;
                    if (!string.IsNullOrEmpty(url))
                    {
                        string html = await HttpClient.GetStringAsync(url);
                        lyrics = GetLyrics(html);
                    }

                    response = await Api.Request(this)
                                        .WithEndpoint($"songs/{id}")
                                        .WithParams("text_format", "plain")
                                        .WithParams("access_token", token)
                                        .CallAsync();
                    var result = response.response?.song;
                    var album = result?.album;
                    string? uri, albumName;
                    try
                    {
                        uri = album?.cover_art_url;
                        albumName = album?.name;
                    }
                    catch
                    {
                        uri = null;
                        albumName = null;
                    }

                    // Fix small cover thumbnails
                    if (uri != null && uri.Contains("1000x1000x1"))
                    {
                        uri = uri.Replace("340x340", "1000x1000");
                    }

                    if (result != null)
                    {
                        return [
                            Tags.Title + (string?)result.title,
                            Tags.Album + albumName,
                            Tags.Description + (string?)result.description?.plain,
                            uri != null ? await CoverTag.DownloadCoverAsync(new(uri), HttpClient) : null,
                            Tags.Performers + TrackNameParser.GetPerformers((string?)result.artist_names ?? string.Empty),
                            Tags.Year + ((uint?)((DateTime?)result.release_date)?.Year).GetValueOrDefault(),
                            Tags.Lyrics + lyrics
                        ];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Picks the first of the top search hits that is really the requested track.
        /// </summary>
        /// <param name="hits">Search hits in the order Genius returned them.</param>
        /// <param name="title">Title of the track to look for.</param>
        /// <param name="artist">Artist string of the track to look for.</param>
        /// <returns>The matching hit or <see langword="null"/> if none of the first five hits matches.</returns>
        internal static GeniusHit? PickMatchingHit(IEnumerable<GeniusHit> hits, string title, string artist)
        {
            foreach (var hit in hits.Take(5))
            {
                if (TrackTitleMatcher.IsSameTrack(title, artist, hit.Title, hit.ArtistNames))
                    return hit;
            }

            return null;
        }

        private static string GetLyrics(string html)
        {
            var content = string.Join('\n', LyricsDivRegex.Matches(html).Select(x => x.Groups["content"].Value));
            content = content.Replace("<br/>", "\n");
            string filtered = TagRegex.Replace(content, string.Empty);
            string multilineFiltered = MultilineRegex.Replace(filtered, "\n");
            string multiSpaceFiltered = MultiSpaceRegex.Replace(multilineFiltered, " ");
            string lyrics = WebUtility.HtmlDecode(multiSpaceFiltered.Trim());
            return lyrics;
        }

        [GeneratedRegex("""<div .*?data-lyrics-container=\"true\".*?>(?<content>.*?)<\/div>""")]
        private static partial Regex GetLyricsDivRegex();

        [GeneratedRegex(@"<.*?>")]
        private static partial Regex GetTagRegex();

        [GeneratedRegex(@"\n\n+")]
        private static partial Regex GetMultilineRegex();

        [GeneratedRegex(@"\s\s+")]
        private static partial Regex GetMultiSpaceRegex();
    }
}