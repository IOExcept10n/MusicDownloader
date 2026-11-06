// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.Tests
{
    public class WebSearchLauncherTests
    {
        [Fact]
        public void LyricsQuery_EscapesSpecialCharacters()
        {
            var uri = WebSearchLauncher.LyricsQuery("A B", "C&D");

            Assert.StartsWith("?q=", uri.Query);
            Assert.DoesNotContain("&D", uri.Query);
            string decoded = Uri.UnescapeDataString(uri.Query.Replace('+', ' '));
            Assert.Contains("A B", decoded);
            Assert.Contains("C&D", decoded);
        }

        [Fact]
        public void CoverQuery_IsAnImageSearch()
        {
            var uri = WebSearchLauncher.CoverQuery("Artist", "Song");

            Assert.Contains("tbm=isch", uri.Query);
            Assert.Contains("Artist", Uri.UnescapeDataString(uri.Query));
        }

        [Fact]
        public void Open_NonWebAddress_IsRejected() =>
            Assert.Throws<ArgumentException>(() => WebSearchLauncher.Open(new Uri("file:///C:/Windows/notepad.exe")));
    }
}
