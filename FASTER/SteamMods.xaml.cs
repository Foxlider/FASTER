using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for SteamMods.xaml
    /// </summary>
    public partial class SteamMods : UserControl
    {
        public SteamMods()
        {
            Initialized += SteamMods_Initialized;
            InitializeComponent();
            Loaded                             += SteamMods_Loaded;
            IModActionButtons.SelectionChanged += IActionButtons_SelectionChanged;
            IImportSteamModDialog.KeyUp += IImportSteamModDialog_KeyUp;
            
        }

        private void SteamMods_Loaded(object sender, RoutedEventArgs e)
        { UpdateModsView(); }


        private async void SteamMods_Initialized(object sender, EventArgs e)
        {
            if (Properties.Options.Default.steamMods?.SteamMods?.Count > 0 && Properties.Options.Default.checkForModUpdates)
            {
                IUpdateProgress.IsIndeterminate = true;
                IModView.IsEnabled = false;
                IProgressInfo.Visibility = Visibility.Visible;
                IProgressInfo.Content = "Checking for updates...";

                var tasks = new List<Task>
                {
                    Task.Run(SteamMod.UpdateInfoFromSteam)
                };

                await Task.WhenAll(tasks);

                IModView.IsEnabled = true;
                IProgressInfo.Visibility = Visibility.Collapsed;
                IUpdateProgress.IsIndeterminate = false;
                UpdateModsView();
            }
        }

        private void IActionButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ICheckForUpdates.IsSelected)
            { CheckForUpdates(); }
            if (IUpdateAll.IsSelected)
            { UpdateAllMods(); }
            if (IImportLauncherFile.IsSelected)
            { ImportLauncherFile(); }
            if (IAddSteamMod.IsSelected)
            { IImportSteamModDialog.IsOpen = true; }

            Thread thread = new Thread(() =>
            {
                Thread.Sleep(600);
                Dispatcher.Invoke(() =>
                {
                    ((ListBox)sender).SelectedItem = null;
                });
            });
            thread.Start();
        }


        private void IImportSteamModDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IImportSteamModDialog.IsOpen = false;
                IPrivateModCheck.IsChecked   = false;
                ISteamItemBox.Text           = "";
            }
        }


        private void IImportModButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor         = Cursors.Wait;
            IImportSteamModDialog.IsOpen = false;
            SteamMod.AddSteamMod(ISteamItemBox.Text);
            IPrivateModCheck.IsChecked = false;
            ISteamItemBox.Text         = "";
            Mouse.OverrideCursor       = Cursors.Arrow;
            UpdateModsView();
        }

        private async void ImportLauncherFile()
        {
            IUpdateProgress.IsIndeterminate = true;
            IModView.IsEnabled              = true;
            IProgressInfo.Visibility        = Visibility.Visible;
            IProgressInfo.Content           = "Importing Mods...";

            var tasks = new List<Task>
            {
                Task.Run(OpenLauncherFile)
            };

            await Task.WhenAll(tasks);

            IModView.IsEnabled = true;
            IProgressInfo.Visibility = Visibility.Collapsed;
            IUpdateProgress.IsIndeterminate = false;
            UpdateModsView();
        }

        private static void OpenLauncherFile()
        {
            string modsFile = Functions.SelectFile("Arma 3 Launcher File|*.html");

            if (modsFile != string.Empty)
            {
                StreamReader dataReader = new StreamReader(modsFile);

                do
                {
                    var modLine = dataReader.ReadLine();
                    if (modLine == null)
                        break;
                    if (modLine.Contains("data-type=\"Link\">"))
                    {
                        var link = modLine.Substring(modLine.IndexOf("http://steam", StringComparison.Ordinal));
                        link = Reverse(link);
                        link = link.Substring(link.IndexOf("epyt-atad", StringComparison.Ordinal) + 11);
                        link = Reverse(link);
                        SteamMod.AddSteamMod(link, true);
                    }
                }
                while (true);
                dataReader.Close();
            }
        }

        private static void UpdateAllMods()
        {
            if (MainWindow.Instance.ReadyToUpdate())
            {
                var modsToUpdate = new List<string>();

                foreach (var steamMod in Properties.Options.Default.steamMods.SteamMods)
                {
                    if (steamMod.SteamLastUpdated > steamMod.LocalLastUpdated)
                    {
                        modsToUpdate.Add(steamMod.WorkshopId.ToString());
                        UpdateMod(steamMod.WorkshopId, steamMod.Name, false);
                    }
                }

                if (modsToUpdate.Count > 0)
                {
                    string steamCommand = "+login " + MainWindow.Instance.ISteamUserBox.Text + " " + MainWindow.Instance.ISteamPassBox.Password;

                    foreach (var steamMod in modsToUpdate)
                    { steamCommand = steamCommand + " +workshop_download_item 107410 " + steamMod; }

                    string steamCmd = MainWindow.Instance.ISteamDirBox.Text + @"\steamcmd.exe";
                    steamCommand = steamCommand + " validate +quit";
                    MainWindow.Instance.RunSteamCommand(steamCmd, steamCommand, "addon", modsToUpdate);
                }
                else
                {
                    MainWindow.Instance.IMessageDialog.IsOpen   = true;
                    MainWindow.Instance.IMessageDialogText.Text = "No Mods to Update";
                }
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "Check all fields are correctly filled out on Steam Updater";
            }
        }

        private static void UpdateMod(int modId, string modName, bool singleMod = true)
        {
            if (MainWindow.Instance.ReadyToUpdate())
            {
                string modPath = Path.Combine(Properties.Options.Default.steamCMDPath, "steamapps", "workshop", "content", "107410", modId.ToString());

                if (!Directory.Exists(modPath))
                { Directory.CreateDirectory(modPath); }

                try
                {
                    Directory.CreateDirectory(modPath);

                    var linkPath = Path.Combine(Properties.Options.Default.serverPath, $"@{Functions.SafeName(modName)}");
                    var linkCommand = "/c mklink /D \"" + linkPath + "\" \"" + modPath + "\"";
                    
                    ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe")
                    {
                        WindowStyle     = ProcessWindowStyle.Hidden,
                        CreateNoWindow  = true,
                        UseShellExecute = false,
                        Arguments       = linkCommand
                    };
                    Process.Start(startInfo);
                }

                // Process.Start("cmd", linkCommand)

                catch (Exception ex)
                {
                    MessageBox.Show("An exception occurred: \n\n" + ex.Message);
                }

                if (singleMod)
                {
                    string steamCmd     = MainWindow.Instance.ISteamDirBox.Text                                                                                                              + @"\steamcmd.exe";
                    string steamCommand = "+login " + MainWindow.Instance.ISteamUserBox.Text + " " + MainWindow.Instance.ISteamPassBox.Password + " +workshop_download_item 107410 " + modId + " validate +quit";

                    List<string> modIDs = new List<string>()
                    {
                        modId.ToString()
                    };

                    MainWindow.Instance.RunSteamCommand(steamCmd, steamCommand, "addon", modIDs);
                }
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "Check all fields are correctly filled out on Steam Updater";
            }
        }

        private async void CheckForUpdates()
        {
            if (Properties.Options.Default.steamMods.SteamMods.Count > 0)
            {
                IUpdateProgress.IsIndeterminate = true;
                IModView.IsEnabled              = false;
                IProgressInfo.Visibility        = Visibility.Visible;
                IProgressInfo.Content           = "Checking for updates...";

                List<Task> tasks = new List<Task>
                {
                    Task.Run(SteamMod.UpdateInfoFromSteam)
                };

                await Task.WhenAll(tasks);

                IModView.IsEnabled              = true;
                IProgressInfo.Visibility        = Visibility.Collapsed;
                IUpdateProgress.IsIndeterminate = false;
                UpdateModsView();
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "No Mods To Check";
            }
        }

        private void UpdateModsView()
        {
            IModView.Items.Clear();

            if (Properties.Options.Default.steamMods != null)
            {
                foreach (var steamMod in Properties.Options.Default.steamMods.SteamMods)
                { IModView.Items.Add(steamMod); }
            }
        }

        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;

            if (Directory.Exists(Properties.Options.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + steamMod.WorkshopId))
            { Directory.Delete(Properties.Options.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + steamMod.WorkshopId, true); }

            if (Directory.Exists(Properties.Options.Default.serverPath + @"\@" + Functions.SafeName(steamMod.Name)))
            { Directory.Delete(Properties.Options.Default.serverPath + @"\@" + Functions.SafeName(steamMod.Name), true); }

            SteamMod.DeleteSteamMod(steamMod.WorkshopId);
            UpdateModsView();
        }

        private void UpdateMod(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;
            UpdateMod(steamMod.WorkshopId, steamMod.Name);
        }

        private void OpenModPage(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;
            Process.Start("https://steamcommunity.com/workshop/filedetails/?id=" + steamMod.WorkshopId);
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
