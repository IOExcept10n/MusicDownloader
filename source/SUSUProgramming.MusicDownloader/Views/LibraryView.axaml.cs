using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.ViewModels;
using System;
using System.IO;

namespace SUSUProgramming.MusicDownloader.Views;

[View]
public sealed partial class LibraryView : UserControl, IDisposable
{
    private readonly IServiceScope scope;

    public LibraryView()
    {
        InitializeComponent();
        scope = App.Services.CreateScope();
        DataContext = App.Services.GetRequiredService<LibraryViewModel>();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        var library = App.Services.GetRequiredService<MediaLibrary>();
        await library.ScanAsync();
        base.OnLoaded(e);
    }

    public void Dispose()
    {
        scope.Dispose();
    }

    private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        // I didn't know that events bubble up to custom handler
        if (e.Source is not TabControl)
            return;
        var library = (LibraryViewModel?)DataContext;
        if (library == null)
            return;
        library.SelectedTracks.Clear();
    }

    private async void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        // TODO: load lyrics
        var topView = TopLevel.GetTopLevel(this);
        var library = (LibraryViewModel?)DataContext;
        if (topView == null || library == null) return;
        var files = await topView.StorageProvider.OpenFilePickerAsync(new()
        {
            AllowMultiple = false,
            Title = "Open song pack file or players list"
        });

        if (files.Count >= 1)
        {
            string path = files[0].Path.LocalPath;
            string content = File.ReadAllText(path);
            library.EditingModel.CommonLyrics = content;
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var library = (LibraryViewModel?)DataContext;
        if (library == null) return;
        library.EditingModel.Save();
    }
}