using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.ViewModels;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Views;

[View]
public partial class PreferencesView : UserControl
{
    public PreferencesView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PreferencesViewModel>();
    }

    private async void EditBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? result = await new EditPathWindow(selectedText).ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(result))
            {
                var paths = prefs.Settings.BlacklistedPaths;
                int index = paths.IndexOf(selectedText);
                paths.RemoveAt(index);
                paths.Insert(index, result);
            }
        }
        catch
        {
        }
    }

    private void RemoveBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        prefs.Settings.BlacklistedPaths.Remove(selectedText);
    }

    private async void AddBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? path = await new EditPathWindow().ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(path))
            {
                prefs.Settings.BlacklistedPaths.Add(path);
            }
        }
        catch
        {
        }
    }

    private async void EditTrackedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? result = await new EditPathWindow(selectedText).ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(result))
            {
                var paths = prefs.Settings.TrackedPaths;
                int index = paths.IndexOf(selectedText);
                paths.RemoveAt(index);
                paths.Insert(index, result);
            }
        }
        catch
        {
        }
    }

    private async void AddTrackedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? path = await new EditPathWindow().ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(path))
            {
                prefs.Settings.TrackedPaths.Add(path);
            }
        }
        catch
        {
        }
    }

    private void RemoveTrackedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        prefs.Settings.TrackedPaths.Remove(selectedText);
    }
}