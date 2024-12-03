using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class OnlineLibViewModel(IMediaProvider provider, TracksAsyncLoader loader, MediaLibrary library, AppConfig settings, TagService tagger) : ViewModelBase
    {
        public IMediaProvider Provider => provider;

        public TracksAsyncLoader Loader => loader;

        public ReadOnlyCollection<OnlineTrackViewModel> Tracks { get; } = loader.LoadedTracks.Project(x => new OnlineTrackViewModel(x, library, settings));

        public async Task<TrackDetails?> DownloadTrack(OnlineTrackViewModel track)
        {
            var result = await provider.DownloadTrackAsync(track.Model, settings.UnsortedTracksPath);
            if (result == null)
                return null;
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
