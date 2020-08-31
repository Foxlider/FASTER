
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;

namespace FASTER.Models
{
    [Serializable]
    public class ServerProfileCollection : List<ServerProfileNew>
    {
        [XmlElement(Order = 1)]
        public string CollectionName { get; set; }

        public ServerProfileCollection()
        { CollectionName = "Main"; }

        internal static void AddServerProfile(string profileName)
        {
            var currentProfiles = Properties.Settings.Default.Profiles;
            var p = new ServerProfileNew(profileName);
            p.ServerCfg.ServerCfgContent     = p.ServerCfg.ProcessFile();
            p.BasicCfg.BasicContent          = p.BasicCfg.ProcessFile();
            p.ArmaProfile.ArmaProfileContent = p.ArmaProfile.ProcessFile();
            currentProfiles.Add(p);
            Properties.Settings.Default.Profiles = currentProfiles;
            Properties.Settings.Default.Save();
            MainWindow.Instance.LoadServerProfiles();
        }

        internal static void AddServerProfile(ServerProfileNew profile)
        {
            var currentProfiles = Properties.Settings.Default.Profiles;
            profile.GenerateNewId();
            currentProfiles.Add(profile);
            Properties.Settings.Default.Profiles = currentProfiles;
            Properties.Settings.Default.Save();
            MainWindow.Instance.LoadServerProfiles();
        }
    }

    [Serializable]
    public class ServerProfileNew : INotifyPropertyChanged
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
                if (MainWindow.HasLoaded())
                {
                    var menuItem = MainWindow.Instance.IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == _id);
                    if (menuItem != null)
                    { menuItem.Content = _name; }
                }
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

        //CTORS
        public ServerProfileNew(string name, bool createFolder = true)
        {
            _id = $"_{Guid.NewGuid():N}";
            Name = name;
            Executable = Path.Combine(Properties.Settings.Default.serverPath, "arma3server_x64.exe");
            ServerCfg = new ServerCfg(){ Hostname = name};
            ArmaProfile = new Arma3Profile();
            BasicCfg = new BasicCfg();
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
            ArmaProfile.ArmaProfileContent = ArmaProfile.ProcessFile();
            BasicCfg.BasicContent = BasicCfg.ProcessFile();

            if (createFolder)
            { Directory.CreateDirectory(Path.Combine(Properties.Settings.Default.serverPath, "Servers", Id)); }
        }

        public ServerProfileNew()
        {
            _id  = $"_{Guid.NewGuid():N}";
            Name = _id;
            ServerCfg   = new ServerCfg(){ Hostname = Name};
            ArmaProfile = new Arma3Profile();
            BasicCfg    = new BasicCfg();
            ServerCfg.ServerCfgContent = ServerCfg.ProcessFile();
            ArmaProfile.ArmaProfileContent = ArmaProfile.ProcessFile();
            BasicCfg.BasicContent = BasicCfg.ProcessFile();
        }

        public void GenerateNewId()
        { _id = $"_{Guid.NewGuid():N}"; }

        public ServerProfileNew Clone()
        {
            ServerProfileNew p = (ServerProfileNew) MemberwiseClone();
            p.GenerateNewId();
            if (p.Name.EndsWith(")") && p.Name.Contains('(') && int.TryParse(p.Name.Substring(p.Name.Length - 2, 1), out _))
            {
                var i   = p.Name.IndexOf('(');
                var j   = p.Name.Length;
                var num = p.Name.Substring(i+1,   j -1 - i-1);
                p.Name = $"{p.Name.Substring(0, p.Name.Length - i+1)} ({int.Parse(num) + 1})";
            }
            else
            { p.Name = $"{p.Name} (2)"; }
            return p;
        }

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
