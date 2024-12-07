// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for managing a user's online tracks library.
    /// This class is responsible for interacting with various services to load,
    /// download, and manage metadata for online tracks.
    /// </summary>
    /// <param name="provider">An instance of <see cref="IMediaProvider"/> used to access and download tracks.</param>
    /// <param name="loader">An instance of <see cref="TracksAsyncLoader"/> responsible for loading tracks asynchronously.</param>
    /// <param name="library">An instance of <see cref="MediaLibrary"/> used to check the status of downloaded tracks.</param>
    /// <param name="settings">An instance of <see cref="AppConfig"/> that contains application settings for loading tracks.</param>
    /// <param name="tagger">An instance of <see cref="TagService"/> used to retrieve and manage metadata for tracks.</param>
    internal partial class OnlineLibViewModel(IMediaProvider provider, TracksAsyncLoader loader, MediaLibrary library, AppConfig settings, TagService tagger) : ViewModelBase
    {
        /// <summary>
        /// Gets the loader instance used to load tracks.
        /// </summary>
        public TracksAsyncLoader Loader => loader;

        /// <summary>
        /// Gets the media provider instance used to access tracks.
        /// </summary>
        public IMediaProvider Provider => provider;

        /// <summary>
        /// Gets a read-only collection of online track view models.
        /// Each track is represented as an <see cref="OnlineTrackViewModel"/> instance.
        /// </summary>
        public ReadOnlyCollection<OnlineTrackViewModel> Tracks { get; } = loader.LoadedTracks.Project(x => new OnlineTrackViewModel(x, library, settings));

        /// <summary>
        /// Downloads a specified track and optionally tags it with metadata.
        /// </summary>
        /// <param name="track">The <see cref="OnlineTrackViewModel"/> representing the track to download.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the <see cref="TrackDetails"/> of the downloaded track, or null if the download failed.
        /// </returns>
        public async Task<TrackDetails?> DownloadTrack(OnlineTrackViewModel track)
        {
            track.Model.SetTag(nameof(VirtualTags.LoadingState), TrackLoadingState.Loading);
            var result = await provider.DownloadTrackAsync(track.Model, settings.UnsortedTracksPath);

            if (result == null)
            {
                track.Model.SetTag(nameof(VirtualTags.LoadingState), TrackLoadingState.None);
                return null;
            }

            if (settings.AutoTagOnDownload)
            {
                try
                {
                    var taggingResult = await tagger.TagAsync(result);
                    if (taggingResult.Details != null)
                    {
                        result = TagService.CombineResults(settings, result, taggingResult);
                    }

                    MetadataManager.SaveMetadata(result);
                }
                catch
                {
                    return null;
                }
            }

            track.Model.SetTag(nameof(VirtualTags.LoadingState), TrackLoadingState.Exists);
            return result;
        }
    }
}