using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

[View]
public partial class SearchView : UserControl
{
    private readonly DelayedNotifier notifier;

    public SearchView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<OnlineLibViewModel>();
        notifier = new(async token =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            try
                            {
                                if (DataContext is not OnlineLibViewModel online)
                                    return;
                                if (SearchBox.Text == null)
                                {
                                    online.Loader.LoadedTracks.Clear();
                                    return;
                                }
                                await online.Loader.ScanAsync(x => x.SearchAsync(SearchBox.Text), token);
                            }
                            catch
                            {

                            }
                        });
                    }, 750);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is not OnlineLibViewModel online)
            return;
        online.Loader.LoadedTracks.Clear();
        base.OnLoaded(e);
    }

    private void OnSearchTextUpdate(object? sender, TextChangedEventArgs e)
    {
        notifier.NotifyUpdate();
    }

    private async void OnSearchClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await notifier.RaiseAsync();
    }

    private async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not OnlineTrackViewModel vm)
            return;
        if (DataContext is not OnlineLibViewModel online)
            return;
        await online.DownloadTrack(vm);
    }

    private async void OnSearchKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            await notifier.RaiseAsync();
        }
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
        if (DataContext is not OnlineLibViewModel online)
            return;
        var settings = App.Services.GetRequiredService<AppConfig>();
        foreach (OnlineTrackViewModel vm in TracksList.SelectedItems!)
        {
            string name = vm.Model.FormedTrackName;
            if (settings.BlacklistedTrackNames.Contains(name))
                settings.BlacklistedTrackNames.Remove(name);
            else
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