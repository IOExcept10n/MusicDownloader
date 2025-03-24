// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;
using SUSUProgramming.MusicDownloader.Music.Metadata.LyricsProviders;
using SUSUProgramming.MusicDownloader.Services;
using Xunit;

namespace SUSUProgramming.Tests
{
    public class TagServiceTests
    {
        private readonly Mock<ILogger<TagService>> loggerMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly TagService tagService;

        public TagServiceTests()
        {
            loggerMock = new Mock<ILogger<TagService>>();
            serviceProviderMock = new Mock<IServiceProvider>();
            tagService = new TagService(serviceProviderMock.Object, loggerMock.Object);
        }

        [Fact]
        public void TagService_Initialization_ShouldHaveNoProviders()
        {
            // Assert
            Assert.Empty(tagService.LyricsProviders);
            Assert.Empty(tagService.DetailsProviders);
        }

        [Fact]
        public void TagService_WithProviders_ShouldHaveCorrectCount()
        {
            // Arrange
            var lyricsProvider = new Mock<ILyricsProvider>();
            var detailProvider = new Mock<IDetailProvider>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(lyricsProvider.Object);
            serviceCollection.AddSingleton(detailProvider.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<ILyricsProvider>)))
                .Returns(new[] { lyricsProvider.Object });
            serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IDetailProvider>)))
                .Returns(new[] { detailProvider.Object });

            // Act
            var service = new TagService(serviceProviderMock.Object, loggerMock.Object);

            // Assert
            Assert.Single(service.LyricsProviders);
            Assert.Single(service.DetailsProviders);
        }

        [Fact]
        public void TagService_CombineResults_ShouldMergeTagsCorrectly()
        {
            // Arrange
            var original = new TrackDetails();
            original.SetTag("Title", "Original Title");
            original.SetTag("Artist", "Original Artist");

            var result = new TaggingResult(new(), []);
            result.Details.SetTag("Title", "New Title");
            result.Details.SetTag("Year", "2024");

            var settings = new AppConfig(".");

            // Act
            var combined = TagService.CombineResults(settings, original, result);

            // Assert
            Assert.Equal("New Title", combined.GetTag<string>("Title"));
            Assert.Equal("Original Artist", combined.GetTag<string>("Artist"));
            Assert.Equal("2024", combined.GetTag<string>("Year"));
        }

        [Fact]
        public void TagService_CombineResults_ShouldPreserveOriginalTagsWhenNoNewData()
        {
            // Arrange
            var original = new TrackDetails();
            original.SetTag("Title", "Original Title");
            original.SetTag("Artist", "Original Artist");

            var result = new TaggingResult(new(), []);

            var settings = new AppConfig(".");

            // Act
            var combined = TagService.CombineResults(settings, original, result);

            // Assert
            Assert.Equal("Original Title", combined.GetTag<string>("Title"));
            Assert.Equal("Original Artist", combined.GetTag<string>("Artist"));
        }

        [Fact]
        public void TagService_CombineResults_ShouldHandleEmptyInputs()
        {
            // Arrange
            var original = new TrackDetails();
            var result = new TaggingResult(new(), []);
            var settings = new AppConfig(".");

            // Act
            var combined = TagService.CombineResults(settings, original, result);

            // Assert
            Assert.Empty(combined);
        }

        [Fact]
        public void TagService_CombineResults_ShouldHandleNullInputs()
        {
            // Arrange
            var settings = new AppConfig(".");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TagService.CombineResults(settings, null!, new TaggingResult(new(), [])));
            Assert.Throws<ArgumentNullException>(() => TagService.CombineResults(settings, new TrackDetails(), null!));
            Assert.Throws<ArgumentNullException>(() => TagService.CombineResults(null!, new TrackDetails(), new TaggingResult(new(), [])));
        }
    }
} 