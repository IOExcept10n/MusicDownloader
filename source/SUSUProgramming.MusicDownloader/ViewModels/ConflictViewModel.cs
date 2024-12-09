using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for single conflict row.
    /// </summary>
    /// <param name="conflict">Conflict to visualize.</param>
    public partial class ConflictViewModel(TaggingConflictInfo conflict) : ViewModelBase
    {
        [ObservableProperty]
        private string currentValue = string.Empty;

        /// <summary>
        /// Gets conflict info to visualize.
        /// </summary>
        public TaggingConflictInfo Conflict => conflict;

        /// <summary>
        /// Gets conflicting values.
        /// </summary>
        public ReadOnlyCollection<ConflictValueViewModel> Values => conflict.FoundData.Project(x => new ConflictValueViewModel(x, conflict), (x, y) => x.Tag == y);
    }
}
