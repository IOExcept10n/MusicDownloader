// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Provides a collection of predefined tags for media items, such as albums and tracks.
    /// This class contains static properties representing various tags and methods for tag manipulation.
    /// </summary>
    internal static class Tags
    {
        /// <summary>
        /// Gets a tag representing the album name.
        /// </summary>
        public static Tag<string> Album { get; } = new(x => x.Album);

        /// <summary>
        /// Gets a tag representing the artists of the album.
        /// </summary>
        public static Tag<string[]> AlbumArtists { get; } = new(x => x.AlbumArtists);

        /// <summary>
        /// Gets a tag representing comments associated with the media item.
        /// </summary>
        public static Tag<string> Comment { get; } = new(x => x.Comment);

        /// <summary>
        /// Gets a tag representing a description of the media item.
        /// </summary>
        public static Tag<string> Description { get; } = new(x => x.Description);

        /// <summary>
        /// Gets a tag representing the disc number of the media item.
        /// </summary>
        public static Tag<uint> Disc { get; } = new(x => x.Disc);

        /// <summary>
        /// Gets a tag representing the total number of discs for the media item.
        /// </summary>
        public static Tag<uint> DiscCount { get; } = new(x => x.DiscCount);

        /// <summary>
        /// Gets a tag representing the genres associated with the media item.
        /// </summary>
        public static Tag<string[]> Genres { get; } = new(x => x.Genres);

        /// <summary>
        /// Gets a tag representing the performers of the media item.
        /// </summary>
        public static Tag<string[]> Performers { get; } = new(x => x.Performers);

        /// <summary>
        /// Gets a tag representing the roles of the performers in the media item.
        /// </summary>
        public static Tag<string[]> PerformersRole { get; } = new(x => x.PerformersRole);

        /// <summary>
        /// Gets a tag representing the subtitle of the media item.
        /// </summary>
        public static Tag<string> Subtitle { get; } = new(x => x.Subtitle);

        /// <summary>
        /// Gets a tag representing the title of the media item.
        /// </summary>
        public static Tag<string> Title { get; } = new(x => x.Title);

        /// <summary>
        /// Gets a tag representing the track number of the media item.
        /// </summary>
        public static Tag<uint> Track { get; } = new(x => x.Track);

        /// <summary>
        /// Gets a tag representing the total number of tracks for the media item.
        /// </summary>
        public static Tag<uint> TrackCount { get; } = new(x => x.TrackCount);

        /// <summary>
        /// Gets a tag representing the year of release for the media item.
        /// </summary>
        public static Tag<uint> Year { get; } = new(x => x.Year);

        /// <summary>
        /// Gets a tag representing the lyrics of the media item.
        /// </summary>
        public static Tag<string> Lyrics { get; } = new(x => x.Lyrics);

        /// <summary>
        /// Gets an array of all predefined tags, including a common cover tag.
        /// This method clones the tags to avoid the overhead of compiling lambdas for access.
        /// </summary>
        public static ITag[] AllTags =>
        [
            Album with { },
            AlbumArtists with { },
            Comment with { },
            Description with { },
            Disc with { },
            DiscCount with { },
            Genres with { },
            Lyrics with { },
            Performers with { },
            PerformersRole with { },
            Subtitle with { },
            Title with { },
            Track with { },
            TrackCount with { },
            Year with { },

            // Just because cover is a common tag too
            new CoverTag(),
        ];

        /// <summary>
        /// Creates a new tag based on a prototype tag with a specified value.
        /// </summary>
        /// <typeparam name="T">The type of the tag value.</typeparam>
        /// <param name="prototype">The prototype tag to clone.</param>
        /// <param name="value">The value to set for the new tag.</param>
        /// <returns>A new tag with the specified value.</returns>
        public static Tag<T> Create<T>(Tag<T> prototype, T? value) => prototype with { Value = value! };
    }
}