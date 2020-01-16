using AutoUpdaterDotNET;
using FASTER.Models;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.AppCenter.Analytics;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _instance;
        internal bool    InstallSteamCmd { get; set; } = false;
        private bool    _cancelled;
        private Process _oProcess = new Process();
        internal string Version;

        public MainWindow()
        {
            Properties.Options.Default.Reload();
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Properties.Options.Default.PropertyChanged += Default_PropertyChanged;
            
            IMessageDialogClose.Click += IMessageDialogClose_Click;
            ISteamUserBox.LostFocus += ISteamSettings_Changed;
            ISteamPassBox.LostFocus += ISteamSettings_Changed;
            IServerDirBox.LostFocus += ISteamSettings_Changed;
            ISteamDirBox.LostFocus += ISteamSettings_Changed;
            IServerBranch.LostFocus += ISteamSettings_Changed;
            IServerDirBox.TextChanged += IServerDirBox_TextChanged;
            IToolsDialog.MouseLeftButtonUp += IToolsDialog_MouseLeftButtonUp;
            ISteamUpdaterTabSelect.Selected += MenuItem_Selected;
            ISteamModsTabSelect.Selected += MenuItem_Selected;
            ISettingsTabSelect.Selected += MenuItem_Selected;
            IAboutTabSelect.Selected += MenuItem_Selected;
            ILocalModsTabSelect.Selected += MenuItem_Selected;
            IServerStatusTabSelect.Selected += MenuItem_Selected;
            IToolsDialog.KeyUp += IToolsDialog_KeyUp;
            IMessageDialog.KeyUp += IMessageDialog_KeyUp;
            ISteamGuardDialog.KeyUp += ISteamGuardDialog_KeyUp;
            INewServerProfileDialog.KeyUp += INewServerProfileDialog_KeyUp;
            MouseDown += IDialog_LostFocus;
            ISteamOutputBox.MouseLeftButtonDown  += IDialog_LostFocus;

            try
            {
                if (Properties.Options.Default.checkForAppUpdates)
                {
                    AutoUpdater.ReportErrors = true;
                    AutoUpdater.LetUserSelectRemindLater = false;
                    AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
                    AutoUpdater.RemindLaterAt = 1;
                    AutoUpdater.RunUpdateAsAdmin = true;
                    AutoUpdater.Start("https://raw.githubusercontent.com/Foxlider/Fox-s-Arma-Server-Tool-Extended-Rewrite/master/FASTER_Version.xml");
                }
            }
            catch
            {
                MessageBox.Show(
                    @"There is a problem reaching update server please check your internet connection and try again later.",
                    @"Update check failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Properties.Options.Default.Save();
        }

        /// <summary>
        ///     Gets the one and only instance.
        /// </summary>
        public static MainWindow Instance => _instance ??= new MainWindow();

        #region event handlers
        
        #region Custom Window Bar Click events
        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = true;
        }
        #endregion
        
        #region WindowEvents
        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            if (CheckAdmin())
            {
                Functions.CheckSettings();
                LoadServerProfiles();
                LoadSteamUpdaterSettings();
            }
            else
            { Close(); }
        }

        private void MetroWindow_Loaded(object sender, EventArgs e)
        {
            //FIX for issue #22 : not necessary
            LoadSteamUpdaterSettings();

            if (InstallSteamCmd)
            { InstallSteam(); }
        }

        private void MetroWindow_Closing(object sender, EventArgs e)
        {
            Properties.Options.Default.Save();
            Application.Current.Shutdown();
        }
        #endregion

        // Opens folder select dialog when clicking certain buttons
        private void DirButton_Click(object sender, RoutedEventArgs e)
        {
            if (Equals(sender, ISteamDirButton))
            {
                string path = SelectFolder(Properties.Options.Default.steamCMDPath);
                if (path != null)
                {
                    ISteamDirBox.Text = path;
                    ISteamDirBox.Focus();
                }
            }
            else if (Equals(sender, IServerDirButton))
            {
                string path = SelectFolder(Properties.Options.Default.serverPath);
                if (path != null)
                {
                    IServerDirBox.Text = path;
                    IServerDirBox.Focus();
                }
            }
        }
        // Handles when a user presses the cancel button
        private void ISteamUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string steamCmd = ISteamDirBox.Text + @"\steamcmd.exe";
            string branch = "";
            switch (IServerBranch.Text)
            {
                case "Stable":
                    branch = "233780"; //Arma3 Server main branch
                    break;
                case "Contact": //Arma 3 server Contact DLC
                    branch = "233780 -beta contact";
                    break;
                case "Creator DLC": //Arma 3 server Creator DLC
                    branch = "233780 -beta creatordlc";
                    break;
                case "LegacyPorts": //Arma 3 server Legacy Ports branch for linux
                    branch = "233780 -beta legacyPorts Arma3LegacyPorts";
                    break;
                case "Developpment": //Arma 3 Developpment branch, only for developpment clients
                    branch = "107410 -beta development";
                    break;
                default:
                    Console.WriteLine("Nothing to see here");
                    break;
            }

            var steamCommand = "+login " + ISteamUserBox.Text + " " + ISteamPassBox.Password + " +force_install_dir \"" + IServerDirBox.Text + "\" +app_update " + branch + " validate +quit";
            
            _ = RunSteamCommand(steamCmd, steamCommand, "server");
        }

        private void ISteamCancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _oProcess?.Kill();
                _cancelled = true;
            }
            catch (Exception ex)
            { MessageBox.Show($"CancelUpdateButton - An exception occurred:\n{ex.Message}", "Error"); }
        }

        private void NewServerProfileButton_Click(object sender, RoutedEventArgs e)
        {
            INewServerProfileDialog.IsOpen = true;
        }

        private void INewServerProfileDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                INewServerProfileDialog.IsOpen = false;
                MainGrid.Effect = null;
                INewProfileName.Text = string.Empty;
            }
        }

        private void IDialog_LostFocus(object sender, MouseButtonEventArgs e)
        {
            if (!ISteamGuardDialogContent.IsMouseOver && ISteamGuardDialog.IsOpen)
            {
                ISteamGuardDialog.IsOpen = false;
                MainGrid.Effect = null;
                ISteamGuardCode.Text     = string.Empty;
            }
            if (!IToolsDialogContent.IsMouseOver && IToolsDialog.IsOpen)
            {
                IToolsDialog.IsOpen = false;
                MainGrid.Effect = null;
            }
            if (!IMessageDialogContent.IsMouseOver && IMessageDialog.IsOpen)
            {
                IMessageDialog.IsOpen = false;
                MainGrid.Effect = null;
            }
            if (!INewServerProfileDialogContent.IsMouseOver && INewServerProfileDialog.IsOpen)
            {
                INewServerProfileDialog.IsOpen = false;
                MainGrid.Effect = null;
            }
        }

        private void ISteamGuardDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ISteamGuardDialog.IsOpen = false;
                MainGrid.Effect = null;
                ISteamGuardCode.Text     = string.Empty;
            }
        }

        private void IMessageDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IMessageDialog.IsOpen = false;
                MainGrid.Effect = null;
            }
        }

        private void IMessageDialogClose_Click(object sender, RoutedEventArgs e)
        {
            IMessageDialog.IsOpen = false;
        }

        private void IToolsDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IToolsDialog.IsOpen = false;
            }
        }

        private void ICreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("Creating new profile");
            INewProfileName.Text = INewProfileName.Text.Trim();
            if (string.IsNullOrEmpty(INewProfileName.Text))
            {
                INewServerProfileDialog.IsOpen = false;
                IMessageDialog.IsOpen = true;
                IMessageDialogText.Text = "Please use a suitable profile name.";
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var profileName = INewProfileName.Text;
                INewServerProfileDialog.IsOpen = false;
                ServerCollection.AddServerProfile(profileName, "_" + Functions.SafeName(profileName));
                INewProfileName.Text = string.Empty;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void MenuItemClone_Click(object sender, RoutedEventArgs e)
        {
            if (IServerProfilesMenu.SelectedIndex == -1) 
            { return; }
            var temp = Properties.Options.Default.Servers.ServerProfiles.FirstOrDefault(s => s.SafeName == ((ListBoxItem) IServerProfilesMenu.SelectedItem).Name);
            if (temp == null)
            {
                Instance.IMessageDialog.IsOpen   = true;
                Instance.IMessageDialogText.Text = "Could not find the selected profile.";
                return;
            }
            Models.ServerProfile serverProfile             =  temp.CloneObjectSerializable();
            serverProfile.DisplayName += " 2";
            serverProfile.SafeName    =  "_" + Functions.SafeName(serverProfile.DisplayName);
            ServerCollection.AddServerProfile(serverProfile);
        }
        
        //Handles when any menu item is selected
        private void MenuItem_Selected(object sender, RoutedEventArgs e)
        {
            var menus = new List<ListBox> { IMainMenuItems, IServerProfilesMenu, IOtherMenuItems };
            ListBoxItem lbItem = sender as ListBoxItem;
            foreach (var list in menus)
            {
                foreach (ListBoxItem item in list.Items)
                {
                    if (item.Name != lbItem?.Name)
                    { item.IsSelected = false; } 
                } 
            }

            foreach (TabItem item in IMainContent.Items)
            {
                if (item.Name == lbItem?.Name.Replace("Select", ""))
                { IMainContent.SelectedItem = item; } 
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
            MainGrid.Effect = null;
            if (!string.IsNullOrEmpty(IServerDirBox.Text) && Directory.Exists(IServerDirBox.Text))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = IServerDirBox.Text, FileName = "explorer.exe" };
                    Process.Start(startInfo);
                }
                catch
                { MessageBox.Show($" Could not open {IServerDirBox.Text}"); }
            }
            else
            { MessageBox.Show($"{IServerDirBox.Text} Directory does not exist!"); }
        }

        private void OpenSteamCmdLocation_Click(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            MainGrid.Effect = null;
            if (!string.IsNullOrEmpty(ISteamDirBox.Text) && Directory.Exists(ISteamDirBox.Text))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = ISteamDirBox.Text, FileName = "explorer.exe" };
                    Process.Start(startInfo);
                }
                catch
                { MessageBox.Show($" Could not open {ISteamDirBox.Text}"); }
            }
            else
            { MessageBox.Show($"{ISteamDirBox.Text} Directory does not exist!"); }
        }

        private void IToolsDialog_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            IToolsDialog.IsOpen = false;
            MainGrid.Effect = null;
        }

        private void IServerDirBox_TextChanged(object sender, RoutedEventArgs e)
        { Properties.Options.Default.serverPath = IServerDirBox.Text; }

        private void ISubmitCode_Click(object sender, RoutedEventArgs e)
        {
            var oStreamWriter = _oProcess.StandardInput;
            Dispatcher?.Invoke(() =>
            { oStreamWriter.Write(ISteamGuardCode.Text + "\n"); });
            ISteamGuardDialog.IsOpen = false;
            MainGrid.Effect = null;
        }

        private void ISteamSettings_Changed(object sender, RoutedEventArgs e)
        { UpdateSteamUpdaterSettings(); }
        #endregion

        //FIX for issue #22 not necessary
        private bool CheckAdmin()
        {
            try
            {
                using WindowsIdentity identity  = WindowsIdentity.GetCurrent();
                WindowsPrincipal      principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("Application must be run as administrator",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return false;
                }
                return true;
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
            if (Properties.Options.Default.Servers != null)
            {
                var currentProfiles = Properties.Options.Default.Servers;
                Dispatcher?.Invoke(() =>
                {
                    IServerProfilesMenu.Items.Clear();
                });

                for (int i = IMainContent.Items.Count - 4; i <= 0; i++)
                    IMainContent.Items.RemoveAt(i);

                foreach (var profile in currentProfiles.ServerProfiles)
                {
                    ListBoxItem newItem = new ListBoxItem
                    {
                        Name    = profile.SafeName,
                        Content = profile.DisplayName
                    };
                    Dispatcher?.Invoke(() =>
                    {
                        IServerProfilesMenu.Items.Add(newItem);
                    });

                    newItem.Selected += MenuItem_Selected;

                    var duplicate = false;

                    foreach (TabItem tab in IMainContent.Items)
                    {
                        if (profile.SafeName == tab.Name)
                            duplicate = true;
                    }

                    if (!duplicate)
                    {
                        var tabControls = new ServerProfile(profile);

                        TabItem newTab = new TabItem
                        {
                            Name    = profile.SafeName,
                            Content = tabControls,
                            Header  = profile.SafeName
                        };
                        Dispatcher?.Invoke(() =>
                        {
                            IMainContent.Items.Add(newTab);
                        });
                    }
                }
            }
        }

        public bool ReadyToUpdate()
        {
            return !string.IsNullOrEmpty(ISteamDirBox.Text) 
                && !string.IsNullOrEmpty(ISteamUserBox.Text) 
                && !string.IsNullOrEmpty(ISteamPassBox.Password) 
                && !string.IsNullOrEmpty(IServerDirBox.Text) 
                && File.Exists(Properties.Options.Default.steamCMDPath + "\\steamcmd.exe");
        }

        // Opens Folder select dialog and returns selected path
        public string SelectFolder(string default_folder = "")
        {
            var dlg = new CommonOpenFileDialog
            {
                Title                     = "Select the folder",
                IsFolderPicker            = true,
                AddToMostRecentlyUsedList = false,
                InitialDirectory          = default_folder,
                DefaultDirectory          = default_folder,
                AllowNonFileSystemItems   = false,
                EnsureFileExists          = true,
                EnsurePathExists          = true,
                EnsureReadOnly            = false,
                EnsureValidNames          = true,
                Multiselect               = false,
                ShowPlacesList            = true
            };

            return dlg.ShowDialog() == CommonFileDialogResult.Ok 
                ? dlg.FileName 
                : null;
        }

        private void UpdateSteamUpdaterSettings()
        {
            Properties.Options.Default.steamCMDPath  = ISteamDirBox.Text;
            Properties.Options.Default.steamUserName = ISteamUserBox.Text;
            Properties.Options.Default.steamPassword = Encryption.Instance.EncryptData(ISteamPassBox.Password);
            Properties.Options.Default.serverPath    = IServerDirBox.Text;
            Properties.Options.Default.serverBranch  = IServerBranch.Text;
        }

        private void LoadSteamUpdaterSettings()
        {
            ISteamDirBox.Text      = Properties.Options.Default.steamCMDPath;
            ISteamUserBox.Text     = Properties.Options.Default.steamUserName;
            ISteamPassBox.Password = Encryption.Instance.DecryptData(Properties.Options.Default.steamPassword);
            IServerDirBox.Text     = Properties.Options.Default.serverPath;
            IServerBranch.Text     = Properties.Options.Default.serverBranch;
        }

        private void InstallSteam()
        {
            if (string.IsNullOrEmpty(ISteamDirBox.Text))
            {
                Instance.IMessageDialog.IsOpen   = true;
                Instance.IMessageDialogText.Text = "Please make sure you have set a valid path for SteamCMD.";
            }
            else if (!File.Exists(Properties.Options.Default.steamCMDPath + "\\steamcmd.exe"))
            {
                IMessageDialog.IsOpen = true;
                IMessageDialogText.Text = "Steam CMD will now download and start the install process. If prompted please enter your Steam Guard " +
                                          "Code.\n\nYou will receive this by email from steam. When this is all complete type \'quit\' to finish.";
                ISteamOutputBox.Document.Blocks.Clear();
                ISteamOutputBox.AppendText("Installing SteamCMD");
                ISteamOutputBox.AppendText("\nFile Downloading...");
                const string url      = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                string       fileName = Properties.Options.Default.steamCMDPath + "\\steamcmd.zip";
                if (!Directory.Exists(Properties.Options.Default.steamCMDPath)) Directory.CreateDirectory(Properties.Options.Default.steamCMDPath);
                using WebClient client = new WebClient();
                client.DownloadFileCompleted += SteamDownloadCompleted;
                client.DownloadFileAsync(new Uri(url), fileName);
            }
            else
            {
                Instance.IMessageDialog.IsOpen = true;
                Instance.IMessageDialogText.Text = "SteamCMD already appears to be installed.\n\nPlease delete all files in the selected folder to reinstall.";
            }
        }

        private void SteamDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ISteamOutputBox.AppendText(Environment.NewLine + "Download Finished");

            var steamPath = Properties.Options.Default.steamCMDPath;
            string zip = steamPath + "\\steamcmd.zip";

            ISteamOutputBox.AppendText("\nUnzipping...");
            ZipFile.ExtractToDirectory(zip, steamPath);
            ISteamOutputBox.AppendText("\nInstalling...");
            _ = RunSteamCommand(steamPath + "\\steamcmd.exe", "+login anonymous +quit", "install");

            File.Delete(zip);
        }

        private static bool   _runLog;
        private static object _runLogLock = new object();
        private static int threadSlept;
        private void UpdateTextBox(string text)
        {
            if (_oProcess == null) return;

            Dispatcher?.Invoke(() =>
            {
                ISteamOutputBox.AppendText(text + "\n");
                ISteamOutputBox.ScrollToEnd();
            });

            if (text.StartsWith("Logging in user") && text.Contains("to Steam"))
            {
                _runLog = true;
                Thread t = new Thread(() =>
                {
                    threadSlept = 0;
                    bool _localRunThread;
                    do
                    {
                        Thread.Sleep(500);
                        threadSlept += 500;
                        lock (_runLogLock)
                        { _localRunThread = _runLog; }
                    }
                    while (_localRunThread && threadSlept < 10000);
                    if (_localRunThread)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            ISteamGuardDialog.IsOpen = true;
                        });
                    }
                });
                t.Start();
            }

            if (text.Contains("Logged in OK"))
            {
                lock (_runLogLock)
                { _runLog = false; }
            }

            if (text.StartsWith("Retrying..."))
            { threadSlept = 0; }

            if (text.EndsWith("..."))
            {
                Dispatcher?.Invoke(() =>
                {
                    ISteamOutputBox.AppendText(Environment.NewLine);
                });
            }

            if (text.Contains("Update state"))
            {
                int    counter  = text.IndexOf(":", StringComparison.Ordinal);
                string progress = text.Substring(counter + 2, 2);
                int    progressValue;
                if (progress.Contains(".")) { int.TryParse(progress.Substring(0, 1), out progressValue); }
                else { int.TryParse(progress,                                        out progressValue); }

                Dispatcher?.Invoke(() =>
                {
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value           = progressValue;
                });
            }

            if (text.Contains("Success"))
            {
                Dispatcher?.Invoke(() =>
                {
                    ISteamProgressBar.Value = 100;
                });
            }

            if (text.Contains("Timeout"))
            {
                Dispatcher?.Invoke(() =>
                {
                    Instance.IMessageDialog.IsOpen   = true;
                    Instance.IMessageDialogText.Text = "A Steam Download timed out. You may have to download again when task is complete.";
                });
            }
        }

        public async Task RunSteamCommand(string steamCmd, string steamCommand, string type, List<string> modIds = null)
        {
            if (ReadyToUpdate())
            {
                _oProcess = new Process();
                ISteamProgressBar.Value = 0;
                ISteamCancelButton.IsEnabled = true;
                ISteamUpdateButton.IsEnabled = false;
                IMainMenuItems.SelectedItem = null;

                var controls = FindVisualChildren<Control>(ISteamModsTab);
                var enumerable = controls.ToList();
                foreach (Control control in enumerable)
                { control.IsEnabled = false; }

                IMainContent.SelectedItem = ISteamUpdaterTab;
                var tasks = new List<Task>();

                ISteamProgressBar.IsIndeterminate = true;

                switch (type)
                {
                    case "addon":
                        ISteamOutputBox.Document.Blocks.Clear();
                        ISteamOutputBox.AppendText("Starting SteamCMD to update Addon" + Environment.NewLine + Environment.NewLine);
                        break;
                    case "server:":
                        ISteamOutputBox.Document.Blocks.Clear();
                        ISteamOutputBox.AppendText("Starting SteamCMD to update Server" + Environment.NewLine);
                        break;
                    case "install":
                        ISteamOutputBox.AppendText("Proceeding with install" + Environment.NewLine);
                        break;
                }

                tasks.Add(Task.Run(() =>
                {
                    _oProcess.StartInfo.FileName               = steamCmd;
                    _oProcess.StartInfo.Arguments              = steamCommand;
                    _oProcess.StartInfo.UseShellExecute        = false;
                    _oProcess.StartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
                    _oProcess.StartInfo.CreateNoWindow         = true;
                    _oProcess.StartInfo.RedirectStandardOutput = true;
                    _oProcess.StartInfo.RedirectStandardError  = true;
                    _oProcess.StartInfo.RedirectStandardInput  = true;
                    _oProcess.EnableRaisingEvents              = true;
                    
                    _oProcess.Start();

                    //NOTES
                    // SteamCMD's behaviour is quite odd. It seems like it does not keep on writing new lines but is almost constantly editing lines
                    // Editing lines does not trigger the OutputDataReceived event and nor does the Input header
                    // (When asking for user input, the programmer can add a header specifying what to enter and it is not picked by the event until the user entered its text)

                    ProcessOutputCharacters(_oProcess.StandardError);
                    ProcessOutputCharacters(_oProcess.StandardOutput);
                    
                    _oProcess.WaitForExit();
                }));

                await Task.WhenAll(tasks);

                if (_cancelled)
                {
                    _cancelled = false;
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value = 0;

                    ISteamOutputBox.Document.Blocks.Clear();
                    ISteamOutputBox.AppendText("Process Canceled");

                    _oProcess.Close();
                    _oProcess = null;
                    CheckModUpdatesComplete(modIds);
                }
                else
                {
                    ISteamOutputBox.AppendText("SteamCMD Exited" + Environment.NewLine);
                    ISteamOutputBox.ScrollToEnd();
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value = 100;
                    switch (type)
                    {
                        case "addon":
                            CheckModUpdatesComplete(modIds);
                            break;
                        case "server:":
                            Instance.IMessageDialog.IsOpen = true;
                            Instance.IMessageDialogText.Text = "Server Installed/ Updated.";
                            break;
                        case "install":
                            Instance.IMessageDialog.IsOpen = true;
                            Instance.IMessageDialogText.Text = "SteamCMD Installed.";
                            break;
                    }
                }
                ISteamCancelButton.IsEnabled = false;
                ISteamUpdateButton.IsEnabled = true;

                foreach (Control control in enumerable)
                { control.IsEnabled = true; }

            }
            else
            {
                IMessageDialog.IsOpen = true;
                IMessageDialogText.Text = "Please check that SteamCMD is installed and that all fields are correct: \n\n\n"
                                        + "   -  Steam Dir\n\n"
                                        + "   -  User Name & Pass\n\n"
                                        + "   -  Server Dir";
            }
        }

        private void ProcessOutputCharacters(StreamReader output)
        {
            while (!output.EndOfStream)
            {
                string line = output.ReadLine();
                if (line != null && !line.Contains("\\src\\common\\contentmanifest.cpp (650) : Assertion Failed: !m_bIsFinalized*"))
                { UpdateTextBox(line); }
            }
        }
        
        private static void CheckModUpdatesComplete(IReadOnlyCollection<string> modIds)
        {
            if (modIds != null)
            {
                foreach (var modID in modIds)
                {
                    var modToUpdate = Properties.Options.Default.steamMods.SteamMods.Find(m => m.WorkshopId.ToString() == modID);
                    var steamCmdOutputText = Functions.StringFromRichTextBox(Instance.ISteamOutputBox);

                    if (steamCmdOutputText.Contains("ERROR! Timeout downloading"))
                    { modToUpdate.Status = "Download Not Complete"; }
                    else
                    {
                        string modTempPath = Properties.Options.Default.steamCMDPath + @"\steamapps\workshop\downloads\107410\" + modID;
                        string modPath = Properties.Options.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + modID;

                        if (Directory.Exists(modTempPath))
                            modToUpdate.Status = "Download Not Complete";
                        else if (Directory.GetFiles(modPath).Length != 0)
                        {
                            modToUpdate.Status = "Up to Date";
                            var nx = new DateTime(1970, 1, 1);
                            var ts = DateTime.UtcNow - nx;

                            modToUpdate.LocalLastUpdated = (int)ts.TotalSeconds;
                        }
                    }
                }
                Properties.Options.Default.Save();
            }
        }


        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T dependencyObject)
                    {
                        yield return dependencyObject;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
