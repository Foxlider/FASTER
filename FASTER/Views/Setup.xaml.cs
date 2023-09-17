using FASTER.Models;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup
    {
        readonly bool convertMods;


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

            if (Properties.Settings.Default.armaMods == null)
            {
                //We just updated to 1.8
                Properties.Settings.Default.armaMods = new ArmaModCollection();
                Properties.Settings.Default.Save();
                convertMods = true;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.modStagingDirectory))
            {
                Properties.Settings.Default.modStagingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModStagingDirectory");
                Properties.Settings.Default.Save();
            }

            ISteamUserBox.Text = Properties.Settings.Default.steamUserName;
            var installId = AppCenter.GetInstallIdAsync().Result;
            AppCenter.SetUserId($"{installId}_{Properties.Settings.Default.steamUserName}");
            ISteamPassBox.Password = Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword);
            IModStaging.Text = Properties.Settings.Default.modStagingDirectory;
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

            if (Equals(sender, IModStagingDirButton))
            { IModStaging.Text = path; }
            else if (Equals(sender, IServerDirButton))
            { IServerDirBox.Text = path; }
        }

        private void APIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.OpenBrowser("https://steamcommunity.com/dev/apikey");
        }

        private void IContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var encryption = Encryption.Instance;

            if(string.IsNullOrEmpty(IModStaging.Text))
            {
                DisplaySetupMessage("Please enter a valid Mod Staging Directory");
                return;
            }

            if (string.IsNullOrEmpty(IServerDirBox.Text) || !Directory.Exists(IServerDirBox.Text))
            {
                DisplaySetupMessage("Please enter a valid Arma Server Directory");
                return;
            }

            var settings = Properties.Settings.Default;
            settings.serverPath = IServerDirBox.Text;
            settings.modStagingDirectory = IModStaging.Text;
            settings.steamUserName = ISteamUserBox.Text;
            settings.steamPassword = encryption.EncryptData(ISteamPassBox.Password);
            if (!string.IsNullOrEmpty(IApiKeyBox.Text))
                Properties.Settings.Default.SteamAPIKey = IApiKeyBox.Text;
            settings.firstRun = false;
            settings.Save();

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.ModStagingDirectory = settings.modStagingDirectory;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.ApiKey = settings.SteamAPIKey;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.InstallDirectory = settings.serverPath;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Username = settings.steamUserName;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Password = settings.steamPassword;

            if (convertMods)
            { MainWindow.Instance.ConvertMods = true; }

            try
            { MainWindow.Instance.Show(); }
            catch (Exception exception)
            { Crashes.TrackError(exception); }

            Close();
        }
    }
}
