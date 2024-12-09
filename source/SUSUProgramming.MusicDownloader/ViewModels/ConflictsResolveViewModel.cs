// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for <see cref="Views.ConflictsResolveWindow"/>.
    /// </summary>
    public partial class ConflictsResolveViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TaggingConflictInfo> sources = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictsResolveViewModel"/> class.
        /// </summary>
        public ConflictsResolveViewModel()
        {
            Conflicts = Sources.Project(x => new ConflictViewModel(x), (x, y) => x.Conflict == y);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictsResolveViewModel"/> class.
        /// </summary>
        /// <param name="conflicts">List of conflicts to show.</param>
        public ConflictsResolveViewModel(ObservableCollection<TaggingConflictInfo> conflicts)
        {
            Sources = conflicts;
            Conflicts = Sources.Project(x => new ConflictViewModel(x), (x, y) => x.Conflict == y);
        }

        /// <summary>
        /// Gets list of conflicts prepared for UI display.
        /// </summary>
        public ReadOnlyCollection<ConflictViewModel> Conflicts { get; }
    }
}