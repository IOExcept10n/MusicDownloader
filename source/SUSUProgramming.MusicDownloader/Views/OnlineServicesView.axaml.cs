using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Services;
using SUSUProgramming.MusicDownloader.ViewModels;
using SUSUProgramming.MusicDownloader.Views.OnlineServices;

namespace SUSUProgramming.MusicDownloader.Views;

[View]
public partial class OnlineServicesView : UserControl, IAuthorizationNavigator
{
    private readonly AuthorizationCoordinator authCoordinator;

    public OnlineServicesView()
    {
        InitializeComponent();
        authCoordinator = App.Services.GetRequiredService<AuthorizationCoordinator>();
        DataContext = App.Services.GetRequiredService<OnlineServicesViewModel>();
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();
        await authCoordinator.EnsureAuthorizedAsync(this);
    }

    public async void OnAuthorizationFailed()
    {
        var failView = new AuthorizationFailView();
        ServiceFrame.Content = failView;
        await failView.WaitUntilButtonClicked();
        await authCoordinator.EnsureAuthorizedAsync(this);
    }

    public void OnAuthorizationSucceeded()
    {
        ServiceFrame.Content = new OnlineView();
    }

    public async void OpenAuthorizationPage()
    {
        var confirmView = new AuthConfirmationView();
        ServiceFrame.Content = confirmView;
        await confirmView.WaitUntilButtonClicked();
        ServiceFrame.Content = new AuthorizationView();
    }
}