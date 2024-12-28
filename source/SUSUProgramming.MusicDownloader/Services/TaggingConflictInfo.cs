// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents an info about the tagging conflict.
    /// </summary>
    /// <param name="track">Track in which conflict appeared.</param>
    /// <param name="tags">Conflicted values set.</param>
    /// <param name="tagName">Name of the conflicted tag.</param>
    public partial class TaggingConflictInfo(TrackDetails track, ObservableCollection<ITag> tags, string tagName) : ObservableObject
    {
        private static readonly TagComparer TagComparer = new();

        [ObservableProperty]
        private ObservableCollection<ITag> foundData = tags;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StateColor))]
        [NotifyPropertyChangedFor(nameof(IsIndeterminate))]
        private ConflictResolveState resolveState;
        [ObservableProperty]
        private TrackDetails track = track;

        /// <summary>
        /// Occurs when the conflict is resolved.
        /// </summary>
        public event EventHandler? Resolved;

        /// <summary>
        /// Occurs when the conflict is rejected.
        /// </summary>
        public event EventHandler? Rejected;

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
            ConflictResolveState.None => Colors.Transparent,
            ConflictResolveState.Resolved => new(0x80, 0x00, 0xFF, 0x00),
            ConflictResolveState.Rejected => new(0x80, 0xFF, 0x00, 0x00),
            _ => Colors.Black,
        });

        /// <summary>
        /// Gets the name of the conflict tag.
        /// </summary>
        public string TagName => tagName;

        /// <summary>
        /// Gets a value indicating whether the conflict is in unresolved state.
        /// </summary>
        public bool IsIndeterminate => ResolveState == ConflictResolveState.None;

        /// <summary>
        /// Adds a tag to the conflict info. If there are tags with the same value, this tag won't be added.
        /// </summary>
        /// <param name="tag">Tag instance to add.</param>
        public void Accumulate(ITag tag)
        {
            int position = FoundData.BinarySearch(tag, TagComparer);
            if (position >= 0)
                return;
            FoundData.Insert(~position, tag);
        }

        /// <summary>
        /// Rejects the conflict.
        /// </summary>
        public void Reject()
        {
            ResolveState = ConflictResolveState.Rejected;
            Rejected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resolves the conflict using one of the specified solutions.
        /// </summary>
        /// <param name="preferredCase">Case to resolve conflict with.</param>
        public void Resolve(int preferredCase)
        {
            Track.Add(FoundData[preferredCase]);

            // For now, state for resolved tracks is unknown because some values can be missing.
            // However, values can be complete as well.
            Track.SetTag(nameof(VirtualTags.State), TrackProcessingState.Unknown);
            MetadataManager.SaveMetadata(Track);
            ResolveState = ConflictResolveState.Resolved;
            Resolved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Manually resolves the conflict.
        /// </summary>
        /// <param name="preferredValue">Custom value set to resolve the conflict.</param>
        public void ResolveManually(object? preferredValue)
        {
            var tagToSet = FoundData[0].Clone();

            // HACK: just to make sure that input value is within correct format.
            if (tagToSet.Value is string[])
            {
                preferredValue = TrackNameParser.GetPerformers(preferredValue?.ToString() ?? string.Empty);
            }

            try
            {
                tagToSet.Value = preferredValue;
                Track.Add(tagToSet);
            }
            catch
            {
                // Do nothing.
            }

            // For now, state for resolved tracks is unknown because some values can be missing.
            // However, values can be complete as well.
            Track.SetTag(nameof(VirtualTags.State), TrackProcessingState.Unknown);
            MetadataManager.SaveMetadata(Track);
            ResolveState = ConflictResolveState.Resolved;
            Resolved?.Invoke(this, EventArgs.Empty);
        }
    }
}