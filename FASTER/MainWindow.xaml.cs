using FASTER.Models;
using FASTER.Views;

using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.AppCenter.Crashes;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal bool InstallSteamCmd { get; set; }
        internal string Version;
        internal bool NavEnabled = true;

        private ToggleButton lastNavButton;

        #region INSTANCES
        private static MainWindow _instance;
        public static MainWindow Instance => _instance ??= new MainWindow();

        SteamUpdater _steamUpdater;
        public SteamUpdater ContentSteamUpdater
        {
            get => _steamUpdater ??= new SteamUpdater();
            set => _steamUpdater = value;
        }

        SteamMods _steamMods;
        public SteamMods ContentSteamMods
        {
            get => _steamMods ??= new SteamMods();
            set => _steamMods = value;
        }

        LocalMods _localMods;
        public LocalMods ContentLocalMods
        {
            get => _localMods ??= new LocalMods();
            set => _localMods = value;
        }

        ServerStatus _serverStatus;
        public ServerStatus ContentServerStatus
        {
            get => _serverStatus ??= new ServerStatus();
            set => _serverStatus = value;
        }

        List<Views.ServerProfile> _profiles;
        public List<Views.ServerProfile> ContentProfiles
        {
            get => _profiles ??= new List<Views.ServerProfile>();
            set => _profiles = value;
        }

        Settings _settings;
        public Settings ContentSettings
        {
            get => _settings ??= new Settings();
            set => _settings = value;
        }

        About _about;
        public About ContentAbout
        {
            get => _about ??= new About();
            set => _about = value;
        }

        Profile _profile;
        public Profile ContentProfile
        {
            get => _profile ?? new Profile();
            set => _profile = value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            Version = GetVersion();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NavigateToConsole();
        }

        #region EVENTS
        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            if (CheckAdmin())
            {
                Functions.CheckSettings();
                LoadServerProfiles();
            }
            else
            { Close(); }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (InstallSteamCmd)
            { InstallSteam(); }
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ToggleButton>();
            list.AddRange(IMainMenuItems.Items.Cast<ToggleButton>().Where(i => i.IsChecked == true));
            list.AddRange(IServerProfilesMenu.Items.Cast<ToggleButton>().Where(i => i.IsChecked == true));
            list.AddRange(IOtherMenuItems.Items.Cast<ToggleButton>().Where(i => i.IsChecked == true));

            if (!(sender is ToggleButton nav) || !NavEnabled) return;

            //Don't navigate if same menu is clicked
            if (nav == lastNavButton) return;

            

            //Clear selected Buttons
            IServerProfilesMenu.SelectedItem = null;
            foreach (var item in list.Where(item => item.Name != nav.Name))
            { item.IsChecked = false; }
                
            nav.IsChecked = true;
            lastNavButton = nav;

            AppInsights.Client.TrackPageView(nav.Name);
            
            //Get loading screen
            switch (nav.Name)
            {
                case "navSteamUpdater":
                    MainContent.Navigate(ContentSteamUpdater);
                    break;
                case "navSteamMods":
                    MainContent.Navigate(ContentSteamMods);
                    break;
                case "navLocalMods":
                    MainContent.Navigate(ContentLocalMods);
                    break;
                case "navServerStatus":
                    //MainContent.Navigate(ContentServerStatus);
                    MainContent.Navigate(ContentProfile);
                    break;
                case "navSettings":
                    MainContent.Navigate(ContentSettings);
                    break;
                case "navAbout":
                    MainContent.Navigate(ContentAbout);
                    break;
                default:
                    if (IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == nav.Name) != null)
                    { MainContent.Navigate(ContentProfiles.First(p => p.Name == nav.Name)); }
                    break;
            }
        }

        private void MainContent_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        { MainContent.NavigationService.RemoveBackEntry(); }

        private void INewServerProfileButton_Click(object sender, RoutedEventArgs e)
        {
            INewServerProfileDialog.Visibility = Visibility.Visible;
            INewServerProfileDialog.IsOpen = true;
        }

        private void IToolsDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            { IToolsDialog.IsOpen = false; }
        }

        private void ICreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("Main - Creating new profile");
            AppInsights.Client.TrackEvent("Main - Creating new profile");
            INewProfileName.Text = INewProfileName.Text.Trim();
            if (string.IsNullOrEmpty(INewProfileName.Text))
            {
                INewServerProfileDialog.IsOpen = false;
                DisplayMessage("Please use a suitable profile name.");
            }
            else
            {
                var profileName = INewProfileName.Text;
                INewServerProfileDialog.IsOpen = false;
                ServerCollection.AddServerProfile(profileName, "_" + Functions.SafeName(profileName));
                INewProfileName.Text = string.Empty;
            }
        }

        private void MenuItemClone_Click(object sender, RoutedEventArgs e)
        {
            if (IServerProfilesMenu.SelectedIndex == -1)
            { return; }

            try
            {
                var temp = Properties.Settings.Default.Servers.ServerProfiles.FirstOrDefault(s =>
                    s.SafeName == ((ToggleButton) IServerProfilesMenu.SelectedItem).Name);
                if (temp == null)
                {
                    DisplayMessage("Could not find the selected profile.");
                    return;
                }

                Models.ServerProfile serverProfile = temp.CloneObjectSerializable();
                serverProfile.DisplayName += " 2";
                serverProfile.SafeName = "_" + Functions.SafeName(serverProfile.DisplayName);
                ServerCollection.AddServerProfile(serverProfile);
            }
            catch (Exception err)
            {
                DisplayMessage("An error occured while cloning your profile");
                Crashes.TrackError(err, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } });
            }
        }

        private void InstallSteamCmd_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            MainGrid.Effect = null;
            InstallSteam();
        }

        private void OpenArmaServerLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            var serverDirBox = ContentSteamUpdater.IServerDirBox.Text;

            if (!string.IsNullOrEmpty(serverDirBox) && Directory.Exists(serverDirBox))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = serverDirBox, FileName = "explorer.exe" };
                    Process.Start(startInfo);
                }
                catch
                { MessageBox.Show($" Could not open {serverDirBox}"); }
            }
            else
            { MessageBox.Show($"{serverDirBox} Directory does not exist!"); }
        }

        private void OpenSteamCmdLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            var steamDirBox = ContentSteamUpdater.ISteamDirBox.Text;

            if (!string.IsNullOrEmpty(steamDirBox) && Directory.Exists(steamDirBox))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = steamDirBox, FileName = "explorer.exe" };
                    Process.Start(startInfo);
                }
                catch
                { MessageBox.Show($" Could not open {steamDirBox}"); }
            }
            else
            { MessageBox.Show($"{steamDirBox} Directory does not exist!"); }
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.Visibility = Visibility.Visible;
            IToolsDialog.IsOpen = true;
        }
        #endregion


        internal void NavigateToConsole()
        {
            navSteamUpdater.IsChecked = true;
            lastNavButton = navSteamUpdater;
            MainContent.Navigate(ContentSteamUpdater);
        }

        private bool CheckAdmin()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                if (principal.IsInRole(WindowsBuiltInRole.Administrator)) return true;

                MessageBox.Show("Application must be run as administrator",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to determine administrator status",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                throw;
            }
        }

        public void LoadServerProfiles()
        {
            if (Properties.Settings.Default.Servers == null) return;

            var currentProfiles = Properties.Settings.Default.Servers;
            Dispatcher?.Invoke(() => { IServerProfilesMenu.Items.Clear(); });

            ContentProfiles.Clear();

            foreach (var profile in currentProfiles.ServerProfiles)
            {
                ToggleButton newItem = new ToggleButton
                {
                    Name = profile.SafeName,
                    Content = profile.DisplayName,
                    Style = (Style)FindResource("MahApps.Styles.ToggleButton.WindowCommands"),
                    Padding = new Thickness(10, 2, 0, 2),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    //Foreground = new SolidColorBrush(Colors.White)
                };
                newItem.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
                Dispatcher?.Invoke(() => { IServerProfilesMenu.Items.Add(newItem); });

                newItem.Click += ToggleButton_Click;

                if (ContentProfiles.Any(tab => profile.SafeName == tab.Name)) 
                    continue;

                ContentProfiles.Add(new Views.ServerProfile(profile) { Name = profile.SafeName});
            }
        }

        public void DisplayMessage(string message)
        {
            IFlyoutMessage.Content = message;
            IFlyout.IsOpen = true;
        }

        // Opens Folder select dialog and returns selected path
        public string SelectFolder(string defaultFolder = "")
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select the folder",
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = false,
                InitialDirectory = defaultFolder,
                DefaultDirectory = defaultFolder,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            return dlg.ShowDialog() == CommonFileDialogResult.Ok
                ? dlg.FileName
                : null;
        }

        private void InstallSteam()
        {
            if (string.IsNullOrEmpty(ContentSteamUpdater.ISteamDirBox.Text))
            { DisplayMessage("Please make sure you have set a valid path for SteamCMD."); }
            else if (!File.Exists(Properties.Settings.Default.steamCMDPath + "\\steamcmd.exe"))
            {
                DisplayMessage("Steam CMD will now download and start the install process. If prompted please enter your Steam Guard " +
                                          "Code.\n\nYou will receive this by email from steam. When this is all complete type \'quit\' to finish.");
                ContentSteamUpdater.ISteamOutputBox.Document.Blocks.Clear();
                ContentSteamUpdater.ISteamOutputBox.AppendText("Installing SteamCMD");
                ContentSteamUpdater.ISteamOutputBox.AppendText("\nFile Downloading...");
                const string url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                string fileName = Properties.Settings.Default.steamCMDPath + "\\steamcmd.zip";

                if (!Directory.Exists(Properties.Settings.Default.steamCMDPath))
                    Directory.CreateDirectory(Properties.Settings.Default.steamCMDPath);

                using WebClient client = new WebClient();
                client.DownloadFileCompleted += SteamDownloadCompleted;
                client.DownloadFileAsync(new Uri(url), fileName);
            }
            else
            { DisplayMessage("SteamCMD already appears to be installed.\n\nPlease delete all files in the selected folder to reinstall."); }
        }
        private void SteamDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ContentSteamUpdater.ISteamOutputBox.AppendText(Environment.NewLine + "Download Finished");

            var steamPath = Properties.Settings.Default.steamCMDPath;
            string zip = steamPath + "\\steamcmd.zip";

            ContentSteamUpdater.ISteamOutputBox.AppendText("\nUnzipping...");
            ZipFile.ExtractToDirectory(zip, steamPath);
            ContentSteamUpdater.ISteamOutputBox.AppendText("\nInstalling...");
            _ = ContentSteamUpdater.RunSteamCommand(steamPath + "\\steamcmd.exe", "+login anonymous +quit", "install");

            File.Delete(zip);
        }

        internal string GetVersion()
        { return Functions.GetVersion(); }
    }
}
