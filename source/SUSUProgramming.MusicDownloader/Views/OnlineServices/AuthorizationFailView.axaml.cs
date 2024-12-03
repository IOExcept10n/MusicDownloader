using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

public partial class AuthorizationFailView : UserControl
{
    private TaskCompletionSource clickSource = new();

    public AuthorizationFailView()
    {
        InitializeComponent();
    }

    public Task WaitUntilButtonClicked() => clickSource.Task;

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        clickSource.SetResult();
    }
}