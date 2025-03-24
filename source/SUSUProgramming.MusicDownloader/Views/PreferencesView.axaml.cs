// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents preferences view.
/// </summary>
[View]
public partial class PreferencesView : UserControl
{
    private readonly ILogger<PreferencesView> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreferencesView"/> class.
    /// </summary>
    public PreferencesView()
    {
        InitializeComponent();
        logger = App.Services.GetRequiredService<ILogger<PreferencesView>>();
        DataContext = App.Services.GetRequiredService<PreferencesViewModel>();
        logger.LogInformation("PreferencesView initialized");
    }

    private async void EditBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Editing blacklisted path: {Path}", selectedText);
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
                logger.LogInformation("Blacklisted path updated from '{OldPath}' to '{NewPath}'", selectedText, result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing blacklisted path: {Path}", selectedText);
        }
    }

    private void RemoveBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Removing blacklisted path: {Path}", selectedText);
            prefs.Settings.BlacklistedPaths.Remove(selectedText);
            logger.LogInformation("Blacklisted path removed: {Path}", selectedText);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing blacklisted path: {Path}", selectedText);
        }
    }

    private async void AddBlacklistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Adding new blacklisted path");
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? path = await new EditPathWindow().ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(path))
            {
                prefs.Settings.BlacklistedPaths.Add(path);
                logger.LogInformation("New blacklisted path added: {Path}", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding blacklisted path");
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
            logger.LogDebug("Editing tracked path: {Path}", selectedText);
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
                logger.LogInformation("Tracked path updated from '{OldPath}' to '{NewPath}'", selectedText, result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing tracked path: {Path}", selectedText);
        }
    }

    private async void AddTrackedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Adding new tracked path");
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? path = await new EditPathWindow().ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(path))
            {
                prefs.Settings.TrackedPaths.Add(path);
                logger.LogInformation("New tracked path added: {Path}", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding tracked path");
        }
    }

    private void RemoveTrackedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Removing tracked path: {Path}", selectedText);
            prefs.Settings.TrackedPaths.Remove(selectedText);
            logger.LogInformation("Tracked path removed: {Path}", selectedText);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing tracked path: {Path}", selectedText);
        }
    }

    private async void EditGenreButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Editing genre: {Genre}", selectedText);
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? result = await new EditPathWindow(selectedText).ShowDialog<string?>(window);
            if (!string.IsNullOrEmpty(result))
            {
                var genres = prefs.Settings.GenresList;
                int index = genres.IndexOf(selectedText);
                genres.RemoveAt(index);
                genres.Insert(index, result);
                logger.LogInformation("Genre updated from '{OldGenre}' to '{NewGenre}'", selectedText, result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing genre: {Genre}", selectedText);
        }
    }

    private void AddGenreButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Adding new genre");
            prefs.Settings.GenresList.Add(string.Empty);
            logger.LogInformation("New empty genre added");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding genre");
        }
    }

    private void RemoveGenreButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control c || c.DataContext is not string selectedText)
            return;
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Removing genre: {Genre}", selectedText);
            prefs.Settings.GenresList.Remove(selectedText);
            logger.LogInformation("Genre removed: {Genre}", selectedText);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing genre: {Genre}", selectedText);
        }
    }

    private void SaveGeniusToken_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Saving Genius token");
            prefs.SaveGeniusToken();
            logger.LogInformation("Genius token saved successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving Genius token");
        }
    }

    private void SaveLastFMToken_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Saving Last.FM token");
            prefs.SaveLastFMToken();
            logger.LogInformation("Last.FM token saved successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving Last.FM token");
        }
    }

    private void SaveLastFMSharedSecret_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;
        try
        {
            logger.LogDebug("Saving Last.FM shared secret");
            prefs.SaveLastFMToken();
            logger.LogInformation("Last.FM shared secret saved successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving Last.FM shared secret");
        }
    }

    private async void EditUnsortedPathButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PreferencesViewModel prefs)
            return;

        try
        {
            logger.LogDebug("Editing unsorted path: {path}", prefs.UnsortedTracksPath);
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null)
                return;
            string? result = await new EditPathWindow(prefs.UnsortedTracksPath).ShowDialog<string?>(window);
            if (result != null)
            {
                prefs.UnsortedTracksPath = result;
                logger.LogInformation($"Unsorted tracks path set successfully.");
            }
            else
            {
                logger.LogInformation("User cancelled unsorted tracks update.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing unsorted path");
        }
    }
}