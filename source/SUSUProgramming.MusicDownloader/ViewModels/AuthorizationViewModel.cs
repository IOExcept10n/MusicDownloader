using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class AuthorizationViewModel(IMediaProvider media) : ViewModelBase
    {
        [ObservableProperty] private string login = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string data2FA = string.Empty;
        [ObservableProperty] private bool is2FARequested;
        [ObservableProperty] private bool enableAuthButton = true;
        [ObservableProperty] private bool enable2FAButton = true;

        private readonly TaskCompletionSource<string> completionSource2FA = new();

        public async Task AuthorizeAsync()
        {
            EnableAuthButton = false;
            Is2FARequested = true;
            var info = media.AuthService.GetAuthParams(Login, Password, completionSource2FA.Task);
            await media.AuthService.AuthorizeAsync(info);
        }

        public void On2FACompleted()
        {
            Is2FARequested = false;
            Enable2FAButton = false;
            completionSource2FA.SetResult(Data2FA);
        }
    }
}
