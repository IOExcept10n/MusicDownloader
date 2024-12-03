using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views;

[View]
public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void OnLibraryOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainView = this.FindLogicalAncestorOfType<MainView>();
        if (mainView == null)
            return;
        mainView.Sidebar.SelectedIndex = 1;
    }

    private void OnOnlineOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainView = this.FindLogicalAncestorOfType<MainView>();
        if (mainView == null)
            return;
        mainView.Sidebar.SelectedIndex = 2;
    }
}