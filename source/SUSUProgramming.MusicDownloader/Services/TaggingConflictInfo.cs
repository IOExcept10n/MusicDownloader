// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents an info about the tagging conflict.
    /// </summary>
    /// <param name="track">Track in which conflict appeared.</param>
    /// <param name="tags">Conflicted values set.</param>
    /// <param name="tagName">Name of the conflicted tag.</param>
    internal partial class TaggingConflictInfo(TrackDetails track, ObservableCollection<ITag> tags, string tagName) : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ITag> foundData = tags;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StateColor))]
        private ConflictResolveState resolveState;
        [ObservableProperty]
        private TrackDetails track = track;

        /// <summary>
        /// Defines a state of the conflicts resolving.
        /// </summary>
        public enum ConflictResolveState
        {
            /// <summary>
            /// Conflict is not resolved.
            /// </summary>
            None,

            /// <summary>
            /// Conflict has been resolved.
            /// </summary>
            Resolved,

            /// <summary>
            /// Conflict has been rejected.
            /// </summary>
            Rejected,
        }

        /// <summary>
        /// Gets the color or the current conflict state.
        /// </summary>
        public SolidColorBrush StateColor => new(ResolveState switch
        {
            ConflictResolveState.None => Colors.White,
            ConflictResolveState.Resolved => Colors.Green,
            ConflictResolveState.Rejected => Colors.Red,
            _ => Colors.Black,
        });

        /// <summary>
        /// Gets the name of the conflict tag.
        /// </summary>
        public string TagName => tagName;

        /// <summary>
        /// Rejects the conflict.
        /// </summary>
        public void Reject() => ResolveState = ConflictResolveState.Rejected;

        /// <summary>
        /// Resolves the conflict using one of the specified solutions.
        /// </summary>
        /// <param name="preferredCase">Case to resolve conflict with.</param>
        public void Resolve(int preferredCase)
        {
            Track.Add(FoundData[preferredCase]);
            ResolveState = ConflictResolveState.Resolved;
        }
    }
}