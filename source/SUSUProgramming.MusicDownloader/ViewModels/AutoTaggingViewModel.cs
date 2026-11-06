using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Localization;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view-model for tagging purposes.
    /// </summary>
    [Singleton]
    internal partial class AutoTaggingViewModel : ViewModelBase
    {
        private readonly TagService tagger;
        private readonly AppConfig settings;

        [ObservableProperty]
        private int processedCount;
        [ObservableProperty]
        private int totalCount;
        [ObservableProperty]
        private ObservableCollection<TaggingConflictInfo> conflicts = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LockControls))]
        private bool isRunning;
        [ObservableProperty]
        private bool isFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTaggingViewModel"/> class.
        /// </summary>
        /// <param name="tagger">An instance of the tagging service to use.</param>
        /// <param name="settings">An instance of the program settings to use.</param>
        public AutoTaggingViewModel(TagService tagger, AppConfig settings)
        {
            this.tagger = tagger;
            this.settings = settings;
            conflicts.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasConflicts));
            Items.CollectionChanged += OnItemsChanged;
        }

        /// <summary>
        /// Gets a value indicating whether the controls should be locked until tagging ended.
        /// </summary>
        public bool LockControls => !IsRunning;

        /// <summary>
        /// Gets a value indicating whether the tagging has conflicts.
        /// </summary>
        public bool HasConflicts => Conflicts.Count > 0;

        /// <summary>
        /// Gets the per-track results of the last tagging run.
        /// </summary>
        public ObservableCollection<TaggingResultItemViewModel> Items { get; } = [];

        /// <summary>
        /// Gets a value indicating whether the last tagging run produced anything to show.
        /// </summary>
        public bool HasResults => Items.Count > 0;

        /// <summary>
        /// Gets the number of tracks that are ready to apply and have not been applied yet.
        /// </summary>
        public int ReadyCount => Items.Count(x => x.IsReady && !x.IsApplied);

        /// <summary>
        /// Gets the number of tracks that need the owner's review.
        /// </summary>
        public int ReviewCount => Items.Count(x => x.NeedsReview);

        /// <summary>
        /// Gets a value indicating whether there is at least one track ready to apply.
        /// </summary>
        public bool HasReady => ReadyCount > 0;

        /// <summary>
        /// Gets the text of the results summary line.
        /// </summary>
        public string Summary => string.Format(Resources.Culture, Resources.TaggingResultsSummary, Items.Count, Items.Count(x => x.IsReady), ReviewCount);

        /// <summary>
        /// Gets the text of the "apply ready" button.
        /// </summary>
        public string ApplyReadyText => string.Format(Resources.Culture, Resources.ApplyReadyTracks, ReadyCount);

        /// <summary>
        /// Saves every ready track that has not been applied yet. Tracks with unresolved conflicts are never touched.
        /// </summary>
        public void ApplyReady()
        {
            foreach (var item in Items.Where(x => x.IsReady && !x.IsApplied).ToList())
            {
                // One locked file must not stop the others, nor take the app down.
                try
                {
                    item.Track.Save();
                    item.SaveError = null;
                    item.IsApplied = true;
                }
                catch (Exception ex)
                {
                    item.SaveError = ex.Message;
                }
            }

            NotifyResultsChanged();
        }

        /// <summary>
        /// Determines whether the tagging run found nothing at all for the track: neither values nor conflicts.
        /// </summary>
        /// <param name="result">Result of the tagging run.</param>
        /// <returns><see langword="true"/> if there is nothing to apply and nothing to choose from.</returns>
        internal static bool NothingFound(TaggingResult result) => result.Details.Count == 0 && result.Conflicts.Count == 0;

        /// <summary>
        /// Adds a result row for a tagged track.
        /// </summary>
        /// <param name="track">The track.</param>
        /// <param name="conflicts">Its conflicts.</param>
        /// <param name="alreadySaved">Whether the tags were already written to the file during the run.</param>
        /// <returns>The added row.</returns>
        internal TaggingResultItemViewModel AddResult(ITaggedTrack track, IReadOnlyList<TaggingConflictInfo> conflicts, bool alreadySaved)
        {
            var item = new TaggingResultItemViewModel(track, conflicts) { IsApplied = alreadySaved };
            Items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a result row for a track that could not be tagged.
        /// </summary>
        /// <param name="track">The track.</param>
        /// <param name="reason">Text that explains what happened.</param>
        internal void AddFailure(ITaggedTrack track, string reason) => Items.Add(TaggingResultItemViewModel.Failed(track, reason));

        private void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TaggingResultItemViewModel item in e.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
            }

            if (e.NewItems != null)
            {
                foreach (TaggingResultItemViewModel item in e.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;
            }

            NotifyResultsChanged();
        }

        private void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => NotifyResultsChanged();

        private void NotifyResultsChanged()
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(ReadyCount));
            OnPropertyChanged(nameof(HasReady));
            OnPropertyChanged(nameof(ReviewCount));
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(ApplyReadyText));
        }

        /// <summary>
        /// Starts tagging process for specified track view models.
        /// </summary>
        /// <param name="viewModels">List of tracks to tag.</param>
        /// <returns>Task to wait until tagging ends.</returns>
        public async Task TagSelectedAsync(IReadOnlyCollection<TrackViewModel> viewModels)
        {
            List<TrackViewModel> vms = [.. viewModels];
            IsFinished = false;
            ProcessedCount = 0;
            TotalCount = vms.Count;
            IsRunning = true;
            Conflicts.Clear();
            Items.Clear();
            foreach (var vm in vms)
            {
                vm.State = TrackProcessingState.Processing;
                if (settings.BackupOnAutoTag)
                    vm.CreateBackup();
            }

            foreach (var vm in vms)
            {
                List<TaggingConflictInfo>? found = null;
                string? failure = null;
                try
                {
                    var result = await tagger.TagAsync(vm.Model);
                    if (NothingFound(result))
                    {
                        vm.State = TrackProcessingState.TagsNotFound;
                        failure = Resources.TaggingReasonNotFound;
                    }
                    else
                    {
                        TrackDetails details;
                        details = TagService.CombineResults(settings, vm.Model, result);
                        vm.Model.Clear();
                        foreach (var tag in details)
                            vm.Model.Add(tag);
                        bool hasConflicts = false;
                        foreach (var conflict in result.Conflicts)
                        {
                            Conflicts.Add(conflict);
                            hasConflicts = true;
                        }

                        found = [.. result.Conflicts];

                        if (hasConflicts)
                            vm.State = TrackProcessingState.Conflicting;
                        else
                            vm.State = TrackProcessingState.Success;
                        OnPropertyChanged(nameof(HasConflicts));
                        ProcessedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Tagging error: {ex}");
                    vm.State = TrackProcessingState.Fault;
                    failure = string.Format(Resources.Culture, Resources.TaggingReasonError, ex.Message);
                }

                bool saved = false;
                if (vm.State == TrackProcessingState.Success && settings.AutoSaveTags)
                {
                    try
                    {
                        vm.Save();
                        saved = true;
                    }
                    catch (Exception ex)
                    {
                        vm.State = TrackProcessingState.Fault;
                        failure = string.Format(Resources.Culture, Resources.TaggingReasonSaveError, ex.Message);
                    }
                }

                // Every processed track gets a row, so nothing is lost silently.
                if (failure != null)
                    AddFailure(vm, failure);
                else if (found != null)
                    AddResult(vm, found, saved);

                vm.Refresh();
            }

            IsRunning = false;
            IsFinished = true;
        }
    }
}
