// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Threading.Tasks;
using Avalonia.Controls;

namespace SUSUProgramming.MusicDownloader;

/// <summary>
/// Represents a view for the authorization confirmation.
/// </summary>
public partial class AuthConfirmationView : UserControl
{
    private readonly TaskCompletionSource taskSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthConfirmationView"/> class.
    /// </summary>
    public AuthConfirmationView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Waits until user clicks authorization button.
    /// </summary>
    /// <returns>A task representing wait.</returns>
    public Task WaitUntilButtonClicked() => taskSource.Task;

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        taskSource.SetResult();
    }
}