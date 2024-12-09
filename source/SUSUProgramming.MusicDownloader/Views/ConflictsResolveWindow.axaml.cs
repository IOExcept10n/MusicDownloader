// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using Avalonia.Controls;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents a window for conflicts resolving.
/// </summary>
public partial class ConflictsResolveWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictsResolveWindow"/> class.
    /// </summary>
    public ConflictsResolveWindow()
    {
        InitializeComponent();
        DataContext = new ConflictsResolveViewModel();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictsResolveWindow"/> class.
    /// </summary>
    /// <param name="conflicts">List of conflicts to resolve.</param>
    public ConflictsResolveWindow(ObservableCollection<TaggingConflictInfo> conflicts)
    {
        InitializeComponent();
        DataContext = new ConflictsResolveViewModel(conflicts);
    }

    private void OnConflictRejected(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not ConflictViewModel conflict)
            return;
        conflict.Conflict.Reject();
    }

    private void OnConflictResolved(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not ConflictViewModel conflict)
            return;
        conflict.Conflict.ResolveManually(conflict.CurrentValue);
    }

    private void OnValueSelected(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not ConflictValueViewModel value)
            return;
        value.Select();
    }
}