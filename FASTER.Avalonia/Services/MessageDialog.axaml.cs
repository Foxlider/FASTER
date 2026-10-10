using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FASTER.Avalonia.Services;

public partial class MessageDialog : Window
{
    public string Message { get => MessageText.Text ?? string.Empty; set => MessageText.Text = value; }

    public MessageDialog() => InitializeComponent();

    private void Ok_Click(object? sender, RoutedEventArgs e) => Close(true);
}
