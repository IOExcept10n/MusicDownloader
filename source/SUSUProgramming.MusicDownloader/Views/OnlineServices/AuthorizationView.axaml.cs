using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.ViewModels;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

/// <summary>
/// Represents a view for user authorization.
/// </summary>
[View]
public partial class AuthorizationView : UserControl
{
    private readonly AuthorizationViewModel vm;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationView"/> class.
    /// </summary>
    public AuthorizationView()
    {
        InitializeComponent();
        DataContext = vm = App.Services.GetRequiredService<AuthorizationViewModel>();
    }

    private async void AuthorizeUser(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await vm.AuthorizeAsync();
    }

    private void Confirm2FA(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        vm.On2FACompleted();
    }
}