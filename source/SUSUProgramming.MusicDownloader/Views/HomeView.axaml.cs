// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents a view for the homepage.
/// </summary>
[View]
public partial class HomeView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeView"/> class.
    /// </summary>
    public HomeView()
    {
        InitializeComponent();
    }

    private void OnLibraryOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainView = this.FindLogicalAncestorOfType<MainView>();
        if (mainView == null)
            return;
        mainView.Sidebar.SelectedIndex = 1;
    }

    private void OnOnlineOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainView = this.FindLogicalAncestorOfType<MainView>();
        if (mainView == null)
            return;
        mainView.Sidebar.SelectedIndex = 2;
    }
}