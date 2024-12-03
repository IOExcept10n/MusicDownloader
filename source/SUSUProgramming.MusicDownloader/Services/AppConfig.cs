using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents the application configuration settings, including paths for tracks, genres, and metadata options.
    /// Implements <see cref="IDisposable"/> to ensure proper resource management.
    /// </summary>
    public class AppConfig : IDisposable
    {
        private string pathToSave;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfig"/> class with the specified path to save the configuration.
        /// </summary>
        /// <param name="pathToSave">The path where the configuration will be saved.</param>
        public AppConfig(string pathToSave)
        {
            this.pathToSave = pathToSave;
        }

        /// <summary>
        /// Gets or sets the path for unsorted tracks.
        /// Defaults to a folder named "Unsorted" in the user's music directory.
        /// </summary>
        public string UnsortedTracksPath { get; set; } =
            Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "Unsorted");

        /// <summary>
        /// Gets or sets the collection of tracked music paths.
        /// Defaults to the user's music directory and the common music directory.
        /// </summary>
        public ObservableCollection<string> TrackedPaths { get; set; } = new ObservableCollection<string>
    {
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
        Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic),
    };

        /// <summary>
        /// Gets or sets the collection of genres.
        /// </summary>
        public ObservableCollection<string> GenresList { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the collection of blacklisted paths.
        /// </summary>
        public ObservableCollection<string> BlacklistedPaths { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the collection of blacklisted track names.
        /// </summary>
        public ObservableCollection<string> BlacklistedTrackNames { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to rewrite metadata.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool RewriteMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically tag tracks on download.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool AutoTagOnDownload { get; set; } = true;

        /// <summary>
        /// Loads an existing configuration or initializes a new one if the specified path does not exist.
        /// </summary>
        /// <param name="loadPath">The path to load the configuration from.</param>
        /// <returns>An instance of <see cref="AppConfig"/>.</returns>
        public static AppConfig LoadOrInitialize(string loadPath)
        {
            if (File.Exists(loadPath))
            {
                return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(loadPath))?.WithLoadPath(loadPath) ?? new AppConfig(loadPath);
            }

            return new AppConfig(loadPath);
        }

        private AppConfig WithLoadPath(string loadPath)
        {
            pathToSave = loadPath;
            return this;
        }

        /// <summary>
        /// Saves the current configuration to the specified path.
        /// </summary>
        public void Save()
        {
            File.WriteAllText(pathToSave, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Releases the resources used by the <see cref="AppConfig"/> class.
        /// This method saves the current configuration before disposing of the instance.
        /// </summary>
        public void Dispose()
        {
            Save();
            GC.SuppressFinalize(this);
        }
    }
}
