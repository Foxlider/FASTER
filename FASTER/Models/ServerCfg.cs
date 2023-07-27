using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace FASTER.Models
{
    static class ServerCfgArrays
    {
        public static string[] AllowFilePatchingStrings { get; } = { "No Clients", "HC Only", "All Clients" };
        public static string[] VerifySignaturesStrings { get; } = { "Disabled", "Deprecated", "Activated" };
        public static string[] VonCodecStrings { get; } = { "SPEEX", "OPUS" };
        public static string[] TimeStampStrings { get; } = { "none", "short", "long" };
    }

    [Serializable]
    public class ServerCfg : INotifyPropertyChanged
    {
        //Server Options
        private string       passwordAdmin;
        private string       password;
        private string       hostname;
        private int          maxPlayers = 32;
        private List<string> motd       = new();
        private int          motdInterval;
        private List<string> admins          = new();
        private List<string> headlessClients = new();
        private List<string> localClient     = new();
        private bool         headlessClientEnabled;
        private bool         votingEnabled;
        private bool         netlogEnabled;

        //Server Behavior
        private double voteThreshold      = 0.33;
        private int    voteMissionPlayers = 3;
        private short  kickduplicate      = 1; // 1 = active ; 0=disabled
        private bool   loopback;
        private bool   upnp = true;
        private short  allowedFilePatching;   // 0 = no clients; 1= HC only; 2= All Clients
        private int    disconnectTimeout = 90; //timeout in seconds
        private int    maxdesync         = 150;
        private int    maxping           = 200;
        private int    maxpacketloss     = 50;
        private bool   kickClientOnSlowNetwork;
        private int    lobbyIdleTimeout   = 300;
        private bool   autoSelectMission  = true;
        private bool   randomMissionOrder = true;
        private int    briefingTimeOut = 60; //
        private int    roleTimeOut = 90; // These are BI base figues
        private int    votingTimeOut = 60; //
        private int    debriefingTimeOut = 45; //
        private bool   LogObjectNotFound = false;			// logging disabled
        private bool   SkipDescriptionParsing = false;		// parse description.ext
        private bool   ignoreMissionLoadErrors = false;	// do not ingore errors
        private int    armaUnitsTimeout = 30; // Defines how long the player will be stuck connecting and wait for armaUnits data. Player will be notified if timeout elapsed and no units data was received

        //Arma server only
        private short  verifySignatures         = 0;        // 0 = Disabled (FASTER Default); 1 = Deprecated Activated ; 2 = Activated (Arma Default)
        private bool   drawingInMap             = true;
        private short  disableVoN;                          // 0 = VoN activated ; 1 = VoN Disabled
        private int    vonCodecQuality          = 3;        // 8kHz is 0-10, 16kHz is 11-20, 32kHz is 21-30 (and 48kHz with OPUS enabled)
        private short  vonCodec;                            // 0 = SPEEX ; 1 = OPUS
        private bool   skipLobby;                           //Overritten by mission parameters
        private string logFile                  = "server_console.log";
        private short  battlEye                 = 1;        // 0 = Disabled ; 1 = Enabled
        private string timeStampFormat          = "short";  // Possible values = "none", "short", "full"
        private short  persistent;
        private bool   requiredBuildChecked;
        private int    requiredBuild            = 999999999;
        private int    steamProtocolMaxDataSize = 1024;

        //Scripting
        private string serverCommandPassword;
        private string doubleIdDetected;
        private string onUserConnected;
        private string onUserDisconnected;
        private string onHackedData = "kick (_this select 0)";
        private string onDifferentData;
        private string onUnsignedData = "kick (_this select 0)";
        private string onUserKicked;

        private bool                 missionSelectorChecked;
        private string               missionContentOverride;
        private List<ProfileMission> _missions = new();
        private bool                 autoInit;
        private string               difficulty = "Custom";

        private bool   maxMemOverride;
        private uint   maxMem = 1024;
        private bool   cpuCountOverride;
        private ushort cpuCount;
        private string commandLineParams;

        private string serverCfgContent;



        #region Server Options
        public string PasswordAdmin
        {
            get => passwordAdmin;
            set
            {
                passwordAdmin = value;
                RaisePropertyChanged("PasswordAdmin");
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string ServerCommandPassword
        {
            get => serverCommandPassword;
            set
            {
                serverCommandPassword = value;
                RaisePropertyChanged("ServerCommandPassword");
            }
        }

        public string Hostname
        {
            get => hostname;
            set
            {
                hostname = value;
                RaisePropertyChanged("Hostname");
            }
        }

        public int MaxPlayers
        {
            get => maxPlayers;
            set
            {
                maxPlayers = value;
                RaisePropertyChanged("MaxPlayers");
            }
        }

        public string Motd
        {
            get => string.Join("\n", motd);
            set
            {
                motd = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged("Motd");
            }
        }

        public int MotdInterval
        {
            get => motdInterval;
            set
            {
                motdInterval = value;
                RaisePropertyChanged("MotdInterval");
            }
        }

        public string Admins
        {
            get => string.Join("\n", admins);
            set
            {
                admins = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged("Admins");
            }
        }

        public string HeadlessClients
        {
            get => string.Join("\n", headlessClients);
            set
            {
                headlessClients = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged("HeadlessClients");
            }
        }

        public string LocalClient
        {
            get => string.Join("\n", localClient);
            set
            {
                localClient = value.Split('\n').ToList();
                RaisePropertyChanged("LocalClient");
            }
        }

        public bool HeadlessClientEnabled
        {
            get => headlessClientEnabled;
            set
            {
                headlessClientEnabled = value;
                RaisePropertyChanged("HeadlessClientEnabled");
                if (value && !headlessClients.Any(e => e.Length > 0))
                { HeadlessClients = "127.0.0.1"; }
            }
        }

        public bool VotingEnabled
        {
            get => votingEnabled;
            set
            {
                votingEnabled = value;
                RaisePropertyChanged("VotingEnabled");
            }
        }

        public bool NetLogEnabled
        {
            get => netlogEnabled;
            set
            {
                netlogEnabled = value;
                RaisePropertyChanged("NetLogEnabled");
            }
        }
        #endregion

        #region Server behavior
        public double VoteThreshold
        {
            get => voteThreshold;
            set
            {
                voteThreshold = value;
                RaisePropertyChanged("VoteThreshold");
            }
        }

        public int VoteMissionPlayers
        {
            get => voteMissionPlayers;
            set
            {
                voteMissionPlayers = value;
                RaisePropertyChanged("VoteMissionPlayers");
            }
        }

        public bool KickDuplicates
        {
            get => kickduplicate == 1;
            set
            {
                kickduplicate = value ? (short)1 : (short)0;
                RaisePropertyChanged("KickDuplicates");
            }
        }

        public bool Loopback
        {
            get => loopback;
            set
            {
                loopback = value;
                RaisePropertyChanged("Loopback");
            }
        }

        public bool Upnp
        {
            get => upnp;
            set
            {
                upnp = value;
                RaisePropertyChanged("Upnp");
            }
        }

        public string AllowedFilePatching
        {
            get => ServerCfgArrays.AllowFilePatchingStrings[allowedFilePatching];
            set
            {
                allowedFilePatching = (short)Array.IndexOf(ServerCfgArrays.AllowFilePatchingStrings, value);
                RaisePropertyChanged("Password");
            }
        }

        public int DisconnectTimeout
        {
            get => disconnectTimeout;
            set
            {
                disconnectTimeout = value;
                RaisePropertyChanged("DisconnectTimeout");
            }
        }

        public int MaxDesync
        {
            get => maxdesync;
            set
            {
                maxdesync = value;
                RaisePropertyChanged("MaxDesync");
            }
        }

        public int MaxPing
        {
            get => maxping;
            set
            {
                maxping = value;
                RaisePropertyChanged("MaxPing");
            }
        }

        public int MaxPacketLoss
        {
            get => maxpacketloss;
            set
            {
                maxpacketloss = value;
                RaisePropertyChanged("MaxPacketLoss");
            }
        }

        public bool KickClientOnSlowNetwork
        {
            get => kickClientOnSlowNetwork;
            set
            {
                kickClientOnSlowNetwork = value;
                RaisePropertyChanged("KickClientOnSlowNetwork");
            }
        }

        public int LobbyIdleTimeout
        {
            get => lobbyIdleTimeout;
            set
            {
                lobbyIdleTimeout = value;
                RaisePropertyChanged("LobbyIdleTimeout");
            }
        }

        public int BriefingTimeOut
        {
            get => briefingTimeOut;
            set
            {
                briefingTimeOut = value;
                RaisePropertyChanged("BriefingTimeOut");
            }
        }

        public int RoleTimeOut
        {
            get => roleTimeOut;
            set
            {
                roleTimeOut = value;
                RaisePropertyChanged("RoleTimeOut");
            }
        }

        public int VotingTimeOut
        {
            get => votingTimeOut;
            set
            {
                votingTimeOut = value;
                RaisePropertyChanged("VotingTimeOut");
            }
        }

        public int DebriefingTimeOut
        {
            get => debriefingTimeOut;
            set
            {
                debriefingTimeOut = value;
                RaisePropertyChanged("DebriefingTimeOut");
            }
        }

        public bool logObjectNotFound
        {
            get => LogObjectNotFound;
            set
            {
                LogObjectNotFound = value;
                RaisePropertyChanged("logObjectNotFound");
            }
        }

        public bool skipDescriptionParsing
        {
            get => SkipDescriptionParsing;
            set
            {
                SkipDescriptionParsing = value;
                RaisePropertyChanged("skipDescriptionParsing");
            }
        }

        public bool IgnoreMissionLoadErrors
        {
            get => ignoreMissionLoadErrors;
            set
            {
                ignoreMissionLoadErrors = value;
                RaisePropertyChanged("IgnoreMissionLoadErrors");
            }
        }

        public int ArmaUnitsTimeout
        {
            get => armaUnitsTimeout;
            set
            {
                armaUnitsTimeout = value;
                RaisePropertyChanged("ArmaUnitsTimeout");
            }
        }

        public bool AutoSelectMission
        {
            get => autoSelectMission;
            set
            {
                autoSelectMission = value;
                RaisePropertyChanged("AutoSelectMission");
            }
        }

        public bool RandomMissionOrder
        {
            get => randomMissionOrder;
            set
            {
                randomMissionOrder = value;
                RaisePropertyChanged("RandomMissionOrder");
            }
        }
        #endregion

        #region Arma Server Only
        public string VerifySignatures
        {
            get => ServerCfgArrays.VerifySignaturesStrings[verifySignatures];
            set
            {
                verifySignatures = (short)Array.IndexOf(ServerCfgArrays.VerifySignaturesStrings, value);
                RaisePropertyChanged("VerifySignatures");
            }
        }

        public bool DrawingInMap
        {
            get => drawingInMap;
            set
            {
                drawingInMap = value;
                RaisePropertyChanged("DrawinginMap");
            }
        }

        public bool VonActivated
        {
            get => disableVoN == 0;
            set
            {
                disableVoN = value ? (short)0 : (short)1;
                RaisePropertyChanged("VonActivated");
            }
        }

        public int VonCodecQuality
        {
            get => vonCodecQuality;
            set
            {
                vonCodecQuality = value;
                RaisePropertyChanged("VonCodecQuality");
            }
        }

        public string VonCodec
        {
            get => ServerCfgArrays.VonCodecStrings[vonCodec];
            set
            {
                vonCodec = (short)Array.IndexOf(ServerCfgArrays.VonCodecStrings, value);
                RaisePropertyChanged("VonCodec");
            }
        }

        public bool SkipLobby
        {
            get => skipLobby;
            set
            {
                skipLobby = value;
                RaisePropertyChanged("SkipLobby");
            }
        }

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                RaisePropertyChanged("LogFile");
            }
        }

        public bool BattlEye
        {
            get => battlEye == 1;
            set
            {
                battlEye = value ? (short)1 : (short)0;
                RaisePropertyChanged("BattlEye");
            }
        }

        public string TimeStampFormat
        {
            get => timeStampFormat;
            set
            {
                timeStampFormat = value;
                RaisePropertyChanged("TimeStampFormat");
            }
        }

        public bool Persistent
        {
            get => persistent == 1;
            set
            {
                persistent = value ? (short)1 : (short)0;
                if (!value)
                    AutoInit = false;
                RaisePropertyChanged("Persistent");
            }
        }

        public bool RequiredBuildChecked
        {
            get => requiredBuildChecked;
            set
            {
                requiredBuildChecked = value;
                RaisePropertyChanged("RequiredBuildChecked");
            }
        }

        public int RequiredBuild
        {
            get => requiredBuild;
            set
            {
                requiredBuild = value;
                RaisePropertyChanged("RequiredBuild");
            }
        }

        public int SteamProtocolMaxDataSize
        {
            get => steamProtocolMaxDataSize;
            set
            {
                steamProtocolMaxDataSize = value;
                RaisePropertyChanged("SteamProtocolMaxDataSize");
            }
        }
        #endregion

        #region Scripting

        public string DoubleIdDetected
        {
            get => doubleIdDetected;
            set
            {
                doubleIdDetected = value;
                RaisePropertyChanged("DoubleIdDetected");
            }
        }

        public string OnUserConnected
        {
            get => onUserConnected;
            set
            {
                onUserConnected = value;
                RaisePropertyChanged("OnUserConnected");
            }
        }

        public string OnUserDisconnected
        {
            get => onUserDisconnected;
            set
            {
                onUserDisconnected = value;
                RaisePropertyChanged("OnUserDisconnected");
            }
        }

        public string OnHackedData
        {
            get => onHackedData;
            set
            {
                onHackedData = value;
                RaisePropertyChanged("OnHackedData");
            }
        }

        public string OnDifferentData
        {
            get => onDifferentData;
            set
            {
                onDifferentData = value;
                RaisePropertyChanged("OnDifferentData");
            }
        }

        public string OnUnsignedData
        {
            get => onUnsignedData;
            set
            {
                onUnsignedData = value;
                RaisePropertyChanged("OnUnsignedData");
            }
        }

        public string OnUserKicked
        {
            get => onUserKicked;
            set
            {
                onUserKicked = value;
                RaisePropertyChanged("OnUserKicked");
            }
        }
        #endregion

        #region Mission
        public bool MissionChecked
        {
            get => missionSelectorChecked;
            set
            {
                missionSelectorChecked = value;
                RaisePropertyChanged("MissionChecked");
            }
        }

        public string MissionContentOverride
        {
            get => missionContentOverride;
            set
            {
                missionContentOverride = value;
                RaisePropertyChanged("MissionContentOverride");
            }
        }

        public bool AutoInit
        {
            get => autoInit;
            set
            {
                autoInit = value;
                RaisePropertyChanged("AutoInit");
            }
        }

        public string Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = value;
                RaisePropertyChanged("Difficulty");
            }
        }

        public List<ProfileMission> Missions
        {
            get => _missions;
            set
            {
                //Removing previous triggers
                _missions.ForEach(m => m.PropertyChanged -= Item_PropertyChanged);

                bool isEqual = _missions.Count == value.Count
                            && !( from mission in value
                                  let local = _missions.FirstOrDefault(m => m.Path == mission.Path)
                                  where local == null || local.MissionChecked != mission.MissionChecked
                                  select mission ).Any();

                if (!isEqual)
                {
                    _missions = value;
                    RaisePropertyChanged("Missions");
                }

                //Adding the trigger to count checked mods
                _missions.ForEach(m => m.PropertyChanged += Item_PropertyChanged);
            }
        }
        #endregion

        #region Performances
        public bool MaxMemOverride
        {
            get => maxMemOverride;
            set
            {
                maxMemOverride = value;
                RaisePropertyChanged("MaxMemOverride");
            }
        }

        public bool CpuCountOverride
        {
            get => cpuCountOverride;
            set
            {
                cpuCountOverride = value;
                RaisePropertyChanged("CpuCountOverride");
            }
        }

        public uint MaxMem
        {
            get => maxMem;
            set
            {
                maxMem = value;
                RaisePropertyChanged("MaxMem");
            }
        }

        public ushort CpuCount
        {
            get => cpuCount;
            set
            {
                cpuCount = value;
                RaisePropertyChanged("CpuCount");
            }
        }

        public string CommandLineParameters
        {
            get => commandLineParams;
            set
            {
                commandLineParams = value;
                RaisePropertyChanged("CommandLineParameters");
            }
        }

        #endregion

        public string ServerCfgContent
        {
            get => serverCfgContent;
            set
            {
                serverCfgContent = value;
                RaisePropertyChanged("ServerCfgContent");
            }
        }

        public ServerCfg()
        {
            if(string.IsNullOrWhiteSpace(serverCfgContent))
            { ServerCfgContent = ProcessFile(); }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("MissionChecked");
            RaisePropertyChanged("MissionContentOverride");
        }

        public string ProcessFile()
        {
            if (!missionSelectorChecked)
            {
                List<string> lines = new() { "class Missions {" };
                foreach (var mission in Missions.Where(m => m.MissionChecked).Select(m => m.Name))
                {
                    lines.AddRange(new List<string>
                    {
                        $"\tclass Mission_{Functions.SafeName(mission)} {{",
                        $"\t\ttemplate = \"{mission}\";",
                        $"\t\tdifficulty = \"{Difficulty}\";",
                        "\t};"
                    });
                }
                lines.Add("};");

                var compiledMission = string.Join("\r\n", lines);
                if(missionContentOverride?.Length != compiledMission.Length)
                { MissionContentOverride = compiledMission; }
            }

            string output = "//\r\n"
                          + "// server.cfg\r\n"
                          + "//\r\n"
                          + "// comments are written with \"//\" in front of them.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// GLOBAL SETTINGS\r\n"
                          + $"hostname = \"{hostname}\";\t\t// The name of the server that shall be displayed in the public server list\r\n"
                          + $"password = \"{password}\";\t\t\t\t\t// Password for joining, eg connecting to the server\r\n"
                          + $"passwordAdmin = \"{passwordAdmin}\";\t\t\t\t// Password to become server admin. When you're in Arma MP and connected to the server, type '#login xyz'\r\n"
                          + $"serverCommandPassword = \"{serverCommandPassword}\";               // Password required by alternate syntax of [[serverCommand]] server-side scripting.\r\n"
                          + $"logFile = \"{logFile}\";\t\t\t// Tells ArmA-server where the logfile should go and what it should be called\r\n"
                          + $"admins[] =  { "{\n\t\"" + string.Join("\",\n\t\"", admins) + "\"\n}" };\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// WELCOME MESSAGE\r\n"
                          + $"motd[] = { "{\n\t\"" + string.Join("\",\n\t \"", motd) + "\"\n}" };\r\n"
                          + $"motdInterval = {motdInterval};\t\t\t\t// Time interval (in seconds) between each message\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// JOINING RULES\r\n"
                          + $"maxPlayers = {maxPlayers};\t\t\t\t// Maximum amount of players. Civilians and watchers, beholder, bystanders and so on also count as player.\r\n"
                          + $"kickDuplicate = {kickduplicate};\t\t\t\t// Each ArmA version has its own ID. If kickDuplicate is set to 1, a player will be kicked when he joins a server where another player with the same ID is playing.\r\n"
                          + $"verifySignatures = {verifySignatures};\t\t\t\t// Verifies .pbos against .bisign files. Valid values 0 (disabled), 1 (prefer v2 sigs but accept v1 too) and 2 (only v2 sigs are allowed). \r\n"
                          + $"allowedFilePatching = {allowedFilePatching};\t\t\t\t// Allow or prevent client using -filePatching to join the server. 0, is disallow, 1 is allow HC, 2 is allow all clients (since Arma 3 1.49+)\r\n"
                          + $"{(requiredBuildChecked ? $"requiredBuild = {requiredBuild}\t\t\t\t// Require clients joining to have at least build 12345 of game, preventing obsolete clients to connect\r\n" : "\r\n")}"
                          + $"steamProtocolMaxDataSize = {steamProtocolMaxDataSize};\t\t\t\t// Increasing this value will fix the modlist length limit in Arma 3 Launcher but mignt not be supported by some routers.\r\n"
                          + $"loopback = {(loopback ? "1" : "0")};\t\t\t\t// Enforces LAN only mode.\r\n"
                          + $"upnp = {(upnp ? "1" : "0")};\t\t\t\t// This setting might slow up server start-up by 600s if blocked by firewall or router.\r\n"
                          + "\r\n"
                          + "// VOTING\r\n"
                          + $"{(votingEnabled ? $"voteMissionPlayers = {voteMissionPlayers};" : "voteMissionPlayers = 1;")}\t\t\t\t// Tells the server how many people must connect so that it displays the mission selection screen.\r\n"
                          + $"{(votingEnabled ? $"voteThreshold = {voteThreshold.ToString(CultureInfo.InvariantCulture)};" : "voteThreshold = 0;")}\t\t\t\t// 33% or more players need to vote for something, for example an admin or a new map, to become effective\r\n"
                          + $"{(votingEnabled ? "" : "allowedVoteCmds[] = {};")}\r\n"
                          + $"{(votingEnabled ? "" : "allowedVotedAdminCmds[] = {};")}\r\n"
                          + $"briefingTimeOut = {briefingTimeOut}; // The amount of time a player can sit in briefing mode before being kicked.\r\n"
                          + $"roleTimeOut = {roleTimeOut}; // The amount of time a player can sit in role selection before being kicked.\r\n"
                          + $"votingTimeOut = {votingTimeOut}; // The amount of time a vote will last before ending.\r\n"
                          + $"debriefingTimeOut = {debriefingTimeOut}; // // The amount of time a player can sit in breifing mode before being kicked.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// INGAME SETTINGS\r\n"
                          + $"disableVoN = {disableVoN};\t\t\t\t\t// If set to 1, Voice over Net will not be available\r\n"
                          + $"vonCodec = {vonCodec}; \t\t\t\t\t// If set to 1 then it uses IETF standard OPUS codec, if to 0 then it uses SPEEX codec (since Arma 3 update 1.58+)  \r\n"
                          + $"skipLobby = {(skipLobby ? "1" : "0")};\t\t\t\t// Overridden by mission parameters\r\n"
                          + $"vonCodecQuality = {vonCodecQuality};\t\t\t\t// since 1.62.95417 supports range 1-20 //since 1.63.x will supports range 1-30 //8kHz is 0-10, 16kHz is 11-20, 32kHz(48kHz) is 21-30 \r\n"
                          + $"persistent = {persistent};\t\t\t\t\t// If 1, missions still run on even after the last player disconnected.\r\n"
                          + $"timeStampFormat = \"{timeStampFormat}\";\t\t\t// Set the timestamp format used on each report line in server-side RPT file. Possible values are \"none\" (default),\"short\",\"full\".\r\n"
                          + $"BattlEye = {battlEye};\t\t\t\t\t// Server to use BattlEye system\r\n"
                          + $"LogObjectNotFound = {logObjectNotFound};\t\t\t\t\t // When false to skip logging 'Server: Object not found messages'.\r\n"
                          + $"SkipDescriptionParsing = {skipDescriptionParsing};\t\t\t\t\t // When true to skip parsing of description.ext/mission.sqm. Will show pbo filename instead of configured missionName. OverviewText and such won't work, but loading the mission list is a lot faster when there are many missions \r\n"
                          + $"ignoreMissionLoadErrors = {ignoreMissionLoadErrors};\t\t\t\t\t // When set to true, the mission will load no matter the amount of loading errors. If set to false, the server will abort mission's loading and return to mission selection.\r\n"
                          + "\r\n"
                          + "// TIMEOUTS\r\n"
                          + $"disconnectTimeout = {disconnectTimeout}; // Time to wait before disconnecting a user which temporarly lost connection. Range is 5 to 90 seconds.\r\n"
                          + $"maxDesync = {maxdesync}; // Max desync value until server kick the user\r\n"
                          + $"maxPing= {maxping}; // Max ping value until server kick the user\r\n"
                          + $"maxPacketLoss= {maxpacketloss}; // Max packetloss value until server kick the user\r\n"
                          + $"kickClientsOnSlowNetwork[] = {( kickClientOnSlowNetwork ? "{ 1, 1, 1, 1 }" : "{ 0, 0, 0, 0 }")}; //Defines if {{<MaxPing>, <MaxPacketLoss>, <MaxDesync>, <DisconnectTimeout>}} will be logged (0) or kicked (1)\r\n"
                          + $"lobbyIdleTimeout = {lobbyIdleTimeout}; // The amount of time the server will wait before force-starting a mission without a logged-in Admin.\r\n"
                          + $"armaUnitsTimeout = {armaUnitsTimeout}; // Defines how long the player will be stuck connecting and wait for armaUnits data. Player will be notified if timeout elapsed and no units data was received.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// SCRIPTING ISSUES\r\n"
                          + $"onUserConnected = \"{onUserConnected}\";\t\t\t\t//\r\n"
                          + $"onUserDisconnected = \"{onUserDisconnected}\";\t\t\t//\r\n"
                          + $"doubleIdDetected = \"{doubleIdDetected}\";\t\t\t\t//\r\n"
                          + "\r\n"
                          + "// SIGNATURE VERIFICATION\r\n"
                          + $"onUnsignedData = \"{onUnsignedData}\";\t// unsigned data detected\r\n"
                          + $"onHackedData = \"{onHackedData}\";\t\t// tampering of the signature detected\r\n"
                          + $"onDifferentData = \"{onDifferentData}\";\t\t\t\t// data with a valid signature, but different version than the one present on server detected\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// MISSIONS CYCLE (see below)\r\n"
                          + $"randomMissionOrder = {randomMissionOrder}; // Randomly iterate through Missions list\r\n"
                          + $"autoSelectMission = {autoSelectMission}; // Server auto selects next mission in cycle\r\n"
                          + "\r\n"
                          + $"{MissionContentOverride}\t\t\t\t// An empty Missions class means there will be no mission rotation\r\n"
                          + "\r\n"
                          + "missionWhitelist[] = {};\r\n"
                          + "// An empty whitelist means there is no restriction on what missions available"
                          + "\r\n"
                          + "\r\n"
                          + "// HEADLESS CLIENT\r\n"
                          + $"{(headlessClientEnabled && !headlessClients.Any(string.IsNullOrWhiteSpace) ? $"headlessClients[] =  { "{\n\t\"" + string.Join("\",\n\t \"", headlessClients) + "\"\n}" };\r\n" : "")}"
                          + $"{(headlessClientEnabled && !localClient.Any(string.IsNullOrWhiteSpace)? $"localClient[] =  { "{\n\t\"" + string.Join("\",\n\t \"", localClient) + "\"\n}" };" : "")}";
            return output;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
            if(property != "ServerCfgContent") ServerCfgContent = ProcessFile();
        }
    }

    [Serializable]
    public class ProfileMission : INotifyPropertyChanged
    {
        private bool missionChecked;
        private string name;
        private string path;

        public bool MissionChecked
        {
            get => missionChecked;
            set
            {
                missionChecked = value;
                RaisePropertyChanged("MissionChecked");
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

        public string Path
        {
            get => path;
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
