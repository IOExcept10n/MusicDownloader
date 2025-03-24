// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents the track tagging service.
    /// </summary>
    internal class TagService
    {
        private static ILogger<TagService>? logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagService"/> class.
        /// </summary>
        /// <param name="services">A service provider to access services.</param>
        /// <param name="logger">The logger to use for logging operations.</param>
        public TagService(IServiceProvider services, ILogger<TagService> logger)
        {
            TagService.logger = logger;
            LyricsProviders = [.. services.GetServices<ILyricsProvider>()];
            DetailsProviders = [.. services.GetServices<IDetailProvider>()];
            logger.LogInformation(
                "TagService initialized with {LyricsProviderCount} lyrics providers and {DetailProviderCount} detail providers",
                LyricsProviders.Count,
                DetailsProviders.Count);
        }

        /// <summary>
        /// Gets all the available lyrics providers.
        /// </summary>
        public ObservableCollection<ILyricsProvider> LyricsProviders { get; }

        /// <summary>
        /// Gets all the available detail providers.
        /// </summary>
        public ObservableCollection<IDetailProvider> DetailsProviders { get; }

        /// <summary>
        /// Combines the results of a tagging operation with existing track details.
        /// </summary>
        /// <param name="settings">The application configuration settings.</param>
        /// <param name="original">The original track details.</param>
        /// <param name="result">The result of the tagging operation.</param>
        /// <returns>A new instance of <see cref="TrackDetails"/> containing the combined information.</returns>
        public static TrackDetails CombineResults(AppConfig settings, TrackDetails original, TaggingResult result)
        {
            ArgumentNullException.ThrowIfNull(original);
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(settings);

            logger?.LogDebug("Combining results for track: {TrackName}", original.FormedTrackName);
            var combined = new TrackDetails(original);

            if (settings.RewriteMetadata)
            {
                logger?.LogDebug("Rewriting metadata for track: {TrackName}", original.FormedTrackName);
                foreach (var tag in result.Details)
                {
                    combined.SetTag(tag.Name, tag.Value);
                }
            }
            else
            {
                logger?.LogDebug("Preserving original metadata for track: {TrackName}", original.FormedTrackName);
                foreach (var tag in result.Details)
                {
                    if (!combined.ContainsKey(tag.Name))
                    {
                        combined.SetTag(tag.Name, tag.Value);
                    }
                }
            }

            logger?.LogDebug("Combined results for track: {TrackName}", original.FormedTrackName);
            return combined;
        }

        /// <summary>
        /// Tags the specified track with all available providers.
        /// </summary>
        /// <param name="track">Track instance to tag.</param>
        /// <returns>Result of the track tagging.</returns>
        public async Task<TaggingResult> TagAsync(TrackDetails track)
        {
            logger?.LogInformation("Starting tagging process for track: {TrackTitle} by {Artist}", track.FormedTitle, track.FormedArtistString);
            ConflictsCollection conflicts = [];
            int detailProviderCount = 0;
            int lyricsProviderCount = 0;

            // Step 1: search for details:
            foreach (var provider in DetailsProviders)
            {
                try
                {
                    logger?.LogDebug("Searching for track details using provider: {ProviderName}", provider.GetType().Name);
                    Stopwatch sw = Stopwatch.StartNew();
                    var details = await provider.SearchTrackDetailsAsync(track);
                    sw.Stop();
                    logger?.LogDebug("Provider {ProviderName} completed in {ElapsedMilliseconds}ms", provider.GetType().Name, sw.ElapsedMilliseconds);

                    if (details == null)
                    {
                        logger?.LogDebug("Provider {ProviderName} returned no details", provider.GetType().Name);
                        continue;
                    }

                    logger?.LogDebug("Provider {ProviderName} found {DetailCount} details", provider.GetType().Name, details.Count);
                    foreach (var tag in details)
                    {
                        if (!conflicts.TryGetValue(tag.Name, out var conflict))
                        {
                            logger?.LogTrace("Creating new conflict for tag: {TagName}", tag.Name);
                            conflicts.Add(conflict = new(track, [], tag.Name));
                        }

                        conflict.Accumulate(tag);
                        logger?.LogTrace("Accumulated tag value for: {TagName}", tag.Name);
                    }

                    detailProviderCount++;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error getting details from provider {ProviderName}", provider.GetType().Name);
                }
            }

            // Step 2: search for lyrics
            foreach (var provider in LyricsProviders)
            {
                try
                {
                    logger?.LogDebug("Searching for lyrics using provider: {ProviderName}", provider.GetType().Name);
                    Stopwatch sw = Stopwatch.StartNew();
                    string? lyrics = await provider.SearchLyricsAsync(track);
                    sw.Stop();
                    logger?.LogDebug("Provider {ProviderName} completed in {ElapsedMilliseconds}ms", provider.GetType().Name, sw.ElapsedMilliseconds);

                    if (lyrics == null)
                    {
                        logger?.LogDebug("Provider {ProviderName} returned no lyrics", provider.GetType().Name);
                        continue;
                    }

                    logger?.LogDebug("Provider {ProviderName} found lyrics", provider.GetType().Name);
                    var lyricsTag = Tags.Lyrics + lyrics;
                    if (!conflicts.TryGetValue(lyricsTag.Name, out var conflict))
                    {
                        logger?.LogTrace("Creating new conflict for lyrics tag");
                        conflicts.Add(conflict = new(track, [], lyricsTag.Name));
                    }

                    conflict.Accumulate(lyricsTag);
                    logger?.LogTrace("Accumulated lyrics value");
                    lyricsProviderCount++;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error getting lyrics from provider {ProviderName}", provider.GetType().Name);
                }
            }

            var result = conflicts.AutoResolve();
            logger?.LogInformation(
                "Tagging process completed. Found {ConflictCount} conflicts. Successful providers: {DetailProviderCount} details, {LyricsProviderCount} lyrics",
                conflicts.Count,
                detailProviderCount,
                lyricsProviderCount);
            return new(result, conflicts);
        }
    }
}