using System.Collections.Generic;
using System.Threading.Tasks;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.Music.StreamingServices
{
    /// <summary>
    /// Represents the interface for the online tracks provider.
    /// </summary>
    public interface IMediaProvider
    {
        /// <summary>
        /// Gets the name of the tracks provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the service is authorized.
        /// </summary>
        bool Authorized { get; }

        /// <summary>
        /// Gets the authorization service for this <see cref="IMediaProvider"/> instance.
        /// </summary>
        IOAuthService AuthService { get; }

        /// <summary>
        /// Gets user library tracks.
        /// </summary>
        /// <returns>An async enumeration of user tracks.</returns>
        IAsyncEnumerable<TrackDetails> ListUserTracksAsync();

        /// <summary>
        /// Downloads track into the specified path.
        /// </summary>
        /// <param name="track">Track details to load.</param>
        /// <param name="downloadDirectoryPath">Path to save track to.</param>
        /// <returns>Details of the downloaded track or <see langword="null"/> if track wasn't loaded.</returns>
        Task<TrackDetails?> DownloadTrackAsync(TrackDetails track, string downloadDirectoryPath);

        /// <summary>
        /// Searches for the tracks with the specified query.
        /// </summary>
        /// <param name="query">Query to search tracks.</param>
        /// <returns>An async enumeration of found tracks.</returns>
        IAsyncEnumerable<TrackDetails> SearchAsync(string query);

        /// <summary>
        /// Gets user recommendations list from the presented streaming service.
        /// </summary>
        /// <returns>An async enumeration of the user recommendations.</returns>
        IAsyncEnumerable<TrackDetails> ListUserRecommendationsAsync();
    }
}
