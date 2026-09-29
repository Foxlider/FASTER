using FASTER.Models;

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
            { wasFirstRun = AppSettings.Current.FirstRun; }
            catch (Exception)
            {
                DisplaySetupMessage("Could not read your configuration file. Check file before continuing");
                return;
            }

            if (wasFirstRun)
            {
                AppSettings.Current.Upgrade();
                AppSettings.Current.FirstRun = false;
                AppSettings.Current.Save();
            }

            if (AppSettings.Current.ClearSettings)
                AppSettings.Current.Reset();

            if (AppSettings.Current.SteamMods == null)
            {
                AppSettings.Current.SteamMods = new SteamModCollection();
                AppSettings.Current.Save();
            }

            if (AppSettings.Current.LocalMods == null)
            {
                AppSettings.Current.LocalMods = new List<LocalMod>();
                AppSettings.Current.Save();
            }

            if (AppSettings.Current.LocalModFolders == null)
            {
                AppSettings.Current.LocalModFolders = new List<string>();
                AppSettings.Current.Save();
            }

            if (AppSettings.Current.ArmaMods == null)
            {
                //We just updated to 1.8
                AppSettings.Current.ArmaMods = new ArmaModCollection();
                AppSettings.Current.Save();
                convertMods = true;
            }

            if (string.IsNullOrEmpty(AppSettings.Current.ModStagingDirectory))
            {
                AppSettings.Current.ModStagingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModStagingDirectory");
                AppSettings.Current.Save();
            }

            ISteamUserBox.Text = AppSettings.Current.SteamUserName;
            ISteamPassBox.Password = Encryption.Instance.DecryptData(AppSettings.Current.SteamPassword);
            IModStaging.Text = AppSettings.Current.ModStagingDirectory;
            IServerDirBox.Text = AppSettings.Current.ServerPath;

            //Do not skip to mainwindow if it was FirstRun
            if (wasFirstRun) return;

            try
            {
                string rev = $"{(char)(Assembly.GetExecutingAssembly().GetName().Version.Build + 96)}";
#if DEBUG
                rev += "-DEV";
#endif
                MainWindow.Instance.Version = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}."
                               + $"{Assembly.GetExecutingAssembly().GetName().Version.Minor}"
                               + $"{rev}";
                MainWindow.Instance.Show();
            }
            catch (Exception e)
            {
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

            if (string.IsNullOrEmpty(IModStaging.Text))
            {
                DisplaySetupMessage("Please enter a valid Mod Staging Directory");
                return;
            }

            if (string.IsNullOrEmpty(IServerDirBox.Text) || !Directory.Exists(IServerDirBox.Text))
            {
                DisplaySetupMessage("Please enter a valid Arma Server Directory");
                return;
            }

            var settings = AppSettings.Current;
            settings.ServerPath = IServerDirBox.Text;
            settings.ModStagingDirectory = IModStaging.Text;
            settings.SteamUserName = ISteamUserBox.Text;
            settings.SteamPassword = encryption.EncryptData(ISteamPassBox.Password);
            if (!string.IsNullOrEmpty(IApiKeyBox.Text))
                AppSettings.Current.SteamAPIKey = IApiKeyBox.Text;
            settings.FirstRun = false;
            settings.Save();

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.ModStagingDirectory = settings.ModStagingDirectory;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.ApiKey = settings.SteamAPIKey;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.InstallDirectory = settings.ServerPath;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Username = settings.SteamUserName;
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Password = settings.SteamPassword;

            if (convertMods)
            { MainWindow.Instance.ConvertMods = true; }

            try
            { MainWindow.Instance.Show(); }
            catch
            { DisplaySetupMessage("Could not start FASTER. Check the Windows Event Logs for details."); }

            Close();
        }
    }
}
