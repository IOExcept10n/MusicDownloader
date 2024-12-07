// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Localization;
using SUSUProgramming.MusicDownloader.Views;

namespace SUSUProgramming.MusicDownloader.ViewModels;

/// <summary>
/// Represents a view model for the main view.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private UserControl currentPage = null!;
    [ObservableProperty]
    private bool isPaneOpened;
    [ObservableProperty]
    private SidebarItemViewModel selectedListItem = null!;

    /// <summary>
    /// Gets the menu items for the sidebar.
    /// </summary>
    public ObservableCollection<SidebarItemViewModel> SidebarItems { get; } = [
        new()
        {
            IconText = "\uE80F",
            TitleText = Resources.Homepage,
            NavigationTargetTypeName = nameof(HomeView),
        },
        new()
        {
            IconText = "\uEA69",
            TitleText = Resources.MediaLibrary,
            NavigationTargetTypeName = nameof(LibraryView),
        },
        new()
        {
            IconText = "\uF6FA",
            TitleText = Resources.OnlineServices,
            NavigationTargetTypeName = nameof(OnlineServicesView),
        },
        new()
        {
            IconText = "\uE713",
            TitleText = Resources.Preferences,
            NavigationTargetTypeName = nameof(PreferencesView),
        }
    ];

    /// <summary>
    /// Handles sidebar item selection.
    /// </summary>
    /// <param name="value">An item that has been selected.</param>
    public void OnSidebarItemSelected(SidebarItemViewModel? value)
    {
        if (value == null) return;
        SelectedListItem = value;
        if (string.IsNullOrEmpty(value.NavigationTargetTypeName)) return;
        if (CurrentPage is IDisposable disposable)
            disposable.Dispose();
        CurrentPage = App.Services.GetRequiredKeyedService<UserControl>(value.NavigationTargetTypeName);
    }
}