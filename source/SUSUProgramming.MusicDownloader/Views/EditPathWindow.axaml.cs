// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.IO;
using Avalonia.Controls;

namespace SUSUProgramming.MusicDownloader.Views;

/// <summary>
/// Represents a window for the path editing.
/// </summary>
public partial class EditPathWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditPathWindow"/> class.
    /// </summary>
    /// <param name="path">Default path to set to text box.</param>
    public EditPathWindow(string? path = null)
        : this()
    {
        PathTextBox.Text = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditPathWindow"/> class.
    /// </summary>
    public EditPathWindow()
    {
        InitializeComponent();
    }

    private void SetFailVisibility(bool failVisibility)
    {
        if (failVisibility)
        {
            PathTextBox.BorderBrush = Avalonia.Media.Brushes.Red;
            PathTextBox.BorderThickness = new Avalonia.Thickness(2);
            FailNotification.IsVisible = true;
        }
        else
        {
            PathTextBox.BorderBrush = Avalonia.Media.Brushes.Transparent;
            PathTextBox.BorderThickness = default;
            FailNotification.IsVisible = false;
        }
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnSave();
    }

    private void OnSave()
    {
        if (!Path.Exists(PathTextBox.Text))
        {
            SetFailVisibility(true);
            return;
        }

        Close(PathTextBox.Text);
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        SetFailVisibility(false);
    }

    private void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            OnSave();
        }
    }
}