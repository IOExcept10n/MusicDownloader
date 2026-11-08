// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata;

namespace SUSUProgramming.Tests
{
    public class TrackTitleMatcherTests
    {
        [Fact]
        public void Normalize_DropsFeaturingAndPunctuation() =>
            Assert.Equal(TrackTitleMatcher.Normalize("hello"), TrackTitleMatcher.Normalize("Hello (feat. B)!"));

        [Fact]
        public void IsSameTrack_IgnoresCaseAndWhitespace() =>
            Assert.True(TrackTitleMatcher.IsSameTrack("Song", "A", "SONG ", "a"));

        [Fact]
        public void IsSameTrack_DifferentTitle_False() =>
            Assert.False(TrackTitleMatcher.IsSameTrack("Song", "A", "Other", "A"));

        [Fact]
        public void IsSameTrack_SharedFirstArtist_True() =>
            Assert.True(TrackTitleMatcher.IsSameTrack("Song", "A, B", "Song", "A"));

        [Fact]
        public void IsSameTrack_NoSharedArtist_False() =>
            Assert.False(TrackTitleMatcher.IsSameTrack("Song", "A", "Song", "C"));

        [Fact]
        public void IndexOfTitle_MatchesIgnoringCaseAndSuffix() =>
            Assert.Equal(1, TrackTitleMatcher.IndexOfTitle(["One", "Two (Live)"], "two"));

        [Fact]
        public void IndexOfTitle_Absent_MinusOne() =>
            Assert.Equal(-1, TrackTitleMatcher.IndexOfTitle(["One", "Two"], "three"));
    }
}
