// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.Tests
{
    public class TagValuePolicyTests
    {
        [Fact]
        public void Year0_NotMeaningful() =>
            Assert.False(TagValuePolicy.IsMeaningful(Tags.Year + 0u));

        [Fact]
        public void Year2019_Meaningful() =>
            Assert.True(TagValuePolicy.IsMeaningful(Tags.Year + 2019u));

        [Fact]
        public void TitleEqualsFileName_NotMeaningful() =>
            Assert.False(TagValuePolicy.IsMeaningful(Tags.Title + "Song", "Song"));

        [Fact]
        public void TitleUnknown_NotMeaningful() =>
            Assert.False(TagValuePolicy.IsMeaningful(Tags.Title + "Untitled"));

        [Fact]
        public void PerformersUnknownArtist_NotMeaningful() =>
            Assert.False(TagValuePolicy.IsMeaningful(Tags.Performers + new[] { "Unknown Artist" }));

        [Fact]
        public void EmptyStringArray_NotMeaningful() =>
            Assert.False(TagValuePolicy.IsMeaningful(Tags.Genres + System.Array.Empty<string>()));

        [Fact]
        public void AlbumUnknownPleasures_Meaningful() =>
            Assert.True(TagValuePolicy.IsMeaningful(Tags.Album + "Unknown Pleasures"));

        [Fact]
        public void RealAlbum_Meaningful() =>
            Assert.True(TagValuePolicy.IsMeaningful(Tags.Album + "Album X"));
    }
}
