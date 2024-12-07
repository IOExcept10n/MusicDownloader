// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using SUSUProgramming.MusicDownloader.Music.StreamingServices;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model for the online track services.
    /// </summary>
    /// <param name="provider">An instance of the media provider to use.</param>
    internal partial class OnlineServicesViewModel(IMediaProvider provider) : ViewModelBase
    {
        /// <summary>
        /// Gets an instance of the media provider to use.
        /// </summary>
        public IMediaProvider MediaProvider => provider;
    }
}