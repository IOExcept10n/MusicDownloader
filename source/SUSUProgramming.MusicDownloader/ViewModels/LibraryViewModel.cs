using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Services;
using System.Collections.ObjectModel;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    [Scoped]
    internal partial class LibraryViewModel : ViewModelBase
    {
        [ObservableProperty] private ObservableCollection<TrackViewModel> selectedTracks = [];

        public AppConfig Settings { get; }

        public MultiTrackViewModel EditingModel { get; }

        public LibraryViewModel(AppConfig settings)
        {
            Settings = settings;
            EditingModel = new(selectedTracks);
        }
    }
}
