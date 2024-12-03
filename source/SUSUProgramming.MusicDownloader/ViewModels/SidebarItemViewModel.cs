using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    public partial class SidebarItemViewModel : ViewModelBase
    {
        [ObservableProperty] private string iconText = string.Empty;
        [ObservableProperty] private string titleText = string.Empty;
        [ObservableProperty] private string navigationTargetTypeName = string.Empty;
    }
}
