using FASTER.Models;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.AppCenter.Analytics;
using System.Xml.Linq;

namespace FASTER.ViewModel
{
    internal class ProfileViewModel
    {
        public ProfileViewModel()
        { Profile = new ServerProfile("Server", false); }

        public ProfileViewModel(ServerProfile p)
        { Profile = p; }
        public ServerProfile Profile { get; set; }

        public ObservableCollection<string> VonCodecs           { get; } = new ObservableCollection<string>(ServerCfgArrays.VonCodecStrings);
        public ObservableCollection<string> FilePatching        { get; } = new ObservableCollection<string>(ServerCfgArrays.AllowFilePatchingStrings);
        public ObservableCollection<string> VerifySignatures    { get; } = new ObservableCollection<string>(ServerCfgArrays.VerifySignaturesStrings);
        public ObservableCollection<string> TimestampFormats    { get; } = new ObservableCollection<string>(ServerCfgArrays.TimeStampStrings);
        public ObservableCollection<string> EnabledStrings      { get; } = new ObservableCollection<string>(ProfileCfgArrays.EnabledStrings);
        public ObservableCollection<string> MissionDifficulties { get; } = new ObservableCollection<string> { "Recruit", "Regular", "Veteran", "Custom" };
        public ObservableCollection<string> PerfPresets         { get; } = new ObservableCollection<string>(BasicCfgArrays.PerfPresets);
        public ObservableCollection<double> TerrainGrids        { get; } = new ObservableCollection<double>(BasicCfgArrays.TerrainGrids);

        internal void DisplayMessage(string msg)
        {
            MainWindow.Instance.IFlyout.IsOpen         = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

        internal void OpenProfileLocation()
        {
            Analytics.TrackEvent("Profile - Clicked OpenProfile", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName}
            });

            string folderPath = Path.Combine(Profile.ArmaPath, "Servers", Profile.Id);
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = folderPath,
                    FileName  = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            else
            { DisplayMessage("Could not open profile location..."); }
        }

        internal void LaunchHCs()
        {
            if (!Profile.ServerCfg.HeadlessClientEnabled) return;
            if (!VerifyBeforeLaunch()) return;

            //Launching... 
            DisplayMessage($"Launching Headless Clients for {Profile.Name}...");
            string commandLine;
            for (int hc = 1; hc <= Profile.HeadlessNumber; hc++ )
            {
                commandLine = SetHCCommandLine(hc);
                #if DEBUG
                DisplayMessage($"{Profile.HeadlessNumber} Headless Clients launched !\n{commandLine}");
                #else
                ProcessStartInfo hcStartInfo = new ProcessStartInfo(Profile.Executable, commandLine);
                Process          hcProcess   = new Process { StartInfo = hcStartInfo };
                hcProcess.Start();
                #endif
            }
        }

        private string SetHCCommandLine(int hc)
        {
            string headlessMods = string.Join(";", Profile.ProfileMods.Where(m => m.HeadlessChecked).Select(m =>$"@{Functions.SafeName(m.Name)}"));
            List<string> arguments = new()
            {
                "-client",
                " -connect=127.0.0.1",
                $" -password={Profile.ServerCfg.Password}",
                $" \"-profiles={Path.Combine(Profile.ArmaPath, "Servers", $"{Profile.Id}_hc{hc}")}\"",
                " -nosound",
                $" -port={Profile.Port}",
                Profile.GetDlcAndPlayerMods(headlessMods),
                $"{(Profile.ServerCfg.MaxMemOverride ? $" -maxMem={Profile.ServerCfg.MaxMem}" : "")}",
                $"{(Profile.ServerCfg.CpuCountOverride ? $" -cpuCount={Profile.ServerCfg.CpuCount}" : "")}",
                $"{(Profile.EnableHyperThreading ? " -enableHT" : "")}",
                $"{(!string.IsNullOrWhiteSpace(Profile.ServerCfg.CommandLineParameters) ? $" {Profile.ServerCfg.CommandLineParameters}" : "")}"
            };

            string commandLine = string.Join("", arguments);

            try { Clipboard.SetText(commandLine); }
            catch (COMException e)
            {
                try
                {
                    Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } });
                    Clipboard.SetDataObject(commandLine);
                }
                catch (COMException ex) { Crashes.TrackError(ex, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } }); }
            }
            return commandLine;
        }

        internal void LaunchServer()
        {
            if (!VerifyBeforeLaunch()) return;

            //Launching... 
            DisplayMessage($"Launching Profile {Profile.Name}...");

            Analytics.TrackEvent("Profile - Clicked LaunchServer", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName}
            });

            Profile.RaisePropertyChanged("CommandLine");
            var commandLine = Profile.CommandLine;
            try { Clipboard.SetText(commandLine); }
            catch (COMException e)
            {
                try
                {
                    Crashes.TrackError(e, new Dictionary<string, string>
                                           {{ "Name", Properties.Settings.Default.steamUserName }});
                    Clipboard.SetDataObject(commandLine);
                }
                catch (COMException ex)
                {
                    Crashes.TrackError(ex, new Dictionary<string, string>
                                           {{ "Name", Properties.Settings.Default.steamUserName }});
                }
            }
            #if DEBUG
            DisplayMessage($"Launching Arma3Server with commandline : \n{commandLine}");
            #else
            DisplayMessage($"Profile {Profile.Name}'s server launched !\nCommand line copied to clipboard.");
            ProcessStartInfo sStartInfo = new ProcessStartInfo(Profile.Executable, commandLine);
            Process          sProcess   = new Process { StartInfo = sStartInfo };
            sProcess.Start();

            LaunchHCs();
            #endif
            
        }

        /// <summary>
        /// Check if the server can be launched
        /// </summary>
        /// <returns></returns>
        private bool VerifyBeforeLaunch()
        {
            if (!ProfileFilesExist(Profile.Id))
            {
                DisplayMessage("The profile does not exist in the game files.\nYou might need to save it first.");
                return false;
            }

            if (!(Profile.Executable != null && Profile.Executable.Contains("arma3server") && Profile.Executable.EndsWith(".exe")))
            {
                DisplayMessage("Please select a valid Arma 3 Sever Executable.");
                return false;
            }

            if (File.Exists(Profile.Executable)) return true;
            DisplayMessage("Arma 3 Server Executable does not exist. Please reselect correct file.");
            return false;
        }

        private bool ProfileFilesExist(string profile)
        {
            string path = Profile.ArmaPath;

            if (!Directory.Exists(Path.Combine(path, "Servers", profile)))
            { return false; }

            return File.Exists(Path.Combine(path, "Servers", profile, "server_config.cfg")) 
                && File.Exists(Path.Combine(path, "Servers", profile, "server_basic.cfg"));
        }

        internal void DeleteProfile()
        {
            if (Directory.Exists(Path.Combine(Profile.ArmaPath, "Servers", Profile.Id)))
            { Directory.Delete(Path.Combine(Profile.ArmaPath, "Servers", Profile.Id), true); }
            Properties.Settings.Default.Profiles.Remove(Profile);
            Properties.Settings.Default.Save();
            MainWindow.Instance.ContentProfileViews.Remove(MainWindow.Instance.ContentProfileViews.FirstOrDefault(p => p.Profile.Id == Profile.Id));
            var menuItem = MainWindow.Instance.IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == Profile.Id);
            if(menuItem != null)
                MainWindow.Instance.IServerProfilesMenu.Items.Remove(menuItem);
            
            MainWindow.Instance.NavigateToConsole();
        }

        internal void SaveProfile()
        {
            string config        = Path.Combine(Profile.ArmaPath, "Servers", Profile.Id, "server_config.cfg");
            string basic         = Path.Combine(Profile.ArmaPath, "Servers", Profile.Id, "server_basic.cfg");
            string serverProfile = Path.Combine(Profile.ArmaPath, "Servers", Profile.Id, "users", Profile.Id, $"{Profile.Id}.Arma3Profile");

            //Creating profile directory
            Directory.CreateDirectory(Path.Combine(Profile.ArmaPath, "Servers", Profile.Id, "users", Profile.Id));

            //Writing files
            try
            {
                File.WriteAllLines(config,        Profile.ServerCfg.ServerCfgContent.Replace("\r", "").Split('\n'));
                File.WriteAllLines(basic,         Profile.BasicCfg.BasicContent.Replace("\r", "").Split('\n'));
                File.WriteAllLines(serverProfile, Profile.ArmaProfile.ArmaProfileContent.Replace("\r", "").Split('\n'));
            }
            catch
            { DisplayMessage("Could not write the config files. Please ensure the server is not running and retry."); }

            var armaPath = Path.GetDirectoryName(Profile.Executable);
            var links = Directory.EnumerateDirectories(armaPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
            bool displayMissingModMessage = false;
            foreach (ProfileMod profileMod in Profile.ProfileMods.Where(m => m.ClientSideChecked || m.HeadlessChecked || m.ServerSideChecked))
            {
                if (!links.Any(l => l.Name == $"@{Functions.SafeName(profileMod.Name)}"))
                    displayMissingModMessage = true;
            }
            

            var index = Properties.Settings.Default.Profiles.FindIndex(p => p.Id == Profile.Id);  
            if (index != -1)
            { Properties.Settings.Default.Profiles[index] = Profile; }

            Properties.Settings.Default.Save();
            Profile.RaisePropertyChanged("CommandLine");
            DisplayMessage($"Saved Profile {Profile.Name}");

            if (displayMissingModMessage)
                DisplayMessage("Some mods were not found in the Arma directory.\nMake sure you have deployed the correct mods");

        }

        public ObservableCollection<string> FadeOutStrings         { get; } = new ObservableCollection<string>(ProfileCfgArrays.FadeOutStrings);

        internal void LoadModsFromFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title                     = "Select the Arma3 mod preset",
                IsFolderPicker            = false,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems   = false,
                EnsureFileExists          = true,
                EnsurePathExists          = true,
                EnsureReadOnly            = false,
                EnsureValidNames          = true,
                Multiselect               = false,
                ShowPlacesList            = true
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Arma 3 Mod Preset", ".html"));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            if (dialog.FileName == null)
            {
                MessageBox.Show("Please enter a valid arma3server executable location");
                return;
            }

            var lines = File.ReadAllLines(dialog.FileName)
                            .AsEnumerable()
                            .Where(l => l.Contains("steamcommunity.com/sharedfiles/filedetails"));
            
            //Clear mods
            foreach (var mod in Profile.ProfileMods)
            { mod.ClientSideChecked = false; }

            ushort? loadPriority = 1;

            foreach (string line in lines)
            {
                var link = XElement.Parse(line).Attribute("href").Value;
                if (link == null)
                    continue;
                var modIdS = System.Web.HttpUtility.ParseQueryString(new Uri(link).Query).Get("id");

                if (!uint.TryParse(modIdS, out uint modId)) continue;
                var mod = Profile.ProfileMods.FirstOrDefault(m => m.Id == modId);
                if (mod != null)
                { 
                    mod.ClientSideChecked = true; 
                    mod.LoadPriority =  loadPriority;
                    loadPriority     += 1;
                }
                else
                { DisplayMessage("Some mods in the preset were not downloaded yet. Import the preset and retry later."); }
            }
        }

        internal void SelectServerFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title                     = "Select the arma server executable",
                IsFolderPicker            = false,
                AddToMostRecentlyUsedList = false,
                InitialDirectory          = Properties.Settings.Default.serverPath,
                DefaultDirectory          = Properties.Settings.Default.serverPath,
                AllowNonFileSystemItems   = false,
                EnsureFileExists          = true,
                EnsurePathExists          = true,
                EnsureReadOnly            = false,
                EnsureValidNames          = true,
                Multiselect               = false,
                ShowPlacesList            = true
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Arma 3 Server Executable", ".exe"));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            if (dialog.FileName != null)
            { Profile.Executable = dialog.FileName; }
            else
            { MessageBox.Show("Please enter a valid arma3server executable location"); }
        }

        internal async Task CopyModKeys()
        {
            var mods = new List<string>();

            if (!Directory.Exists(Properties.Settings.Default.modStagingDirectory))
            {
                MainWindow.Instance.IFlyout.IsOpen         = true;
                MainWindow.Instance.IFlyoutMessage.Content = $"The SteamCMD path does not exist :\n{Properties.Settings.Default.modStagingDirectory}";
                return;
            }
            var steamMods = Profile.ProfileMods.Where(p => p.ClientSideChecked).ToList();

            foreach (var line in steamMods)
            {
                try
                { mods.AddRange(Directory.GetFiles(Path.Combine(Properties.Settings.Default.modStagingDirectory, line.Id.ToString()), "*.bikey", SearchOption.AllDirectories)); }
                catch (DirectoryNotFoundException)
                { /*there was no directory*/ }
            }

            await ClearModKeys();

            Directory.CreateDirectory(Path.Combine(Profile.ArmaPath, "keys"));

            foreach (var link in mods)
            {
                try { File.Copy(link, Path.Combine(Profile.ArmaPath, "keys", Path.GetFileName(link)), true); }
                catch (IOException)
                {
                    MainWindow.Instance.IFlyout.IsOpen         = true;
                    MainWindow.Instance.IFlyoutMessage.Content = $"Some keys could not be copied : {Path.GetFileName(link)}";
                }
            }
            await Task.Delay(1000);
        }

        internal async Task ClearModKeys()
        {
            var ignoredKeys = new[] {"a3.bikey", "a3c.bikey", "gm.bikey"};
            if (Directory.Exists(Path.Combine(Profile.ArmaPath, "keys")))
            {
                foreach (var keyFile in Directory.GetFiles(Path.Combine(Profile.ArmaPath, "keys")))
                {
                    if (ignoredKeys.Any(keyFile.Contains))
                        continue;
                    try
                    {
                        await Task.Run(() => File.Delete(keyFile));
                    }
                    catch (Exception)
                    {
                        MainWindow.Instance.IFlyout.IsOpen         = true;
                        MainWindow.Instance.IFlyoutMessage.Content = $"Some keys could not be cleared : {Path.GetFileName(keyFile)}";
                    }
                }
            }
        }
        
        public ObservableCollection<string> LimitedDistanceStrings { get; } = new ObservableCollection<string>(ProfileCfgArrays.LimitedDistanceStrings);
        public ObservableCollection<string> AiPresetStrings        { get; } = new ObservableCollection<string>(ProfileCfgArrays.AiPresetStrings);
        public ObservableCollection<string> ThirdPersonStrings     { get; } = new ObservableCollection<string>(ProfileCfgArrays.ThirdPersonStrings);
        
        public void LoadData()
        {
            var modlist = new List<ProfileMod>();
            foreach(var mod in Properties.Settings.Default.armaMods.ArmaMods)
            {
                ProfileMod existingMod = Profile.ProfileMods.FirstOrDefault(m => m.Id == mod.WorkshopId);
                if (existingMod == null)
                {
                    var newProfile = new ProfileMod { Name = mod.Name, Id = mod.WorkshopId, IsLocal = mod.IsLocal};
                    modlist.Add(newProfile);
                    continue;
                }
                modlist.Add(existingMod);
            }
            
            Profile.ProfileMods = modlist;

            LoadMissions();
        }

        public void UnloadData()
        {
            var index = Properties.Settings.Default.Profiles.FindIndex(p => p.Id == Profile.Id);  
            if (index != -1)
            { Properties.Settings.Default.Profiles[index] = Profile; }
        }
        
        internal void LoadMissions()
        {
            if (!Directory.Exists(Path.Combine(Profile.ArmaPath, "mpmissions"))) return;

            var missionList = new List<ProfileMission>();
            List<string> newMissions = new();

            //Load PBO files
            newMissions.AddRange(Directory.EnumerateFiles(Path.Combine(Profile.ArmaPath, "mpmissions"), "*.pbo",  searchOption: SearchOption.TopDirectoryOnly)
                                                    .Select(mission => mission.Replace(Path.Combine(Profile.ArmaPath, "mpmissions") + "\\", "")));
            //Load folders
            //Credits to Pucker and LinkIsParking
            newMissions.AddRange(Directory.GetDirectories(Path.Combine(Profile.ArmaPath, "mpmissions"))
                                                .Select(mission => mission.Replace(Path.Combine(Profile.ArmaPath, "mpmissions") + "\\", "")));


            foreach (var mission in newMissions)
            {
                ProfileMission existingMission = Profile.ServerCfg.Missions.FirstOrDefault(m => m.Path == mission);
                if (existingMission == null)
                {
                    var newMission = new ProfileMission { Name = mission.Replace(".pbo", ""), Path = mission };
                    missionList.Add(newMission);
                    continue;
                }
                missionList.Add(existingMission);
            }

            Profile.ServerCfg.Missions = missionList;
        }

        internal void ClearModOrder()
        {
            foreach (ProfileMod mod in Profile.ProfileMods)
            { mod.LoadPriority = null; }
        }

        internal void ModsCopyFrom(object to, string from)
        {
            foreach (var mod in Profile.ProfileMods)
            {
                switch (to.ToString())
                {
                    case "Server":
                        {
                            if (from == "Client") mod.ServerSideChecked = mod.ClientSideChecked;
                            if (from == "Headless") mod.ServerSideChecked = mod.HeadlessChecked;
                            break;
                        }
                    case "Client":
                        {
                            if (from == "Server") mod.ClientSideChecked = mod.ServerSideChecked;
                            if (from == "Headless") mod.ClientSideChecked = mod.HeadlessChecked;
                            break;
                        }
                    case "Headless":
                        {
                            if (from == "Client") mod.HeadlessChecked = mod.ClientSideChecked;
                            if (from == "Server") mod.HeadlessChecked = mod.ServerSideChecked;
                            break;
                        }
                }
            }
        }

        internal void ModsSelectAll(object to, bool select)
        {
            foreach (var mod in Profile.ProfileMods)
            {
                switch (to.ToString())
                {
                    case "Server":
                        mod.ServerSideChecked = select;
                        break;
                    case "Client":
                        mod.ClientSideChecked = select;
                        break;
                    case "Headless":
                        mod.HeadlessChecked = select;
                        break;
                }
            }
        }

        internal void MissionSelectAll(bool select)
        {
            foreach (var mission in Profile.ServerCfg.Missions)
            { mission.MissionChecked = select; }
        }
    }
}
