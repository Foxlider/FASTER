using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace FASTER.ViewModel
{
    class ProfileViewModel
    {
        public ServerCfg ServerCfg { get; set; }

        public CollectionView VonCodecs { get; } = new CollectionView(ServerCfgArrays.VonCodecStrings);
        public CollectionView FilePatching { get; } = new CollectionView(ServerCfgArrays.AllowFilePatchingStrings);
        public CollectionView VerifySignatures { get; } = new CollectionView(ServerCfgArrays.VerifySignaturesStrings);
        public CollectionView TimestampFormats { get; } = new CollectionView(ServerCfgArrays.TimeStampStrings);

        public void LoadServerCfg()
        {
            ServerCfg = new ServerCfg { Hostname = "New Server" };
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
        }


    }
}
