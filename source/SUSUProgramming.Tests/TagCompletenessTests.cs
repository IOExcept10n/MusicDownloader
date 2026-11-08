// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.Tests
{
    public class TagCompletenessTests
    {
        private static TrackDetails RequiredOnly() => [Tags.Title + "Song", Tags.Performers + new[] { "Artist" }];

        private static TrackDetails Recommended()
        {
            var track = RequiredOnly();
            track.Add(Tags.Album + "Album");
            track.Add(Tags.Genres + new[] { "Rock" });
            track.Add(Tags.Year + 2019u);
            track.Add(Tags.Track + 3u);
            return track;
        }

        [Fact]
        public void Evaluate_TitleAndPerformersOnly_Incomplete() =>
            Assert.Equal(TrackProcessingState.Incomplete, TagCompleteness.Evaluate(RequiredOnly()));

        [Fact]
        public void Evaluate_NoRequiredTags_Untagged() =>
            Assert.Equal(TrackProcessingState.Untagged, TagCompleteness.Evaluate([Tags.Album + "Album"]));

        [Fact]
        public void Evaluate_RecommendedWithoutCover_Incomplete() =>
            Assert.Equal(TrackProcessingState.Incomplete, TagCompleteness.Evaluate(Recommended()));

        [Fact]
        public void Evaluate_RecommendedWithCover_Complete()
        {
            var track = Recommended();
            track.SetTag(nameof(VirtualTags.HasCover), true);

            Assert.Equal(TrackProcessingState.Complete, TagCompleteness.Evaluate(track));
        }

        [Fact]
        public void IsReady_RequiredFilledNoConflicts_True() =>
            Assert.True(TagCompleteness.IsReady(RequiredOnly(), hasConflicts: false));

        [Fact]
        public void IsReady_WithConflicts_False() =>
            Assert.False(TagCompleteness.IsReady(RequiredOnly(), hasConflicts: true));

        [Fact]
        public void IsReady_RequiredMissing_False() =>
            Assert.False(TagCompleteness.IsReady([Tags.Title + "Song"], hasConflicts: false));

        [Fact]
        public void ExtrasFilled_CountsOnlyExtraTags()
        {
            var track = Recommended();
            track.Add(Tags.Lyrics + "text");
            track.Add(Tags.Comment + "note");
            track.Add(Tags.TrackCount + 12u);

            Assert.Equal(3, TagCompleteness.ExtrasFilled(track));
        }
    }
}
