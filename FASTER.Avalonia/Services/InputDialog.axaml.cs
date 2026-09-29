using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FASTER.Avalonia.Services;

public partial class InputDialog : Window
{
    public string Message { get => MessageText.Text ?? string.Empty; set => MessageText.Text = value; }

    public InputDialog() => InitializeComponent();

    private void Ok_Click(object? sender, RoutedEventArgs e) => Close(InputBox.Text);
    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);
}
