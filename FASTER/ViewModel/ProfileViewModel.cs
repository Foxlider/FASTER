using FASTER.Models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FASTER.ViewModel
{
    internal class ProfileViewModel
    {
        public ProfileViewModel()
        {
            Profile = new ServerProfileNew("Server");
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
            ArmaProfile                    = Profile.ArmaProfile;
            ArmaProfile.ArmaProfileContent = ArmaProfile.ProcessFile();
            BasicCfg              = Profile.BasicCfg;
            BasicCfg.BasicContent = BasicCfg.ProcessFile();
        }
        public ProfileViewModel(string name)
        {
            Profile = new ServerProfileNew(name);
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
            BasicCfg              = Profile.BasicCfg;
            BasicCfg.BasicContent = BasicCfg.ProcessFile();
        }

        public ProfileViewModel(string name, Guid id)
        {
            Profile = new ServerProfileNew(name, id);
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
            ArmaProfile = Profile.ArmaProfile;
            ArmaProfile.ArmaProfileContent = ArmaProfile.ProcessFile();
            BasicCfg = Profile.BasicCfg;
            BasicCfg.BasicContent = BasicCfg.ProcessFile();
        }

        public ServerCfg ServerCfg { get; set; }
        public Arma3Profile ArmaProfile { get; set; }
        public ServerProfileNew Profile { get; set; }
        public BasicCfg BasicCfg { get; set; }

        public ObservableCollection<string> VonCodecs              { get; } = new ObservableCollection<string>(ServerCfgArrays.VonCodecStrings);
        public ObservableCollection<string> FilePatching           { get; } = new ObservableCollection<string>(ServerCfgArrays.AllowFilePatchingStrings);
        public ObservableCollection<string> VerifySignatures       { get; } = new ObservableCollection<string>(ServerCfgArrays.VerifySignaturesStrings);
        public ObservableCollection<string> TimestampFormats       { get; } = new ObservableCollection<string>(ServerCfgArrays.TimeStampStrings);
        public ObservableCollection<string> EnabledStrings         { get; } = new ObservableCollection<string>(ProfileCfgArrays.EnabledStrings);
        public ObservableCollection<string> FadeOutStrings         { get; } = new ObservableCollection<string>(ProfileCfgArrays.FadeOutStrings);

        internal void LoadModsFromFile()
        {
            throw new NotImplementedException();
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
            { Directory.Delete(Path.Combine(Properties.Settings.Default.serverPath, "keys"), true); }

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
            Profile.ProfileMods = modlist;

            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }

        internal void LoadMissions()
        {
            if (!Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"))) return;

            var missionList = new List<ProfileMission>();
            List<string> newMissions = new List<string>();
            newMissions.AddRange(Directory.GetFiles(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"), "*.pbo")
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
