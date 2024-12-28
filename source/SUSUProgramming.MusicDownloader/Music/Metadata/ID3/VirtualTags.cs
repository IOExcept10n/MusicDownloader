using System;
using System.Collections.Generic;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Provides a collection of virtual tags for media tracks, representing metadata that won't be stored in track file.
    /// </summary>
    internal class VirtualTags
    {
        /// <summary>
        /// Gets a virtual tag representing the URI of the track.
        /// </summary>
        public static VirtualTag<Uri> TrackUri => new(nameof(Uri));

        /// <summary>
        /// Gets a virtual tag representing the file path of the track.
        /// </summary>
        public static VirtualTag<string> TrackFilePath => new(nameof(TrackDetails.FilePath));

        /// <summary>
        /// Gets a virtual tag representing the processing state of the track.
        /// </summary>
        public static VirtualTag<TrackProcessingState> State => new(nameof(State));

        /// <summary>
        /// Gets a virtual tag representing the loading state of the track.
        /// </summary>
        public static VirtualTag<TrackLoadingState> LoadingState => new(nameof(LoadingState));

        /// <summary>
        /// Gets a virtual tag representing an incremental number associated with the track.
        /// </summary>
        public static VirtualTag<int> IncrementalNumber => new(nameof(IncrementalNumber));

        /// <summary>
        /// Gets a virtual tag indicating whether the track has an associated cover.
        /// </summary>
        public static VirtualTag<bool> HasCover => new(nameof(HasCover));

        /// <summary>
        /// Gets number of track listeners in <a href="https://last.fm">Last.FM</a> service.
        /// </summary>
        public static VirtualTag<int> ListenersCount => new(nameof(ListenersCount));

        /// <summary>
        /// Gets a list of all virtual tags defined in this class.
        /// </summary>
        public static List<VirtualTag> AllTags =>
        [
            TrackUri,
            TrackFilePath,
            State,
            LoadingState,
            IncrementalNumber,
            HasCover,
        ];
    }
}
