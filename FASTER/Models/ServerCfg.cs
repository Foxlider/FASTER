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
        public static string[] TimeStampStrings { get; } = { "none", "short", "full" };
        public static string[] ZeusScriptLevelStrings { get; } = { "No scripts", "Attributes only", "All scripts" };
        public static string[] RotorLibStrings { get; } = { "Player choice", "Force AFM", "Force SFM" };
        public static string[] HazeQualityStrings { get; } = { "Don't force", "Very Low", "Low", "Standard" };
        public static short[] HazeQualityValues { get; } = { -1, 0, 1, 2 };
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
        private double voteThreshold            = 0.33;
        private int    voteMissionPlayers       = 3;
        private short  kickduplicate            = 1;        // 1 = active ; 0=disabled
        private bool   loopback;                            // Adding this option will force server into LAN mode
        private bool   upnp                     = false;    // False by default due to 600s slow down if not set-up right. Thus in such case is recommended to disable it.
        private short  allowedFilePatching;                 // 0 = no clients; 1= HC only; 2= All Clients
        private int    disconnectTimeout        = 90;       // Server wait time before disconnecting client after loss of active traffic connection, range 5 to 90 seconds.
        private int    maxdesync                = 150;      // Max desync value until server kick the user
        private int    maxping                  = 200;      // Max ping value until server kick the user
        private int    maxpacketloss            = 50;       // Max packetloss value until server kick the user
        private bool   kickClientOnSlowNetwork;
        private int    lobbyIdleTimeout         = 300;
        private bool   autoSelectMission        = true;
        private bool   randomMissionOrder       = true;
        private int    briefingTimeOut          = 60; 	     // <-
        private int    roleTimeOut              = 90; 	 	 // <- These are BI base figues
        private int    votingTimeOut            = 60; 	 	 // <-
        private int    debriefingTimeOut        = 45;        // <-
        private bool   _logObjectNotFound       = true;      // logging enabled
        private bool   _skipDescriptionParsing  = false;     // parse description.ext
        private bool   ignoreMissionLoadErrors  = false;     // do not ingore errors
        private int    armaUnitsTimeout         = 30; 	     // Defines how long the player will be stuck connecting and wait for armaUnits data. Player will be notified if timeout elapsed and no units data was received
        private int    queueSizeLogG            = 1000000; 	 // if a specific players message queue is larger than 1MB and '#monitor' is running, dump his messages to a logfile for analysis
        private string forcedDifficulty         = "Custom";  // By default forcedDifficulty is only applying Custom
        private short  missionsEndAction;                    // 0 = nothing ; 1 = missionsToServerRestart ; 2 = missionsToShutdown (both can't be combined)
        private int    missionsEndCount         = 8;
        private int    kickTimeoutManual        = 60;        // <- kickTimeout[] in seconds, -1 = until mission end, -2 = until server restart
        private int    kickTimeoutConnectivity  = 60;        // <-
        private int    kickTimeoutBattlEye      = 60;        // <- These are BI base figures
        private int    kickTimeoutHarmless      = 60;        // <-
        private int    idleFPSLimit             = 30;        // FPS limit of a server without players, range 5-60
        private VoteCommand[] voteCommands       = VoteCommand.CreateDefaults(VoteCommand.VoteCommandNames);
        private VoteCommand[] votedAdminCommands = VoteCommand.CreateDefaults(VoteCommand.VotedAdminCommandNames);
        private ChannelRestriction[] disableChannels = ChannelRestriction.CreateDefaults();


        //Arma server only
        private short  verifySignatures         = 0;         // 0 = Disabled (FASTER Default); 1 = Deprecated Activated ; 2 = Activated (Arma Default)
        private bool   drawingInMap             = true;
        private short  disableVoN;                           // 0 = VoN activated ; 1 = VoN Disabled
        private int    vonCodecQuality          = 3;         // 8kHz is 0-10, 16kHz is 11-20, 32kHz is 21-30 (and 48kHz with OPUS enabled)
        private short  vonCodec;                             // 0 = SPEEX ; 1 = OPUS
        private bool   skipLobby;                            //Overritten by mission parameters
        private string logFile                  = "server_console.log";
        private short  battlEye                 = 1;         // 0 = Disabled ; 1 = Enabled
        private string timeStampFormat          = "short";   // Possible values = "none", "short", "full"
        private string timeStampFormatConsole   = "short";   // Same values, for the server console (Arma 3 2.22+)
        private bool   statisticsEnabled        = true;      // BI analytics, false to opt out
        private bool   allowProfileGlasses      = true;      // Only used if the mission doesn't define it
        private short  zeusCompositionScriptLevel = 1;       // 0 = no scripts ; 1 = attributes only ; 2 = all scripts. Only used if the mission doesn't define it
        private short  forceRotorLibSimulation;              // 0 = player choice ; 1 = forced AFM ; 2 = forced SFM
        private short  overrideHazeQuality      = -1;        // -1 = don't force ; 0 = very low ; 1 = low ; 2 = standard
        private short  persistent;
        private bool   requiredBuildChecked;
        private int    requiredBuild            = 999999999; // Minimum required client version. Clients with version lower than requiredBuild will not be able to connect
        private int    steamProtocolMaxDataSize = 10000;     // BI Default value is 1024. Increasing this value is dangerous for older routers as it will cause UDP packets to be fragmented. Though increasing this value can help with modulier length limit in a3 launcher.

        //Security
        private static readonly string[] RecommendedLoadExtensions = { "hpp", "sqs", "sqf", "fsm", "cpp", "paa", "txt", "xml", "inc", "ext", "sqm", "ods", "fxy", "lip", "csv", "kb", "bik", "bikb", "html", "htm", "biedi" };
        private List<string> allowedLoadFileExtensions       = RecommendedLoadExtensions.ToList();
        private List<string> allowedPreprocessFileExtensions = RecommendedLoadExtensions.ToList();
        private List<string> allowedHTMLLoadExtensions       = new() { "htm", "html", "xml", "txt" };
        private List<string> allowedHTMLLoadURIs             = new();
        private List<string> filePatchingExceptions          = new();

        //Scripting
        private string serverCommandPassword;
        private string doubleIdDetected;
        private string onUserConnected;
        private string onUserDisconnected;
        private string onHackedData = "kick (_this select 0)";
        private string onDifferentData;
        private string onUnsignedData = "kick (_this select 0)";
        private string onUserKicked;
        private string regularCheck;
        private int    callExtReportLimit = 1000;
        private bool   enablePlayerDiag;

        private bool                 missionSelectorChecked;
        private string               missionContentOverride;
        private List<ProfileMission> _missions = new();
        private bool                 autoInit;
        private string               difficulty = "Custom";
        private string               _missionHTTPDownloadBaseURL = "";

        // AntiFlood (Arma 2.18+)
        private bool _antiFloodEnabled        = false;
        private double _antiFloodCycleTime    = 0.5;
        private int  _antiFloodCycleLimit     = 400;
        private int  _antiFloodCycleHardLimit = 4000;
        private int  _antiFloodEnableKick     = 0;  // 0 = disabled, 1 = enabled

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
                RaisePropertyChanged(nameof(PasswordAdmin));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }

        public string ServerCommandPassword
        {
            get => serverCommandPassword;
            set
            {
                serverCommandPassword = value;
                RaisePropertyChanged(nameof(ServerCommandPassword));
            }
        }

        public string Hostname
        {
            get => hostname;
            set
            {
                hostname = value;
                RaisePropertyChanged(nameof(Hostname));
            }
        }

        public int MaxPlayers
        {
            get => maxPlayers;
            set
            {
                maxPlayers = value;
                RaisePropertyChanged(nameof(MaxPlayers));
            }
        }

        public string Motd
        {
            get => string.Join("\n", motd);
            set
            {
                motd = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(Motd));
            }
        }

        public int MotdInterval
        {
            get => motdInterval;
            set
            {
                motdInterval = value;
                RaisePropertyChanged(nameof(MotdInterval));
            }
        }

        public string Admins
        {
            get => string.Join("\n", admins);
            set
            {
                admins = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(Admins));
            }
        }

        public string HeadlessClients
        {
            get => string.Join("\n", headlessClients);
            set
            {
                headlessClients = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(HeadlessClients));
            }
        }

        public string LocalClient
        {
            get => string.Join("\n", localClient);
            set
            {
                localClient = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(LocalClient));
            }
        }

        public bool HeadlessClientEnabled
        {
            get => headlessClientEnabled;
            set
            {
                headlessClientEnabled = value;
                RaisePropertyChanged(nameof(HeadlessClientEnabled));
                if (value && !headlessClients.Exists(e => e.Length > 0))
                { HeadlessClients = "127.0.0.1"; }
            }
        }

        public bool VotingEnabled
        {
            get => votingEnabled;
            set
            {
                votingEnabled = value;
                RaisePropertyChanged(nameof(VotingEnabled));
            }
        }

        public bool NetLogEnabled
        {
            get => netlogEnabled;
            set
            {
                netlogEnabled = value;
                RaisePropertyChanged(nameof(NetLogEnabled));
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
                RaisePropertyChanged(nameof(VoteThreshold));
            }
        }

        public int VoteMissionPlayers
        {
            get => voteMissionPlayers;
            set
            {
                voteMissionPlayers = value;
                RaisePropertyChanged(nameof(VoteMissionPlayers));
            }
        }

        public bool KickDuplicates
        {
            get => kickduplicate == 1;
            set
            {
                kickduplicate = value ? (short)1 : (short)0;
                RaisePropertyChanged(nameof(KickDuplicates));
            }
        }

        public bool Loopback
        {
            get => loopback;
            set
            {
                loopback = value;
                RaisePropertyChanged(nameof(Loopback));
            }
        }

        public bool Upnp
        {
            get => upnp;
            set
            {
                upnp = value;
                RaisePropertyChanged(nameof(Upnp));
            }
        }

        public string AllowedFilePatching
        {
            get => ServerCfgArrays.AllowFilePatchingStrings[allowedFilePatching];
            set
            {
                allowedFilePatching = (short)Array.IndexOf(ServerCfgArrays.AllowFilePatchingStrings, value);
                RaisePropertyChanged(nameof(AllowedFilePatching));
            }
        }

        public int DisconnectTimeout
        {
            get => disconnectTimeout;
            set
            {
                disconnectTimeout = value;
                RaisePropertyChanged(nameof(DisconnectTimeout));
            }
        }

        public int MaxDesync
        {
            get => maxdesync;
            set
            {
                maxdesync = value;
                RaisePropertyChanged(nameof(MaxDesync));
            }
        }

        public int MaxPing
        {
            get => maxping;
            set
            {
                maxping = value;
                RaisePropertyChanged(nameof(MaxPing));
            }
        }

        public int MaxPacketLoss
        {
            get => maxpacketloss;
            set
            {
                maxpacketloss = value;
                RaisePropertyChanged(nameof(MaxPacketLoss));
            }
        }

        public bool KickClientOnSlowNetwork
        {
            get => kickClientOnSlowNetwork;
            set
            {
                kickClientOnSlowNetwork = value;
                RaisePropertyChanged(nameof(KickClientOnSlowNetwork));
            }
        }

        public int LobbyIdleTimeout
        {
            get => lobbyIdleTimeout;
            set
            {
                lobbyIdleTimeout = value;
                RaisePropertyChanged(nameof(LobbyIdleTimeout));
            }
        }

        public int BriefingTimeOut
        {
            get => briefingTimeOut;
            set
            {
                briefingTimeOut = value;
                RaisePropertyChanged(nameof(BriefingTimeOut));
            }
        }

        public int RoleTimeOut
        {
            get => roleTimeOut;
            set
            {
                roleTimeOut = value;
                RaisePropertyChanged(nameof(RoleTimeOut));
            }
        }

        public int VotingTimeOut
        {
            get => votingTimeOut;
            set
            {
                votingTimeOut = value;
                RaisePropertyChanged(nameof(VotingTimeOut));
            }
        }

        public int DebriefingTimeOut
        {
            get => debriefingTimeOut;
            set
            {
                debriefingTimeOut = value;
                RaisePropertyChanged(nameof(DebriefingTimeOut));
            }
        }

        public bool logObjectNotFound
        {
            get => _logObjectNotFound;
            set
            {
                _logObjectNotFound = value;
                RaisePropertyChanged(nameof(logObjectNotFound));
            }
        }

        public bool skipDescriptionParsing
        {
            get => _skipDescriptionParsing;
            set
            {
                _skipDescriptionParsing = value;
                RaisePropertyChanged(nameof(skipDescriptionParsing));
            }
        }

        public bool IgnoreMissionLoadErrors
        {
            get => ignoreMissionLoadErrors;
            set
            {
                ignoreMissionLoadErrors = value;
                RaisePropertyChanged(nameof(IgnoreMissionLoadErrors));
            }
        }

        public int ArmaUnitsTimeout
        {
            get => armaUnitsTimeout;
            set
            {
                armaUnitsTimeout = value;
                RaisePropertyChanged(nameof(ArmaUnitsTimeout));
            }
        }

		 public int QueueSizeLogG
        {
            get => queueSizeLogG;
            set
            {
                queueSizeLogG = value;
                RaisePropertyChanged(nameof(QueueSizeLogG));
            }
        }

		public string ForcedDifficulty
        {
            get => forcedDifficulty;
            set
            {
                forcedDifficulty = value;
                RaisePropertyChanged(nameof(ForcedDifficulty));
            }
        }

        public bool AutoSelectMission
        {
            get => autoSelectMission;
            set
            {
                autoSelectMission = value;
                RaisePropertyChanged(nameof(AutoSelectMission));
            }
        }

        public bool RandomMissionOrder
        {
            get => randomMissionOrder;
            set
            {
                randomMissionOrder = value;
                RaisePropertyChanged(nameof(RandomMissionOrder));
            }
        }

        public string MissionHTTPDownloadBaseURL
        {
            get => _missionHTTPDownloadBaseURL;
            set
            {
                _missionHTTPDownloadBaseURL = value;
                RaisePropertyChanged(nameof(MissionHTTPDownloadBaseURL));
            }
        }

        public bool AntiFloodEnabled
        {
            get => _antiFloodEnabled;
            set
            {
                _antiFloodEnabled = value;
                RaisePropertyChanged(nameof(AntiFloodEnabled));
            }
        }

        public double AntiFloodCycleTime
        {
            get => _antiFloodCycleTime;
            set
            {
                _antiFloodCycleTime = value;
                RaisePropertyChanged(nameof(AntiFloodCycleTime));
            }
        }

        public int AntiFloodCycleLimit
        {
            get => _antiFloodCycleLimit;
            set
            {
                _antiFloodCycleLimit = value;
                RaisePropertyChanged(nameof(AntiFloodCycleLimit));
            }
        }

        public int AntiFloodCycleHardLimit
        {
            get => _antiFloodCycleHardLimit;
            set
            {
                _antiFloodCycleHardLimit = value;
                RaisePropertyChanged(nameof(AntiFloodCycleHardLimit));
            }
        }

        public bool AntiFloodEnableKick
        {
            get => _antiFloodEnableKick == 1;
            set
            {
                _antiFloodEnableKick = value ? 1 : 0;
                RaisePropertyChanged(nameof(AntiFloodEnableKick));
            }
        }
        #endregion

        public bool RestartAfterMissions
        {
            get => missionsEndAction == 1;
            set
            {
                if (value)
                { missionsEndAction = 1; }
                else if (missionsEndAction == 1)
                { missionsEndAction = 0; }
                RaiseMissionsEndActionChanged();
            }
        }

        public bool ShutdownAfterMissions
        {
            get => missionsEndAction == 2;
            set
            {
                if (value)
                { missionsEndAction = 2; }
                else if (missionsEndAction == 2)
                { missionsEndAction = 0; }
                RaiseMissionsEndActionChanged();
            }
        }

        public bool MissionsEndActionEnabled => missionsEndAction != 0;

        private void RaiseMissionsEndActionChanged()
        {
            RaisePropertyChanged(nameof(RestartAfterMissions));
            RaisePropertyChanged(nameof(ShutdownAfterMissions));
            RaisePropertyChanged(nameof(MissionsEndActionEnabled));
        }

        public int MissionsEndCount
        {
            get => missionsEndCount;
            set
            {
                missionsEndCount = value;
                RaisePropertyChanged(nameof(MissionsEndCount));
            }
        }


        public int KickTimeoutManual
        {
            get => kickTimeoutManual;
            set
            {
                kickTimeoutManual = value;
                RaisePropertyChanged(nameof(KickTimeoutManual));
            }
        }

        public int KickTimeoutConnectivity
        {
            get => kickTimeoutConnectivity;
            set
            {
                kickTimeoutConnectivity = value;
                RaisePropertyChanged(nameof(KickTimeoutConnectivity));
            }
        }

        public int KickTimeoutBattlEye
        {
            get => kickTimeoutBattlEye;
            set
            {
                kickTimeoutBattlEye = value;
                RaisePropertyChanged(nameof(KickTimeoutBattlEye));
            }
        }

        public int KickTimeoutHarmless
        {
            get => kickTimeoutHarmless;
            set
            {
                kickTimeoutHarmless = value;
                RaisePropertyChanged(nameof(KickTimeoutHarmless));
            }
        }

        public int IdleFPSLimit
        {
            get => idleFPSLimit;
            set
            {
                idleFPSLimit = value;
                RaisePropertyChanged(nameof(IdleFPSLimit));
            }
        }

        public VoteCommand[] VoteCommands
        {
            get => voteCommands;
            set
            {
                foreach (var c in voteCommands) c.PropertyChanged -= Vote_PropertyChanged;
                voteCommands = value ?? VoteCommand.CreateDefaults(VoteCommand.VoteCommandNames);
                foreach (var c in voteCommands) c.PropertyChanged += Vote_PropertyChanged;
                RaisePropertyChanged(nameof(VoteCommands));
            }
        }

        public VoteCommand[] VotedAdminCommands
        {
            get => votedAdminCommands;
            set
            {
                foreach (var c in votedAdminCommands) c.PropertyChanged -= Vote_PropertyChanged;
                votedAdminCommands = value ?? VoteCommand.CreateDefaults(VoteCommand.VotedAdminCommandNames);
                foreach (var c in votedAdminCommands) c.PropertyChanged += Vote_PropertyChanged;
                RaisePropertyChanged(nameof(VotedAdminCommands));
            }
        }

        private void Vote_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(VoteCommands));
        }

        public ChannelRestriction[] DisableChannels
        {
            get => disableChannels;
            set
            {
                foreach (var c in disableChannels) c.PropertyChanged -= Channel_PropertyChanged;
                disableChannels = value ?? ChannelRestriction.CreateDefaults();
                foreach (var c in disableChannels) c.PropertyChanged += Channel_PropertyChanged;
                RaisePropertyChanged(nameof(DisableChannels));
            }
        }

        private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(DisableChannels));
        }

        private static string FormatVoteCmds(string name, VoteCommand[] commands)
        {
            if (commands.All(c => c.IsDefault))
            { return ""; }
            return $"{name}[] = {{ {string.Join(", ", commands.Select(c => c.ToCfg()))} }};\r\n";
        }

        private string FormatDisableChannels()
        {
            var channels = disableChannels.Where(c => !c.IsDefault).Select(c => c.ToCfg()).ToList();
            if (channels.Count == 0)
            { return ""; }
            return $"disableChannels[] = {{ {string.Join(", ", channels)} }};\t// {{ channelID, text, voice, mapMarkers, drawOnMap }} - true disables it. Overridden by the mission's description.ext\r\n";
        }

        #region Arma Server Only
        public string VerifySignatures
        {
            get => ServerCfgArrays.VerifySignaturesStrings[verifySignatures];
            set
            {
                verifySignatures = (short)Array.IndexOf(ServerCfgArrays.VerifySignaturesStrings, value);
                RaisePropertyChanged(nameof(VerifySignatures));
            }
        }

        public bool DrawingInMap
        {
            get => drawingInMap;
            set
            {
                drawingInMap = value;
                RaisePropertyChanged(nameof(DrawingInMap));
            }
        }

        public bool VonActivated
        {
            get => disableVoN == 0;
            set
            {
                disableVoN = value ? (short)0 : (short)1;
                RaisePropertyChanged(nameof(VonActivated));
            }
        }

        public int VonCodecQuality
        {
            get => vonCodecQuality;
            set
            {
                vonCodecQuality = value;
                RaisePropertyChanged(nameof(VonCodecQuality));
            }
        }

        public string VonCodec
        {
            get => ServerCfgArrays.VonCodecStrings[vonCodec];
            set
            {
                vonCodec = (short)Array.IndexOf(ServerCfgArrays.VonCodecStrings, value);
                RaisePropertyChanged(nameof(VonCodec));
            }
        }

        public bool SkipLobby
        {
            get => skipLobby;
            set
            {
                skipLobby = value;
                RaisePropertyChanged(nameof(SkipLobby));
            }
        }

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                RaisePropertyChanged(nameof(LogFile));
            }
        }

        public bool BattlEye
        {
            get => battlEye == 1;
            set
            {
                battlEye = value ? (short)1 : (short)0;
                RaisePropertyChanged(nameof(BattlEye));
            }
        }

        public string TimeStampFormat
        {
            get => timeStampFormat;
            set
            {
                timeStampFormat = value;
                RaisePropertyChanged(nameof(TimeStampFormat));
            }
        }

        public string TimeStampFormatConsole
        {
            get => timeStampFormatConsole;
            set
            {
                timeStampFormatConsole = value;
                RaisePropertyChanged(nameof(TimeStampFormatConsole));
            }
        }

        public bool StatisticsEnabled
        {
            get => statisticsEnabled;
            set
            {
                statisticsEnabled = value;
                RaisePropertyChanged(nameof(StatisticsEnabled));
            }
        }

        public bool AllowProfileGlasses
        {
            get => allowProfileGlasses;
            set
            {
                allowProfileGlasses = value;
                RaisePropertyChanged(nameof(AllowProfileGlasses));
            }
        }

        public string ZeusCompositionScriptLevel
        {
            get => ServerCfgArrays.ZeusScriptLevelStrings[zeusCompositionScriptLevel];
            set
            {
                zeusCompositionScriptLevel = (short)Array.IndexOf(ServerCfgArrays.ZeusScriptLevelStrings, value);
                RaisePropertyChanged(nameof(ZeusCompositionScriptLevel));
            }
        }

        public string ForceRotorLibSimulation
        {
            get => ServerCfgArrays.RotorLibStrings[forceRotorLibSimulation];
            set
            {
                forceRotorLibSimulation = (short)Array.IndexOf(ServerCfgArrays.RotorLibStrings, value);
                RaisePropertyChanged(nameof(ForceRotorLibSimulation));
            }
        }

        public string OverrideHazeQuality
        {
            get => ServerCfgArrays.HazeQualityStrings[Array.IndexOf(ServerCfgArrays.HazeQualityValues, overrideHazeQuality)];
            set
            {
                overrideHazeQuality = ServerCfgArrays.HazeQualityValues[Array.IndexOf(ServerCfgArrays.HazeQualityStrings, value)];
                RaisePropertyChanged(nameof(OverrideHazeQuality));
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
                RaisePropertyChanged(nameof(Persistent));
            }
        }

        public bool RequiredBuildChecked
        {
            get => requiredBuildChecked;
            set
            {
                requiredBuildChecked = value;
                RaisePropertyChanged(nameof(RequiredBuildChecked));
            }
        }

        public int RequiredBuild
        {
            get => requiredBuild;
            set
            {
                requiredBuild = value;
                RaisePropertyChanged(nameof(RequiredBuild));
            }
        }

        public int SteamProtocolMaxDataSize
        {
            get => steamProtocolMaxDataSize;
            set
            {
                steamProtocolMaxDataSize = value;
                RaisePropertyChanged(nameof(SteamProtocolMaxDataSize));
            }
        }
        #endregion

        #region Security
        public string AllowedLoadFileExtensions
        {
            get => string.Join(", ", allowedLoadFileExtensions);
            set
            {
                allowedLoadFileExtensions = ParseExtensions(value);
                RaisePropertyChanged(nameof(AllowedLoadFileExtensions));
            }
        }

        public string AllowedPreprocessFileExtensions
        {
            get => string.Join(", ", allowedPreprocessFileExtensions);
            set
            {
                allowedPreprocessFileExtensions = ParseExtensions(value);
                RaisePropertyChanged(nameof(AllowedPreprocessFileExtensions));
            }
        }

        public string AllowedHTMLLoadExtensions
        {
            get => string.Join(", ", allowedHTMLLoadExtensions);
            set
            {
                allowedHTMLLoadExtensions = ParseExtensions(value);
                RaisePropertyChanged(nameof(AllowedHTMLLoadExtensions));
            }
        }

        public string AllowedHTMLLoadURIs
        {
            get => string.Join("\n", allowedHTMLLoadURIs);
            set
            {
                allowedHTMLLoadURIs = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(AllowedHTMLLoadURIs));
            }
        }

        public string FilePatchingExceptions
        {
            get => string.Join("\n", filePatchingExceptions);
            set
            {
                filePatchingExceptions = value.Replace("\r", "").Split('\n').ToList();
                RaisePropertyChanged(nameof(FilePatchingExceptions));
            }
        }

        private static List<string> ParseExtensions(string value)
        {
            return (value ?? "").Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(e => e.Trim('"', '\'').TrimStart('.'))
                                .Where(e => e.Length > 0)
                                .ToList();
        }

        private static string FormatArray(string name, IEnumerable<string> values, string comment)
        {
            var items = values.Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
            if (items.Count == 0)
            { return ""; }
            return $"{name}[] = {{ \"{string.Join("\", \"", items)}\" }};\t// {comment}\r\n";
        }
        #endregion

        #region Scripting

        public string DoubleIdDetected
        {
            get => doubleIdDetected;
            set
            {
                doubleIdDetected = value;
                RaisePropertyChanged(nameof(DoubleIdDetected));
            }
        }

        public string OnUserConnected
        {
            get => onUserConnected;
            set
            {
                onUserConnected = value;
                RaisePropertyChanged(nameof(OnUserConnected));
            }
        }

        public string OnUserDisconnected
        {
            get => onUserDisconnected;
            set
            {
                onUserDisconnected = value;
                RaisePropertyChanged(nameof(OnUserDisconnected));
            }
        }

        public string OnHackedData
        {
            get => onHackedData;
            set
            {
                onHackedData = value;
                RaisePropertyChanged(nameof(OnHackedData));
            }
        }

        public string OnDifferentData
        {
            get => onDifferentData;
            set
            {
                onDifferentData = value;
                RaisePropertyChanged(nameof(OnDifferentData));
            }
        }

        public string OnUnsignedData
        {
            get => onUnsignedData;
            set
            {
                onUnsignedData = value;
                RaisePropertyChanged(nameof(OnUnsignedData));
            }
        }

        public string OnUserKicked
        {
            get => onUserKicked;
            set
            {
                onUserKicked = value;
                RaisePropertyChanged(nameof(OnUserKicked));
            }
        }

        public string RegularCheck
        {
            get => regularCheck;
            set
            {
                regularCheck = value;
                RaisePropertyChanged(nameof(RegularCheck));
            }
        }

        public int CallExtReportLimit
        {
            get => callExtReportLimit;
            set
            {
                callExtReportLimit = value;
                RaisePropertyChanged(nameof(CallExtReportLimit));
            }
        }

        public bool EnablePlayerDiag
        {
            get => enablePlayerDiag;
            set
            {
                enablePlayerDiag = value;
                RaisePropertyChanged(nameof(EnablePlayerDiag));
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
                RaisePropertyChanged(nameof(MissionChecked));
            }
        }

        public string MissionContentOverride
        {
            get => missionContentOverride;
            set
            {
                missionContentOverride = value;
                RaisePropertyChanged(nameof(MissionContentOverride));
            }
        }

        public bool AutoInit
        {
            get => autoInit;
            set
            {
                autoInit = value;
                RaisePropertyChanged(nameof(AutoInit));
            }
        }

        public string Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = value;
                RaisePropertyChanged(nameof(Difficulty));
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
                                  let local = _missions.Find(m => m.Path == mission.Path)
                                  where local == null || local.MissionChecked != mission.MissionChecked
                                  select mission ).Any();

                if (!isEqual)
                {
                    _missions = value;
                    RaisePropertyChanged(nameof(Missions));
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
                RaisePropertyChanged(nameof(MaxMemOverride));
            }
        }

        public bool CpuCountOverride
        {
            get => cpuCountOverride;
            set
            {
                cpuCountOverride = value;
                RaisePropertyChanged(nameof(CpuCountOverride));
            }
        }

        public uint MaxMem
        {
            get => maxMem;
            set
            {
                maxMem = value;
                RaisePropertyChanged(nameof(MaxMem));
            }
        }

        public ushort CpuCount
        {
            get => cpuCount;
            set
            {
                cpuCount = value;
                RaisePropertyChanged(nameof(CpuCount));
            }
        }

        public string CommandLineParameters
        {
            get => commandLineParams;
            set
            {
                commandLineParams = value;
                RaisePropertyChanged(nameof(CommandLineParameters));
            }
        }

        #endregion

        public string ServerCfgContent
        {
            get => serverCfgContent;
            set
            {
                serverCfgContent = value;
                RaisePropertyChanged(nameof(ServerCfgContent));
            }
        }

        public ServerCfg()
        {
            foreach (var c in disableChannels) c.PropertyChanged += Channel_PropertyChanged;
            foreach (var c in voteCommands.Concat(votedAdminCommands)) c.PropertyChanged += Vote_PropertyChanged;

            if(string.IsNullOrWhiteSpace(serverCfgContent))
            { ServerCfgContent = ProcessFile(); }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MissionChecked));
            RaisePropertyChanged(nameof(MissionContentOverride));
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
                          + $"hostname = \"{hostname}\";\t\t\t// The name of the server that shall be displayed in the public server list\r\n"
                          + $"password = \"{password}\";\t\t\t\t// Password for joining, eg connecting to the server\r\n"
                          + $"passwordAdmin = \"{passwordAdmin}\";\t\t\t// Password to become server admin. When you're in Arma MP and connected to the server, type '#login xyz'\r\n"
                          + $"serverCommandPassword = \"{serverCommandPassword}\";\t\t// Password required by alternate syntax of [[serverCommand]] server-side scripting.\r\n"
                          + $"logFile = \"{logFile}\";\t\t// Tells Arma-server where the logfile should go and what it should be called\r\n"
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
                          + $"kickDuplicate = {kickduplicate};\t\t\t\t// Each Arma version has its own ID. If kickDuplicate is set to 1, a player will be kicked when he joins a server where another player with the same ID is playing.\r\n"
                          + $"verifySignatures = {verifySignatures};\t\t\t// Verifies .pbos against .bisign files. Valid values 0 (disabled), 1 (prefer v2 sigs but accept v1 too) and 2 (only v2 sigs are allowed). \r\n"
                          + $"allowedFilePatching = {allowedFilePatching};\t\t\t// Allow or prevent client using -filePatching to join the server. 0, is disallow, 1 is allow HC, 2 is allow all clients (since Arma 3 1.49+)\r\n"
                          + $"{(requiredBuildChecked ? $"requiredBuild = {requiredBuild};\t\t\t// Require clients joining to have at least build 12345 of game, preventing obsolete clients to connect\r\n" : "\r\n")}"
                          + $"steamProtocolMaxDataSize = {steamProtocolMaxDataSize};\t\t// Increasing this value will fix the modlist length limit in Arma 3 Launcher but mignt not be supported by some routers.\r\n"
                          + $"loopback = {(loopback ? "1" : "0")};\t\t\t\t// Enforces LAN only mode.\r\n"
                          + $"upnp = {(upnp ? "1" : "0")};\t\t\t\t// This setting might slow up server start-up by 600s if blocked by firewall or router.\r\n"
                          + "\r\n"
                          + "// SECURITY\r\n"
                          + FormatArray("allowedLoadFileExtensions", allowedLoadFileExtensions, "Only allow files with these extensions to be loaded via loadFile")
                          + FormatArray("allowedPreprocessFileExtensions", allowedPreprocessFileExtensions, "Only allow files with these extensions to be loaded via preprocessFile / preprocessFileLineNumbers")
                          + FormatArray("allowedHTMLLoadExtensions", allowedHTMLLoadExtensions, "Only allow files and URLs with these extensions to be loaded via htmlLoad")
                          + FormatArray("allowedHTMLLoadURIs", allowedHTMLLoadURIs, "Only allow files from these URIs to be loaded via htmlLoad")
                          + FormatArray("filePatchingExceptions", filePatchingExceptions, "Steam IDs allowed to join ignoring allowedFilePatching and verifySignatures (since Arma 3 2.10)")
                          + "\r\n"
                          + "// VOTING\r\n"
                          + $"{(votingEnabled ? $"voteMissionPlayers = {voteMissionPlayers};" : "voteMissionPlayers = 1;")}\t\t\t// Tells the server how many people must connect so that it displays the mission selection screen.\r\n"
                          + $"{(votingEnabled ? $"voteThreshold = {voteThreshold.ToString(CultureInfo.InvariantCulture)};" : "voteThreshold = 0;")}\t\t\t// 33% or more players need to vote for something, for example an admin or a new map, to become effective\r\n"
                          + (votingEnabled ? FormatVoteCmds("allowedVoteCmds", voteCommands) : "allowedVoteCmds[] = {};\t\t\t// Voting disabled\r\n")
                          + (votingEnabled ? FormatVoteCmds("allowedVotedAdminCmds", votedAdminCommands) : "allowedVotedAdminCmds[] = {};\t\t// Voting disabled\r\n")
                          + $"votingTimeOut = {votingTimeOut};\t\t\t// The amount of time a vote will last before ending.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// INGAME SETTINGS\r\n"
                          + $"disableVoN = {disableVoN};\t\t\t\t// If set to 1, Voice over Net will not be available\r\n"
                          + FormatDisableChannels()
                          + $"vonCodec = {vonCodec};\t\t\t\t// If set to 1 then it uses IETF standard OPUS codec, if to 0 then it uses SPEEX codec (since Arma 3 update 1.58+)  \r\n"
                          + $"skipLobby = {(skipLobby ? "1" : "0")};\t\t\t\t// Overridden by mission parameters\r\n"
                          + $"allowProfileGlasses = {(allowProfileGlasses ? "1" : "0")};\t\t\t// If 0, glasses set in player profiles are ignored. Overridden by mission parameters\r\n"
                          + $"zeusCompositionScriptLevel = {zeusCompositionScriptLevel};\t\t// 0 = no scripts, 1 = only attributes, 2 = all scripts in Zeus compositions. Overridden by mission parameters\r\n"
                          + $"forceRotorLibSimulation = {forceRotorLibSimulation};\t\t// 0 = up to the player, 1 = forced Advanced Flight Model, 2 = forced Standard Flight Model\r\n"
                          + (overrideHazeQuality >= 0 ? $"overrideHazeQuality = {overrideHazeQuality};\t\t\t// Forces haze quality on all clients: 0 = very low, 1 = low, 2 = standard\r\n" : "")
                          + $"statisticsEnabled = {(statisticsEnabled ? "1" : "0")};\t\t\t// 0 to opt out of Arma 3 analytics\r\n"
                          + $"vonCodecQuality = {vonCodecQuality};\t\t\t// since 1.62.95417 supports range 1-20 //since 1.63.x will supports range 1-30 //8kHz is 0-10, 16kHz is 11-20, 32kHz(48kHz) is 21-30 \r\n"
                          + $"persistent = {persistent};\t\t\t\t// If 1, missions still run on even after the last player disconnected.\r\n"
                          + $"timeStampFormat = \"{timeStampFormat}\";\t\t// Set the timestamp format used on each report line in server-side RPT file. Possible values are \"none\" (default),\"short\",\"full\".\r\n"
                          + $"timeStampFormatConsole = \"{timeStampFormatConsole}\";\t// Timestamp format used on each line of the server console. Possible values are \"none\", \"short\", \"full\".\r\n"
                          + $"BattlEye = {battlEye};\t\t\t\t// Server to use BattlEye system\r\n"
                          + $"idleFPSLimit = {idleFPSLimit};\t\t\t\t// Servers with no players will limit their FPS to this value (5-60)\r\n"
                          + $"enablePlayerDiag = {(enablePlayerDiag ? "1" : "0")};\t\t\t// Logs players' bandwidth and desync info every 60 seconds\r\n"
                          + $"drawingInMap = {(drawingInMap ? "1" : "0")};\t\t\t\t// Enables or disables the ability to place markers and draw lines in map.\r\n"
                          + "class AdvancedOptions\r\n{\r\n"
                          + $"\tLogObjectNotFound = {(logObjectNotFound ? "1" : "0")};\t\t// When false to skip logging 'Server: Object not found messages'.\r\n"
                          + $"\tSkipDescriptionParsing = {(skipDescriptionParsing ? "1" : "0")};\t\t// When true to skip parsing of description.ext/mission.sqm. Will show pbo filename instead of configured missionName. OverviewText and such won't work, but loading the mission list is a lot faster when there are many missions.\r\n"
                          + $"\tignoreMissionLoadErrors = {(ignoreMissionLoadErrors ? "1" : "0")};\t\t// When set to true, the mission will load no matter the amount of loading errors. If set to false, the server will abort mission's loading and return to mission selection.\r\n"
                          + $"\tqueueSizeLogG = {queueSizeLogG};\t\t\t// If a specific players message queue is larger than 1MB and #monitor is running, dump his messages to a logfile for analysis \r\n"
                          + "};\r\n"
                          + $"forcedDifficulty = \"{forcedDifficulty}\";\t\t\t// Forced difficulty (Recruit, Regular, Veteran, Custom)\r\n"
                          + "\r\n"
                          + "// TIMEOUTS\r\n"
                          + $"disconnectTimeout = {disconnectTimeout};\t\t\t// Time to wait before disconnecting a user which temporarly lost connection. Range is 5 to 90 seconds.\r\n"
                          + $"maxDesync = {maxdesync};\t\t\t// Max desync value until server kick the user\r\n"
                          + $"maxPing= {maxping};\t\t\t\t// Max ping value until server kick the user\r\n"
                          + $"maxPacketLoss= {maxpacketloss};\t\t\t// Max packetloss value until server kick the user\r\n"
                          + $"kickClientsOnSlowNetwork[] = {( kickClientOnSlowNetwork ? "{ 1, 1, 1, 1 }" : "{ 0, 0, 0, 0 }")};\t// Defines if {{<MaxPing>, <MaxPacketLoss>, <MaxDesync>, <DisconnectTimeout>}} will be logged (0) or kicked (1)\r\n"
                          + $"kickTimeout[] = {{ {{ 0, {kickTimeoutManual} }}, {{ 1, {kickTimeoutConnectivity} }}, {{ 2, {kickTimeoutBattlEye} }}, {{ 3, {kickTimeoutHarmless} }} }};\t// {{ kickID, timeout }} for manual, connectivity, BattlEye and harmless kicks. Seconds, -1 = until mission end, -2 = until server restart\r\n"
                          + $"lobbyIdleTimeout = {lobbyIdleTimeout};\t\t\t// The amount of time the server will wait before force-starting a mission without a logged-in Admin.\r\n"
                          + $"roleTimeOut = {roleTimeOut};\t\t\t\t// The amount of time a player can sit in role selection before being kicked.\r\n"
                          + $"debriefingTimeOut = {debriefingTimeOut};\t\t\t// The amount of time a player can sit in breifing mode before being kicked.\r\n"
                          + $"briefingTimeOut = {briefingTimeOut};\t\t\t// The amount of time a player can sit in briefing mode before being kicked.\r\n"
                          + $"armaUnitsTimeout = {armaUnitsTimeout};\t\t\t// Defines how long the player will be stuck connecting and wait for armaUnits data. Player will be notified if timeout elapsed and no units data was received.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// SCRIPTING ISSUES\r\n"
                          + $"onUserConnected = \"{onUserConnected}\";\t\t\t//\r\n"
                          + $"onUserDisconnected = \"{onUserDisconnected}\";\t\t\t//\r\n"
                          + $"doubleIdDetected = \"{doubleIdDetected}\";\t\t\t//\r\n"
						  + $"onUserKicked = \"{onUserKicked}\";\t\t\t\t//\r\n"
                          + $"regularCheck = \"{regularCheck}\";\t\t\t\t//\r\n"
                          + $"callExtReportLimit = {callExtReportLimit};\t\t// Log a warning if a server callExtension takes longer than this (ms)\r\n"
                          + "\r\n"
                          + "// SIGNATURE VERIFICATION\r\n"
                          + $"onUnsignedData = \"{onUnsignedData}\";\t// unsigned data detected\r\n"
                          + $"onHackedData = \"{onHackedData}\";\t// tampering of the signature detected\r\n"
                          + $"onDifferentData = \"{onDifferentData}\";\t\t\t// data with a valid signature, but different version than the one present on server detected\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// MISSIONS CYCLE (see below)\r\n"
                          + $"randomMissionOrder = {(randomMissionOrder ? "1" : "0")};\t\t// Randomly iterate through Missions list\r\n"
                          + $"autoSelectMission = {(autoSelectMission ? "1" : "0")};\t\t\t// Server auto selects next mission in cycle\r\n"
                          + (missionsEndAction == 1 ? $"missionsToServerRestart = {missionsEndCount};\t\t// Restart the server after this many missions ended\r\n" : "")
                          + (missionsEndAction == 2 ? $"missionsToShutdown = {missionsEndCount};\t\t\t// Shut down the server after this many missions ended\r\n" : "")
                          + (!string.IsNullOrWhiteSpace(MissionHTTPDownloadBaseURL) ? $"missionHTTPDownloadBaseURL = \"{MissionHTTPDownloadBaseURL}\";\r\n" : "")
                          + "\r\n"
                          + $"{MissionContentOverride}\t\t\t\t\t// An empty Missions class means there will be no mission rotation\r\n"
                          + "\r\n"
                          + "missionWhitelist[] = {};\t\t\t// An empty whitelist means there is no restriction on what missions available\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// HEADLESS CLIENT\r\n"
                          + $"{(headlessClientEnabled && headlessClients.Count > 0 && !headlessClients.Exists(string.IsNullOrWhiteSpace) ? $"headlessClients[] =  { "{\n\t\"" + string.Join("\",\n\t \"", headlessClients) + "\"\n}" };\r\n" : "")}"
                          + $"{(headlessClientEnabled && localClient.Count > 0 && !localClient.Exists(string.IsNullOrWhiteSpace)? $"localClient[] =  { "{\n\t\"" + string.Join("\",\n\t \"", localClient) + "\"\n}" };\r\n" : "")}"
                          + (AntiFloodEnabled ? $"class AntiFlood\r\n{{\r\n\tcycleTime = {AntiFloodCycleTime.ToString(CultureInfo.InvariantCulture)};\r\n\tcycleLimit = {AntiFloodCycleLimit};\r\n\tcycleHardLimit = {AntiFloodCycleHardLimit};\r\n\tenableKick = {_antiFloodEnableKick};\r\n}};\r\n" : "");
            return output;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
                RaisePropertyChanged(nameof(MissionChecked));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
