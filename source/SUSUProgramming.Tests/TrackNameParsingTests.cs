// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music;
using TD = Xunit.TheoryData<string, string[], string, string, bool>;

namespace SUSUProgramming.Tests
{
    public class TrackNameParsingTests
    {
        #region Test cases

        // Basic Valid Cases
        public static TD GetBasicValidCases() => new()
        {
            { "Artist - Title", ["Artist"], "Title", string.Empty, true },
            { "Artist1, Artist2 - Title", ["Artist1", "Artist2"], "Title", string.Empty, true },
            { "Artist - Title (Subtitle)", ["Artist"], "Title", "Subtitle", true },
            { "Artist1; Artist2 - Title (Subtitle)", ["Artist1", "Artist2"], "Title", "Subtitle", true },
            { "Title", [], "Title", string.Empty, true },
        };

        // Invalid Cases
        public static TD GetInvalidCases() => new()
        {
            { " - - ", [], string.Empty, string.Empty, false },
            { "-", [], string.Empty, string.Empty, false },
            { null, [], string.Empty, string.Empty, false },
            { string.Empty, [], string.Empty, string.Empty, false }
        };

        // Edge Cases for Artists
        public static TD GetArtistEdgeCases() => new()
        {
            { "Artist1, Artist2 - Title", ["Artist1", "Artist2"], "Title", string.Empty, true },
            { "Artist1; Artist2 - Title", ["Artist1", "Artist2"], "Title", string.Empty, true },
            { "Artist1, Artist2; Artist3 - Title", ["Artist1", "Artist2", "Artist3"], "Title", string.Empty, true },
            { "Artist1 - Title (Subtitle)", ["Artist1"], "Title", "Subtitle", true },
            { "Artist1, Artist2 - Title (Subtitle)", ["Artist1", "Artist2"], "Title", "Subtitle", true },
        };

        // Edge Cases for Titles and Subtitles
        public static TD GetTitleSubtitleEdgeCases() => new()
        {
            { "Artist - Title with Special Characters !@#$%", ["Artist"], "Title with Special Characters !@#$%", string.Empty, true },
            { "Artist - Title (Subtitle with Special Characters !@#$%)", ["Artist"], "Title", "Subtitle with Special Characters !@#$%", true },
            { "Artist - Title (123)", ["Artist"], "Title", "123", true },
            { "Artist - Title(Part of the title)", ["Artist"], "Title(Part of the title)", string.Empty, true },
            { "Artist - Title ( )", ["Artist"], "Title", string.Empty, true }, // Empty subtitle
        };

        // Special Characters and Formats
        public static TD GetSpecialCharacterCases() => new()
        {
            { "Artist - Title (Subtitle with special chars !@#$%^&*)", ["Artist"], "Title", "Subtitle with special chars !@#$%^&*", true },
            { "Artist - Title (Subtitle with numbers 12345)", ["Artist"], "Title", "Subtitle with numbers 12345", true },
            { "Artist - Title (Subtitle with mixed 123!@#)", ["Artist"], "Title", "Subtitle with mixed 123!@#", true },
        };

        #endregion Test cases

        [Theory]
        [MemberData(nameof(GetBasicValidCases))]
        [MemberData(nameof(GetInvalidCases))]
        [MemberData(nameof(GetArtistEdgeCases))]
        [MemberData(nameof(GetTitleSubtitleEdgeCases))]
        [MemberData(nameof(GetSpecialCharacterCases))]
        public void TestParsing(string name, string[] artists, string title, string subtitle, bool parsingResult)
        {
            // Act
            bool result = TrackNameParser.TryParseName(name, out var actualArtists, out var actualTitle, out var actualSubtitle);

            // Assert
            Assert.Equal(parsingResult, result);
            Assert.Equal(artists, actualArtists);
            Assert.Equal(title, actualTitle);
            Assert.Equal(subtitle, actualSubtitle);
        }
    }
}