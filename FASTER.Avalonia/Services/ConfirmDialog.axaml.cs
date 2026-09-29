using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FASTER.Avalonia.Services;

public partial class ConfirmDialog : Window
{
    public string Message { get => MessageText.Text ?? string.Empty; set => MessageText.Text = value; }

    public ConfirmDialog()
    {
        InitializeComponent();
        Opened += (_, _) => OkButton.Focus();
    }

    private void Ok_Click(object? sender, RoutedEventArgs e) => Close(true);
    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(false);
}
