// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a service for the token storage.
    /// </summary>
    internal class TokenStorage
    {
        private const string DefaultTokensPath = ".tokens";

        private readonly Dictionary<string, TokenInfo> tokens = [];
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStorage"/> class.
        /// </summary>
        /// <param name="config">App configuration for the storage.</param>
        public TokenStorage(IConfiguration config)
        {
            configuration = config;
            Load();
        }

        /// <summary>
        /// Checks if the specified token was registered.
        /// </summary>
        /// <param name="serviceName">Name of the service to check.</param>
        /// <returns><see langword="true"/> if token exists; otherwise <see langword="false"/>.</returns>
        public bool TokenRegistered(string serviceName) => tokens.ContainsKey(serviceName);

        /// <summary>
        /// Gets the token info for the specified service.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>Token value.</returns>
        public string GetToken(string serviceName) => tokens[serviceName].Token;

        /// <summary>
        /// Gets the user ID for the specified service.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>User ID value.</returns>
        public long GetUserId(string serviceName) => tokens[serviceName].UserId;

        /// <summary>
        /// Saves the token for the specified service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="token">Token value.</param>
        public void SaveToken(string serviceName, long userId, string token)
        {
            tokens[serviceName] = new(userId, token);
            Save();
        }

        /// <summary>
        /// Deletes the specified token.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public void DeleteToken(string serviceName)
        {
            tokens.Remove(serviceName);
            Save();
        }

        private void Save()
        {
            using var writer = new StreamWriter(configuration["TokenStoragePath"] ?? DefaultTokensPath);
            string json = JsonConvert.SerializeObject(tokens);
            writer.WriteLine(json);
        }

        private void Load()
        {
            string path = configuration["TokenStoragePath"] ?? DefaultTokensPath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(path))
                return;
            using var reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            JsonConvert.PopulateObject(json, tokens);
        }

        private readonly record struct TokenInfo(long UserId, string Token);
    }
}