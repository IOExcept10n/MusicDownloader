// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;

namespace SUSUProgramming.Tests
{
    public class CoverSearchParsersTests
    {
        // Hand-written from the public iTunes Search API and Deezer API documentation (no live recording).
        private const string ItunesJson = """
            {
              "resultCount": 2,
              "results": [
                { "trackName": "Other", "artistName": "Artist", "artworkUrl100": "https://example.test/other/100x100bb.jpg" },
                { "trackName": "Song (feat. Guest)", "artistName": "Artist & Guest", "artworkUrl100": "https://example.test/song/100x100bb.jpg" }
              ]
            }
            """;

        private const string DeezerJson = """
            {
              "data": [
                { "title": "Other", "artist": { "name": "Artist" }, "album": { "cover_xl": "https://example.test/other/1000x1000.jpg" } },
                { "title": "Song", "artist": { "name": "Artist" }, "album": { "cover_xl": "https://example.test/song/1000x1000.jpg" } }
              ]
            }
            """;

        [Fact]
        public void ParseItunes_PicksMatchingItemAndEnlargesArtwork() =>
            Assert.Equal(new Uri("https://example.test/song/1000x1000bb.jpg"), CoverSearchParsers.ParseItunes(ItunesJson, "Song", "Artist"));

        [Fact]
        public void ParseItunes_NoMatch_ReturnsNull() =>
            Assert.Null(CoverSearchParsers.ParseItunes(ItunesJson, "Missing", "Artist"));

        [Fact]
        public void ParseItunes_MalformedJson_ReturnsNull() =>
            Assert.Null(CoverSearchParsers.ParseItunes("<html>429</html>", "Song", "Artist"));

        [Fact]
        public void ParseDeezer_PicksMatchingItem() =>
            Assert.Equal(new Uri("https://example.test/song/1000x1000.jpg"), CoverSearchParsers.ParseDeezer(DeezerJson, "Song", "Artist"));

        [Fact]
        public void ParseDeezer_NoMatch_ReturnsNull() =>
            Assert.Null(CoverSearchParsers.ParseDeezer(DeezerJson, "Missing", "Artist"));

        [Fact]
        public void ParseDeezer_MalformedJson_ReturnsNull() =>
            Assert.Null(CoverSearchParsers.ParseDeezer("not json", "Song", "Artist"));
    }
}
