// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.Tests
{
    public class TaggingResultItemTests
    {
        private static TrackDetails RequiredFilled() => [Tags.Title + "Song", Tags.Performers + new[] { "Artist" }];

        private static TaggingConflictInfo YearConflict()
        {
            var conflict = new TaggingConflictInfo(new TrackDetails(), [], Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2018u, "File");
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            return conflict;
        }

        private static TaggingConflictInfo TrackConflict()
        {
            var conflict = new TaggingConflictInfo(new TrackDetails(), [], Tags.Track.Name);
            conflict.Accumulate(Tags.Track + 3u, "LastFm");
            conflict.Accumulate(Tags.Track + 4u, "Genius");
            return conflict;
        }

        private static AutoTaggingViewModel CreateViewModel()
        {
            var tagger = new TagService(new ServiceCollection().BuildServiceProvider(), new Mock<ILogger<TagService>>().Object);
            return new AutoTaggingViewModel(tagger, new AppConfig("."));
        }

        [Fact]
        public void IsReady_RequiredFilledNoConflicts_True()
        {
            var item = new TaggingResultItemViewModel(new FakeTrack(RequiredFilled()), []);

            Assert.True(item.IsReady);
        }

        [Fact]
        public void IsReady_UnresolvedConflict_False()
        {
            var item = new TaggingResultItemViewModel(new FakeTrack(RequiredFilled()), [YearConflict()]);

            Assert.False(item.IsReady);
        }

        [Fact]
        public void IsReady_ConflictRejected_True()
        {
            var conflict = YearConflict();
            var item = new TaggingResultItemViewModel(new FakeTrack(RequiredFilled()), [conflict]);

            conflict.Reject();

            Assert.True(item.IsReady);
        }

        [Fact]
        public void IsReady_RequiredMissing_False()
        {
            var item = new TaggingResultItemViewModel(new FakeTrack([Tags.Title + "Song"]), []);

            Assert.False(item.IsReady);
        }

        [Fact]
        public void Reason_ListsConflictedTagNames()
        {
            var item = new TaggingResultItemViewModel(new FakeTrack(RequiredFilled()), [YearConflict(), TrackConflict()]);

            Assert.Contains(Tags.Year.Name, item.Reason);
            Assert.Contains(Tags.Track.Name, item.Reason);
        }

        [Fact]
        public void Reason_ReadyTrack_IsEmpty()
        {
            var item = new TaggingResultItemViewModel(new FakeTrack(RequiredFilled()), []);

            Assert.Equal(string.Empty, item.Reason);
        }

        [Fact]
        public void ApplyReady_SavesOnlyReadyTracksOnce()
        {
            var vm = CreateViewModel();
            var ready = new FakeTrack(RequiredFilled());
            var withConflict = new FakeTrack(RequiredFilled());
            vm.Items.Add(new TaggingResultItemViewModel(ready, []));
            vm.Items.Add(new TaggingResultItemViewModel(withConflict, [YearConflict()]));

            Assert.Equal(1, vm.ReadyCount);
            vm.ApplyReady();
            vm.ApplyReady();

            Assert.Equal(1, ready.SaveCount);
            Assert.Equal(0, withConflict.SaveCount);
            Assert.Equal(0, vm.ReadyCount);
        }

        private sealed class FakeTrack(TrackDetails model) : ITaggedTrack
        {
            public TrackDetails Model => model;

            public int SaveCount { get; private set; }

            public void Save() => SaveCount++;
        }
    }
}
