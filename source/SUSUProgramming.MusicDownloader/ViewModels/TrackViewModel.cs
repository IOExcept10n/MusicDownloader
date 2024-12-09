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
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
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
        private readonly TrackDetails track;
        private Bitmap? cover;
        private bool isCoverDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackViewModel"/> class.
        /// </summary>
        /// <param name="track">Track instance to use.</param>
        public TrackViewModel(TrackDetails track)
        {
            this.track = track;
            track.PropertyChanged += OnTrackUpdated;
            track.CollectionChanged += OnTrackTagsUpdated;
            ResetState();
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
        /// Gets or sets the lyrics of the track.
        /// </summary>
        public string? Lyrics { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets the model representing the track details.
        /// </summary>
        public TrackDetails Model => track;

        /// <summary>
        /// Gets or sets the array of performers associated with the track.
        /// </summary>
        public string[]? Performers { get => Read<string[]>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the array of roles for the performers.
        /// </summary>
        public string[]? PerformersRole { get => Read<string[]>(); set => Update(value); }

        /// <summary>
        /// Gets or sets a string representation of the performers, formatted as a single string.
        /// </summary>
        public string? PerformersString
        {
            get => track.FormedArtistString;
            set => Performers = value == null ? null : TrackNameParser.GetPerformers(value);
        }

        /// <summary>
        /// Gets the color representing the processing state of the track.
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
        /// Gets or sets the processing state of the track.
        /// </summary>
        public TrackProcessingState State { get => Read<TrackProcessingState>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the subtitle of the track.
        /// </summary>
        public string? Subtitle { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets the count of tags associated with the track, excluding virtual tags.
        /// </summary>
        public int TagsCount => track.Count(x => x is not VirtualTag);

        /// <summary>
        /// Gets or sets the title of the track.
        /// </summary>
        public string? Title { get => Read<string>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the track number within the album.
        /// </summary>
        public uint Track { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets or sets the total number of tracks in the album.
        /// </summary>
        public uint TrackCount { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Gets the cover image for the track. If the cover is marked as dirty, it will be updated.
        /// </summary>
        public Bitmap TrackCover
        {
            get
            {
                if (isCoverDirty)
                    UpdateCover();
                return track.HasCover ? (cover ?? EmptyCover) : EmptyCover;
            }
        }

        /// <summary>
        /// Gets or sets the year the track was released.
        /// </summary>
        public uint Year { get => Read<uint>(); set => Update(value); }

        /// <summary>
        /// Refreshes the properties of the track by notifying property changes for required and recommended tags.
        /// </summary>
        public void Refresh()
        {
            foreach (var property in RequiredTags)
            {
                OnPropertyChanged(property);
            }

            foreach (var property in RecommendedTags)
            {
                OnPropertyChanged(property);
            }
        }

        /// <summary>
        /// Resets the processing state of the track based on the presence of required and recommended tags.
        /// </summary>
        public void ResetState()
        {
            if (RequiredTags.All(track.Contains))
            {
                if (RecommendedTags.All(track.Contains) && (track.Contains(nameof(VirtualTags.HasCover)) || track.Contains(nameof(CoverTag.Cover))))
                {
                    State = TrackProcessingState.Complete;
                    return;
                }

                State = TrackProcessingState.Incomplete;
                return;
            }

            State = TrackProcessingState.Untagged;
        }

        /// <summary>
        /// Saves the current metadata of the track using the metadata manager.
        /// </summary>
        public void Save()
        {
            MetadataManager.SaveMetadata(Model);
        }

        /// <summary>
        /// Sets new cover to current track.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <returns>Task to wait for.</returns>
        internal async Task SetNewCoverAsync(Uri? value)
        {
            if (value == CoverTag.InternalImagesURI)
                return;
            if (value == null)
            {
                if (track.TryGetValue(nameof(CoverTag.Cover), out var cover))
                    track.Remove(cover);
                return;
            }

            var http = App.Services.GetRequiredService<ApiHelper>().Client;
            if (!track.HasCover)
            {
                track.Add((await CoverTag.DownloadCoverAsync(value!, http))!);
            }
            else
            {
                await ((CoverTag)track[nameof(CoverTag.Cover)]).UpdateCoverAsync(value, http);
            }
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName is nameof(Performers) or nameof(Genres) or nameof(AlbumArtists))
            {
                OnPropertyChanged(e.PropertyName + nameof(String));
            }

            if (e.PropertyName == nameof(State))
                OnPropertyChanged(nameof(ProcessingStateColor));
        }

        private void OnTrackTagsUpdated(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TagsCount));
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems!.OfType<CoverTag>().Any())
            {
                isCoverDirty = true;
                OnPropertyChanged(nameof(TrackCover));
            }
        }

        private void OnTrackUpdated(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private T? Read<T>([CallerMemberName] string? callerProperty = null)
        {
            if (string.IsNullOrEmpty(callerProperty))
                return default;
            if (track.TryGetTag<T>(callerProperty, out var tag))
                return tag;
            return default;
        }

        private void Update<T>(T value, [CallerMemberName] string? callerProperty = null)
        {
            if (!string.IsNullOrEmpty(callerProperty))
            {
                track.SetTag(callerProperty, value);
                OnPropertyChanged(callerProperty);
                if (callerProperty != nameof(State))
                    ResetState();
            }
        }

        private void UpdateCover()
        {
            cover?.Dispose();
            var data = track.OfType<CoverTag>().FirstOrDefault()?.Cover?.Data;
            if (data == null)
                return;
            using var memory = new MemoryStream(data.Data);
            cover = new Bitmap(memory);
        }
    }
}