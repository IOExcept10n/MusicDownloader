// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.Tests
{
    public class TrackDetailsTests
    {
        [Fact]
        public void TrackDetails_EmptyConstructor_ShouldCreateEmptyCollection()
        {
            // Act
            var details = new TrackDetails();

            // Assert
            Assert.Empty(details);
        }

        [Fact]
        public void TrackDetails_ConstructorWithSource_ShouldInitializeWithTags()
        {
            // Arrange
            var sourceTags = new[]
            {
                new VirtualTag<string>("Title") + "Test Title",
                new VirtualTag<string>("Artist") + "Test Artist"
            };

            // Act
            var details = new TrackDetails(sourceTags);

            // Assert
            Assert.Equal(2, details.Count);
            Assert.Equal("Test Title", details.GetTag<string>("Title"));
            Assert.Equal("Test Artist", details.GetTag<string>("Artist"));
        }

        [Fact]
        public void TrackDetails_SetTag_ShouldAddNewTag()
        {
            // Arrange
            var details = new TrackDetails();

            // Act
            details.SetTag("Title", "Test Title");

            // Assert
            Assert.Single(details);
            Assert.Equal("Test Title", details.GetTag<string>("Title"));
        }

        [Fact]
        public void TrackDetails_SetTag_ShouldUpdateExistingTag()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Original Title");

            // Act
            details.SetTag("Title", "Updated Title");

            // Assert
            Assert.Single(details);
            Assert.Equal("Updated Title", details.GetTag<string>("Title"));
        }

        [Fact]
        public void TrackDetails_GetTag_ShouldReturnCorrectValue()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Album", "The best");

            // Act
            var album = details.GetTag<string>("Album");

            // Assert
            Assert.Equal("The best", album);
        }

        [Fact]
        public void TrackDetails_TryGetTag_ShouldReturnTrueForExistingTag()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Test Title");

            // Act
            bool success = details.TryGetTag("Title", out string? title);

            // Assert
            Assert.True(success);
            Assert.Equal("Test Title", title);
        }

        [Fact]
        public void TrackDetails_TryGetTag_ShouldReturnFalseForNonExistingTag()
        {
            // Arrange
            var details = new TrackDetails();

            // Act
            bool success = details.TryGetTag("NonExisting", out string? value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TrackDetails_Remove_ShouldRemoveTag()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Test Title");

            // Act
            details.Remove("Title");

            // Assert
            Assert.Empty(details);
            Assert.False(details.TryGetTag<string>("Title", out _));
        }

        [Fact]
        public void TrackDetails_Clear_ShouldRemoveAllTags()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Test Title");
            details.SetTag("Artist", "Test Artist");

            // Act
            details.Clear();

            // Assert
            Assert.Empty(details);
        }

        [Fact]
        public void TrackDetails_ContainsKey_ShouldReturnTrueForExistingTag()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Test Title");

            // Act
            bool contains = details.ContainsKey("Title");

            // Assert
            Assert.True(contains);
        }

        [Fact]
        public void TrackDetails_ContainsKey_ShouldReturnFalseForNonExistingTag()
        {
            // Arrange
            var details = new TrackDetails();

            // Act
            bool contains = details.ContainsKey("NonExisting");

            // Assert
            Assert.False(contains);
        }

        [Fact]
        public void TrackDetails_CollectionChanged_ShouldRaiseEventOnAdd()
        {
            // Arrange
            var details = new TrackDetails();
            bool eventRaised = false;
            details.CollectionChanged += (s, e) => eventRaised = true;

            // Act
            details.SetTag("Title", "Test Title");

            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void TrackDetails_CollectionChanged_ShouldRaiseEventOnRemove()
        {
            // Arrange
            var details = new TrackDetails();
            details.SetTag("Title", "Test Title");
            bool eventRaised = false;
            details.CollectionChanged += (s, e) => eventRaised = true;

            // Act
            details.Remove("Title");

            // Assert
            Assert.True(eventRaised);
        }
    }
} 