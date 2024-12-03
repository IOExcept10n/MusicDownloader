using Avalonia.Controls;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Views;

public partial class EditPathWindow : Window
{
    public EditPathWindow(string? path = null) : this()
    {
        PathTextBox.Text = path;
    }

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