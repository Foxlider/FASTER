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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FASTER.ViewModel
{
    internal class ProfileViewModel
    {
        public ProfileViewModel()
        { Profile = new ServerProfileNew("Server", false); }

        public ProfileViewModel(ServerProfileNew p)
        { Profile = p; }

        //Hash for ID generation
        private readonly System.Security.Cryptography.SHA1 hash = new System.Security.Cryptography.SHA1CryptoServiceProvider();

        public ServerProfileNew Profile { get; set; }

        public ObservableCollection<string> VonCodecs           { get; } = new ObservableCollection<string>(ServerCfgArrays.VonCodecStrings);
        public ObservableCollection<string> FilePatching        { get; } = new ObservableCollection<string>(ServerCfgArrays.AllowFilePatchingStrings);
        public ObservableCollection<string> VerifySignatures    { get; } = new ObservableCollection<string>(ServerCfgArrays.VerifySignaturesStrings);
        public ObservableCollection<string> TimestampFormats    { get; } = new ObservableCollection<string>(ServerCfgArrays.TimeStampStrings);
        public ObservableCollection<string> EnabledStrings      { get; } = new ObservableCollection<string>(ProfileCfgArrays.EnabledStrings);
        public ObservableCollection<string> MissionDifficulties { get; } = new ObservableCollection<string> { "Recruit", "Regular", "Veteran", "Custom" };
        public ObservableCollection<string> PerfPresets         { get; } = new ObservableCollection<string>(BasicCfgArrays.PerfPresets);

        internal void DisplayMessage(string msg)
        {
            MainWindow.Instance.IFlyout.IsOpen         = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

        internal void OpenProfileLocation()
        {
            string folderPath = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id);
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
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

            for (int hc = 1; hc <= Profile.HeadlessNumber; hc++ )
            {
                string headlessMods = string.Join(";", Profile.ProfileMods.Where(m => m.HeadlessChecked).Select(m => $"@{Functions.SafeName(m.Name)}"));
                List<string> arguments = new List<string>
                {
                    "-client",
                    "-connect=127.0.0.1",
                    $"-password={Profile.ServerCfg.Password}",
                    $"\"-profiles={Path.Combine(Properties.Settings.Default.serverPath, "Servers")}\"",
                    "-nosound",
                    $"-port={Profile.Port}",
                    $"{(!string.IsNullOrWhiteSpace(headlessMods) || Profile.ContactDLCChecked || Profile.GMDLCChecked ? $"\"-mod={(Profile.ContactDLCChecked ? "contact;" : "")}{(Profile.GMDLCChecked ? "GM;" : "")}{(!string.IsNullOrWhiteSpace(headlessMods) ? headlessMods + ";" : "")}\"" : "")}",
                    $"{(Profile.ServerCfg.MaxMemOverride ? $"-maxMem={Profile.ServerCfg.MaxMem}" : "")}",
                    $"{(Profile.ServerCfg.CpuCountOverride ? $"-cpuCount={Profile.ServerCfg.CpuCount}" : "")}",
                    $"{(Profile.EnableHyperThreading ? "-enableHT" : "")}"
                };

                string commandLine = string.Join(" ", arguments);

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
                DisplayMessage($"{Profile.HeadlessNumber} Headless Clients launched !\n{commandLine}");
                #else
                ProcessStartInfo hcStartInfo = new ProcessStartInfo(Profile.Executable, commandLine);
                Process          hcProcess   = new Process { StartInfo = hcStartInfo };
                hcProcess.Start();
                #endif
            }
        }

        internal void LaunchServer()
        {
            if (!VerifyBeforeLaunch()) return;

            //Launching... 
            DisplayMessage($"Launching Profile {Profile.Name}...");

            var commandLine = SetCommandLine();
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

        //Process the command line arguments for the Server
        private string SetCommandLine()
        {
            string config        = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "server_config.cfg");
            string basic         = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "server_basic.cfg");

            string playerMods = string.Join(";", Profile.ProfileMods.Where(m => m.ClientSideChecked).Select(m => $"@{Functions.SafeName(m.Name)}"));
            string serverMods = string.Join(";", Profile.ProfileMods.Where(m => m.ServerSideChecked).Select(m => $"@{Functions.SafeName(m.Name)}"));
            List<string> arguments = new List<string>
            {
                $"-port={Profile.Port}",
                $"\"-config={config}\"",
                $"\"-cfg={basic}\"",
                $"\"-profiles={Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id)}\"",
                $"-name={Profile.Id}",
                $"{(!string.IsNullOrWhiteSpace(playerMods) || Profile.ContactDLCChecked || Profile.GMDLCChecked ? $"\"-mod={(Profile.ContactDLCChecked ? "contact;" : "")}{(Profile.GMDLCChecked ? "GM;" : "")}{(!string.IsNullOrWhiteSpace(playerMods) ? playerMods + ";" : "" )}\"" : "")}",
                $"{(!string.IsNullOrWhiteSpace(serverMods) ? $"\"-serverMod={serverMods};\"" : "")}",
                $"{(Profile.EnableHyperThreading ? "-enableHT" : "")}",
                $"{(Profile.ServerCfg.AllowedFilePatching != ServerCfgArrays.AllowFilePatchingStrings[0] ? "-filePatching" : "")}",
                $"{(Profile.ServerCfg.NetLogEnabled ? "-netlog" : "")}",
                $"{(Profile.RankingChecked ? $"-ranking={Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "ranking.log")}" : "")}",
                $"{(Profile.ServerCfg.AutoInit ? "-autoInit" : "")}",
                $"{(Profile.ServerCfg.MaxMemOverride ? $"-maxMem={Profile.ServerCfg.MaxMem}" : "")}",
                $"{(Profile.ServerCfg.CpuCountOverride ? $"-cpuCount={Profile.ServerCfg.CpuCount}" : "")}",
                $"{(!string.IsNullOrWhiteSpace(Profile.ServerCfg.CommandLineParameters) ? Profile.ServerCfg.CommandLineParameters : "")}"
            };

            string commandLine = string.Join(" ", arguments);
            return commandLine;
        }

        private static bool ProfileFilesExist(string profile)
        {
            string path = Properties.Settings.Default.serverPath;

            if (!Directory.Exists(Path.Combine(path, "Servers", profile)))
            { return false; }

            return File.Exists(Path.Combine(path, "Servers", profile, "server_config.cfg")) 
                && File.Exists(Path.Combine(path, "Servers", profile, "server_basic.cfg"));
        }

        internal void DeleteProfile()
        {
            if (Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id)))
            { Directory.Delete(Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id), true); }
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
            string config        = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "server_config.cfg");
            string basic         = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "server_basic.cfg");
            string serverProfile = Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "users", Profile.Id, $"{Profile.Id}.Arma3Profile");

            //Creating profile directory
            Directory.CreateDirectory(Path.Combine(Properties.Settings.Default.serverPath, "Servers", Profile.Id, "users", Profile.Id));

            //Writing files
            try
            {
                File.WriteAllText(config,        Profile.ServerCfg.ServerCfgContent);
                File.WriteAllText(basic,         Profile.BasicCfg.BasicContent);
                File.WriteAllText(serverProfile, Profile.ArmaProfile.ArmaProfileContent);
            }
            catch
            { DisplayMessage("Could not write the config files. Please ensure the server is not running and retry."); }

            var tempProfile = Properties.Settings.Default.Profiles.FirstOrDefault(p => p.Id == Profile.Id);
            if (tempProfile != null)
            { tempProfile = Profile; }
            Properties.Settings.Default.Save();
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

            var lines = File.ReadAllLines(dialog.FileName).ToList();
            
            //Clear mods
            foreach (var mod in Profile.ProfileMods)
            { mod.ClientSideChecked = false; }

            //Select new ones
            foreach (var modIdS in from line in lines where line.Contains("steamcommunity.com/sharedfiles/filedetails") select line.Split("?id=") into extract select extract[1].Split('"')[0])
            {
                if (!uint.TryParse(modIdS, out uint modId)) continue;
                var mod = Profile.ProfileMods.FirstOrDefault(m => m.Id == modId);
                if (mod != null)
                { mod.ClientSideChecked = true; }
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
            var path = Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410");
            var ignoredKeys = new string[] { "a3.bikey", "a3c.bikey", "gm.bikey" };


            if (!Directory.Exists(path))
            {
                MainWindow.Instance.IFlyout.IsOpen         = true;
                MainWindow.Instance.IFlyoutMessage.Content = $"The SteamCMD path does not exist :\n{path}";
                return;
            }
            var steamMods = Profile.ProfileMods.Where(p => p.ClientSideChecked).ToList();

            foreach (var line in steamMods)
            {
                try
                {
                    mods.AddRange(Directory.GetFiles(Path.Combine(path, line.Id.ToString()), "*.bikey", SearchOption.AllDirectories));
                }
                catch (DirectoryNotFoundException)
                { /*there was no directory*/ }
            }

            foreach (var folder in Properties.Settings.Default.localModFolders)
            {
                try 
                { mods.AddRange(Directory.GetFiles(folder, "*.bikey", SearchOption.AllDirectories)); }
                catch (DirectoryNotFoundException)
                { /*there was no directory*/ }
            }

            if (Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "keys"))) 
            {
                foreach (var keyFile in Directory.GetFiles(Path.Combine(Properties.Settings.Default.serverPath, "keys")))
                {
                    if (ignoredKeys.Any(keyFile.Contains))
                        continue;
                    try
                    { File.Delete(keyFile); }
                    catch (Exception)
                    {
                        MainWindow.Instance.IFlyout.IsOpen = true;
                        MainWindow.Instance.IFlyoutMessage.Content = $"Some keys could not be cleared : {Path.GetFileName(keyFile)}";
                    }
                }
            }

            Directory.CreateDirectory(Path.Combine(Properties.Settings.Default.serverPath, "keys"));

            foreach (var link in mods)
            {
                try { File.Copy(link, Path.Combine(Properties.Settings.Default.serverPath, "keys", Path.GetFileName(link)), true); }
                catch (IOException)
                {
                    MainWindow.Instance.IFlyout.IsOpen         = true;
                    MainWindow.Instance.IFlyoutMessage.Content = $"Some keys could not be copied : {Path.GetFileName(link)}";
                }
            }
            await Task.Delay(1000);
        }

        public ObservableCollection<string> LimitedDistanceStrings { get; } = new ObservableCollection<string>(ProfileCfgArrays.LimitedDistanceStrings);
        public ObservableCollection<string> AiPresetStrings        { get; } = new ObservableCollection<string>(ProfileCfgArrays.AiPresetStrings);
        public ObservableCollection<string> ThirdPersonStrings     { get; } = new ObservableCollection<string>(ProfileCfgArrays.ThirdPersonStrings);
        
        public void LoadData()
        {
            var modlist = new List<ProfileMod>();
            foreach(var mod in Properties.Settings.Default.steamMods.SteamMods)
            {
                ProfileMod existingMod = Profile.ProfileMods.FirstOrDefault(m => m.Id == mod.WorkshopId);
                if (existingMod == null)
                {
                    var newProfile = new ProfileMod { Name = mod.Name, Id = mod.WorkshopId };
                    modlist.Add(newProfile);
                    continue;
                }
                modlist.Add(existingMod);
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.serverPath))
            {
                List<string> localMods = Directory.GetDirectories(Properties.Settings.Default.serverPath, "@*")
                                                  .Select(addon => addon.Replace(Properties.Settings.Default.serverPath + @"\", ""))
                                                  .ToList();
                List<string> targetForDeletion = new List<string>();
                foreach (var folder in Properties.Settings.Default.localModFolders)
                {
                    if (Directory.Exists(folder))
                        localMods.AddRange(Directory.GetDirectories(folder, "@*"));
                    else
                    {
                        DisplayMessage("A folder could not be found and have been deleted");
                        targetForDeletion.Add(folder);
                    }
                }
                foreach (var folder in targetForDeletion)
                { Properties.Settings.Default.localModFolders.Remove(folder); }

                foreach (var addon in localMods.ToList()
                                             .Where(addon => modlist.FirstOrDefault(m => addon.Contains($"@{Functions.SafeName(m.Name)}")) != null)) 
                { localMods.Remove(addon); }

                foreach(var mod in localMods)
                {
                    var newId = GetUInt32HashCode(mod);
                    ProfileMod existingMod = Profile.ProfileMods.FirstOrDefault(m => m.Id == newId);
                
                    if (existingMod == null)
                    {
                        var newProfile = new ProfileMod { Name = mod, Id = newId };
                        modlist.Add(newProfile);
                        continue;
                    }
                    modlist.Add(existingMod);
                }
            }
            
            Profile.ProfileMods = modlist;

            LoadMissions();
        }

        public void UnloadData()
        {
            var tempProfile = Properties.Settings.Default.Profiles.FirstOrDefault(p => p.Id == Profile.Id);
            if (tempProfile != null)
            { tempProfile = Profile; }
        }


        private uint GetUInt32HashCode(string strText)
        {
            if (string.IsNullOrEmpty(strText)) return 0;

            //Unicode Encode Covering all characterset
            byte[] byteContents   = Encoding.Unicode.GetBytes(strText);
            byte[] hashText       = hash.ComputeHash(byteContents);
            uint   hashCodeStart  = BitConverter.ToUInt32(hashText, 0);
            uint   hashCodeMedium = BitConverter.ToUInt32(hashText, 8);
            uint   hashCodeEnd    = BitConverter.ToUInt32(hashText, 16);
            var    hashCode       = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            return uint.MaxValue - hashCode;
        } 

        internal void LoadMissions()
        {
            if (!Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"))) return;

            var missionList = new List<ProfileMission>();
            List<string> newMissions = new List<string>();

            //Load PBO files
            newMissions.AddRange(Directory.EnumerateFiles(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"), "*.pbo",  searchOption: SearchOption.TopDirectoryOnly)
                                                    .Select(mission => mission.Replace(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions") + "\\", "")));
            //Load folders
            //Credits to Pucker and LinkIsParking
            newMissions.AddRange(Directory.GetDirectories(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"))
                                                .Select(mission => mission.Replace(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions") + "\\", "")));


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
                    default:
                        break;
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
                    default:
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
