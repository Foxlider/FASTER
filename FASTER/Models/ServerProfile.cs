
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;

namespace FASTER.Models
{
    [Serializable]
    public class ServerProfileCollection : List<ServerProfile>
    {
        [XmlElement(Order = 1)]
        public string CollectionName { get; set; }

        public ServerProfileCollection()
        { CollectionName = "Main"; }

        internal static void AddServerProfile(string profileName)
        {
            var currentProfiles = Properties.Settings.Default.Profiles;
            var p = new ServerProfile(profileName);
            p.ServerCfg.ServerCfgContent     = p.ServerCfg.ProcessFile();
            p.BasicCfg.BasicContent          = p.BasicCfg.ProcessFile();
            p.ArmaProfile.ArmaProfileContent = p.ArmaProfile.ProcessFile();
            currentProfiles.Add(p);
            Properties.Settings.Default.Profiles = currentProfiles;
            Properties.Settings.Default.Save();
            MainWindow.Instance.LoadServerProfiles();
        }

        internal static void AddServerProfile(ServerProfile profile)
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
        private bool _pfDlcChecked;
        private bool _cslaDlcChecked;
        private bool _wsDlcChecked;
        private bool _speDlcChecked;
        private bool _rfDlcChecked;
        private bool _enableHT = true;
        private bool _enableRanking;

        private List<ProfileMod> _profileMods = new List<ProfileMod>();
        private string _profileModsFilter = "";
        private bool _profileModsFilterIsCaseSensitive = false;
        private bool _profileModsFilterIsWholeWord = false;
        private bool _profileModsFilterIsRegex = false;
        private bool _profileModsFilterIsInvalid = false;
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
                RaisePropertyChanged("ArmaPath");
            }
        }

        public string ArmaPath => Path.GetDirectoryName(_executable);

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

        public bool PFDLCChecked
        {
            get => _pfDlcChecked;
            set
            {
                _pfDlcChecked = value;
                RaisePropertyChanged("PFDLCChecked");
            }
        }

        public bool CSLADLCChecked
        {
            get => _cslaDlcChecked;
            set
            {
                _cslaDlcChecked = value;
                RaisePropertyChanged("CSLADLCChecked");
            }
        }

        public bool WSDLCChecked
        {
            get => _wsDlcChecked;
            set
            {
                _wsDlcChecked = value;
                RaisePropertyChanged(nameof(WSDLCChecked));
            }
        }

        public bool SPEDLCChecked
        {
            get => _speDlcChecked;
            set
            {
                _speDlcChecked = value;
                RaisePropertyChanged(nameof(SPEDLCChecked));
            }
        }

        public bool RFDLCChecked
        {
            get => _rfDlcChecked;
            set
            {
                _rfDlcChecked = value;
                RaisePropertyChanged(nameof(RFDLCChecked));
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

        //Current logic to count the checked mods
        public int ServerModsChecked => ProfileMods.Count(m => m.ServerSideChecked);
        public int ClientModsChecked => ProfileMods.Count(m => m.ClientSideChecked);
        public int HeadlessModsChecked => ProfileMods.Count(m => m.HeadlessChecked);
        public int OptModsChecked => ProfileMods.Count(m => m.OptChecked);

        public string CommandLine => GetCommandLine();

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
                RaisePropertyChanged("FilteredProfileMods");
            }
        }

        public List<ProfileMod> FilteredProfileMods
        {
            get
            {
                if (string.IsNullOrEmpty(ProfileModsFilter))
                {
                    if (ProfileModsFilterIsInvalid)
                    {
                        ProfileModsFilterIsInvalid = false;
                    }
                    return new List<ProfileMod>(_profileMods);
                }

                var pattern = ProfileModsFilter;
                if (!ProfileModsFilterIsRegex)
                {
                    pattern = Regex.Replace(pattern, @"[\\\{\}\*\+\?\|\^\$\.\[\]\(\)]", "\\$&");
                }

                if (ProfileModsFilterIsWholeWord)
                {
                    if (!Regex.IsMatch(pattern[0].ToString(), @"\B"))
                    {
                        pattern = $"\\b{pattern}";
                    }
                    if (!Regex.IsMatch(pattern[pattern.Length - 1].ToString(), @"\B"))
                    {
                        pattern = $"{pattern}\\b";
                    }
                }

                var options = ProfileModsFilterIsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                try
                {
                    var filteredProfileMods = _profileMods.Where(m => Regex.IsMatch(m.Name, pattern, options)).ToList();
                    if (ProfileModsFilterIsInvalid)
                    {
                        ProfileModsFilterIsInvalid = false;
                    }
                    return filteredProfileMods;
                }
                catch (ArgumentException)
                {
                    if (!ProfileModsFilterIsInvalid)
                    {
                        ProfileModsFilterIsInvalid = true;
                    }
                    return new List<ProfileMod>();
                }
            }
        }

        public string ProfileModsFilter
        {
            get => _profileModsFilter;
            set
            {
                _profileModsFilter = value;
                RaisePropertyChanged("ProfileModsFilter");
                RaisePropertyChanged("FilteredProfileMods");
            }
        }

        public bool ProfileModsFilterIsCaseSensitive
        {
            get => _profileModsFilterIsCaseSensitive;
            set
            {
                _profileModsFilterIsCaseSensitive = value;
                RaisePropertyChanged("ProfileModsFilterIsCaseSensitive");
                RaisePropertyChanged("FilteredProfileMods");
            }
        }

        public bool ProfileModsFilterIsWholeWord
        {
            get => _profileModsFilterIsWholeWord;
            set
            {
                _profileModsFilterIsWholeWord = value;
                RaisePropertyChanged("ProfileModsFilterIsWholeWord");
                RaisePropertyChanged("FilteredProfileMods");
            }
        }

        public bool ProfileModsFilterIsRegex
        {
            get => _profileModsFilterIsRegex;
            set
            {
                _profileModsFilterIsRegex = value;
                RaisePropertyChanged("ProfileModsFilterIsRegex");
                RaisePropertyChanged("FilteredProfileMods");
            }
        }

        public bool ProfileModsFilterIsInvalid
        {
            get => _profileModsFilterIsInvalid;
            set
            {
                _profileModsFilterIsInvalid = value;
                RaisePropertyChanged("ProfileModsFilterIsInvalid");
            }
        }

        public ServerCfg ServerCfg 
        { 
            get => _serverCfg;
            set
            {
                if(_serverCfg != null)
                    _serverCfg.PropertyChanged -= Class_PropertyChanged;
                _serverCfg                  =  value;
                _serverCfg.PropertyChanged  += Class_PropertyChanged;
                RaisePropertyChanged("ServerCfg");
            }
        }

        public Arma3Profile ArmaProfile
        {
            get => _armaProfile;
            set
            {
                if (_armaProfile != null)
                    _armaProfile.PropertyChanged -= Class_PropertyChanged;
                _armaProfile               =  value;
                _armaProfile.PropertyChanged += Class_PropertyChanged;
                RaisePropertyChanged("ArmaProfile");
            }
        }

        public BasicCfg BasicCfg
        {
            get => _basicCfg;
            set
            {
                if (_basicCfg != null)
                    _basicCfg.PropertyChanged -= Class_PropertyChanged;
                _basicCfg                  =  value;
                _basicCfg.PropertyChanged += Class_PropertyChanged;
                RaisePropertyChanged("BasicCfg");
            }
        }

        //CTORS
        public ServerProfile(string name, bool createFolder = true)
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

        public ServerProfile()
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

        public ServerProfile Clone()
        {
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            ServerProfile p = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerProfile>(serialized);

            if (p != null)
            {
                p.GenerateNewId();

                if (p.Name.EndsWith(")") && p.Name.Contains('(') && int.TryParse(p.Name.Substring(p.Name.Length - 2, 1), out _))
                {
                    var i   = p.Name.IndexOf('(');
                    var j   = p.Name.Length;
                    var num = p.Name.Substring(i + 1, j - 1 - i - 1);
                    p.Name = $"{p.Name.Substring(0,   p.Name.Length - i + 1)} ({int.Parse(num) + 1})";
                }
                else
                {
                    p.Name = $"{p.Name} (2)";
                }
            }
            else
            {
                p = new ServerProfile();
                p.GenerateNewId();
                p.Name = "New Profile";
            }
            
            return p;
        }

        public string GetDlcAndPlayerMods(string playerMods)
        {
            StringBuilder mods = new StringBuilder();
            if (ContactDLCChecked)
            {
                _ = mods.Append("contact;");
            }
            if (GMDLCChecked)
            {
                _ = mods.Append("gm;");
            }
            if (PFDLCChecked)
            {
                _ = mods.Append("vn;");
            }
            if (CSLADLCChecked)
            {
                _ = mods.Append("csla;");
            }
            if (WSDLCChecked)
            {
                _ = mods.Append("ws;");
            }
            if (SPEDLCChecked)
            {
                _ = mods.Append("spe;");
            }
            if (RFDLCChecked)
            {
                _ = mods.Append("rf;");
            }
            if (!string.IsNullOrWhiteSpace(playerMods))
            {
                _ = mods.Append($"{playerMods};");
            }
            return !string.IsNullOrWhiteSpace(mods.ToString()) ? $" \"-mod={mods}\"" : "";
        }

        private string GetCommandLine()
        {


            string config = Path.Combine(ArmaPath, "Servers", Id, "server_config.cfg");
            string basic  = Path.Combine(ArmaPath, "Servers", Id, "server_basic.cfg");

            string playerMods = string.Join(";", ProfileMods.Where(m => m.ClientSideChecked).OrderBy(m => m.LoadPriority).Select(m => $"@{Functions.SafeName(m.Name)}"));
            string serverMods = string.Join(";", ProfileMods.Where(m => m.ServerSideChecked).OrderBy(m => m.LoadPriority).Select(m => $"@{Functions.SafeName(m.Name)}"));
            List<string> arguments = new List<string>
            {
                $"-port={Port}",
                $" \"-config={config}\"",
                $" \"-cfg={basic}\"",
                $" \"-profiles={Path.Combine(ArmaPath, "Servers", Id)}\"",
                $" -name={Id}",
                GetDlcAndPlayerMods(playerMods),
                $"{(!string.IsNullOrWhiteSpace(serverMods) ? $" \"-serverMod={serverMods};\"" : "")}",
                $"{(EnableHyperThreading ? " -enableHT" : "")}",
                $"{(ServerCfg.AllowedFilePatching != ServerCfgArrays.AllowFilePatchingStrings[0] ? " -filePatching" : "")}",
                $"{(ServerCfg.NetLogEnabled ? " -netlog" : "")}",
                $"{(RankingChecked ? $" \"-ranking={Path.Combine(ArmaPath, "Servers", Id, "ranking.log")}\"" : "")}",
                $"{(ServerCfg.AutoInit ? " -autoInit" : "")}",
                $"{(ServerCfg.MaxMemOverride ? $" -maxMem={ServerCfg.MaxMem}" : "")}",
                $"{(ServerCfg.CpuCountOverride ? $" -cpuCount={ServerCfg.CpuCount}" : "")}",
                $"{(!string.IsNullOrWhiteSpace(ServerCfg.CommandLineParameters) ? $" {ServerCfg.CommandLineParameters}" : "")}"
            };

            string commandLine = string.Join("", arguments);
            return commandLine;
        }


        //This is used to trigger PropertyChanged to count each checked mod
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("ServerModsChecked");
            RaisePropertyChanged("ClientModsChecked");
            RaisePropertyChanged("HeadlessModsChecked");
            RaisePropertyChanged("OptModsChecked");
        }

        private void Class_PropertyChanged(object sender, PropertyChangedEventArgs e)
        { RaisePropertyChanged("CommandLine"); }

        //INOTIFYPROPERTYCHANGED
        public event PropertyChangedEventHandler PropertyChanged;
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property != "CommandLine")
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandLine)));
        }
    }

    [Serializable]
    public class ProfileMod : INotifyPropertyChanged
    {
        private bool    serverSideChecked;
        private bool    clientSideChecked;
        private bool    headlessChecked;
        private bool 	optChecked;
        private ushort? loadPriority;
        private bool    isLocal;
        private uint    _id;
        private string  name;

        public bool ServerSideChecked
        {
            get => serverSideChecked;
            set
            {
                serverSideChecked = value;
                RaisePropertyChanged("ServerSideChecked");
                RaisePropertyChanged("ServerModsChecked");
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

        public bool OptChecked
        {
            get => optChecked;
            set
            {
                optChecked = value;
                RaisePropertyChanged("OptChecked");
                RaisePropertyChanged("OptModsChecked");
            }
        }

        public ushort? LoadPriority
        {
            get => loadPriority;
            set
            {
                loadPriority = value;
                RaisePropertyChanged("LoadPriority");
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

        public bool IsLocal
        {
            get => isLocal;
            set
            {
                isLocal = value;
                RaisePropertyChanged("IsLocal");
            }
        }

        public override string ToString()
        { return $"{_id} {name}"; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
    }
}
