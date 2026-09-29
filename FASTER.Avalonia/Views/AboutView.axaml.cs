using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Models;

namespace FASTER.Avalonia.Views;

public partial class AboutView : UserControl
{
    private const string DiscordInviteUrl = "https://discord.gg/2BUuZa3";
    private const string GitHubRepoUrl = "https://github.com/Foxlider/FASTER";
    private const string WikiUrl = "https://github.com/Foxlider/FASTER/wiki";
    private const string DonateUrl = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA&source=url";

    public AboutView()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {Functions.GetVersion()} ({Functions.GetRawVersion()})";
    }

    private void Discord_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser(DiscordInviteUrl);
    private void GitHub_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser(GitHubRepoUrl);
    private void Wiki_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser(WikiUrl);
    private void Donate_Click(object? sender, RoutedEventArgs e) => Functions.OpenBrowser(DonateUrl);
}
