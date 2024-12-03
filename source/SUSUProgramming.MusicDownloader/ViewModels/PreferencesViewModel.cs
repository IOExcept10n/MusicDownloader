using SUSUProgramming.MusicDownloader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    internal partial class PreferencesViewModel(AppConfig settings) : ViewModelBase
    {
        public AppConfig Settings => settings;

        public string AppInfo => Assembly.GetExecutingAssembly().GetName() switch { var name => $"{name.Name} v.{name.Version}" };
    }
}
