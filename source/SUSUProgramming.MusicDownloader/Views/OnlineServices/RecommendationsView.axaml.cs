// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

/// <summary>
/// Represents a view for user's recommendations.
/// </summary>
[View]
public partial class RecommendationsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendationsView"/> class.
    /// </summary>
    public RecommendationsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<OnlineLibViewModel>();
    }

    /// <inheritdoc/>
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is not OnlineLibViewModel online)
            return;
        await online.Loader.ScanAsync(x => x.ListUserRecommendationsAsync());
        base.OnLoaded(e);
    }

    private async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not OnlineTrackViewModel vm)
            return;
        if (DataContext is not OnlineLibViewModel online)
            return;
        await online.DownloadTrack(vm);
    }

    private async Task DownloadAndRunSelectedAsync()
    {
        if (DataContext is not OnlineLibViewModel online)
            return;
        string tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile += ".m3u8");
        using (var writer = new StreamWriter(tempFile))
        {
            foreach (OnlineTrackViewModel vm in TracksList.SelectedItems!)
            {
                var result = await online.DownloadTrack(vm);
                if (result?.FilePath == null)
                    continue;
                writer.WriteLine(result.FilePath);
            }
        }

        var info = new ProcessStartInfo()
        {
            FileName = tempFile,
            UseShellExecute = true,
        };
        Process.Start(info);
    }

    private void ToggleIgnored()
    {
        if (DataContext is not OnlineLibViewModel)
            return;
        var settings = App.Services.GetRequiredService<AppConfig>();
        foreach (OnlineTrackViewModel vm in TracksList.SelectedItems!)
        {
            string name = vm.Model.FormedTrackName;
            if (!settings.BlacklistedTrackNames.Remove(name))
                settings.BlacklistedTrackNames.Add(name);
        }
    }

    private async void OnLoadAndPlayClick(object? sender, RoutedEventArgs e)
    {
        await DownloadAndRunSelectedAsync();
    }

    private void OnSwitchIgnoreClick(object? sender, RoutedEventArgs e)
    {
        ToggleIgnored();
    }

    private async void OnDoubleTap(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        await DownloadAndRunSelectedAsync();
    }
}