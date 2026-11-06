// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;

namespace SUSUProgramming.Tests
{
    public class MusicBrainzParserTests
    {
        // Hand-written from the public MusicBrainz recording search documentation (no live recording).
        private const string TwoRecordings = """
            {
              "recordings": [
                {
                  "id": "r1",
                  "title": "Another Song",
                  "artist-credit": [ { "name": "Artist" } ],
                  "releases": [ { "id": "wrong", "title": "Wrong Album", "date": "2001-01-01" } ]
                },
                {
                  "id": "r2",
                  "title": "Song",
                  "artist-credit": [ { "name": "Artist" }, { "name": "Guest" } ],
                  "releases": [
                    {
                      "id": "late", "title": "Greatest Hits", "date": "2020",
                      "media": [ { "track-count": 20, "track": [ { "number": "9", "title": "Song" } ] } ]
                    },
                    {
                      "id": "early", "title": "Original Album", "date": "2019-05-03",
                      "media": [ { "track-count": 12, "track": [ { "number": "5", "title": "Song" } ] } ]
                    }
                  ]
                }
              ]
            }
            """;

        [Fact]
        public void Parse_PicksMatchingRecordingAndEarliestRelease()
        {
            var match = MusicBrainzParser.Parse(TwoRecordings, "Song", "Artist");

            Assert.NotNull(match);
            Assert.Equal("Song", match.Title);
            Assert.Equal(["Artist", "Guest"], match.Artists);
            Assert.Equal("Original Album", match.Album);
            Assert.Equal("early", match.ReleaseId);
            Assert.Equal(2019u, match.Year);
            Assert.Equal(5u, match.Track);
            Assert.Equal(12u, match.TrackCount);
        }

        [Fact]
        public void Parse_YearOnlyDate_IsParsed()
        {
            const string json = """{ "recordings": [ { "title": "Song", "artist-credit": [ { "name": "Artist" } ], "releases": [ { "id": "x", "title": "A", "date": "2018" } ] } ] }""";

            Assert.Equal(2018u, MusicBrainzParser.Parse(json, "Song", "Artist")?.Year);
        }

        [Fact]
        public void Parse_MissingDate_YearIsZero()
        {
            const string json = """{ "recordings": [ { "title": "Song", "artist-credit": [ { "name": "Artist" } ], "releases": [ { "id": "x", "title": "A" } ] } ] }""";

            var match = MusicBrainzParser.Parse(json, "Song", "Artist");

            Assert.NotNull(match);
            Assert.Equal(0u, match.Year);
            Assert.Equal(0u, match.Track);
        }

        [Fact]
        public void Parse_NoMatchingRecording_ReturnsNull() =>
            Assert.Null(MusicBrainzParser.Parse(TwoRecordings, "Missing", "Artist"));

        [Fact]
        public void Parse_EmptyRecordings_ReturnsNull() =>
            Assert.Null(MusicBrainzParser.Parse("""{ "recordings": [] }""", "Song", "Artist"));

        [Fact]
        public void Parse_MalformedJson_ReturnsNull() =>
            Assert.Null(MusicBrainzParser.Parse("<html>503</html>", "Song", "Artist"));
    }
}
