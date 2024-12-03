// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System;
using System.Net.Http;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Represents a base class for all track metadata providers.
    /// </summary>
    /// <param name="api">An API service that helps with raw API calls.</param>
    /// <param name="url">A base <see cref="Uri"/> for the API provider.</param>
    internal abstract class MetadataProvider(ApiHelper api, string url) : IMetadataProvider
    {
        /// <summary>
        /// Category of the metadata provider (used for logging, not presented yet).
        /// </summary>
        public const string Category = "Metadata";

        /// <summary>
        /// Gets the API helper instance for this metadata provider.
        /// </summary>
        protected ApiHelper Api { get; } = api;

        /// <inheritdoc/>
        public Uri BaseUrl { get; } = new(url);

        /// <summary>
        /// Gets the HTTP client for this API client.
        /// </summary>
        protected HttpClient HttpClient => Api.Client;
    }
}