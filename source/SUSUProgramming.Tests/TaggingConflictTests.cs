// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.Tests
{
    public class TaggingConflictTests
    {
        private static TaggingConflictInfo NewConflict(string tagName) => new(new TrackDetails(), [], tagName);

        private static uint YearAt(TaggingConflictInfo conflict, int? index) => (uint)conflict.FoundData[index!.Value].Value!;

        [Fact]
        public void Accumulate_ZeroYear_IsIgnored()
        {
            var conflict = NewConflict(Tags.Year.Name);

            conflict.Accumulate(Tags.Year + 0u, "LastFm");

            Assert.Empty(conflict.FoundData);
        }

        [Fact]
        public void Accumulate_SameValueTwoSources_OneCaseTwoSources()
        {
            var conflict = NewConflict(Tags.Year.Name);

            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            conflict.Accumulate(Tags.Year + 2019u, "MusicBrainz");

            Assert.Single(conflict.FoundData);
            Assert.Equal(["LastFm", "MusicBrainz"], conflict.SourcesOf(0));
        }

        [Fact]
        public void Recommended_TwoSourcesAgreeOneDiffers_PicksMajority()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            conflict.Accumulate(Tags.Year + 2020u, "Genius");
            conflict.Accumulate(Tags.Year + 2019u, "MusicBrainz");

            Assert.Equal(2, conflict.FoundData.Count);
            Assert.Equal(2019u, YearAt(conflict, conflict.RecommendedCase));
        }

        [Fact]
        public void Recommended_Tie_IsNull()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            conflict.Accumulate(Tags.Year + 2020u, "Genius");

            Assert.Null(conflict.RecommendedCase);
        }

        [Fact]
        public void Current_ExistingValueMarked()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2018u, "File");
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");

            Assert.Equal(2018u, YearAt(conflict, conflict.CurrentCase));
        }

        [Fact]
        public void AutoResolve_SingleMeaningfulValue_Fills()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            ConflictsCollection conflicts = [conflict];

            var resolved = conflicts.AutoResolve();

            Assert.Equal(2019u, resolved.GetTag<uint>(Tags.Year.Name));
            Assert.Empty(conflicts);
        }

        [Fact]
        public void AutoResolve_ExistingPlusDifferentValue_StaysConflict()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2018u, "File");
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            ConflictsCollection conflicts = [conflict];

            var resolved = conflicts.AutoResolve();

            Assert.False(resolved.ContainsKey(Tags.Year.Name));
            Assert.Single(conflicts);
        }

        [Fact]
        public void AutoResolve_ExistingEqualsFound_NoChange()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 2019u, "File");
            conflict.Accumulate(Tags.Year + 2019u, "LastFm");
            ConflictsCollection conflicts = [conflict];

            var resolved = conflicts.AutoResolve();

            Assert.Equal(2019u, resolved.GetTag<uint>(Tags.Year.Name));
            Assert.Empty(conflicts);
        }

        [Fact]
        public void AutoResolve_OnlyZeroValues_RemovesEmptyConflict()
        {
            var conflict = NewConflict(Tags.Year.Name);
            conflict.Accumulate(Tags.Year + 0u, "LastFm");
            ConflictsCollection conflicts = [conflict];

            var resolved = conflicts.AutoResolve();

            Assert.Empty(resolved);
            Assert.Empty(conflicts);
        }

        [Fact]
        public void CombineResults_RewriteMetadataTrue_DoesNotOverwriteMeaningfulTag()
        {
            TrackDetails original = [Tags.Title + "A"];
            TrackDetails found = [Tags.Title + "B"];
            var settings = new AppConfig(".") { RewriteMetadata = true };

            var combined = TagService.CombineResults(settings, original, new TaggingResult(found, []));

            Assert.Equal("A", combined.GetTag<string>(Tags.Title.Name));
        }

        [Fact]
        public void CombineResults_OriginalHasNoYear_IsFilled()
        {
            TrackDetails original = [Tags.Year + 0u];
            TrackDetails found = [Tags.Year + 2019u];

            var combined = TagService.CombineResults(new AppConfig("."), original, new TaggingResult(found, []));

            Assert.Equal(2019u, combined.GetTag<uint>(Tags.Year.Name));
        }

        [Fact]
        public void CombineResults_OriginalUnknownArtist_IsReplaced()
        {
            TrackDetails original = [Tags.Performers + new[] { TrackDetails.UnknownArtist }];
            TrackDetails found = [Tags.Performers + new[] { "Real Artist" }];

            var combined = TagService.CombineResults(new AppConfig("."), original, new TaggingResult(found, []));

            Assert.Equal(["Real Artist"], combined.GetTag<string[]>(Tags.Performers.Name));
        }

        [Fact]
        public async Task TagAsync_ProviderReturnsZeroYear_NoConflictAndNoYear()
        {
            var service = CreateService(new TrackDetails { Tags.Title + "Song", Tags.Year + 0u });

            var result = await service.TagAsync(new TrackDetails());

            Assert.Empty(result.Conflicts);
            Assert.False(result.Details.ContainsKey(Tags.Year.Name));
            Assert.Equal("Song", result.Details.GetTag<string>(Tags.Title.Name));
        }

        [Fact]
        public async Task TagAsync_ExistingYearDiffersFromSource_BecomesConflictWithFileSource()
        {
            var service = CreateService(new TrackDetails { Tags.Year + 2019u });
            TrackDetails track = [Tags.Year + 2018u];

            var result = await service.TagAsync(track);

            var conflict = Assert.Single(result.Conflicts);
            Assert.Equal(2, conflict.FoundData.Count);
            Assert.Equal(2018u, YearAt(conflict, conflict.CurrentCase));
            Assert.Contains("File", conflict.SourcesOf(conflict.CurrentCase!.Value));
            Assert.False(result.Details.ContainsKey(Tags.Year.Name));
        }

        private static TagService CreateService(TrackDetails providerResult)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDetailProvider>(new FakeDetailProvider(providerResult));
            return new TagService(services.BuildServiceProvider(), new Mock<ILogger<TagService>>().Object);
        }

        // Moq cannot proxy the internal IDetailProvider interface, so a hand-written fake is used.
        private sealed class FakeDetailProvider(TrackDetails result) : IDetailProvider
        {
            public Task<TrackDetails?> SearchTrackDetailsAsync(TrackDetails prototype) => Task.FromResult<TrackDetails?>(result);
        }
    }
}
