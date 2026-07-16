using FASTER.Models;
using FASTER.ViewModel;
using FASTER.Views;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal bool ConvertMods { get; set; }
        internal string Version;
        internal bool NavEnabled = true;

        private ToggleButton lastNavButton;

        #region INSTANCES
        private static MainWindow _instance;
        public static MainWindow Instance => _instance ??= new MainWindow();

        private SteamUpdaterViewModel _steamUpdaterVM;
        Updater                       _steamUpdater;
        public Updater ContentSteamUpdater
        {
            get => _steamUpdater ??= new Updater();
            set => _steamUpdater = value;
        }
        public SteamUpdaterViewModel SteamUpdaterViewModel
        {
            get => _steamUpdaterVM ??= SteamUpdaterViewModel.Instance;
            set => _steamUpdaterVM = value;
        }


        private Mods  _mods;
        public Mods ContentSteamMods
        {
            get => _mods ??= new Mods();
            set => _mods = value;
        }

        ModsViewModel _modsVM;
        public ModsViewModel ModsViewModel
        {
            get => _modsVM ??= new ModsViewModel();
            set => _modsVM = value;
        }

        Deployment _deploy;
        public Deployment ContentDeploy
        {
            get => _deploy ??= new Deployment();
            set => _deploy = value;
        }

        DeploymentViewModel _deployVM;
        public DeploymentViewModel DeployViewModel
        {
            get => _deployVM ??= new DeploymentViewModel();
            set => _deployVM = value;
        }

        ServerStatus _serverStatus;
        public ServerStatus ContentServerStatus
        {
            get => _serverStatus ??= new ServerStatus();
            set => _serverStatus = value;
        }
        
        Settings _settings;
        public Settings ContentSettings
        {
            get => _settings ??= new Settings(this);
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
            get => _profile ??= new Profile();
            set => _profile = value;
        }

        private List<ProfileViewModel> _profileViews;

        internal List<ProfileViewModel> ContentProfileViews
        {
            get => _profileViews ??= new List<ProfileViewModel>();
            set => _profileViews = value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //Set font preferences
            FontFamily = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == Properties.Settings.Default.font);

            _instance = this;
            Version = GetVersion();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NavigateToConsole();
        }

        public static bool HasLoaded()
        { return _instance != null; }

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

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;

            if (!Directory.Exists(settings.modStagingDirectory))
                Directory.CreateDirectory(settings.modStagingDirectory);
            
            if (ConvertMods)
                await ModConversion();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            SteamUpdaterViewModel.Instance.SteamClient?.Shutdown();
            SteamUpdaterViewModel.Instance.SteamClient?.Dispose();
            Application.Current.Shutdown();
        }

        private static IEnumerable<ToggleButton> GetProfileToggleButtons(System.Windows.Controls.ListBox menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is System.Windows.Controls.DockPanel dp)
                {
                    var tb = dp.Children.OfType<ToggleButton>().FirstOrDefault();
                    if (tb != null) yield return tb;
                }
                else if (item is ToggleButton t)
                {
                    yield return t;
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ToggleButton>();
            list.AddRange(IMainMenuItems.Items.Cast<ToggleButton>().Where(i => i.IsChecked == true));
            list.AddRange(GetProfileToggleButtons(IServerProfilesMenu).Where(i => i.IsChecked == true));
            list.AddRange(IOtherMenuItems.Items.Cast<ToggleButton>().Where(i => i.IsChecked == true));

            if (sender is not ToggleButton nav || !NavEnabled) return;

            //Don't navigate if same menu is clicked
            if (nav == lastNavButton) return;



            //Clear selected Buttons
            IServerProfilesMenu.SelectedItem = null;
            foreach (var item in list.Where(item => item.Name != nav.Name))
            { item.IsChecked = false; }

            nav.IsChecked = true;
            lastNavButton = nav;

            //Saving just in case
            Properties.Settings.Default.Save();

            //Get loading screen
            switch (nav.Name)
            {
                case "navSteamUpdater":
                    ContentSteamUpdater.DataContext = SteamUpdaterViewModel;
                    MainContent.Content  = ContentSteamUpdater;

                    break;
                case "navMods":
                    ContentSteamMods.DataContext = ModsViewModel;
                    MainContent.Content = ContentSteamMods;
                    break;
                case "navDeploy":
                    ContentDeploy.DataContext = DeployViewModel;
                    MainContent.Content = ContentDeploy;
                    break;
                case "navServerStatus":
                    MainContent.Content = ContentServerStatus;
                    break;
                case "navSettings":
                    MainContent.Content = ContentSettings;
                    break;
                case "navAbout":
                    MainContent.Content = ContentAbout;
                    break;
                default:
                    if (GetProfileToggleButtons(IServerProfilesMenu).FirstOrDefault(p => p.Name == nav.Name) != null)
                    {
                        var profile = new Profile();
                        MainContent.Content = profile;

                        ContentProfile.DataContext = ContentProfileViews.First(p => p.Profile.Id == nav.Name);
                        ContentProfile.Refresh();
                        MainContent.Content = ContentProfile;
                    }
                    break;
            }
        }

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
                ServerProfileCollection.AddServerProfile(profileName);
                INewProfileName.Text = string.Empty;
            }
        }

        private ToggleButton GetSelectedProfileToggleButton()
        {
            var selected = IServerProfilesMenu.SelectedItem;
            if (selected is System.Windows.Controls.DockPanel dp)
                return dp.Children.OfType<ToggleButton>().FirstOrDefault();
            return selected as ToggleButton;
        }

        private void MenuItemClone_Click(object sender, RoutedEventArgs e)
        {
            if (IServerProfilesMenu.SelectedIndex == -1)
            { return; }

            try
            {
                var selectedBtn = GetSelectedProfileToggleButton();
                var temp = Properties.Settings.Default.Profiles.FirstOrDefault(s =>
                    s.Id == selectedBtn?.Name);
                if (temp == null)
                {
                    DisplayMessage("Could not find the selected profile.");
                    return;
                }

                ServerProfile serverProfile = temp.Clone();
                ServerProfileCollection.AddServerProfile(serverProfile);
            }
            catch (Exception err)
            {
                DisplayMessage("An error occured while cloning your profile");
                Crashes.TrackError(err, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } });
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IServerProfilesMenu.SelectedIndex == -1)
            { return; }

            try
            {
                var selectedBtn = GetSelectedProfileToggleButton();
                var temp = Properties.Settings.Default.Profiles.FirstOrDefault(s =>
                    s.Id == selectedBtn?.Name);
                if (temp == null)
                {
                    DisplayMessage("Could not find the selected profile.");
                    return;
                }

                ContentProfileViews.FirstOrDefault(p => p.Profile.Id == temp.Id)?.DeleteProfile();
            }
            catch (Exception err)
            {
                DisplayMessage("An error occured while cloning your profile");
                Crashes.TrackError(err, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } });
            }

        }


        private void OpenModStagingLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            var serverDirBox = SteamUpdaterViewModel.Parameters.ModStagingDirectory;

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

        private void OpenArmaServerLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            var serverDirBox = SteamUpdaterViewModel.Parameters.InstallDirectory;

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

        private void OpenAppDataLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            var appdataDirectory= Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

            if (!string.IsNullOrEmpty(appdataDirectory) && Directory.Exists(appdataDirectory))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = appdataDirectory, FileName = "explorer.exe" };
                    Process.Start(startInfo);
                }
                catch
                { MessageBox.Show($" Could not open {appdataDirectory}"); }
            }
            else
            { MessageBox.Show($"{appdataDirectory} Directory does not exist!"); }
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
            MainContent.Content = ContentSteamUpdater;
        }

        //Checks if FASTER is running as Admin
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
            if (Properties.Settings.Default.Profiles == null)
            {
                Properties.Settings.Default.Profiles = new ServerProfileCollection();
                Properties.Settings.Default.Save();
            }
            var currentProfilesNew = Properties.Settings.Default.Profiles;

            Dispatcher?.Invoke(() => { IServerProfilesMenu.Items.Clear(); });

            ContentProfileViews.Clear();

            foreach (var profile in currentProfilesNew)
            {
                ToggleButton newItem = new ToggleButton
                {
                    Name = profile.Id,
                    Content = profile.Name,
                    Style = (Style)FindResource("MahApps.Styles.ToggleButton.WindowCommands"),
                    Padding = new Thickness(10, 2, 0, 2),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                };
                newItem.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);

                var profileId = profile.Id;

                var btnUp = new System.Windows.Controls.Button
                {
                    Content = "▲",
                    FontSize = 10,
                    Width = 18,
                    Height = 18,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 1, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)FindResource("MahApps.Styles.Button.MetroSquare"),
                    BorderThickness = new Thickness(0),
                    ToolTip = "Move Up",
                };
                btnUp.Click += (s, e) => { e.Handled = true; MoveProfileUp(profileId); };

                var btnDown = new System.Windows.Controls.Button
                {
                    Content = "▼",
                    FontSize = 10,
                    Width = 18,
                    Height = 18,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)FindResource("MahApps.Styles.Button.MetroSquare"),
                    BorderThickness = new Thickness(0),
                    ToolTip = "Move Down",
                };
                btnDown.Click += (s, e) => { e.Handled = true; MoveProfileDown(profileId); };

                var rowPanel = new System.Windows.Controls.DockPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    LastChildFill = true,
                };
                System.Windows.Controls.DockPanel.SetDock(btnUp,   System.Windows.Controls.Dock.Right);
                System.Windows.Controls.DockPanel.SetDock(btnDown, System.Windows.Controls.Dock.Right);
                rowPanel.Children.Add(btnDown);
                rowPanel.Children.Add(btnUp);
                rowPanel.Children.Add(newItem);

                Dispatcher?.Invoke(() => { IServerProfilesMenu.Items.Add(rowPanel); });

                newItem.Click += ToggleButton_Click;

                if (ContentProfileViews.Any(tab => profile.Id == tab.Profile.Id))
                    continue;

                var p = new ProfileViewModel(profile);
                ContentProfileViews.Add(p);
            }
        }

        private void MoveProfileUp(string profileId)
        {
            var profiles = Properties.Settings.Default.Profiles;
            int idx = profiles.FindIndex(p => p.Id == profileId);
            if (idx <= 0) return;
            var item = profiles[idx];
            profiles.RemoveAt(idx);
            profiles.Insert(idx - 1, item);
            Properties.Settings.Default.Profiles = profiles;
            Properties.Settings.Default.Save();
            LoadServerProfiles();
        }

        private void MoveProfileDown(string profileId)
        {
            var profiles = Properties.Settings.Default.Profiles;
            int idx = profiles.FindIndex(p => p.Id == profileId);
            if (idx < 0 || idx >= profiles.Count - 1) return;
            var item = profiles[idx];
            profiles.RemoveAt(idx);
            profiles.Insert(idx + 1, item);
            Properties.Settings.Default.Profiles = profiles;
            Properties.Settings.Default.Save();
            LoadServerProfiles();
        }

        private async Task ModConversion()
        {
            var properties    = Properties.Settings.Default;
            var modStagingDir = properties.modStagingDirectory;

            var controller = await this.ShowProgressAsync("Please wait...", "Checking Drive Space...");
            controller.Maximum = properties.steamMods.SteamMods.Count;
            var progress = 0;

            long fullzize = 0;
            foreach (var mod in properties.steamMods.SteamMods.Select(m => Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410", m.WorkshopId.ToString())).Concat(properties.localMods.Select(m => m.Path)))
            {
                if(!Directory.Exists(mod))
                    continue;

                string[] a = Directory.GetFiles(mod, "*.*", SearchOption.AllDirectories);
                fullzize += a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();

                controller.SetMessage($"Checking Drive Space... {Functions.ParseFileSize(fullzize)}");
            }

            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == Path.GetPathRoot(modStagingDir));

            if (drive.AvailableFreeSpace < fullzize)
            {
                properties.armaMods = null;
                properties.firstRun = true;
                properties.Save();

                var closing = 10000;

                while (closing > 0)
                {
                    controller.SetMessage($"Not enough free space on your drive for your mods. ({Functions.ParseFileSize(drive.AvailableFreeSpace)} / {Functions.ParseFileSize(fullzize)} )\nClear some space and retry.\n\nFASTER will close in {closing/1000} seconds.");
                    await Task.Delay(1000);
                    closing -= 1000;
                }

                await controller.CloseAsync();
                Instance.OnClosing(new CancelEventArgs(true));
                return;
            }
                

            foreach (var steamMod in properties.steamMods.SteamMods)
            {
                var newPath = Path.Combine(modStagingDir,                            steamMod.WorkshopId.ToString());
                var oldPath = Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410", steamMod.WorkshopId.ToString());
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);

                await MoveMod(oldPath, newPath);

                var newMod = new ArmaMod
                {
                    WorkshopId       = steamMod.WorkshopId,
                    Name             = steamMod.Name,
                    Path             = newPath,
                    Author           = steamMod.Author,
                    IsLocal          = false,
                    LocalLastUpdated = ulong.MinValue,
                    SteamLastUpdated = Convert.ToUInt64(steamMod.SteamLastUpdated),
                    Status           = ArmaModStatus.UpdateRequired
                };
                await Task.Run(() => properties.armaMods.AddSteamMod(newMod));
                progress += 1;
                controller.SetMessage($"Converting Steam Mods... {progress} / {controller.Maximum}");
                controller.SetProgress(progress);
            }

            properties.steamMods = new SteamModCollection();

            if (properties.localMods == null || properties.localMods.Count == 0)
            {
                await controller.CloseAsync();
                properties.Save();
                return;
            }

            var r = new Random();
            progress = 0;
            controller.Maximum = properties.localMods.Count;
            controller.SetMessage($"Converting Local Mods... {progress} / {controller.Maximum}");
            controller.SetProgress(progress);
            foreach (var localMod in properties.localMods)
            {
                var modID   = (uint) (uint.MaxValue - r.Next(ushort.MaxValue/2));
                var newPath = Path.Combine(modStagingDir, modID.ToString());
                var oldPath = localMod.Path;
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);

                await MoveMod(oldPath, newPath);
                
                var newMod = new ArmaMod
                {
                    WorkshopId       = modID,
                    Name             = localMod.Name,
                    Path             = newPath,
                    Author           = localMod.Author,
                    IsLocal          = true,
                    Status           = ArmaModStatus.Local
                };
                await Task.Run(() => properties.armaMods.AddSteamMod(newMod));
                progress += 1;
                controller.SetMessage($"Converting Local Mods... {progress} / {controller.Maximum}");
                controller.SetProgress(progress * 100.0 / controller.Maximum );
            }

            await controller.CloseAsync();
            properties.Save();
        }

        private static async Task MoveMod(string oldPath, string newPath)
        {
            if(Directory.Exists(oldPath))
            {
                foreach (var file in Directory.EnumerateFiles(oldPath, "*", SearchOption.AllDirectories))
                {
                    var newFile = file.Replace(oldPath, newPath);
                    if(!Directory.Exists(Path.GetDirectoryName(newFile))) Directory.CreateDirectory(Path.GetDirectoryName(newFile));

                    await CopyFileAsync(file, newFile);
                }
            }
        }

        private static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            await using var sourceStream      = new FileStream(sourceFile,      FileMode.Open,   FileAccess.Read,  FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await sourceStream.CopyToAsync(destinationStream);
            destinationStream.Close();
            sourceStream.Close();
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
        
        internal string GetVersion()
        { return Functions.GetVersion(); }
    }
}
