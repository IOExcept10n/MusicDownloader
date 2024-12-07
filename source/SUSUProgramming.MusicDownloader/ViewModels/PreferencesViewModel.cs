// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Reflection;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for user preferences.
    /// </summary>
    /// <param name="settings">Gets the settings to use.</param>
    internal partial class PreferencesViewModel(AppConfig settings) : ViewModelBase
    {
        /// <summary>
        /// Gets the settings instance to use.
        /// </summary>
        public AppConfig Settings => settings;

        /// <summary>
        /// Gets the application short title to display.
        /// </summary>
        public string AppInfo => Assembly.GetExecutingAssembly().GetName() switch { var name => $"{name.Name} v.{name.Version}" };
    }
}