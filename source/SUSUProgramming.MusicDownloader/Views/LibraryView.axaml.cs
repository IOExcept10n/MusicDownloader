// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents a view for the tracks library.
/// </summary>
[View]
public sealed partial class LibraryView : UserControl, IDisposable
{
    private readonly IServiceScope scope;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryView"/> class.
    /// </summary>
    public LibraryView()
    {
        InitializeComponent();
        scope = App.Services.CreateScope();
        DataContext = App.Services.GetRequiredService<LibraryViewModel>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        scope.Dispose();
    }

    /// <inheritdoc/>
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        var library = App.Services.GetRequiredService<MediaLibrary>();
        await library.ScanAsync();
        base.OnLoaded(e);
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
            Title = "Open song pack file or players list",
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
}