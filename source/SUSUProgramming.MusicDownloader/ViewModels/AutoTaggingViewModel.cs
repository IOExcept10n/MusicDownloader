using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
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
        /// Starts tagging process for specified track view models.
        /// </summary>
        /// <param name="viewModels">List of tracks to tag.</param>
        /// <returns>Task to wait until tagging ends.</returns>
        public async Task TagSelectedAsync(IReadOnlyCollection<TrackViewModel> viewModels)
        {
            IsFinished = false;
            ProcessedCount = 0;
            TotalCount = viewModels.Count;
            IsRunning = true;
            foreach (var vm in viewModels)
                vm.State = TrackProcessingState.Processing;
            foreach (var vm in viewModels)
            {
                try
                {
                    var result = await tagger.TagAsync(vm.Model);
                    if (result.Details.Count == 0)
                    {
                        vm.State = TrackProcessingState.TagsNotFound;
                        continue;
                    }

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

                    if (hasConflicts)
                        vm.State = TrackProcessingState.Conflicting;
                    else
                        vm.State = TrackProcessingState.Success;
                    OnPropertyChanged(nameof(HasConflicts));
                    ProcessedCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Tagging error: {ex}");
                    vm.State = TrackProcessingState.Fault;
                }

                vm.Save();
                vm.Refresh();
            }

            IsRunning = false;
            IsFinished = true;
        }
    }
}
