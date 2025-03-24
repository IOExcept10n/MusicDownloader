// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Reflection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for user preferences.
    /// </summary>
    internal partial class PreferencesViewModel : ViewModelBase
    {
        private readonly AppConfig settings;
        private readonly TokenStorage tokenStorage;
        private readonly ILogger<PreferencesViewModel> logger;
        private string geniusToken;
        private string lastFMToken;
        private string lastFMSharedSecret;
        private string unsortedTracksPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesViewModel"/> class.
        /// </summary>
        /// <param name="settings">The settings to use.</param>
        /// <param name="tokenStorage">The token storage to use.</param>
        /// <param name="logger">The logger to use.</param>
        public PreferencesViewModel(AppConfig settings, TokenStorage tokenStorage, ILogger<PreferencesViewModel> logger)
        {
            this.settings = settings;
            this.tokenStorage = tokenStorage;
            this.logger = logger;
            geniusToken = tokenStorage.GetToken("Genius") ?? string.Empty;
            lastFMToken = tokenStorage.GetToken("LastFM") ?? string.Empty;
            lastFMSharedSecret = tokenStorage.GetSharedSecret("LastFM") ?? string.Empty;
            unsortedTracksPath = settings.UnsortedTracksPath;
            logger.LogInformation("PreferencesViewModel initialized");
        }

        /// <summary>
        /// Gets the settings instance to use.
        /// </summary>
        public AppConfig Settings => settings;

        /// <summary>
        /// Gets or sets the Genius API token.
        /// </summary>
        public string GeniusToken
        {
            get => geniusToken;
            set
            {
                if (SetProperty(ref geniusToken, value))
                {
                    logger.LogDebug("Genius token changed");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Last.FM API token.
        /// </summary>
        public string LastFMToken
        {
            get => lastFMToken;
            set
            {
                if (SetProperty(ref lastFMToken, value))
                {
                    logger.LogDebug("Last.FM token changed");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Last.FM shared secret.
        /// </summary>
        public string LastFMSharedSecret
        {
            get => lastFMSharedSecret;
            set
            {
                if (SetProperty(ref lastFMSharedSecret, value))
                {
                    logger.LogDebug("Last.FM shared secret changed");
                }
            }
        }

        /// <summary>
        /// Gets or sets the unsorted tracks path.
        /// </summary>
        public string UnsortedTracksPath
        {
            get => unsortedTracksPath;
            set
            {
                if (SetProperty(ref unsortedTracksPath, value))
                {
                    settings.UnsortedTracksPath = unsortedTracksPath;
                    logger.LogDebug("Unsorted tracks path has been updated.");
                }
            }
        }

        /// <summary>
        /// Gets the application short title to display.
        /// </summary>
        public string AppInfo => Assembly.GetExecutingAssembly().GetName() switch { var name => $"{name.Name} v.{name.Version}" };

        /// <summary>
        /// Saves the Genius API token.
        /// </summary>
        public void SaveGeniusToken()
        {
            if (!string.IsNullOrEmpty(GeniusToken))
            {
                logger.LogInformation("Saving Genius API token");
                tokenStorage.SaveToken("Genius", 0, GeniusToken, null);
                logger.LogInformation("Genius API token saved successfully");
            }
            else
            {
                logger.LogWarning("Attempted to save empty Genius API token");
            }
        }

        /// <summary>
        /// Saves the Last.FM API token.
        /// </summary>
        public void SaveLastFMToken()
        {
            if (!string.IsNullOrEmpty(LastFMToken))
            {
                logger.LogInformation("Saving Last.FM API token");
                tokenStorage.SaveToken("LastFM", 0, LastFMToken, LastFMSharedSecret);
                logger.LogInformation("Last.FM API token saved successfully");
            }
            else
            {
                logger.LogWarning("Attempted to save empty Last.FM API token");
            }
        }
    }
}