using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

[View]
public partial class MyOnlineTracksView : UserControl
{
    public MyOnlineTracksView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<OnlineLibViewModel>();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is not OnlineLibViewModel online)
            return;
        await online.Loader.ScanAsync(x => x.ListUserTracksAsync());
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