using FASTER.Models;

using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for SteamMods.xaml
    /// </summary>
    public partial class SteamMods : UserControl
    {
        bool firstLoad = true;
        public SteamMods()
        {
            InitializeComponent();
        }

        public MainWindow MetroWindow
        { get { return (MainWindow)Window.GetWindow(this); } }

        private void SteamMods_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.steamMods?.SteamMods?.Count > 0 && Properties.Settings.Default.checkForModUpdates && firstLoad)
            {
                UpdateModsView();
                firstLoad = false;
                IUpdateProgress.IsIndeterminate = true;
                IProgressInfo.Visibility        = Visibility.Visible;
                IProgressInfo.Content           = "Checking for updates...";

                Thread thread = new Thread(() =>
                {
                    SteamMod.UpdateInfoFromSteam();
                    try
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            IModView.IsEnabled              = true;
                            IProgressInfo.Visibility        = Visibility.Collapsed;
                            IUpdateProgress.IsIndeterminate = false;
                        });
                        UpdateModsView();
                    }
                    catch
                    { /* FASTER CLOSED ALREADY, DON'T CATCH THIS */ }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
            { UpdateModsView(); }
        }

        private void SteamMods_Initialized(object sender, EventArgs e)
        {
            if (!(Properties.Settings.Default.steamMods?.SteamMods?.Count > 0) || !Properties.Settings.Default.checkForModUpdates) return;

            IUpdateProgress.IsIndeterminate = true;
            IModView.IsEnabled              = false;
            IProgressInfo.Visibility        = Visibility.Visible;
        }

        private void ICheckForUpdates_Click(object sender, RoutedEventArgs e)
        { CheckForUpdates(); }

        private void IMessageButton_Click(object sender, RoutedEventArgs e)
        { IMessageDialog.IsOpen = false; }

        private async void IUpdateAll_Click(object sender, RoutedEventArgs e)
        { await UpdateAllModsAsync(); }


        private void IImportLauncherFile_Click(object sender, RoutedEventArgs e)
        { ImportLauncherFile(); }

        private void IAddSteamMod_Click(object sender, RoutedEventArgs e)
        { IImportSteamModDialog.IsOpen = true; }
        
        private void IImportSteamModDialog_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IImportSteamModDialog.IsMouseOver) return;

            IImportSteamModDialog.IsOpen = false;
            IPrivateModCheck.IsChecked   = false;
            ISteamItemBox.Text           = "";
        }   

        private void IImportSteamModDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            IImportSteamModDialog.IsOpen = false;
            IPrivateModCheck.IsChecked   = false;
            ISteamItemBox.Text           = "";
        }

        private async void IScanSteam_Click(object sender, RoutedEventArgs e)
        {
            IScanSteam.IsEnabled = false;
            await Task.Run(ImportModsFromSteam);
            IScanSteam.IsEnabled = true;
        }

        private void ImportModsFromSteam()
        {
            if (!Directory.Exists(Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410")))
            {
                Dispatcher?.Invoke(() =>
                {
                    IMessageText.Text     = "Could not find Arma 3 workshop folder";
                    IMessageDialog.IsOpen = true;
                });
                return;
            }
            var            steamMods   = Directory.GetDirectories(Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410"));
            List<SteamMod> currentMods = new List<SteamMod>();

            if (Properties.Settings.Default.steamMods != null)
            {
                Properties.Settings.Default.Reload();
                currentMods = Properties.Settings.Default.steamMods?.SteamMods;
            }

            foreach (var steamMod in steamMods)
            {
                try
                {
                    var sModId = steamMod.Replace(Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410"), "").Replace("\\", "");
                    var modId = int.Parse(sModId);
                    var info  = SteamMod.GetModInfo(modId);
                    var temp  = currentMods?.FirstOrDefault(m => m.WorkshopId == modId);

                    if (info == null || temp != null) continue;

                    var modName         = info.Item1;
                    var steamUpdateTime = info.Item3;
                    var author          = info.Item2;

                    currentMods?.Add(new SteamMod(modId, modName, author, steamUpdateTime));

                    var modCollection = new SteamModCollection { CollectionName = "Steam", SteamMods = currentMods };

                    Properties.Settings.Default.steamMods = modCollection;
                    Properties.Settings.Default.Save();
                }
                catch (Exception e)
                { Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } }); }
            }
            UpdateModsView();
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
            { Task.Run(OpenLauncherFile) };

            await Task.WhenAll(tasks);

            IModView.IsEnabled = true;
            IProgressInfo.Visibility = Visibility.Collapsed;
            IUpdateProgress.IsIndeterminate = false;
            UpdateModsView();
        }

        private void OpenLauncherFile()
        {
            string modsFile = Functions.SelectFile("Arma 3 Launcher File|*.html");

            if (string.IsNullOrEmpty(modsFile)) return;
            using StreamReader dataReader = new StreamReader(modsFile);
            do
            {
                try
                { ModLineHandler(dataReader); }
                catch (Exception e)
                {
                    using EventLog eventLog = new EventLog("Application")
                        { Source = "FASTER" };
                    eventLog.WriteEntry($"Error occured while importing mod file : [{e.GetType()}] {e.Message}", EventLogEntryType.Warning);
                }
            }
            while (!dataReader.EndOfStream);
        }

        private void ModLineHandler(StreamReader dataReader)
        {
            var modLine = dataReader.ReadLine();
            if (string.IsNullOrEmpty(modLine) || !modLine.Contains("data-type=\"Link\">http://steam") || !modLine.Contains("/?id=")) return;

            var link = modLine.Substring(modLine.IndexOf("http://steam", StringComparison.Ordinal));
            link = Reverse(link);
            link = link.Substring(link.IndexOf("epyt-atad", StringComparison.Ordinal) + 11);
            link = Reverse(link);
            try 
            { SteamMod.AddSteamMod(link, true); }
            catch
            {
                using EventLog eventLog = new EventLog("Application") { Source = "FASTER" };
                eventLog.WriteEntry($"Could not import mod {link}", EventLogEntryType.Error);
                Dispatcher?.Invoke(() =>
                {
                    IMessageText.Text     = $"Could not import mod {link}";
                    IMessageDialog.IsOpen = true;
                });
            }
        }

        private async Task UpdateAllModsAsync()
        {
            if (MetroWindow.ContentSteamUpdater.ReadyToUpdate())
            {
                var modsToUpdate = new List<string>();

                foreach (var steamMod in Properties.Settings.Default.steamMods.SteamMods.Where(steamMod => steamMod.SteamLastUpdated > steamMod.LocalLastUpdated))
                {
                    modsToUpdate.Add(steamMod.WorkshopId.ToString());
                    await UpdateMod(steamMod.WorkshopId, steamMod.Name, false);
                }

                if (modsToUpdate.Count > 0)
                {
                    string steamCommand = "+login " + MetroWindow.ContentSteamUpdater.ISteamUserBox.Text + " " + Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword);

                    steamCommand = modsToUpdate.Aggregate(steamCommand, (current, steamMod) => $"{current} +workshop_download_item 107410 {steamMod}");

                    string steamCmd = MetroWindow.ContentSteamUpdater.ISteamDirBox.Text + @"\steamcmd.exe";
                    steamCommand += " validate +quit";
                    _ = MetroWindow.ContentSteamUpdater.RunSteamCommand(steamCmd, steamCommand, "addon", modsToUpdate);
                }
                else
                { MetroWindow.DisplayMessage("No Mods to Update"); }
            }
            else
            { MetroWindow.DisplayMessage("Check all fields are correctly filled out on Steam Updater"); }
        }
        private async void UpdateMod(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;
            await UpdateMod(steamMod.WorkshopId, steamMod.Name);
        }

        private async Task UpdateMod(int modId, string modName, bool singleMod = true)
        {
            if (MetroWindow.ContentSteamUpdater.ReadyToUpdate())
            {
                string modPath = Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410", modId.ToString());

                if (!Directory.Exists(modPath))
                { Directory.CreateDirectory(modPath); }

                try
                {
                    Directory.CreateDirectory(modPath);
                    var linkPath = Path.Combine(Properties.Settings.Default.serverPath, $"@{Functions.SafeName(modName)}");
                    var linkCommand = "/c mklink /D \"" + linkPath + "\" \"" + modPath + "\"";
                    
                    ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe")
                    {
                        WindowStyle     = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                        CreateNoWindow  = true,
                        UseShellExecute = false,
                        Arguments       = linkCommand
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                { MessageBox.Show("An exception occurred: \n\n" + ex.Message); }

                if (!singleMod) return;

                string steamCmd     = MetroWindow.ContentSteamUpdater.ISteamDirBox.Text                                                                                                              + @"\steamcmd.exe";
                string steamCommand = "+login " + MetroWindow.ContentSteamUpdater.ISteamUserBox.Text + " " + Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword) + " +workshop_download_item 107410 " + modId + " validate +quit";

                List<string> modIDs = new List<string>
                { modId.ToString() };

                MetroWindow.NavigateToConsole();
                try
                { await MetroWindow.ContentSteamUpdater.RunSteamCommand(steamCmd, steamCommand, "addon", modIDs); }
                catch (Exception e)
                { MetroWindow.DisplayMessage($"An error occured : {e.Message}"); }
            }
            else
            { MetroWindow.DisplayMessage("Check all fields are correctly filled out on Steam Updater"); }
        }

        private async void CheckForUpdates()
        {
            if (Properties.Settings.Default.steamMods.SteamMods.Count > 0)
            {
                IUpdateProgress.IsIndeterminate = true;
                IModView.IsEnabled              = false;
                IProgressInfo.Visibility        = Visibility.Visible;
                IProgressInfo.Content           = "Checking for updates...";

                List<Task> tasks = new List<Task>
                { Task.Run(SteamMod.UpdateInfoFromSteam) };

                await Task.WhenAll(tasks);

                IModView.IsEnabled              = true;
                IProgressInfo.Visibility        = Visibility.Collapsed;
                IUpdateProgress.IsIndeterminate = false;
                UpdateModsView();
            }
            else
            { MetroWindow.DisplayMessage("No Mods To Check"); }
        }

        private void UpdateModsView()
        {
            Dispatcher?.Invoke(() => IModView.Items.Clear());

            if (Properties.Settings.Default.steamMods == null) return;

            foreach (var steamMod in Properties.Settings.Default.steamMods.SteamMods)
            { Dispatcher?.Invoke(() => IModView.Items.Add(steamMod)); }
        }

        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;

            if (Directory.Exists(Properties.Settings.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + steamMod.WorkshopId))
            { 
                try
                { Directory.Delete(Properties.Settings.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + steamMod.WorkshopId, true); }
                catch
                {
                    MainWindow.Instance.IFlyoutMessage.Content = $"Could not delete mod \"{steamMod.Name}\"";
                    MainWindow.Instance.IFlyout.IsOpen         = true;
                }
            }

            if (Directory.Exists(Properties.Settings.Default.serverPath + @"\@" + Functions.SafeName(steamMod.Name)))
            {
                try 
                { Directory.Delete(Properties.Settings.Default.serverPath + @"\@" + Functions.SafeName(steamMod.Name), true); }
                catch
                {
                    MainWindow.Instance.IFlyoutMessage.Content = $"Could not delete mod \"{steamMod.Name}\"";
                    MainWindow.Instance.IFlyout.IsOpen = true;
                }
            }

            SteamMod.DeleteSteamMod(steamMod.WorkshopId);
            UpdateModsView();
        }

        private void OpenModPage(object sender, RoutedEventArgs e)
        {
            var steamMod = (SteamMod)((Button)e.Source).DataContext;
            var url = "https://steamcommunity.com/workshop/filedetails/?id=" + steamMod.WorkshopId;
            try 
            { Process.Start(url); }
            catch
            {
                try
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                catch
                {  MessageBox.Show($"Could not open \"{url}\""); }
            }
        }

        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
