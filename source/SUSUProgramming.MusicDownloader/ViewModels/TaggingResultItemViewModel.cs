// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Localization;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents the tagging result of a single track: ready to apply or needing the owner's review.
    /// </summary>
    internal partial class TaggingResultItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool isApplied;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaggingResultItemViewModel"/> class.
        /// </summary>
        /// <param name="track">The tagged track.</param>
        /// <param name="conflicts">Conflicts found for the track.</param>
        public TaggingResultItemViewModel(ITaggedTrack track, IReadOnlyList<TaggingConflictInfo> conflicts)
        {
            Track = track;
            ConflictInfos = conflicts;
            Conflicts = [.. conflicts.Select(x => new ConflictViewModel(x))];
            foreach (var conflict in conflicts)
            {
                conflict.Resolved += OnConflictChanged;
                conflict.Rejected += OnConflictChanged;
            }
        }

        /// <summary>
        /// Gets the tagged track.
        /// </summary>
        public ITaggedTrack Track { get; }

        /// <summary>
        /// Gets the conflicts found for the track.
        /// </summary>
        public IReadOnlyList<TaggingConflictInfo> ConflictInfos { get; }

        /// <summary>
        /// Gets the conflicts prepared for the UI.
        /// </summary>
        public IReadOnlyList<ConflictViewModel> Conflicts { get; }

        /// <summary>
        /// Gets the "Artist - Title" name of the track.
        /// </summary>
        public string TrackName => Track.Model.FormedTrackName;

        /// <summary>
        /// Gets a value indicating whether the track has conflicts the owner has not decided yet.
        /// </summary>
        public bool HasUnresolvedConflicts => ConflictInfos.Any(x => x.IsIndeterminate);

        /// <summary>
        /// Gets a value indicating whether the track can be applied without review.
        /// </summary>
        public bool IsReady => TagCompleteness.IsReady(Track.Model, HasUnresolvedConflicts);

        /// <summary>
        /// Gets a value indicating whether the track needs the owner's review.
        /// </summary>
        public bool NeedsReview => !IsReady;

        /// <summary>
        /// Gets the human readable reason why the track needs review, or an empty string for a ready track.
        /// </summary>
        public string Reason
        {
            get
            {
                var unresolved = ConflictInfos.Where(x => x.IsIndeterminate).Select(x => x.TagName).ToList();
                if (unresolved.Count > 0)
                    return string.Format(Resources.Culture, Resources.TaggingReasonConflicts, string.Join(", ", unresolved));
                if (!TagCompleteness.Required.All(Track.Model.Contains))
                    return Resources.TaggingReasonMissingRequired;
                return string.Empty;
            }
        }

        private void OnConflictChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(HasUnresolvedConflicts));
            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(NeedsReview));
            OnPropertyChanged(nameof(Reason));
        }
    }
}
