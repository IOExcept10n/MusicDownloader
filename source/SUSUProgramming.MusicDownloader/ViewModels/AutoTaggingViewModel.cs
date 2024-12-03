using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    [Singleton]
    internal partial class AutoTaggingViewModel(TagService tagger, AppConfig settings) : ViewModelBase
    {
        [ObservableProperty] private int processedCount;
        [ObservableProperty] private int totalCount;
        [ObservableProperty] private ObservableCollection<TaggingConflictInfo> conflicts = [];
        [ObservableProperty][NotifyPropertyChangedFor(nameof(LockControls))] private bool isRunning;
        [ObservableProperty] private bool isFinished;

        public bool LockControls => !IsRunning;

        public bool HasConflicts => Conflicts.Count > 0;

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
                catch
                {
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
