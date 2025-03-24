// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
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
        /// Gets or sets a value indicating whether to automatically tag tracks on download.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool AutoTagOnDownload { get; set; } = true;

        /// <summary>
        /// Gets or sets the collection of blacklisted paths.
        /// </summary>
        public ObservableCollection<string> BlacklistedPaths { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of blacklisted track names.
        /// </summary>
        public ObservableCollection<string> BlacklistedTrackNames { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of genres.
        /// </summary>
        public ObservableCollection<string> GenresList { get; set; } = [
            "Alternative",
            "Alternative Rock",
            "Alternative Metal",
            "Ambient",
            "Art Rock",
            "Baroque",
            "Blues",
            "Chanson",
            "Chillout",
            "Classical",
            "Country",
            "Dance",
            "Dancehall",
            "Darkwave",
            "Deep House",
            "Disco",
            "Dream Pop",
            "Drum and Bass",
            "Dub",
            "Dubstep",
            "EDM",
            "Electro",
            "Electronic",
            "Emo",
            "Experimental",
            "Folk",
            "Funk",
            "Garage Rock",
            "Gospel",
            "Grime",
            "Hard Rock",
            "Hardcore Punk",
            "Heavy Metal",
            "Hip Hop",
            "House",
            "IDM",
            "Indie Folk",
            "Indie Pop",
            "Indie Rock",
            "Industrial",
            "Jazz",
            "Jazz Fusion",
            "K-Pop",
            "Latin",
            "Latin Jazz",
            "Lo-Fi",
            "Melodic Death Metal",
            "Metal",
            "Minimal",
            "New Wave",
            "Opera",
            "Orchestral",
            "Pop",
            "Pop Punk",
            "Pop Rock",
            "Post-Punk",
            "Post-Rock",
            "Progressive House",
            "Progressive Metal",
            "Progressive Rock",
            "Psychedelic Rock",
            "Punk",
            "Punk Rock",
            "R&B",
            "Rap",
            "Reggae",
            "Reggaeton",
            "Rock",
            "Rock & Roll",
            "Salsa",
            "Shoegaze",
            "Ska",
            "Soft Rock",
            "Soul",
            "Soundtrack",
            "Southern Rock",
            "Surf Rock",
            "Synthpop",
            "Tech House",
            "Techno",
            "Traditional Pop",
            "Trance",
            "Trap",
            "Trip Hop",
            "Tropical House",
            "Vaporwave",
            "World Music",
            "Yacht Rock",
            "Bluegrass",
            "Boogie",
            "Bossa Nova",
            "Celtic",
            "Chillwave",
            "Death Metal",
            "Flamenco",
            "Glam Rock",
            "Hardstyle",
            "J-Pop",
            "Neo-Soul",
            "Post-Hardcore",
            "Progressive Trance",
            "Synthwave",
            "Russian Rock",
            "Russian Romance"
        ];

        /// <summary>
        /// Gets or sets a value indicating whether to rewrite metadata.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool RewriteMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets default path to save app tokens.
        /// </summary>
        public string TokenStoragePath { get; set; } = ".tokencache";

        /// <summary>
        /// Gets or sets the list of artists with slash in their name.
        /// </summary>
        public string[] SlashContainedPerformersList { get; set; } = [
            "AC/DC",
            "A/T/O/S",
            "As/Hi Soundworks",
            "Au/Ra",
            "Bremer/McCoy",
            "b/bqスタヂオ",
            "DOV/S",
            "DJ'TEKINA//SOMETHING",
            "IX/ON",
            "J-CORE SLi//CER",
            "M(a/u)SH",
            "Kaoru/Brilliance",
            "signum/ii",
            "Richiter(LORB/DUGEM DI BARAT)",
            "이달의 소녀 1/3",
            "R!N / Gemie",
            "LOONA 1/3",
            "LOONA / yyxy",
            "LOONA / ODD EYE CIRCLE",
            "K/DA",
            "22/7",
            "諭吉佳作/men",
            "//dARTH nULL",
            "Phantom/Ghost",
            "She/Her/Hers",
            "5/8erl in Ehr'n",
            "Smith/Kotzen"
        ];

        /// <summary>
        /// Gets or sets the collection of tracked music paths.
        /// Defaults to the user's music directory and the common music directory.
        /// </summary>
        public ObservableCollection<string> TrackedPaths { get; set; } =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic),
        ];

        /// <summary>
        /// Gets or sets the path for unsorted tracks.
        /// Defaults to a folder named "Unsorted" in the user's music directory.
        /// </summary>
        public string UnsortedTracksPath { get; set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unsorted");

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

        /// <summary>
        /// Releases the resources used by the <see cref="AppConfig"/> class.
        /// This method saves the current configuration before disposing of the instance.
        /// </summary>
        public void Dispose()
        {
            Save();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Saves the current configuration to the specified path.
        /// </summary>
        public void Save()
        {
            File.WriteAllText(pathToSave, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        private AppConfig WithLoadPath(string loadPath)
        {
            pathToSave = loadPath;
            return this;
        }
    }
}