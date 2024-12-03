using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;
using System.Collections.ObjectModel;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class UnsortedTracksViewModel(MediaLibrary library, AppConfig settings, AutoTaggingViewModel autoTagger) : ViewModelBase
    {
        [ObservableProperty] private int selectedPathIndex;
        [ObservableProperty] private ObservableCollection<TrackViewModel> selectedItems = [];

        public AppConfig Settings => settings;

        public AutoTaggingViewModel AutoTagger => autoTagger;

        public ObservableProjection<TrackDetails, TrackViewModel> UnsortedTracks { get; } = ((ObservableCollection<TrackDetails>)library.Unsorted.Tracks).Project(x => new TrackViewModel(x), (y, x) => y.Model == x);

        [DependsOn(nameof(SelectedPathIndex))]
        public bool IsPathSelected => SelectedPathIndex >= 0 && SelectedPathIndex < settings.TrackedPaths.Count;

        public bool FullSelect
        {
            get => SelectedItems.Count == UnsortedTracks.Count;
            set
            {
                SelectedItems.Clear();
                if (value)
                {
                    foreach (var item in UnsortedTracks)
                    {
                        SelectedItems.Add(item);
                    }
                }
            }
        }
    }
}
