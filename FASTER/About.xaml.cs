using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
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

        { OpenBrowser("https://discord.gg/uPjgqHU"); }

        private void IGitHubButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://github.com/Foxlider/Fox-s-Arma-Server-Tool-Extended-Rewrite");
        }

        private void IForumsButton_Click(object sender, RoutedEventArgs e)
        { OpenBrowser("https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/"); }

        private void IDonateButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA&source=url");
        }
        
        private void OpenBrowser(string url)
        {
            try
            { Process.Start(url); }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                { Process.Start("xdg-open", url); }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                { Process.Start("open", url); }
                else
                { throw; }
            }
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
