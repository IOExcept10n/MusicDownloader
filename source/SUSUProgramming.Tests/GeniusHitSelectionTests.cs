// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;

namespace SUSUProgramming.Tests
{
    public class GeniusHitSelectionTests
    {
        private static GeniusHit Hit(int id, string title, string artists) => new(id, title, artists, $"https://genius.com/{id}");

        [Fact]
        public void PickMatchingHit_FirstIsAnotherSong_ReturnsSecond()
        {
            var hits = new[] { Hit(1, "Other Song", "Artist"), Hit(2, "Song", "Artist") };

            var picked = GeniusProvider.PickMatchingHit(hits, "Song", "Artist");

            Assert.Equal(2, picked?.Id);
        }

        [Fact]
        public void PickMatchingHit_NothingMatches_ReturnsNull()
        {
            var hits = new[] { Hit(1, "Other Song", "Artist"), Hit(2, "Song", "Someone Else") };

            Assert.Null(GeniusProvider.PickMatchingHit(hits, "Song", "Artist"));
        }

        [Fact]
        public void PickMatchingHit_IgnoresCaseAndFeaturing()
        {
            var hits = new[] { Hit(7, "SONG (feat. Guest)", "artist, Guest") };

            var picked = GeniusProvider.PickMatchingHit(hits, "Song", "Artist");

            Assert.Equal(7, picked?.Id);
        }

        [Fact]
        public void PickMatchingHit_OnlyFirstFiveAreChecked()
        {
            var hits = Enumerable.Range(1, 5).Select(i => Hit(i, "Other " + i, "Artist")).Append(Hit(6, "Song", "Artist"));

            Assert.Null(GeniusProvider.PickMatchingHit(hits, "Song", "Artist"));
        }
    }
}
