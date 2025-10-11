using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUSUProgramming.MusicDownloader.Persistence
{
    /// <summary>
    /// Represents statistics for the track.
    /// </summary>
    public class TrackStatsModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the track statistics entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the artist name(s) for the track.
        /// </summary>
        public required string ArtistString { get; set; }

        /// <summary>
        /// Gets or sets the title of the track.
        /// </summary>
        public required string TitleString { get; set; }

        /// <summary>
        /// Gets or sets the number of times the track has been listened to.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> if the listen count is unknown.
        /// </remarks>
        public int? ListenCount { get; set; }

        /// <summary>
        /// Gets or sets the path to which the track has been downloaded.
        /// </summary>
        public string? TrackPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the the track has been downloaded.
        /// </summary>
        [NotMapped]
        public bool IsDownloaded => !string.IsNullOrEmpty(TrackPath);
    }
}
