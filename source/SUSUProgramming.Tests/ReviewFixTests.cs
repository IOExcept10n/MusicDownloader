// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.Tests
{
    /// <summary>
    /// Tests for the problems found by the whole-branch review.
    /// </summary>
    public class ReviewFixTests
    {
        private static TrackDetails RequiredFilled() => [Tags.Title + "Song", Tags.Performers + new[] { "Artist" }];

        private static AutoTaggingViewModel CreateViewModel()
        {
            var tagger = new TagService(new ServiceCollection().BuildServiceProvider(), new Mock<ILogger<TagService>>().Object);
            return new AutoTaggingViewModel(tagger, new AppConfig("."));
        }

        private static TagService CreateService(TrackDetails providerResult)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDetailProvider>(new FakeDetailProvider(providerResult));
            return new TagService(services.BuildServiceProvider(), new Mock<ILogger<TagService>>().Object);
        }

        // --- Critical: an embedded cover must never be replaced silently ---

        [Fact]
        public void CombineResults_TrackHasUnloadedCover_FoundCoverIsNotApplied()
        {
            TrackDetails original = RequiredFilled();
            original.SetTag(nameof(VirtualTags.HasCover), true);
            TrackDetails found = [new FakeCoverTag()];

            var combined = TagService.CombineResults(new AppConfig("."), original, new TaggingResult(found, []));

            Assert.False(combined.ContainsKey("Cover"));
        }

        [Fact]
        public void CombineResults_TrackHasNoCover_FoundCoverIsApplied()
        {
            TrackDetails found = [new FakeCoverTag()];

            var combined = TagService.CombineResults(new AppConfig("."), RequiredFilled(), new TaggingResult(found, []));

            Assert.True(combined.ContainsKey("Cover"));
        }

        [Fact]
        public async Task TagAsync_TrackHasUnloadedCover_ProviderCoverIsNotAutoResolved()
        {
            var service = CreateService([Tags.Title + "Song", new FakeCoverTag()]);
            TrackDetails track = RequiredFilled();
            track.SetTag(nameof(VirtualTags.HasCover), true);

            var result = await service.TagAsync(track);

            Assert.False(result.Details.ContainsKey("Cover"));
            Assert.DoesNotContain(result.Conflicts, x => x.TagName == "Cover");
        }

        // --- Important: cover recommendation by pixel size ---

        [Fact]
        public void ImageSize_Png_ReadsDimensions()
        {
            byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52, 0, 0, 0x01, 0x90, 0, 0, 0x01, 0x2C];

            Assert.Equal((400, 300), ImageSize.TryRead(png));
        }

        [Fact]
        public void ImageSize_JpegWithOtherSegmentsFirst_ReadsDimensions()
        {
            byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x01, 0x2C, 0x01, 0x90, 0x03];

            Assert.Equal((400, 300), ImageSize.TryRead(jpeg));
        }

        [Fact]
        public void ImageSize_GarbageOrTruncated_ReturnsNull()
        {
            Assert.Null(ImageSize.TryRead([1, 2, 3, 4]));
            Assert.Null(ImageSize.TryRead([0xFF, 0xD8, 0xFF]));
            Assert.Null(ImageSize.TryRead([]));
        }

        [Fact]
        public void CoverRanking_PicksStrictlyLargest()
        {
            Assert.Equal(1, CoverRanking.IndexOfLargest([100, 400, 200]));
            Assert.Equal(1, CoverRanking.IndexOfLargest([null, 50]));
        }

        [Fact]
        public void CoverRanking_TieOrUnknown_ReturnsNull()
        {
            Assert.Null(CoverRanking.IndexOfLargest([400, 400]));
            Assert.Null(CoverRanking.IndexOfLargest([null, null]));
        }

        // --- Important: MusicBrainz should prefer the album over an earlier single ---

        [Fact]
        public void MusicBrainz_PrefersOfficialAlbumOverEarlierSingle()
        {
            const string json = """
                {
                  "recordings": [
                    {
                      "title": "Song",
                      "artist-credit": [ { "name": "Artist" } ],
                      "releases": [
                        { "id": "single", "title": "Song (Single)", "date": "2018-01-01", "status": "Official", "release-group": { "primary-type": "Single" },
                          "media": [ { "track-count": 2, "track": [ { "number": "1" } ] } ] },
                        { "id": "album", "title": "The Album", "date": "2019-05-03", "status": "Official", "release-group": { "primary-type": "Album" },
                          "media": [ { "track-count": 12, "track": [ { "number": "5" } ] } ] }
                      ]
                    }
                  ]
                }
                """;

            var match = MusicBrainzParser.Parse(json, "Song", "Artist");

            Assert.NotNull(match);
            Assert.Equal("The Album", match.Album);
            Assert.Equal(5u, match.Track);
            Assert.Equal(2019u, match.Year);
        }

        [Fact]
        public void MusicBrainz_OnlySingles_FallsBackToEarliest()
        {
            const string json = """
                { "recordings": [ { "title": "Song", "artist-credit": [ { "name": "Artist" } ], "releases": [
                    { "id": "b", "title": "B", "date": "2020", "status": "Official", "release-group": { "primary-type": "Single" } },
                    { "id": "a", "title": "A", "date": "2018", "status": "Official", "release-group": { "primary-type": "Single" } } ] } ] }
                """;

            Assert.Equal("A", MusicBrainzParser.Parse(json, "Song", "Artist")?.Album);
        }

        // --- Important: "apply ready" must be honest and robust ---

        [Fact]
        public void AddResult_AlreadySaved_IsNotCountedAndNotSavedAgain()
        {
            var vm = CreateViewModel();
            var track = new FakeTrack(RequiredFilled());

            vm.AddResult(track, [], alreadySaved: true);
            vm.ApplyReady();

            Assert.Equal(0, vm.ReadyCount);
            Assert.Equal(0, track.SaveCount);
        }

        [Fact]
        public void ApplyReady_OneSaveFails_OthersAreStillSavedAndFailureIsShown()
        {
            var vm = CreateViewModel();
            var broken = new FakeTrack(RequiredFilled()) { FailWith = "file is locked" };
            var fine = new FakeTrack(RequiredFilled());
            var brokenItem = vm.AddResult(broken, [], alreadySaved: false);
            vm.AddResult(fine, [], alreadySaved: false);

            vm.ApplyReady();

            Assert.Equal(1, fine.SaveCount);
            Assert.False(brokenItem.IsApplied);
            Assert.Contains("file is locked", brokenItem.Reason);
        }

        // --- Important: every processed track gets a row; conflicts are never thrown away ---

        [Fact]
        public void NothingFound_OnlyWhenNeitherDetailsNorConflicts()
        {
            Assert.True(AutoTaggingViewModel.NothingFound(new TaggingResult([], [])));

            var conflict = new TaggingConflictInfo(new TrackDetails(), [], Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2018u, "A");
            conflict.Accumulate(Tags.Year + 2019u, "B");
            ConflictsCollection conflicts = [conflict];

            Assert.False(AutoTaggingViewModel.NothingFound(new TaggingResult([], conflicts)));
            Assert.False(AutoTaggingViewModel.NothingFound(new TaggingResult([Tags.Title + "Song"], [])));
        }

        [Fact]
        public void Failed_Item_IsNotReadyAndShowsReason()
        {
            var item = TaggingResultItemViewModel.Failed(new FakeTrack(RequiredFilled()), "boom");

            Assert.False(item.IsReady);
            Assert.True(item.NeedsReview);
            Assert.Contains("boom", item.Reason);
        }

        private sealed class FakeDetailProvider(TrackDetails result) : IDetailProvider
        {
            public Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype) => Task.FromResult<TrackDetails?>(result);
        }

        private sealed class FakeTrack(TrackDetails model) : ITaggedTrack
        {
            public TrackDetails Model => model;

            public int SaveCount { get; private set; }

            public string? FailWith { get; init; }

            public void Save()
            {
                if (FailWith != null)
                    throw new InvalidOperationException(FailWith);
                SaveCount++;
            }
        }

        // CoverTag cannot be created in tests (it needs the running app), so a stand-in with the same tag name is used.
        private sealed class FakeCoverTag : ITag
        {
            public event EventHandler? ValueUpdated
            {
                add { }
                remove { }
            }

            public string Name => "Cover";

            public bool HasValue => true;

            public object? Value { get; set; } = new object();

            public void Apply(TagLib.File file)
            {
            }

            public ITag Clone() => this;

            public void ReadFrom(TagLib.File file)
            {
            }
        }
    }
}
