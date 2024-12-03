// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents an interface for the application authorization system.
    /// </summary>
    internal interface IAuthorizationCoordinator
    {
        /// <summary>
        /// Gets the media provider instance for this coordinator.
        /// </summary>
        IMediaProvider ApiService { get; }

        /// <summary>
        /// Checks if user is authorized and retries authorization otherwise.
        /// </summary>
        /// <param name="navigator">Navigation helper for the authorization pipeline.</param>
        /// <returns>A task that returns <see langword="true"/> on successful authorization; otherwise <see langword="false"/>.</returns>
        ValueTask<bool> EnsureAuthorizedAsync(IAuthorizationNavigator navigator);
    }
}