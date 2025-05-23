﻿using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
            var settings = App.Services.GetRequiredService<AppConfig>();
            settings.Save();
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("CrashReport.log", ex.ToString());
            // Throw it again for debugging purposes
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
