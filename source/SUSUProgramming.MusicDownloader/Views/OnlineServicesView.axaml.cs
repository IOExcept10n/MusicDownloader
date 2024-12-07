// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;
using SUSUProgramming.MusicDownloader.Views.OnlineServices;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents an online service view.
/// </summary>
[View]
public partial class OnlineServicesView : UserControl, IAuthorizationNavigator
{
    private readonly AuthorizationCoordinator authCoordinator;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnlineServicesView"/> class.
    /// </summary>
    public OnlineServicesView()
    {
        InitializeComponent();
        authCoordinator = App.Services.GetRequiredService<AuthorizationCoordinator>();
        DataContext = App.Services.GetRequiredService<OnlineServicesViewModel>();
    }

    /// <inheritdoc/>
    public async void OnAuthorizationFailed()
    {
        var failView = new AuthorizationFailView();
        ServiceFrame.Content = failView;
        await failView.WaitUntilButtonClicked();
        await authCoordinator.EnsureAuthorizedAsync(this);
    }

    /// <inheritdoc/>
    public void OnAuthorizationSucceeded()
    {
        ServiceFrame.Content = new OnlineView();
    }

    /// <inheritdoc/>
    public async void OpenAuthorizationPage()
    {
        var confirmView = new AuthConfirmationView();
        ServiceFrame.Content = confirmView;
        await confirmView.WaitUntilButtonClicked();
        ServiceFrame.Content = new AuthorizationView();
    }

    /// <inheritdoc/>
    protected override async void OnInitialized()
    {
        base.OnInitialized();
        await authCoordinator.EnsureAuthorizedAsync(this);
    }
}