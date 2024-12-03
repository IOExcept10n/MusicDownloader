using System.Threading.Tasks;
using VkNet.Model;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a service that authorizes user into specified media provider.
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Gets User ID for authorization.
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// Gets Access token to authorize.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Checks if the token is valid for music downloading.
        /// </summary>
        /// <returns><see langword="true"/> if the token is valid; otherwise <see langword="false"/>.</returns>
        Task<bool> CheckTokenAsync();

        /// <summary>
        /// Authorizes user with the specified auth params.
        /// </summary>
        /// <param name="info">Info to authorize.</param>
        /// <returns>Task for the operation.</returns>
        Task AuthorizeAsync(IApiAuthParams? info);

        /// <summary>
        /// Logs user out.
        /// </summary>
        /// <returns>Task for the operation.</returns>
        Task LogoutAsync();

        /// <summary>
        /// Waits until user authorizes.
        /// </summary>
        /// <returns>Task to wait.</returns>
        Task WhenUserAuthorizes();

        /// <summary>
        /// Gets auth params for the specified credentials.
        /// </summary>
        /// <param name="userId">User ID to authorize.</param>
        /// <param name="token">Token to authorize.</param>
        /// <returns>Auth params with the specified data.</returns>
        IApiAuthParams GetAuthParams(long userId, string token);

        /// <summary>
        /// Gets auth params for the specified credentials.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="password">User password.</param>
        /// <param name="twoFactorAuthorization">Function to authorize with 2FA.</param>
        /// <returns>Auth params with the specified data.</returns>
        IApiAuthParams GetAuthParams(string login, string password, Task<string> twoFactorAuthorization);
    }
}
