using FASTER.Models;

using Microsoft.AppCenter.Analytics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup
    {
        public Setup()
        {
            InitializeComponent();
            bool wasFirstRun;

            //Check if configuration can be read. Else, display error message and don't continue
            try
            { wasFirstRun = Properties.Settings.Default.firstRun; }
            catch (Exception)
            {
                DisplaySetupMessage("Could not read your configuration file. Check file before continuing");
                return;
            }
            
            if (wasFirstRun)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.clearSettings)
                Properties.Settings.Default.Reset();

            if (Properties.Settings.Default.steamMods == null)
            {
                Properties.Settings.Default.steamMods = new SteamModCollection();
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.localMods == null)
            {
                Properties.Settings.Default.localMods = new List<LocalMod>();
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.localModFolders == null)
            {
                Properties.Settings.Default.localModFolders = new List<string>();
                Properties.Settings.Default.Save();
            }

            ISteamUserBox.Text = Properties.Settings.Default.steamUserName;
            var installId = AppCenter.GetInstallIdAsync().Result;
            AppCenter.SetUserId($"{installId}_{Properties.Settings.Default.steamUserName}");
            ISteamPassBox.Password = Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword);
            ISteamDirBox.Text = Properties.Settings.Default.steamCMDPath;
            IServerDirBox.Text = Properties.Settings.Default.serverPath;

            //Do not skip to mainwindow if it was FirstRun
            if (wasFirstRun ) return;

            try
            {
                string rev = $"{(char)(Assembly.GetExecutingAssembly().GetName().Version.Build + 96)}";
#if DEBUG
                rev += "-DEV";
#endif
                MainWindow.Instance.Version = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}."
                               + $"{Assembly.GetExecutingAssembly().GetName().Version.Minor}"
                               + $"{rev}";
                Analytics.TrackEvent("Setup - Launching", new Dictionary<string, string> {
                    { "Name", Properties.Settings.Default.steamUserName },
                    { "Version", MainWindow.Instance.Version },
                    { "Region", RegionInfo.CurrentRegion.TwoLetterISORegionName},
                    { "CPU Architecture", Environment.Is64BitOperatingSystem ? "x64" : "x86" },
                    { "OS Version", Environment.OSVersion.VersionString },
                    { "Machine Name", Environment.MachineName }
                });
                AppInsights.Client.TrackEvent("Setup - Launching", new Dictionary<string, string> {
                    { "Name", Properties.Settings.Default.steamUserName },
                    { "Version", MainWindow.Instance.Version },
                    { "Region", RegionInfo.CurrentRegion.TwoLetterISORegionName},
                    { "CPU Architecture", Environment.Is64BitOperatingSystem ? "x64" : "x86" },
                    { "OS Version", Environment.OSVersion.VersionString },
                    { "Machine Name", Environment.MachineName }
                });
                MainWindow.Instance.Show();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e, new Dictionary<string, string> { {"Message", $"Could not start FASTER: \n[{ e.GetType()}] { e.Message}\n\n{ e.StackTrace}"}});
                using EventLog eventLog = new EventLog("Application")
                { Source = "FASTER" };
                eventLog.WriteEntry($"Could not start FASTER : \n[{e.GetType()}] {e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error);
            }

            Close();
        }

        //Display Error messages
        public void DisplaySetupMessage(string message)
        {
            IFlyoutSetupMessage.Text = message;
            IFlyoutSetup.IsOpen = true;
        }

        // Opens folder select dialog when clicking certain buttons
        private void DirButton_Click(object sender, RoutedEventArgs e)
        {
            string path = MainWindow.Instance.SelectFolder();

            if (string.IsNullOrEmpty(path)) return;

            if (Equals(sender, ISteamDirButton))
            { ISteamDirBox.Text = path; }
            else if (Equals(sender, IServerDirButton))
            { IServerDirBox.Text = path; }
        }

        private void IContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var encryption = Encryption.Instance;

            var settings = Properties.Settings.Default;
            settings.serverPath = IServerDirBox.Text;
            settings.steamCMDPath = ISteamDirBox.Text;
            settings.steamUserName = ISteamUserBox.Text;
            settings.steamPassword = encryption.EncryptData(ISteamPassBox.Password);
            settings.firstRun = false;
            settings.Save();

            if (IInstallSteamCheck.IsChecked != null && (bool)IInstallSteamCheck.IsChecked)
            { MainWindow.Instance.InstallSteamCmd = true; }

            try
            { MainWindow.Instance.Show(); }
            catch (Exception exception)
            { Crashes.TrackError(exception); }
            
            Close();
        }
    }
}
