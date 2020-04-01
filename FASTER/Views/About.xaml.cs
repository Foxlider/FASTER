using FASTER.Models;

using Microsoft.AppCenter.Analytics;

using System;
using System.Collections.Generic;
using System.Windows;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About
    {
        private static readonly string[] StartThanks =
        {
            "Big up to",
            "Thanks to",
            "Can't thank enough"
        };

        private static readonly string[] EndThanks =
        {
            " !",
            ", FASTER supporter !",
            "for supporting this application !",
            "for supporting FASTER !"
        };

        private readonly Random r = new Random();

        public About()
        {
            InitializeComponent();
        }

        public MainWindow MetroWindow => (MainWindow)Window.GetWindow(this);

        private void IDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Discord", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            AppInsights.Client.TrackEvent("About - Clicked Discord", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://discord.gg/2BUuZa3");
        }

        private void IGitHubButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Git", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            AppInsights.Client.TrackEvent("About - Clicked Git", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://github.com/Foxlider/Fox-s-Arma-Server-Tool-Extended-Rewrite");
        }

        private void IForumsButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Forums", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            AppInsights.Client.TrackEvent("About - Clicked Forums", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/");
        }

        private void IDonateButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Donate", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            AppInsights.Client.TrackEvent("About - Clicked Donate", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA&source=url");
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            IVersionLabel.Text = MetroWindow.GetVersion();
        }

        private void IDonateButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        { IDonateButton.ToolTip = $"{StartThanks[r.Next(StartThanks.Length)]} {StaticData.Supporters[r.Next(StaticData.Supporters.Length)]} {EndThanks[r.Next(EndThanks.Length)]}"; }
    }
}
