// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;
using SUSUProgramming.MusicDownloader.Views;

namespace SUSUProgramming.MusicDownloader;

/// <summary>
/// Represents a main class for an application. It performs all program logic.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the set of used application services.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Gets the current app clipboard.
    /// </summary>
    /// <returns>Clipboard instance to use or <see langword="null"/> if the clipboard cannot be retrieved.</returns>
    public static IClipboard? GetClipboard()
    {
        // Desktop
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            return window.Clipboard;
        }

        // Android (and iOS?)
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var visualRoot = mainView.GetVisualRoot();
            if (visualRoot is TopLevel topLevel)
            {
                return topLevel.Clipboard;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <inheritdoc/>
    public override void RegisterServices()
    {
        base.RegisterServices();

        var services = new ServiceCollection();

        services.AddAppServices();
        Services = services.BuildServiceProvider();
    }
}