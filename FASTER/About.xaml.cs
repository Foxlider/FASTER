using FASTER.Models;

using Microsoft.AppCenter.Analytics;

using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            Loaded += About_Loaded;
        }

        private void IDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Discord", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://discord.gg/uPjgqHU");
        }

        private void IGitHubButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Git", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://github.com/Foxlider/Fox-s-Arma-Server-Tool-Extended-Rewrite");
        }

        private void IForumsButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Forums", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/");
        }

        private void IDonateButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("About - Clicked Donate", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName }
            });
            Functions.OpenBrowser("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA&source=url");
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            string rev = $"{(char)(Assembly.GetExecutingAssembly().GetName().Version.Build + 96)}";
            #if DEBUG
                rev += "-DEV";
            #endif
            string version = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}."
                 + $"{Assembly.GetExecutingAssembly().GetName().Version.Minor}"
                 + $"{rev}";
            IVersionLabel.Text = $"Version: {version}";
        }
    }
}
