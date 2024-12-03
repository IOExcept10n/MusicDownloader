using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class MyTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger) : ViewModelBase
    {
        public AppConfig Settings => settings;

        public AutoTaggingViewModel AutoTagger => autoTagger;

        public ObservableProjection<TrackDetails, TrackViewModel> Tracks { get; } = library.AllTracks.Project(x => new TrackViewModel(x), (y, x) => y.Model == x);
    }
}
