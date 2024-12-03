using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader;

public partial class AuthConfirmationView : UserControl
{
    private TaskCompletionSource taskSource = new();

    public AuthConfirmationView()
    {
        InitializeComponent();
    }

    public Task WaitUntilButtonClicked() => taskSource.Task;

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        taskSource.SetResult();
    }
}