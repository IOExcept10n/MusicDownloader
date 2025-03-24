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
            logger.LogDebug("Checking authorization status for provider: {ProviderName}", mediaProvider.Name);
            bool result;
            if (!ApiService.Authorized)
            {
                logger.LogInformation("Provider {ProviderName} is not authorized, starting authorization process", mediaProvider.Name);
                result = await AuthorizeAsync(navigator);
            }
            else
            {
                logger.LogDebug("Provider {ProviderName} is already authorized", mediaProvider.Name);
                result = ApiService.Authorized;
            }

            if (result)
            {
                logger.LogInformation("Authorization succeeded for provider: {ProviderName}", mediaProvider.Name);
                navigator.OnAuthorizationSucceeded();
            }
            else
            {
                logger.LogWarning("Authorization failed for provider: {ProviderName}", mediaProvider.Name);
                navigator.OnAuthorizationFailed();
            }

            return result;
        }

        private async Task<bool> AuthorizeAsync(IAuthorizationNavigator navigator)
        {
            try
            {
                logger.LogDebug("Starting authorization process for provider: {ProviderName}", mediaProvider.Name);
                return await AuthorizeInternalAsync(App.Services.GetRequiredService<TokenStorage>(), navigator);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Authorization failed for provider {ProviderName}. Error: {ErrorMessage}", mediaProvider.Name, ex.Message);
                return false;
            }
        }

        private async Task<bool> AuthorizeInternalAsync(TokenStorage tokens, IAuthorizationNavigator navigator)
        {
            string? token = null;
            long userId = 0;
            if (tokens.TokenRegistered(ApiService.Name))
            {
                logger.LogDebug("Found existing token for provider: {ProviderName}", ApiService.Name);
                token = tokens.GetToken(ApiService.Name);
                userId = tokens.GetUserId(ApiService.Name);
            }
            else
            {
                logger.LogDebug("No existing token found for provider: {ProviderName}", ApiService.Name);
            }

            var auth = ApiService.AuthService;
            if (string.IsNullOrEmpty(token))
            {
                logger.LogInformation("Starting new authorization flow for provider: {ProviderName}", ApiService.Name);
                navigator.OpenAuthorizationPage();
                await auth.WhenUserAuthorizes();
                logger.LogInformation("User authorized successfully for provider: {ProviderName}", ApiService.Name);
                tokens.SaveToken(ApiService.Name, auth.UserId, auth.AccessToken);
                logger.LogDebug("Saved new token for provider: {ProviderName}", ApiService.Name);
            }
            else
            {
                logger.LogDebug("Using existing token for provider: {ProviderName}", ApiService.Name);
                IApiAuthParams? info = auth.GetAuthParams(userId, token);
                await auth.AuthorizeAsync(info);
                logger.LogDebug("Reauthorized with existing token for provider: {ProviderName}", ApiService.Name);
            }

            logger.LogDebug("Validating token for provider: {ProviderName}", ApiService.Name);
            bool isValid = await auth.CheckTokenAsync();
            if (!isValid)
            {
                logger.LogWarning("Token validation failed for provider: {ProviderName}", ApiService.Name);
                await auth.LogoutAsync();
                return false;
            }

            logger.LogInformation("Token validation successful for provider: {ProviderName}", ApiService.Name);
            return true;
        }
    }
}