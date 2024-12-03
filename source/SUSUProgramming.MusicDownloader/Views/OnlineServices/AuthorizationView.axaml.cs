using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

[View]
public partial class AuthorizationView : UserControl
{
    private readonly AuthorizationViewModel vm;

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