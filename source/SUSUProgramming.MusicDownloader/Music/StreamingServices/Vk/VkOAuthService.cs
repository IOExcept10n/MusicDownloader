// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Services;
using VkNet;
using VkNet.Model;

namespace SUSUProgramming.MusicDownloader.Music.StreamingServices.Vk
{
    /// <summary>
    /// Represents an OAuth service for the <see cref="VkNet"/>.
    /// </summary>
    /// <param name="api">An instance of the <see cref="VkApi"/> to authorize.</param>
    /// <param name="logger">Logger instance to log data.</param>
    internal partial class VkOAuthService(VkApi api, ILogger<IOAuthService> logger) : IOAuthService
    {
        private TaskCompletionSource authorizationSource = new();

        /// <inheritdoc/>
        public long UserId => api.UserId ?? 0;

        /// <inheritdoc/>
        public string AccessToken => api.Token;

        /// <inheritdoc/>
        public async Task AuthorizeAsync(IApiAuthParams? info)
        {
            try
            {
                await api.AuthorizeAsync(info);
            }
            catch (Exception ex)
            {
                logger.LogError("Couldn't authorize. Exception details: {ex}", ex);
            }

            authorizationSource.TrySetResult();
        }

        /// <inheritdoc/>
        public IApiAuthParams GetAuthParams(string login, string password, Task<string> twoFactorAuthorization)
        {
            return new ApiAuthParams()
            {
                Login = login,
                Password = password,
                TwoFactorAuthorization = () => twoFactorAuthorization.ConfigureAwait(false).GetAwaiter().GetResult(),
                TwoFactorAuthorizationAsync = twoFactorAuthorization,
                TwoFactorSupported = true,
                GrantType = VkNet.Enums.StringEnums.GrantType.Password,
            };
        }

        /// <inheritdoc/>
        public async Task<bool> CheckTokenAsync()
        {
            try
            {
                // TODO: this works only with special authorized apps.
                // To my regret, tokens given by VK ID are not compatible with Audio and Messages API :(
                var results = await api.Audio.GetAsync(new() { Count = 10 });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task LogoutAsync()
        {
            await api.LogOutAsync();
            authorizationSource = new();
        }

        /// <inheritdoc/>
        public Task WhenUserAuthorizes() => authorizationSource.Task;

        /// <inheritdoc/>
        public IApiAuthParams GetAuthParams(long userId, string token) => new ApiAuthParams()
        {
            UserId = userId,
            AccessToken = token,
        };
    }
}