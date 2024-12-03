using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.ViewModels;
using System.Diagnostics;
using System.IO;

namespace SUSUProgramming.MusicDownloader.Views.Library;

public partial class MyTracksView : UserControl
{
    public MyTracksView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext = App.Services.GetRequiredService<MyTracksViewModel>();
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        TracksList.SelectedItems = libraryVM.SelectedTracks;
    }

    private async void OnFindTracksClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UnsortedTracksViewModel vm)
            return;
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        await vm.AutoTagger.TagSelectedAsync(libraryVM.SelectedTracks);
        libraryVM.SelectedTracks.Clear();
    }

    private void OnRefreshClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UnsortedTracksViewModel vm)
            return;
        foreach (var track in vm.UnsortedTracks)
        {
            track.ResetState();
            track.Refresh();
        }
    }

    private void ToggleButton_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TracksList.SelectAll();
    }

    private void ToggleButton_Unchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TracksList.UnselectAll();
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        
    }

    private void OnDeleteClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeleteSelected();
    }

    private void OnPlayClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PlaySelected();
    }

    private void OnShowClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShowSelected();
    }

    private void OnDoubleTap(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        PlaySelected();
    }

    private static void PlaySelected()
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        string tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile += ".m3u8");
        using (var writer = new StreamWriter(tempFile))
        {
            foreach (var track in libraryVM.SelectedTracks)
            {
                if (track.Model.FilePath == null) continue;
                writer.WriteLine(track.Model.FilePath);
            }
        }
        var info = new ProcessStartInfo()
        {
            FileName = tempFile,
            UseShellExecute = true,
        };
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
                ArgumentList = { "/n", "/select", track.Model.FilePath },
                UseShellExecute = true
            };
            Process.Start(info);
        }
    }

    private static void DeleteSelected()
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        var library = App.Services.GetRequiredService<MediaLibrary>();
        for (int i = 0; i < libraryVM.SelectedTracks.Count; i++)
        {
            library.DeleteTrack(libraryVM.SelectedTracks[i].Model);
            i--;
            //libraryVM.SelectedTracks.RemoveAt(i--);
        }
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

    private static void Move(string? newPath)
    {
        var libraryVM = App.Services.GetRequiredService<LibraryViewModel>();
        var library = App.Services.GetRequiredService<MediaLibrary>();
        if (string.IsNullOrEmpty(newPath))
            return;
        for (int i = 0; i < libraryVM.SelectedTracks.Count; i++)
        {
            TrackViewModel? track = libraryVM.SelectedTracks[i];
            library.MoveTrack(track.Model, newPath);
            // Should decrement i because each movement should trigger update event that would remove items from list automatically.
            i--;
        }
    }
}