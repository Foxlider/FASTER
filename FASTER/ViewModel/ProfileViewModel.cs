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
                    modlist.Add(new ProfileMod { Name = mod.Name, Id = mod.WorkshopId });
                    continue;
                }
                modlist.Add(existingMod);
            }
            Profile.ProfileMods = modlist;
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }
    }
}
