// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Linq;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Defines the two levels of tag completeness and the "ready" rule used by the tagging results.
    /// </summary>
    internal static class TagCompleteness
    {
        /// <summary>
        /// Gets names of the tags without which a track is considered untagged.
        /// </summary>
        public static readonly string[] Required = [Tags.Title.Name, Tags.Performers.Name];

        /// <summary>
        /// Gets names of the tags that (together with a cover) make a track complete.
        /// </summary>
        public static readonly string[] Recommended = [Tags.Album.Name, Tags.Genres.Name, Tags.Year.Name, Tags.Track.Name];

        private static readonly string[] Extras =
        [
            Tags.Lyrics.Name,
            Tags.Description.Name,
            Tags.Comment.Name,
            Tags.Subtitle.Name,
            Tags.Disc.Name,
            Tags.DiscCount.Name,
            Tags.AlbumArtists.Name,
            Tags.PerformersRole.Name,
            Tags.TrackCount.Name,
        ];

        /// <summary>
        /// Evaluates the completeness level of the track.
        /// </summary>
        /// <param name="track">Track details to check.</param>
        /// <returns><see cref="TrackProcessingState.Untagged"/>, <see cref="TrackProcessingState.Incomplete"/> or <see cref="TrackProcessingState.Complete"/>.</returns>
        public static TrackProcessingState Evaluate(TrackDetails track)
        {
            if (!Required.All(track.Contains))
                return TrackProcessingState.Untagged;

            return Recommended.All(track.Contains) && track.HasCover
                ? TrackProcessingState.Complete
                : TrackProcessingState.Incomplete;
        }

        /// <summary>
        /// Determines whether the track can be applied without review: required tags are filled and nothing is in conflict.
        /// Completeness of the recommended tags does not matter here.
        /// </summary>
        /// <param name="track">Track details to check.</param>
        /// <param name="hasConflicts">Whether the track has unresolved tagging conflicts.</param>
        /// <returns><see langword="true"/> if the track is ready.</returns>
        public static bool IsReady(TrackDetails track, bool hasConflicts) =>
            !hasConflicts && Required.All(track.Contains);

        /// <summary>
        /// Counts the filled tags that belong to neither completeness level (lyrics, comment, disc info etc.).
        /// </summary>
        /// <param name="track">Track details to check.</param>
        /// <returns>Number of filled extra tags.</returns>
        public static int ExtrasFilled(TrackDetails track) => Extras.Count(track.Contains);
    }
}
