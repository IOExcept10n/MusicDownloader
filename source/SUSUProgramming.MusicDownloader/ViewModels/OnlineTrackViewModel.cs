using Avalonia.Media;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.Services;
using System;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class OnlineTrackViewModel : ViewModelBase
    {
        public OnlineTrackViewModel(TrackDetails track, MediaLibrary library, AppConfig settings)
        {
            TrackLoadingState state = TrackLoadingState.None;
            if (settings.BlacklistedTrackNames.Contains(track.FormedTrackName))
            {
                state = TrackLoadingState.Ignored;
            }
            else if (library.ContainsTrack(track.FormedTrackName))
            {
                state = TrackLoadingState.Exists;
            }
            track.Add(VirtualTags.LoadingState + state);
            track.PropertyChanged += OnTrackUpdated;
            track.CollectionChanged += OnTrackTagsAdded;
            Model = track;
        }

        private void OnTrackTagsAdded(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectionColor));
        }

        private void OnTrackUpdated(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(nameof(SelectionColor));
        }

        public TrackDetails Model { get; }

        public string Title => Model.FormedTitle;

        public string Artist => Model.FormedArtistString;

        public bool CanDownload => Model.ContainsKey(nameof(Uri));

        public SolidColorBrush SelectionColor => new(Model.TryGetTag<TrackLoadingState>(nameof(VirtualTags.LoadingState), out var tag) switch
        {
            _ when tag is TrackLoadingState.Exists => Colors.LightGreen,
            _ when tag is TrackLoadingState.Ignored => Colors.Gray,
            { } => Colors.Transparent,
        });
    }
}
