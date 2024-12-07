using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view-model for user authorization.
    /// </summary>
    /// <param name="media">Media provider instance to authorize in.</param>
    internal partial class AuthorizationViewModel(IMediaProvider media) : ViewModelBase
    {
        private readonly TaskCompletionSource<string> completionSource2FA = new();

        [ObservableProperty]
        private string login = string.Empty;
        [ObservableProperty]
        private string password = string.Empty;
        [ObservableProperty]
        private string data2FA = string.Empty;
        [ObservableProperty]
        private bool is2FARequested;
        [ObservableProperty]
        private bool enableAuthButton = true;
        [ObservableProperty]
        private bool enable2FAButton = true;

        /// <summary>
        /// Authorizes user using selected authorization service.
        /// </summary>
        /// <returns>A task to wait until authorization completes.</returns>
        public async Task AuthorizeAsync()
        {
            EnableAuthButton = false;
            Is2FARequested = true;
            var info = media.AuthService.GetAuthParams(Login, Password, completionSource2FA.Task);
            await media.AuthService.AuthorizeAsync(info);
        }

        /// <summary>
        /// Handles authorization completion.
        /// </summary>
        public void On2FACompleted()
        {
            Is2FARequested = false;
            Enable2FAButton = false;
            completionSource2FA.SetResult(Data2FA);
        }
    }
}
