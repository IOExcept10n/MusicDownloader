using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

[View]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainViewModel>();
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (DataContext as MainViewModel)?.OnSidebarItemSelected(Sidebar.SelectedItem as SidebarItemViewModel);
    }
}
