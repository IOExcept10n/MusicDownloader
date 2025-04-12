// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views.Library;

/// <summary>
/// Represents a view for user's tracks.
/// </summary>
public partial class MyTracksView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MyTracksView"/> class.
    /// </summary>
    public MyTracksView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            var vm = App.Services.GetRequiredService<MyTracksViewModel>();
            DataContext = vm;
            vm.SelectionChanged += (s, e) =>
            {
                if (e)
                {
                    TracksList.SelectAll();
                }
                else
                {
                    TracksList.UnselectAll();
                }
            };
        }

        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        TracksList.SelectedItems = libraryVM.SelectedTracks;
    }

    private static void DeleteSelected()
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        var library = App.Services.GetRequiredService<MediaLibrary>();
        for (int i = 0; i < libraryVM.SelectedTracks.Count; i++)
        {
            library.DeleteTrack(libraryVM.SelectedTracks[i].Model);
            i--;
        }
    }

    private static void Move(string? newPath)
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        var library = App.Services.GetRequiredService<MediaLibrary>();
        if (string.IsNullOrEmpty(newPath))
            return;
        var selectedTracks = libraryVM.SelectedTracks.ToList();
        foreach (var track in selectedTracks)
        {
            library.MoveTrack(track.Model, newPath);
        }
    }

    private static void PlaySelected()
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        if (libraryVM.SelectedTracks.Count == 0)
            return;
        var info = new ProcessStartInfo() { UseShellExecute = true };
        if (libraryVM.SelectedTracks.Count == 1)
        {
            info.FileName = libraryVM.SelectedTracks[0].Model.FilePath;
        }
        else
        {
            string tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile += ".m3u8");
            info.FileName = tempFile;

            using var writer = new StreamWriter(tempFile);
            foreach (var track in libraryVM.SelectedTracks)
            {
                if (track.Model.FilePath == null) continue;
                writer.WriteLine(track.Model.FilePath);
            }
        }

        Process.Start(info);
    }

    private static void ShowSelected()
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        foreach (var track in libraryVM.SelectedTracks)
        {
            if (track.Model.FilePath == null)
                continue;
            var info = new ProcessStartInfo()
            {
                FileName = "explorer",
                Arguments = $"/n,/select,\"{track.Model.FilePath}\"",
                UseShellExecute = true,
            };
            Process.Start(info);
        }
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox)
            return;
    }

    private void OnDeleteClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeleteSelected();
    }

    private void OnDoubleTap(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        PlaySelected();
    }

    private async void OnFindTracksClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MyTracksViewModel vm)
            return;
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        await vm.AutoTagger.TagSelectedAsync(libraryVM.SelectedTracks);
        libraryVM.SelectedTracks.Clear();
    }

    private void OnMarkAsUnsorted(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        Move(libraryVM.Settings.UnsortedTracksPath);
    }

    private void OnMoveTracksTo(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Move((string?)((MenuItem?)sender)?.SelectedItem);
    }

    private void OnPlayClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PlaySelected();
    }

    private async void OnRefreshClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MyTracksViewModel vm)
            return;
        var libraryVM = App.Services.GetRequiredService<MediaLibrary>();
        await libraryVM.ScanAsync();
        foreach (var track in vm.Tracks)
        {
            track.ResetState();
            track.Refresh();
        }
    }

    private void OnShowClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShowSelected();
    }

    private void ToggleButton_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TracksList.SelectAll();
    }

    private void ToggleButton_Unchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TracksList.UnselectAll();
    }

    private async void OnResolveConflictsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MyTracksViewModel vm)
            return;
        var resolver = new ConflictsResolveWindow(vm.AutoTagger.Conflicts);
        var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (window == null)
            return;
        await resolver.ShowDialog(window);

        // Remove all resolved and rejected conflicts.
        for (int i = 0; i < vm.AutoTagger.Conflicts.Count; i++)
        {
            var conflict = vm.AutoTagger.Conflicts[i];
            if (!conflict.IsIndeterminate)
            {
                vm.AutoTagger.Conflicts.RemoveAt(i--);
            }

            var trackVM = vm.Tracks.FirstOrDefault(x => x.Model == conflict.Track);
            trackVM?.Refresh();
            trackVM?.ResetState();
        }
    }

    private void SearchBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
    }

    private void OnSearchClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}