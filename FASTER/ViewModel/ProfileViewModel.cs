using FASTER.Models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FASTER.ViewModel
{
    internal class ProfileViewModel
    {
        public ProfileViewModel()
        {
            Profile = new ServerProfileNew("Server");
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }
        public ProfileViewModel(string name)
        {
            Profile = new ServerProfileNew(name);
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }

        public ProfileViewModel(string name, Guid id)
        {
            Profile = new ServerProfileNew(name, id);
            ServerCfg = Profile.ServerCfg;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }

        public ServerCfg ServerCfg { get; set; }
        public ServerProfileNew Profile { get; set; }

        public CollectionView VonCodecs { get; } = new CollectionView(ServerCfgArrays.VonCodecStrings);
        public CollectionView FilePatching { get; } = new CollectionView(ServerCfgArrays.AllowFilePatchingStrings);
        public CollectionView VerifySignatures { get; } = new CollectionView(ServerCfgArrays.VerifySignaturesStrings);
        public CollectionView TimestampFormats { get; } = new CollectionView(ServerCfgArrays.TimeStampStrings);

        public bool ProfileNameEditMode { get; set; }


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
    }
}
