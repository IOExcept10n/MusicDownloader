using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Defines a builder interface for constructing API calls.
    /// </summary>
    internal interface IApiCallBuilder
    {
        /// <summary>
        /// Executes the API call asynchronously and returns the response as a <see cref="JToken"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the response as a <see cref="JToken"/>.</returns>
        Task<JToken> CallAsync();

        /// <summary>
        /// Sets the endpoint for the API call.
        /// </summary>
        /// <param name="endpoint">The endpoint to be appended to the base URL.</param>
        /// <returns>The current instance of <see cref="IApiCallBuilder"/> for method chaining.</returns>
        IApiCallBuilder WithEndpoint(string endpoint);

        /// <summary>
        /// Adds a parameter to the API call.
        /// </summary>
        /// <param name="key">The key of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The current instance of <see cref="IApiCallBuilder"/> for method chaining.</returns>
        IApiCallBuilder WithParams(string key, string value);
    }
}
