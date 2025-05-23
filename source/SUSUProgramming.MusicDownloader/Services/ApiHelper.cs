﻿// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Provides helper methods for making API calls using an <see cref="HttpClient"/>.
    /// </summary>
    internal class ApiHelper
    {
        private readonly HttpClient client;
        private readonly ILogger<ApiHelper> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHelper"/> class with the specified <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to be used for making API calls.</param>
        /// <param name="logger">The logger to use for logging API operations.</param>
        public ApiHelper(HttpClient client, ILogger<ApiHelper> logger)
        {
            this.client = client;
            this.logger = logger;
            logger.LogInformation("ApiHelper initialized");
        }

        /// <summary>
        /// Gets the <see cref="HttpClient"/> instance used by this helper.
        /// </summary>
        public HttpClient Client => client;

        /// <summary>
        /// Makes an API call to the specified URL and endpoint, with optional parameters.
        /// </summary>
        /// <param name="url">The base URL for the API call.</param>
        /// <param name="endpoint">The endpoint to be appended to the base URL.</param>
        /// <param name="parameters">Optional parameters to include in the API call.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response as a <see cref="JToken"/>.</returns>
        public async Task<JToken> ApiCall(Uri url, string endpoint, Dictionary<string, string>? parameters = null)
        {
            try
            {
                logger.LogDebug("Making API call to {Url}{Endpoint} with {ParamCount} parameters", url, endpoint, parameters?.Count ?? 0);
                Uri destination = new($"{url}{endpoint}{GetParamsString(parameters)}");
                var result = await BuildRequest(destination).CallAsync();
                logger.LogDebug("API call successful");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API call failed to {Url}{Endpoint}", url, endpoint);
                throw;
            }
        }

        /// <summary>
        /// Builds a request using the specified base URL.
        /// </summary>
        /// <param name="url">The base URL for the API call.</param>
        /// <returns>An instance of <see cref="IApiCallBuilder"/> for constructing the API call.</returns>
        public IApiCallBuilder BuildRequest(Uri url)
        {
            logger.LogDebug("Building API request for URL: {Url}", url);
            return new ApiCallBuilder(Client, url, logger);
        }

        /// <summary>
        /// Constructs a query string from the provided parameters.
        /// </summary>
        /// <param name="parameters">A dictionary of parameters to include in the query string.</param>
        /// <returns>A query string formatted as <c>?key=value&amp;key2=value2</c>.</returns>
        private static string GetParamsString(Dictionary<string, string>? parameters)
        {
            StringBuilder paramsList = new();
            if (parameters != null && parameters.Count > 0)
            {
                paramsList.Append('?');
                bool first = true;
                foreach (var parameter in parameters)
                {
                    if (!first) paramsList.Append('&');
                    paramsList.Append(parameter.Key).Append('=').Append(parameter.Value);
                    first = false;
                }
            }

            return paramsList.ToString();
        }

        /// <summary>
        /// A private implementation of <see cref="IApiCallBuilder"/> for constructing API calls.
        /// </summary>
        private class ApiCallBuilder : IApiCallBuilder
        {
            private readonly HttpClient client;
            private readonly ILogger<ApiHelper> logger;
            private Uri uri;

            /// <summary>
            /// Initializes a new instance of the <see cref="ApiCallBuilder"/> class.
            /// </summary>
            /// <param name="client">The <see cref="HttpClient"/> to use for the API call.</param>
            /// <param name="uri">The base URI for the API call.</param>
            /// <param name="logger">The logger to use for logging API operations.</param>
            public ApiCallBuilder(HttpClient client, Uri uri, ILogger<ApiHelper> logger)
            {
                this.client = client;
                this.uri = uri;
                this.logger = logger;
            }

            /// <summary>
            /// Executes the API call asynchronously and returns the response as a <see cref="JToken"/>.
            /// </summary>
            /// <returns>A task that represents the asynchronous operation, containing the response as a <see cref="JToken"/>.</returns>
            public async Task<JToken> CallAsync()
            {
                try
                {
                    logger.LogDebug("Making HTTP GET request to {Uri}", uri);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogDebug("Received response with status code {StatusCode}", response.StatusCode);
                    return JToken.Parse(content);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "HTTP GET request failed to {Uri}", uri);
                    throw;
                }
            }

            /// <summary>
            /// Sets the endpoint for the API call.
            /// </summary>
            /// <param name="endpoint">The endpoint to be appended to the base URI.</param>
            /// <returns>The current instance of <see cref="IApiCallBuilder"/> for method chaining.</returns>
            public IApiCallBuilder WithEndpoint(string endpoint)
            {
                logger.LogDebug("Adding endpoint {Endpoint} to {Uri}", endpoint, uri);
                uri = new Uri(uri, endpoint);
                return this;
            }

            /// <summary>
            /// Adds a parameter to the API call.
            /// </summary>
            /// <param name="key">The key of the parameter.</param>
            /// <param name="value">The value of the parameter.</param>
            /// <returns>The current instance of <see cref="IApiCallBuilder"/> for method chaining.</returns>
            public IApiCallBuilder WithParams(string key, string value)
            {
                logger.LogDebug("Adding parameter {Key}={Value} to {Uri}", key, value, uri);
                string query;
                if (string.IsNullOrWhiteSpace(uri.Query))
                {
                    query = $"?{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                }
                else
                {
                    query = uri.Query + $"&{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                }

                uri = new Uri(uri.GetLeftPart(UriPartial.Path) + query);
                return this;
            }
        }
    }
}