
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

// ReSharper disable once CheckNamespace
namespace FASTER.Models
{
    [Serializable]
    public class ServerProfileCollection : List<ServerProfile>
    {
        [XmlElement(Order = 1)]
        public string CollectionName { get; set; }

        public ServerProfileCollection()
        { CollectionName = "Main"; }
    }

    [Serializable]
    public class ServerProfile : INotifyPropertyChanged
    {
        //PRIVATE VARS DECLARATION
        private string _id;
        private string _name;
        private string _executable;
        private int _port = 2302;
        private int _headlessNum;
        private bool _missionOverride;
        private bool _contactDlcChecked;
        private bool _gmDlcChecked;
        private bool _enableHT = true;
        private bool _enableRanking;

        private List<ProfileMod> _profileMods = new List<ProfileMod>();
        private ServerCfg _serverCfg;
        private Arma3Profile _armaProfile;
        private BasicCfg _basicCfg;

        //PUBLIC VAR DECLARATIONS
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public string Executable
        {
            get => _executable;
            set
            {
                _executable = value;
                RaisePropertyChanged("Executable");
            }
        }
        
        public int Port 
        { 
            get => _port; 
            set 
            { 
                _port = value;
                RaisePropertyChanged("Port");
            } 
        }

        public int HeadlessNumber
        {
            get => _headlessNum;
            set
            {
                _headlessNum = value;
                RaisePropertyChanged("HeadlessNumber");
            }
        }

        public bool MissionSelectorOverride
        {
            get => _missionOverride;
            set
            {
                _missionOverride = value;
                RaisePropertyChanged("MissionSelectorOverride");
            }
        }

        public bool ContactDLCChecked
        {
            get => _contactDlcChecked;
            set
            {
                _contactDlcChecked = value;
                RaisePropertyChanged("ContactDLCChecked");
            }
        }

        public bool GMDLCChecked
        {
            get => _gmDlcChecked;
            set
            {
                _gmDlcChecked = value;
                RaisePropertyChanged("GMDLCChecked");
            }
        }

        public bool EnableHyperThreading
        {
            get => _enableHT;
            set
            {
                _enableHT = value;
                RaisePropertyChanged("EnableHyperThreading");
            }
        }

        public bool RankingChecked
        {
            get => _enableRanking;
            set
            {
                _enableRanking = value;
                RaisePropertyChanged("RankingChecked");
            }
        }

        //Current logit to count the checked mods
        public int ServerModsChecked => ProfileMods.Count(m => m.ServerSideChecked);
        public int ClientModsChecked => ProfileMods.Count(m => m.ClientSideChecked);
        public int HeadlessModsChecked => ProfileMods.Count(m => m.HeadlessChecked);

        public List<ProfileMod> ProfileMods
        {
            get => _profileMods;
            set
            {
                //Removing previous triggers
                _profileMods.ForEach(m => m.PropertyChanged -= Item_PropertyChanged);

                _profileMods = value;

                //Adding the trigger to count checked mods
                _profileMods.ForEach(m => m.PropertyChanged += Item_PropertyChanged);

                RaisePropertyChanged("ProfileMods");
            }
        }

        public ServerCfg ServerCfg 
        { 
            get => _serverCfg;
            set
            {
                _serverCfg = value;
                RaisePropertyChanged("ServerCfg");
            }
        }

        public Arma3Profile ArmaProfile
        {
            get => _armaProfile;
            set
            {
                _armaProfile = value;
                RaisePropertyChanged("ArmaProfile");
            }
        }

        public BasicCfg BasicCfg
        {
            get => _basicCfg;
            set
            {
                _basicCfg = value;
                RaisePropertyChanged("BasicCfg");
            }
        }

        public void GenerateNewId()
        { _id = $"_{Guid.NewGuid():N}"; }

        //This is used to trigger PropertyChanged to count each checked mod
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("ServerModsChecked");
            RaisePropertyChanged("ClientModsChecked");
            RaisePropertyChanged("HeadlessModsChecked");
        }

        //INOTIFYPROPERTYCHANGED
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
    }

    [Serializable]
    public class ProfileMod : INotifyPropertyChanged
    {
        private bool serverSideChecked;
        private bool clientSideChecked;
        private bool headlessChecked;
        private uint _id;
        private string name;

        public bool ServerSideChecked
        {
            get => serverSideChecked;
            set
            {
                serverSideChecked = value;
                RaisePropertyChanged("ServerSideChecked");
                RaisePropertyChanged("HeadlessModsChecked");
            }
        }
        public bool ClientSideChecked
        {
            get => clientSideChecked;
            set
            {
                clientSideChecked = value;
                RaisePropertyChanged("ClientSideChecked");
                RaisePropertyChanged("ClientModsChecked");
            }
        }
        public bool HeadlessChecked
        {
            get => headlessChecked;
            set
            {
                headlessChecked = value;
                RaisePropertyChanged("HeadlessChecked");
                RaisePropertyChanged("HeadlessModsChecked");
            }
        }

        public uint Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
