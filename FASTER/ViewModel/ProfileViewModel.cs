using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Text;
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


        public void LoadServerCfg()
        {
            //ServerCfg = new ServerCfg { Hostname = name };
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }
    }
}
