// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
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

    internal partial class TrackViewModel : ViewModelBase
    {
        private static readonly string[] RecommendedTags = [nameof(Album), nameof(Genres), nameof(Year), nameof(Track)];
        private static readonly string[] RequiredTags = [nameof(Title), nameof(Performers)];
        private static readonly Bitmap EmptyCover = new(AssetLoader.Open(new("avares://SUSUProgramming.MusicDownloader/Assets/music.png")));
        private readonly TrackDetails track;
        private bool isCoverDirty = true;
        private Bitmap? cover;

        public TrackViewModel(TrackDetails track)
        {
            this.track = track;
            track.PropertyChanged += OnTrackUpdated;
            track.CollectionChanged += OnTrackTagsUpdated;
            ResetState();
        }

        public string? Album { get => Read<string>(); set => Update(value); }

        public string[]? AlbumArtists
        {
            get => Read<string[]>();
            set => Update(value);
        }

        public string? AlbumArtistsString
        {
            get => AlbumArtists == null ? null : string.Join(", ", AlbumArtists);
            set => AlbumArtists = value == null ? null : TrackNameParser.GetPerformers(value);
        }

        public string? Comment { get => Read<string>(); set => Update(value); }

        public Bitmap TrackCover
        {
            get
            {
                if (isCoverDirty)
                    UpdateCover();
                return track.HasCover ? (cover ?? EmptyCover) : EmptyCover;
            }
        }

        public Uri? CoverUrl
        {
            get => track.CoverUri;
            set
            {
                if (value == CoverTag.InternalImagesURI)
                    return;
                if (value == null)
                {
                    if (track.TryGetValue(nameof(CoverTag.Cover), out var cover))
                        track.Remove(cover);
                    return;
                }
                // Yeaaah, blocking wait...
                var http = App.Services.GetRequiredService<ApiHelper>().Client;
                if (!track.HasCover)
                {
                    track.Add(CoverTag.DownloadCoverAsync(value!, http).Result!);
                }
                else
                {
                    ((CoverTag)track[nameof(CoverTag.Cover)]).UpdateCoverAsync(value, http).Wait();
                }
            }
        }

        public string? Description { get => Read<string>(); set => Update(value); }
        public uint Disc { get => Read<uint>(); set => Update(value); }
        public uint DiscCount { get => Read<uint>(); set => Update(value); }
        public string[]? Genres { get => Read<string[]>(); set => Update(value); }

        public string? GenresString
        {
            get => Genres == null ? null : string.Join(", ", Genres);
            set => Genres = value?.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public string? Lyrics { get => Read<string>(); set => Update(value); }

        public TrackDetails Model => track;

        public string[]? Performers { get => Read<string[]>(); set => Update(value); }

        public string[]? PerformersRole { get => Read<string[]>(); set => Update(value); }

        public string? PerformersString
        {
            get => track.FormedArtistString;
            set => Performers = value == null ? null : TrackNameParser.GetPerformers(value);
        }

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
            _ => Colors.Violet
        });

        public string? Subtitle { get => Read<string>(); set => Update(value); }
        public int TagsCount => track.Count(x => x is not VirtualTag);
        public string? Title { get => Read<string>(); set => Update(value); }
        public uint Track { get => Read<uint>(); set => Update(value); }
        public uint TrackCount { get => Read<uint>(); set => Update(value); }
        public uint Year { get => Read<uint>(); set => Update(value); }
        public TrackProcessingState State { get => Read<TrackProcessingState>(); set => Update(value); }

        public void Save()
        {
            MetadataManager.SaveMetadata(Model);
        }

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

        private void UpdateCover()
        {
            cover?.Dispose();
            var data = track.OfType<CoverTag>().FirstOrDefault()?.Cover?.Data;
            if (data == null)
                return;
            using var memory = new MemoryStream(data.Data);
            cover = new Bitmap(memory);
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
    }
}