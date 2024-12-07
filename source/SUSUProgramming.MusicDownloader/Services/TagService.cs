// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents the track tagging service.
    /// </summary>
    /// <param name="services">A service provider to access services.</param>
    internal class TagService(IServiceProvider services)
    {
        /// <summary>
        /// Gets all the available lyrics providers.
        /// </summary>
        public ObservableCollection<ILyricsProvider> LyricsProviders { get; } = [.. services.GetServices<ILyricsProvider>()];

        /// <summary>
        /// Gets all the available detail providers.
        /// </summary>
        public ObservableCollection<IDetailProvider> DetailsProviders { get; } = [.. services.GetServices<IDetailProvider>()];

        /// <summary>
        /// Combines multiple track details using current app settings.
        /// </summary>
        /// <param name="settings">App settings instance to use.</param>
        /// <param name="original">Original track details instance.</param>
        /// <param name="result">New track details instance.</param>
        /// <returns>An instance of the combined track details.</returns>
        public static TrackDetails CombineResults(AppConfig settings, TrackDetails original, TaggingResult result)
        {
            TrackDetails details;
            if (settings.RewriteMetadata)
            {
                details = TrackDetails.ReplaceWith(original, result.Details);
            }
            else
            {
                details = TrackDetails.UnionWith(original, result.Details);
            }

            return details;
        }

        /// <summary>
        /// Tags the specified track with all available providers.
        /// </summary>
        /// <param name="track">Track instance to tag.</param>
        /// <returns>Result of the track tagging.</returns>
        public async Task<TaggingResult> TagAsync(TrackDetails track)
        {
            ConflictsCollection conflicts = [];

            // Step 1: search for details:
            foreach (var provider in DetailsProviders)
            {
                var details = await provider.SearchTrackDetailsAsync(track);
                if (details == null) continue;
                foreach (var tag in details)
                {
                    if (!conflicts.TryGetValue(tag.Name, out var conflict))
                    {
                        conflicts.Add(conflict = new(track, [], tag.Name));
                    }

                    conflict.FoundData.Add(tag);
                }
            }

            // Step 2: search for lyrics
            foreach (var provider in LyricsProviders)
            {
                string? lyrics = await provider.SearchLyricsAsync(track);
                if (lyrics == null) continue;
                var lyricsTag = Tags.Lyrics + lyrics;
                if (!conflicts.TryGetValue(lyricsTag.Name, out var conflict))
                {
                    conflicts.Add(conflict = new(track, [], lyricsTag.Name));
                }

                conflict.FoundData.Add(lyricsTag);
            }

            var result = conflicts.AutoResolve();
            return new(result, conflicts);
        }
    }
}