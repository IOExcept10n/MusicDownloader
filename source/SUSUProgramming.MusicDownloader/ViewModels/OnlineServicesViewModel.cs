using SUSUProgramming.MusicDownloader.Music.StreamingServices;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class OnlineServicesViewModel(IMediaProvider provider) : ViewModelBase
    {
        public IMediaProvider MediaProvider => provider;


    }
}
