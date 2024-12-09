// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.Metadata;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents an extension class for the <see cref="ApiHelper"/> class.
    /// </summary>
    internal static class ApiExtensions
    {
        /// <summary>
        /// Builds a request for the specified metadata provider.
        /// </summary>
        /// <param name="api">Api helper class instance.</param>
        /// <param name="provider">Metadata provider instance.</param>
        /// <returns>Api call builder to build a call.</returns>
        public static IApiCallBuilder Request(this ApiHelper api, IMetadataProvider provider) => api.BuildRequest(provider.BaseUrl);
    }
}