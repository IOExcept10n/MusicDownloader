// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagLib;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents a special tag for track cover.
    /// </summary>
    internal class CoverTag : ITag
    {
        private static readonly ILogger<CoverTag> Logger = App.Services.GetRequiredService<ILogger<CoverTag>>();

        private Uri? coverUri;
        private List<IPicture> pictures = [];

        /// <inheritdoc/>
        public event EventHandler? ValueUpdated;

        /// <summary>
        /// Gets a <see cref="Uri"/> for the internally saved track cover.
        /// </summary>
        public static Uri InternalImagesURI { get; } = new(@"internal://cover-image");

        /// <summary>
        /// Gets os sets the main cover image for the tag.
        /// </summary>
        public IPicture? Cover
        {
            get => pictures.FirstOrDefault();
            private set
            {
                ValueUpdated?.Invoke(this, EventArgs.Empty);
                if (value == null)
                {
                    if (pictures.Count > 0)
                        pictures.RemoveAt(0);
                }
                else if (pictures.Count == 0)
                {
                    pictures = [value];
                }
                else
                {
                    pictures[0] = value;
                }
            }
        }

        /// <summary>
        /// Gets the cover <see cref="Uri"/> of the downloaded image.
        /// </summary>
        public Uri? CoverUri => coverUri;

        /// <inheritdoc/>
        public bool HasValue => Value != null;

        /// <inheritdoc/>
        public string Name => nameof(Cover);

        /// <inheritdoc/>
        public object? Value { get => Cover; set => Cover = (IPicture)value!; }

        /// <summary>
        /// Asynchronously downloads cover from the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="coverUri">An <see cref="Uri"/> to download cover from.</param>
        /// <param name="client">An instance of the <see cref="HttpClient"/> to load cover with.</param>
        /// <returns>An instance of the cover tag with loaded image or <see langword="null"/> if the image can't be loaded.</returns>
        public static async Task<CoverTag?> DownloadCoverAsync(Uri coverUri, HttpClient client)
        {
            var cover = await GetCoverAsync(coverUri, client);
            if (cover != null)
            {
                return new CoverTag()
                {
                    coverUri = coverUri,
                    pictures = [cover],
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public void Apply(File file)
        {
            // To reset cover.
            file.Tag.Pictures = null;
            file.Tag.Pictures = [.. pictures];
        }

        /// <inheritdoc/>
        public ITag Clone() => (CoverTag)MemberwiseClone();

        /// <inheritdoc/>
        public void ReadFrom(File file)
        {
            pictures = [.. file.Tag.Pictures];
            if (pictures.Count > 0)
                coverUri = InternalImagesURI;
        }

        /// <summary>
        /// Updates current cover with the new <see cref="Uri"/>.
        /// </summary>
        /// <param name="newUri">New <see cref="Uri"/> to load cover from.</param>
        /// <param name="client">An instance of the <see cref="HttpClient"/> to access online image.</param>
        /// <returns>An asynchronous task for awaiting an operation.</returns>
        public async Task UpdateCoverAsync(Uri newUri, HttpClient client)
        {
            var cover = await GetCoverAsync(newUri, client);
            coverUri = newUri;
            Cover = cover;
        }

        private static async ValueTask<IPicture?> GetCoverAsync(Uri coverUri, HttpClient client)
        {
            if (coverUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return GetCoverFromFile(coverUri.LocalPath);
            }
            else
            {
                return await GetCoverFromHttpAsync(coverUri, client);
            }
        }

        private static PictureLazy? GetCoverFromFile(string filePath) => new(filePath);

        private static async Task<IPicture?> GetCoverFromHttpAsync(Uri coverUri, HttpClient client)
        {
            try
            {
                var response = await client.GetAsync(coverUri);
                if (response.IsSuccessStatusCode)
                {
                    using var memory = new System.IO.MemoryStream();
                    await response.Content.CopyToAsync(memory);
                    memory.Position = 0;
                    return new Picture(ByteVector.FromStream(memory));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Couldn't load image using specified uri: {uri}. Exception details: {ex}", coverUri, ex.Message);
            }

            return null;
        }
    }
}