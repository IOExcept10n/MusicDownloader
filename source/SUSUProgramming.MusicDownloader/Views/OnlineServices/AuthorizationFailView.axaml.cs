// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;
using Avalonia.Controls;

namespace SUSUProgramming.MusicDownloader.Views.OnlineServices;

/// <summary>
/// Represents a view for authorization fail page.
/// </summary>
public partial class AuthorizationFailView : UserControl
{
    private readonly TaskCompletionSource clickSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationFailView"/> class.
    /// </summary>
    public AuthorizationFailView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Waits until user clicks button.
    /// </summary>
    /// <returns>A task that waits until button is clicked.</returns>
    public Task WaitUntilButtonClicked() => clickSource.Task;

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        clickSource.SetResult();
    }
}