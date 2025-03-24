// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Services;
using Xunit;

namespace SUSUProgramming.Tests
{
    public class AppConfigTests
    {
        private readonly string testConfigPath = "test_config.json";

        [Fact]
        public void AppConfig_DefaultConstructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var config = new AppConfig(testConfigPath);

            // Assert
            Assert.True(config.RewriteMetadata);
            Assert.True(config.AutoTagOnDownload);
            Assert.NotEmpty(config.TrackedPaths);
            Assert.Empty(config.BlacklistedPaths);
            Assert.Empty(config.GenresList);
            Assert.NotEmpty(config.SlashContainedPerformersList);
            Assert.Equal(".tokencache", config.TokenStoragePath);
            Assert.NotNull(config.UnsortedTracksPath);
        }

        [Fact]
        public void AppConfig_TrackedPaths_ShouldAddAndRemovePaths()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            var path = "C:\\Music";

            // Act
            config.TrackedPaths.Clear();
            config.TrackedPaths.Add(path);

            // Assert
            Assert.Single(config.TrackedPaths);
            Assert.Contains(path, config.TrackedPaths);

            // Act
            config.TrackedPaths.Remove(path);

            // Assert
            Assert.Empty(config.TrackedPaths);
            Assert.DoesNotContain(path, config.TrackedPaths);
        }

        [Fact]
        public void AppConfig_BlacklistedPaths_ShouldAddAndRemovePaths()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            var path = "C:\\Blacklist";

            // Act
            config.BlacklistedPaths.Add(path);

            // Assert
            Assert.Single(config.BlacklistedPaths);
            Assert.Contains(path, config.BlacklistedPaths);

            // Act
            config.BlacklistedPaths.Remove(path);

            // Assert
            Assert.Empty(config.BlacklistedPaths);
            Assert.DoesNotContain(path, config.BlacklistedPaths);
        }

        [Fact]
        public void AppConfig_GenresList_ShouldAddAndRemoveGenres()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            var genre = "Rock";

            // Act
            config.GenresList.Add(genre);

            // Assert
            Assert.Single(config.GenresList);
            Assert.Contains(genre, config.GenresList);

            // Act
            config.GenresList.Remove(genre);

            // Assert
            Assert.Empty(config.GenresList);
            Assert.DoesNotContain(genre, config.GenresList);
        }

        [Fact]
        public void AppConfig_SlashContainedPerformersList_ShouldContainDefaultValues()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);

            // Assert
            Assert.NotEmpty(config.SlashContainedPerformersList);
            Assert.Contains("AC/DC", config.SlashContainedPerformersList);
            Assert.Contains("K/DA", config.SlashContainedPerformersList);
        }

        [Fact]
        public void AppConfig_TokenStoragePath_ShouldSetAndGetPath()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            var path = "C:\\Tokens";

            // Act
            config.TokenStoragePath = path;

            // Assert
            Assert.Equal(path, config.TokenStoragePath);
        }

        [Fact]
        public void AppConfig_UnsortedTracksPath_ShouldSetAndGetPath()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            var path = "C:\\Unsorted";

            // Act
            config.UnsortedTracksPath = path;

            // Assert
            Assert.Equal(path, config.UnsortedTracksPath);
        }

        [Fact]
        public void AppConfig_CollectionChanged_ShouldRaiseEvent()
        {
            // Arrange
            var config = new AppConfig(testConfigPath);
            bool eventRaised = false;
            config.TrackedPaths.CollectionChanged += (s, e) => eventRaised = true;

            // Act
            config.TrackedPaths.Add("C:\\Music");

            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void AppConfig_LoadOrInitialize_ShouldCreateNewConfigIfFileDoesNotExist()
        {
            // Act
            var config = AppConfig.LoadOrInitialize(testConfigPath);

            // Assert
            Assert.NotNull(config);
            Assert.True(config.RewriteMetadata);
            Assert.True(config.AutoTagOnDownload);
        }
    }
} 