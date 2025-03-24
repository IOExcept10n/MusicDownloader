// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a service for the token storage.
    /// </summary>
    internal class TokenStorage
    {
        private const string DefaultTokensPath = ".tokens";
        private static readonly ILogger<TokenStorage>? Logger = App.Services?.GetService<ILogger<TokenStorage>>();

        private readonly Dictionary<string, TokenInfo> tokens = [];
        private readonly AppConfig configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStorage"/> class.
        /// </summary>
        /// <param name="config">App configuration for the storage.</param>
        public TokenStorage(AppConfig config)
        {
            Logger?.LogDebug("Initializing TokenStorage");
            configuration = config;
            Load();
            Logger?.LogDebug("TokenStorage initialized with {TokenCount} tokens", tokens.Count);
        }

        /// <summary>
        /// Checks if the specified token was registered.
        /// </summary>
        /// <param name="serviceName">Name of the service to check.</param>
        /// <returns><see langword="true"/> if token exists; otherwise <see langword="false"/>.</returns>
        public bool TokenRegistered(string serviceName)
        {
            var exists = tokens.ContainsKey(serviceName);
            Logger?.LogTrace("Checking if token exists for service {ServiceName}: {Exists}", serviceName, exists);
            return exists;
        }

        /// <summary>
        /// Gets the token info for the specified service.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>Token value.</returns>
        public string? GetToken(string serviceName)
        {
            var token = tokens.TryGetValue(serviceName, out var tokenInfo) ? tokenInfo.Token : null;
            Logger?.LogTrace("Getting token for service {ServiceName}: {Found}", serviceName, token != null);
            return token;
        }

        /// <summary>
        /// Gets the user ID for the specified service.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>User ID value.</returns>
        public long GetUserId(string serviceName)
        {
            var userId = tokens[serviceName].UserId;
            Logger?.LogTrace("Getting user ID for service {ServiceName}: {UserId}", serviceName, userId);
            return userId;
        }

        /// <summary>
        /// Gets the service shared secret to work with.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>Shared secret value.</returns>
        public string? GetSharedSecret(string serviceName)
        {
            var secret = tokens.TryGetValue(serviceName, out var token) ? token.SharedSecret : null;
            Logger?.LogTrace("Getting shared secret for service {ServiceName}: {Found}", serviceName, secret != null);
            return secret;
        }

        /// <summary>
        /// Saves the token for the specified service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="token">Token value.</param>
        /// <param name="sharedSecret">Shared service secret to work with.</param>
        public void SaveToken(string serviceName, long userId, string token, string? sharedSecret = null)
        {
            Logger?.LogDebug("Saving token for service {ServiceName}", serviceName);
            tokens[serviceName] = new(userId, token, sharedSecret);
            Save();
            Logger?.LogDebug("Token saved for service {ServiceName}", serviceName);
        }

        /// <summary>
        /// Deletes the specified token.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public void DeleteToken(string serviceName)
        {
            Logger?.LogDebug("Deleting token for service {ServiceName}", serviceName);
            tokens.Remove(serviceName);
            Save();
            Logger?.LogDebug("Token deleted for service {ServiceName}", serviceName);
        }

        private void Save()
        {
            var path = configuration.TokenStoragePath ?? DefaultTokensPath;
            Logger?.LogDebug("Saving tokens to path: {Path}", path);
            using var writer = new StreamWriter(path);
            string json = JsonConvert.SerializeObject(tokens);
            writer.WriteLine(json);
            Logger?.LogDebug("Tokens saved successfully");
        }

        private void Load()
        {
            string path = configuration.TokenStoragePath ?? DefaultTokensPath;
            Logger?.LogDebug("Loading tokens from path: {Path}", path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Logger?.LogTrace("Creating directory: {Directory}", dir);
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(path))
            {
                Logger?.LogDebug("Token file does not exist, starting with empty storage");
                return;
            }

            using var reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            JsonConvert.PopulateObject(json, tokens);
            Logger?.LogDebug("Tokens loaded successfully");
        }

        private readonly record struct TokenInfo(long UserId, string Token, string? SharedSecret);
    }
}