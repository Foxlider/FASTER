using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Models;

namespace FASTER.Avalonia.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {Functions.GetVersion()} ({Functions.GetRawVersion()})";
    }

    private void Discord_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser("https://discord.gg/2BUuZa3");
    private void GitHub_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser("https://github.com/Foxlider/FASTER");
    private void Wiki_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser("https://github.com/Foxlider/FASTER/wiki");
    private void Donate_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA&source=url");
}
