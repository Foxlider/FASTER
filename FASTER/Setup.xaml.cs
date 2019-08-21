using FASTER.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {

        public Setup()
        {
            InitializeComponent();
            IWindowDragBar.MouseDown += WindowDragBar_MouseDown;
            if (Properties.Options.Default.firstRun)
            { Properties.Options.Default.Upgrade(); }
            if (Properties.Options.Default.clearSettings)
                Properties.Options.Default.Reset();

            if (Properties.Options.Default.steamMods == null)
            {
                Properties.Options.Default.steamMods = new SteamModCollection();
                Properties.Options.Default.Save();
            }
            if (Properties.Options.Default.localMods == null)
            {
                Properties.Options.Default.localMods = new List<LocalMod>();
                Properties.Options.Default.Save();
            }
            if (Properties.Options.Default.localModFolders == null)
            {
                Properties.Options.Default.localModFolders = new List<string>();
                Properties.Options.Default.Save();
            }
            //if (Properties.Settings.Default.Servers == null)
            //{ Properties.Settings.Default.Servers = new ServerCollection(); }

            if (!Properties.Options.Default.firstRun)
            {
                MainWindow.Instance.Show();
                Close();
            }
        }

        private void IWindowCloseButton_Click(object sender, RoutedEventArgs e)
        { Close(); }

        private void IWindowMinimizeButton_Click(object sender, RoutedEventArgs e)
        { WindowState = WindowState.Minimized; }

        private void WindowDragBar_MouseDown(object sender, MouseButtonEventArgs e)
        { DragMove(); }

        // Opens folder select dialog when clicking certain buttons
        private void DirButton_Click(object sender, RoutedEventArgs e)
        {
            string path = MainWindow.Instance.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                if ((Equals(sender, ISteamDirButton)))
                { ISteamDirBox.Text = path; }
                else if (Equals(sender, IServerDirButton))
                { IServerDirBox.Text = path; }
            }
        }

        private void IContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var encryption = Encryption.Instance;

            var settings = Properties.Options.Default;
            settings.serverPath = IServerDirBox.Text;
            settings.steamCMDPath = ISteamDirBox.Text;
            settings.steamUserName = ISteamUserBox.Text;
            settings.steamPassword = encryption.EncryptData(ISteamPassBox.Password);
            settings.firstRun = false;
            settings.Save();
            //TODO THIS
            if (IInstallSteamCheck.IsChecked != null && (bool)IInstallSteamCheck.IsChecked)
            { MainWindow.Instance.InstallSteamCmd = true; }

            MainWindow.Instance.Show();
            Close();
        }

        
    }
}
