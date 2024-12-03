// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using VkNet.Model;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a helper class the performs user authorization inside app.
    /// </summary>
    /// <param name="mediaProvider">An instance of the media provider to authorize.</param>
    /// <param name="logger">An instance of logger to log progress.</param>
    internal class AuthorizationCoordinator(IMediaProvider mediaProvider, ILogger<AuthorizationCoordinator> logger) : IAuthorizationCoordinator
    {
        /// <inheritdoc/>
        public IMediaProvider ApiService => mediaProvider;

        /// <inheritdoc/>
        public async ValueTask<bool> EnsureAuthorizedAsync(IAuthorizationNavigator navigator)
        {
            bool result;
            if (!ApiService.Authorized)
                result = await AuthorizeAsync(navigator);
            else
                result = ApiService.Authorized;
            if (result)
            {
                navigator.OnAuthorizationSucceeded();
            }
            else
            {
                navigator.OnAuthorizationFailed();
            }

            return result;
        }

        private async Task<bool> AuthorizeAsync(IAuthorizationNavigator navigator)
        {
            try
            {
                return await AuthorizeInternalAsync(App.Services.GetRequiredService<TokenStorage>(), navigator);
            }
            catch (Exception ex)
            {
                logger.LogError("Couldn't authorize media service ({media}). Exception message: {ex}", mediaProvider.Name, ex.Message);
                return false;
            }
        }

        private async Task<bool> AuthorizeInternalAsync(TokenStorage tokens, IAuthorizationNavigator navigator)
        {
            string? token = null;
            long userId = 0;
            if (tokens.TokenRegistered(ApiService.Name))
            {
                token = tokens.GetToken(ApiService.Name);
                userId = tokens.GetUserId(ApiService.Name);
            }

            var auth = ApiService.AuthService;
            if (string.IsNullOrEmpty(token))
            {
                navigator.OpenAuthorizationPage();
                await auth.WhenUserAuthorizes();
                tokens.SaveToken(ApiService.Name, auth.UserId, auth.AccessToken);
            }
            else
            {
                IApiAuthParams? info = auth.GetAuthParams(userId, token);
                await auth.AuthorizeAsync(info);
            }

            bool isValid = await auth.CheckTokenAsync();
            if (!isValid)
            {
                if (tokens.TokenRegistered(ApiService.Name))
                {
                    tokens.DeleteToken(ApiService.Name);
                }

                await auth.LogoutAsync();
                return false;
            }

            return true;
        }
    }
}