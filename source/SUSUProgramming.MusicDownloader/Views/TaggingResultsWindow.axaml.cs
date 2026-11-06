// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using Avalonia.Controls;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents a window with per-track results of the auto tagging: ready tracks and tracks that need review.
/// </summary>
internal partial class TaggingResultsWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaggingResultsWindow"/> class.
    /// </summary>
    public TaggingResultsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaggingResultsWindow"/> class.
    /// </summary>
    /// <param name="results">Results of the tagging run to show.</param>
    public TaggingResultsWindow(AutoTaggingViewModel results)
    {
        InitializeComponent();
        DataContext = results;
    }

    private void OnApplyReadyClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is AutoTaggingViewModel results)
            results.ApplyReady();
    }

    private async void OnReviewClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not TaggingResultItemViewModel item)
            return;

        // The conflicts of the track are resolved by the owner one by one; nothing is chosen automatically.
        var details = new ConflictsResolveWindow(new ObservableCollection<TaggingConflictInfo>(item.ConflictInfos));
        await details.ShowDialog(this);
    }
}
