// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
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
    public static IServiceProvider Services { get; private set; }

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc/>
    public override void RegisterServices()
    {
        base.RegisterServices();

        var services = new ServiceCollection();

        services.AddAppServices();
        Services = services.BuildServiceProvider();
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
}