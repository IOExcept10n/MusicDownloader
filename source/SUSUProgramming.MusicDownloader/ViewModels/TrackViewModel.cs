// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Localization;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.DetailProviders;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Defines a state of the track listening rate based on its listeners on <a href="https://last.fm">Last.FM</a>.
    /// </summary>
    public enum TrackListeningIndicator
    {
        /// <summary>
        /// Track state is unknown due to API unavailability.
        /// </summary>
        Unknown,

        /// <summary>
        /// Track not found on <a href="https://last.fm">Last.FM</a>.
        /// </summary>
        NotFound,

        /// <summary>
        /// Track listeners count is less than <see langword="10"/> so its tags can be incorrect.
        /// </summary>
        MayBeIncorrect,

        /// <summary>
        /// Track listeners count is between <see langword="10"/> and <see langword="100"/> so it's recognized as unpopular.
        /// </summary>
        LowListened,

        /// <summary>
        /// Track listeners count is between <see langword="100"/> and <see langword="1000"/> so it's like common track.
        /// </summary>
        Common,

        /// <summary>
        /// Track listeners count is between <see langword="1000"/> and <see langword="10000"/> so it's recognized as frequently listened.
        /// </summary>
        FrequentlyListened,

        /// <summary>
        /// Track listeners count is more than <see langword="10000"/> so it's recognized as popular.
        /// </summary>
        Popular,
    }

    /// <summary>
    /// Defines a state of the track processing.
    /// </summary>
    public enum TrackProcessingState
    {
        /// <summary>
        /// State of the track is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Track doesn't have any of required tags such as title or performers.
        /// </summary>
        Untagged,

        /// <summary>
        /// Some main tags haven't been filled.
        /// </summary>
        Incomplete,

        /// <summary>
        /// All main tags are filled.
        /// </summary>
        Complete,

        /// <summary>
        /// Track now is in processing state - auto tagger is working on it.
        /// </summary>
        Processing,

        /// <summary>
        /// Track have been processed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Auto tagger couldn't find tags for this track.
        /// </summary>
        TagsNotFound,

        /// <summary>
        /// Track processing failed.
        /// </summary>
        Fault,

        /// <summary>
        /// Track processing failed with conflict.
        /// </summary>
        Conflicting,
    }

    /// <summary>
    /// Represents a view model for the track details.
    /// </summary>
    internal partial class TrackViewModel : ViewModelBase
    {
        private static readonly Bitmap EmptyCover = new(AssetLoader.Open(new("avares://SUSUProgramming.MusicDownloader/Assets/music.png")));
        private static readonly string[] RecommendedTags = [nameof(Album), nameof(Genres), nameof(Year), nameof(Track)];
        private static readonly string[] RequiredTags = [nameof(Title), nameof(Performers)];
        private static ILogger<TrackViewModel>? logger;
        private readonly TrackDetails track;
        private readonly DelayedNotifier listenStatusUpdater;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ListeningStateBrush))]
        [NotifyPropertyChangedFor(nameof(ListeningStateText))]
        private TrackListeningIndicator listeningState;

        private Bitmap? cover;
        private bool isCoverDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackViewModel"/> class.
        /// </summary>
        /// <param name="track">Track instance to use.</param>
        /// <param name="logger">The logger to use for logging operations.</param>
        public TrackViewModel(TrackDetails track, ILogger<TrackViewModel>? logger = null)
        {
            TrackViewModel.logger = logger;
            logger?.LogDebug("Initializing TrackViewModel for track: {TrackName}", track.FormedTrackName);
            this.track = track;
            listenStatusUpdater = new(
                async (t) =>
                {
                    t.ThrowIfCancellationRequested();
                    await Dispatcher.UIThread.AwaitWithPriority(UpdateListeningState(), DispatcherPriority.Background);
                },
                1000);
            track.PropertyChanged += OnTrackUpdated;
            track.CollectionChanged += OnTrackTagsUpdated;
            ResetState();
            logger?.LogDebug("TrackViewModel initialized for track: {TrackName}", track.FormedTrackName);
        }

        /// <summary>
        /// Gets or sets the album name of the track.
        /// </summary>
        public string? Album { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the array of album artists associated with the track.
        /// </summary>
        public string[]? AlbumArtists
        {
            get => Read<string[]>();
            set => Update(value);
        }

        /// <summary>
        /// Gets or sets a string representation of the album artists, joined by commas.
        /// </summary>
        public string? AlbumArtistsString
        {
            get => AlbumArtists == null ? null : string.Join(", ", AlbumArtists);
            set => AlbumArtists = value == null ? null : TrackNameParser.GetPerformers(value);
        }

        /// <summary>
        /// Gets or sets comments associated with the track.
        /// </summary>
        public string? Comment { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the URL of the cover image for the track.
        /// If the URL is null, the cover is removed from the track.
        /// </summary>
        public Uri? CoverUrl
        {
            get => track.CoverUri;
            set
            {
                // Blocking wait :)
                SetNewCoverAsync(value).Wait();
            }
        }

        /// <summary>
        /// Gets or sets a description of the track.
        /// </summary>
        public string? Description { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the disc number of the track.
        /// </summary>
        public uint Disc { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the total number of discs for the album.
        /// </summary>
        public uint DiscCount { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the array of genres associated with the track.
        /// </summary>
        public string[]? Genres { get => Read<string[]>(); set => Update(value); }

        /// <summary>
        /// Gets or sets a string representation of the genres, joined by commas or semicolons.
        /// </summary>
        public string? GenresString
        {
            get => Genres == null ? null : string.Join(", ", Genres);
            set => Genres = value?.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// Gets indication text for the current listening state.
        /// </summary>
        public string ListeningStateText => Resources.ResourceManager.GetString(ListeningState.ToString()) ?? "Unknown";

        /// <summary>
        /// Gets a brush for the current track listening indication.
        /// </summary>
        public SolidColorBrush ListeningStateBrush => new(ListeningState switch
        {
            TrackListeningIndicator.Unknown => Colors.Gray,
            TrackListeningIndicator.NotFound => Colors.Red,
            TrackListeningIndicator.MayBeIncorrect => Colors.Orange,
            TrackListeningIndicator.LowListened => Colors.Yellow,
            TrackListeningIndicator.Common => Colors.Green,
            TrackListeningIndicator.FrequentlyListened => Colors.BlueViolet,
            TrackListeningIndicator.Popular => Colors.DeepPink,
            _ => Colors.White,
        });

        /// <summary>
        /// Gets or sets the lyrics of the track.
        /// </summary>
        public string? Lyrics { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets the underlying track details model.
        /// </summary>
        public TrackDetails Model => track;

        /// <summary>
        /// Gets or sets the array of performers associated with the track.
        /// </summary>
        public string[]? Performers { get => Read<string[]>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the array of performer roles associated with the track.
        /// </summary>
        public string[]? PerformersRole { get => Read<string[]>(); set => Update(value); }

        /// <summary>
        /// Gets or sets a string representation of the performers, joined by commas.
        /// </summary>
        public string? PerformersString
        {
            get => Performers == null ? null : string.Join(", ", Performers);
            set => Performers = value == null ? null : TrackNameParser.GetPerformers(value);
        }

        /// <summary>
        /// Gets a brush for the current processing state.
        /// </summary>
        public SolidColorBrush ProcessingStateColor => new(State switch
        {
            TrackProcessingState.Unknown => Colors.Black,
            TrackProcessingState.Untagged => Colors.DarkGray,
            TrackProcessingState.Incomplete => Colors.YellowGreen,
            TrackProcessingState.Complete => Colors.Green,
            TrackProcessingState.Processing => Colors.Blue,
            TrackProcessingState.Success => Colors.Azure,
            TrackProcessingState.TagsNotFound => Colors.Yellow,
            TrackProcessingState.Conflicting => Colors.OrangeRed,
            TrackProcessingState.Fault => Colors.Red,
            _ => Colors.Violet,
        });

        /// <summary>
        /// Gets indication text for the current processing state.
        /// </summary>
        public string ProcessingStateText => Resources.ResourceManager.GetString(State.ToString()) ?? "Unknown";

        /// <summary>
        /// Gets or sets the current processing state of the track.
        /// </summary>
        public TrackProcessingState State { get => Read<TrackProcessingState>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the subtitle of the track.
        /// </summary>
        public string? Subtitle { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets the total number of tags associated with the track.
        /// </summary>
        public int TagsCount => track.Count(x => x is not VirtualTag and not CoverTag) + (track.HasCover ? 1 : 0);

        /// <summary>
        /// Gets or sets the title of the track.
        /// </summary>
        public string? Title { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the track number.
        /// </summary>
        public uint Track { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the total number of tracks in the album.
        /// </summary>
        public uint TrackCount { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets the cover image for the track.
        /// </summary>
        public Bitmap TrackCover
        {
            get
            {
                if (isCoverDirty)
                {
                    UpdateCover();
                }

                return cover ?? EmptyCover;
            }
        }

        /// <summary>
        /// Gets or sets the year the track was released.
        /// </summary>
        public uint Year { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Refreshes the track details and updates the UI.
        /// </summary>
        public void Refresh()
        {
            logger?.LogDebug("Refreshing track: {TrackName}", track.FormedTrackName);
            foreach (var property in RequiredTags)
            {
                OnPropertyChanged(property);
            }

            foreach (var property in RecommendedTags)
            {
                OnPropertyChanged(property);
            }

            logger?.LogDebug("Track refreshed: {TrackName}", track.FormedTrackName);
        }

        /// <summary>
        /// Resets the track state to its initial values.
        /// </summary>
        public void ResetState()
        {
            logger?.LogDebug("Resetting state for track: {TrackName}", track.FormedTrackName);
            listenStatusUpdater.NotifyUpdate();
            if (RequiredTags.All(track.Contains))
            {
                if (RecommendedTags.All(track.Contains) && track.HasCover)
                {
                    logger?.LogDebug("Track {TrackName} is complete", track.FormedTrackName);
                    State = TrackProcessingState.Complete;
                    return;
                }

                logger?.LogDebug("Track {TrackName} is incomplete", track.FormedTrackName);
                State = TrackProcessingState.Incomplete;
                return;
            }

            logger?.LogDebug("Track {TrackName} is untagged", track.FormedTrackName);
            State = TrackProcessingState.Untagged;
        }

        /// <summary>
        /// Saves the current track details.
        /// </summary>
        public void Save()
        {
            logger?.LogDebug("Saving track: {TrackName}", track.FormedTrackName);
            MetadataManager.SaveMetadata(Model);
            logger?.LogDebug("Track saved: {TrackName}", track.FormedTrackName);
        }

        /// <summary>
        /// Sets a new cover image for the track.
        /// </summary>
        /// <param name="value">The URL of the new cover image.</param>
        /// <returns>An asynchronous task that describes cover setting process.</returns>
        internal async Task SetNewCoverAsync(Uri? value)
        {
            logger?.LogDebug("Setting new cover for track: {TrackName}", track.FormedTrackName);
            if (value == CoverTag.InternalImagesURI)
            {
                logger?.LogDebug("Using internal cover image for track: {TrackName}", track.FormedTrackName);
                return;
            }

            if (value == null)
            {
                logger?.LogDebug("Removing cover for track: {TrackName}", track.FormedTrackName);
                if (track.TryGetValue(nameof(CoverTag.Cover), out var cover))
                    track.Remove(cover);
                return;
            }

            var http = App.Services.GetRequiredService<ApiHelper>().Client;
            if (!track.HasCover)
            {
                logger?.LogDebug("Downloading new cover for track: {TrackName}", track.FormedTrackName);
                track.Add((await CoverTag.DownloadCoverAsync(value!, http))!);
            }
            else
            {
                logger?.LogDebug("Updating existing cover for track: {TrackName}", track.FormedTrackName);
                await ((CoverTag)track[nameof(CoverTag.Cover)]).UpdateCoverAsync(value, http);
            }

            logger?.LogDebug("Cover set for track: {TrackName}", track.FormedTrackName);
        }

        /// <summary>
        /// Handles property change events from the track.
        /// </summary>
        /// <param name="e">Args for the property update.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(State))
            {
                logger?.LogDebug("Track state changed to {State} for track: {TrackName}", State, track.FormedTrackName);
            }
        }

        /// <summary>
        /// Updates the listening state of the track.
        /// </summary>
        private async Task UpdateListeningState()
        {
            logger?.LogDebug("Updating listening state for track: {TrackName}", track.FormedTrackName);

            // TODO: make Last.FM provider in a separate interface.
            var lastFmProvider = App.Services.GetServices<IDetailProvider>().OfType<LastFmProvider>().FirstOrDefault();
            if (lastFmProvider == null)
            {
                logger?.LogWarning("Last.FM provider not found for track: {TrackName}", track.FormedTrackName);
                ListeningState = TrackListeningIndicator.Unknown;
                return;
            }

            var listensCount = await lastFmProvider.GetListensCountAsync(Model);
            logger?.LogDebug("Retrieved listens count {ListensCount} for track: {TrackName}", listensCount, track.FormedTrackName);
            ListeningState = listensCount switch
            {
                -1 => TrackListeningIndicator.NotFound,
                < 500 => TrackListeningIndicator.MayBeIncorrect,
                < 50_000 => TrackListeningIndicator.LowListened,
                < 1_000_000 => TrackListeningIndicator.Common,
                < 5_000_000 => TrackListeningIndicator.FrequentlyListened,
                >= 5_000_000 => TrackListeningIndicator.Popular,
                _ => TrackListeningIndicator.Unknown,
            };
            logger?.LogDebug("Updated listening state to {ListeningState} for track: {TrackName}", ListeningState, track.FormedTrackName);
        }

        /// <summary>
        /// Handles collection change events from the track.
        /// </summary>
        private void OnTrackTagsUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            logger?.LogDebug("Track tags updated for track: {TrackName}", track.FormedTrackName);
            OnPropertyChanged(nameof(TagsCount));
        }

        /// <summary>
        /// Handles property change events from the track.
        /// </summary>
        private void OnTrackUpdated(object? sender, PropertyChangedEventArgs e)
        {
            logger?.LogDebug("Track property {Property} updated for track: {TrackName}", e.PropertyName, track.FormedTrackName);
            OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Reads a value from the track details.
        /// </summary>
        private T? Read<T>([CallerMemberName] string? callerProperty = null)
        {
            if (callerProperty == null)
                return default;

            track.TryGetTag<T>(callerProperty, out var value);
            logger?.LogTrace("Reading {Property} for track: {TrackName}, Value: {Value}", callerProperty, track.FormedTrackName, value);
            return value;
        }

        /// <summary>
        /// Updates a value in the track details.
        /// </summary>
        private void Update<T>(T value, [CallerMemberName] string? callerProperty = null)
        {
            if (callerProperty == null)
                return;

            logger?.LogTrace("Updating {Property} for track: {TrackName}, Value: {Value}", callerProperty, track.FormedTrackName, value);
            track.SetTag(callerProperty, value);
        }

        /// <summary>
        /// Updates the cover image for the track.
        /// </summary>
        private void UpdateCover()
        {
            logger?.LogDebug("Updating cover for track: {TrackName}", track.FormedTrackName);
            cover?.Dispose();
            var data = track.OfType<CoverTag>().FirstOrDefault()?.Cover?.Data;
            if (data == null)
            {
                logger?.LogDebug("No cover data found for track: {TrackName}", track.FormedTrackName);
                return;
            }

            using var memory = new MemoryStream(data.Data);
            cover = new Bitmap(memory);
            logger?.LogDebug("Cover updated for track: {TrackName}", track.FormedTrackName);
        }
    }
}