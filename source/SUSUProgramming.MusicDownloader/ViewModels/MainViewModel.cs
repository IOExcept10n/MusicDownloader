using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Localization;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.Views;
using System;
using System.Collections.ObjectModel;

namespace SUSUProgramming.MusicDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private bool isPaneOpened;
    [ObservableProperty] private SidebarItemViewModel selectedListItem = null!;
    [ObservableProperty] private UserControl currentPage = null!;

    public ObservableCollection<SidebarItemViewModel> SidebarItems { get; } = [
        new()
        {
            IconText = "\uE80F",
            TitleText = Resources.Homepage,
            NavigationTargetTypeName = nameof(HomeView)
        },
        new()
        {
            IconText = "\uEA69",
            TitleText = Resources.MediaLibrary,
            NavigationTargetTypeName = nameof(LibraryView)
        },
        new()
        {
            IconText = "\uF6FA",
            TitleText = Resources.OnlineServices,
            NavigationTargetTypeName = nameof(OnlineServicesView)
        },
        new()
        {
            IconText = "\uE713",
            TitleText = Resources.Preferences,
            NavigationTargetTypeName = nameof(PreferencesView)
        }
    ];

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
