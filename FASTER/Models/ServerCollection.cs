using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace FASTER.Models
{
    [Serializable]
    public class ServerCollection
    {
        [XmlElement(Order = 1)]
        public string CollectionName { get; set; } = "Main";

        [XmlElement(Order = 2, ElementName = "ServerProfile")]
        public List<ServerProfile> ServerProfiles = new List<ServerProfile>();

        private static ServerCollection GetServerProfiles()
        {
            ServerCollection currentProfiles = new ServerCollection();

            if (Properties.Settings.Default.Servers != null)
                currentProfiles = Properties.Settings.Default.Servers;

            return currentProfiles;
        }

        public static void AddServerProfile(string name, string safeName)
        {
            var duplicate = false;
            var currentProfiles = GetServerProfiles();

            if (currentProfiles.ServerProfiles.Count > 0)
            {
                foreach (var profile in currentProfiles.ServerProfiles
                                                       .Where(profile => profile.DisplayName == name)) 
                { duplicate = true; }
            }

            if (!duplicate)
            {
                currentProfiles.ServerProfiles.Add(new ServerProfile(name, safeName));
                Properties.Settings.Default.Servers = currentProfiles;
                ServerProfile profile = Properties.Settings.Default.Servers.ServerProfiles.Find(newProfile => newProfile.SafeName == safeName);
                profile.ServerName = name;
                profile.Executable = Properties.Settings.Default.serverPath + @"\arma3server_x64.exe";
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen = true;
                MainWindow.Instance.IMessageDialogText.Text = "Profile Already Exists";
            }

            Properties.Settings.Default.Save();
            MainWindow.Instance.LoadServerProfiles();
        }

        public static void AddServerProfile(ServerProfile newProfile)
        {
            var duplicate       = false;
            var currentProfiles = GetServerProfiles();

            if (currentProfiles.ServerProfiles.Count > 0)
            {
                foreach (var profile in currentProfiles.ServerProfiles
                                                       .Where(profile => profile.DisplayName == newProfile.DisplayName)) 
                { duplicate = true; }
            }

            if (!duplicate)
            {
                currentProfiles.ServerProfiles.Add(newProfile);
                Properties.Settings.Default.Servers = currentProfiles;
                ServerProfile profile = Properties.Settings.Default.Servers.ServerProfiles.Find(findProfile => findProfile.SafeName == newProfile.SafeName);
                profile.ServerName = newProfile.ServerName;
                profile.Executable = Properties.Settings.Default.serverPath + @"\arma3server_x64.exe";
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "Profile Already Exists";
            }

            Properties.Settings.Default.Save();
            MainWindow.Instance.LoadServerProfiles();
        }

        public static bool RenameServerProfile(string oldName, string newName)
        {
            try
            {
                var currentProfiles = Properties.Settings.Default.Servers.ServerProfiles;

                ServerProfile currentProfile = currentProfiles.Find(profile => profile.DisplayName == oldName);

                currentProfile.DisplayName = newName;
                currentProfile.SafeName = Functions.SafeName(newName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ServerConfig - An exception occurred:\n{ex.Message}", "Error");
                return false;
            }
        }

        public static void DeleteServerProfile(string safeName)
        {
            var currentProfiles = GetServerProfiles();

            currentProfiles.ServerProfiles.RemoveAll(x => x.SafeName == safeName);

            Properties.Settings.Default.Save();
        }
    }

    [Serializable]
    public class ServerProfile
    {
        public ServerProfile()
        {}

        public ServerProfile(string name, string safeName)
        {
            DisplayName = name;
            SafeName = safeName;
        }

        public string   SafeName              { get; set; } = string.Empty;
        public string   DisplayName           { get; set; } = string.Empty;
        public string   ServerName            { get; set; } = string.Empty;
        public string   Executable            { get; set; } = string.Empty;
        public string   Password              { get; set; } = string.Empty;
        public string   AdminPassword         { get; set; } = string.Empty;
        public int      MaxPlayers            { get; set; } = 32;
        public int      Port                  { get; set; } = 2302;
        public bool     HeadlessClientEnabled { get; set; }
        public string   HeadlessIps           { get; set; } = "127.0.0.1";
        public string   LocalClients          { get; set; } = "127.0.0.1";
        public int      NoOfHeadlessClients   { get; set; }
        public bool     Loopback              { get; set; }
        public bool     Upnp                  { get; set; }
        public bool     Netlog                { get; set; }
        public bool     AutoRestartEnabled    { get; set; } = false;
        public bool     DailyRestartAEnabled  { get; set; } = false;
        public DateTime DailyRestartA         { get; set; }
        public bool     DailyRestartBEnabled  { get; set; } = false;
        public DateTime DailyRestartB         { get; set; }
        public bool     VotingEnabled         { get; set; } = true;
        public int      VotingMinPlayers      { get; set; } = 3;
        public decimal  VotingThreshold       { get; set; } = 33;
        public int      AllowFilePatching     { get; set; }
        public int      VerifySignatures      { get; set; }
        public bool     RequiredBuildEnabled  { get; set; }
        public bool     RequiredBuild         { get; set; } = false;
        public bool     KickDuplicates        { get; set; }
        public bool     VonEnabled            { get; set; } = true;
        public int      CodecQuality          { get; set; } = 3;

        public bool ServerConsoleLogEnabled { get; set; }

        // Public Property ServerConsoleLog As String = "server_console.log"
        public bool PidEnabled { get; set; }

        // Public Property PidLog As String = "pid.log"
        public bool RankingEnabled { get; set; }

        // Public Property RankingLog As String = "ranking.log"
        public string RptTimestamp             { get; set; } = "none";
        public string Motd                     { get; set; } = string.Empty;
        public int    MotdDelay                { get; set; } = 5;
        public string ServerMods               { get; set; } = string.Empty;
        public string ClientMods               { get; set; } = string.Empty;
        public string HeadlessMods             { get; set; } = string.Empty;
        public bool   ManualMissions           { get; set; }
        public string MissionsClass            { get; set; } = string.Empty;
        public string Missions                 { get; set; } = string.Empty;
        public bool   PersistentBattlefield    { get; set; }
        public bool   AutoInit                 { get; set; }
        public string DifficultyPreset         { get; set; } = "Regular";
        public bool   ReducedDamage            { get; set; }
        public string GroupIndicators          { get; set; } = "Never";
        public string FriendlyNameTags         { get; set; } = "Never";
        public string EnemyNameTags            { get; set; } = "Never";
        public string DetectedMines            { get; set; } = "Never";
        public bool   MultipleSaves            { get; set; }
        public bool   ThirdPerson              { get; set; }
        public string WeaponInfo               { get; set; } = "Never";
        public string StanceIndicator          { get; set; } = "Never";
        public bool   StaminaBar               { get; set; }
        public bool   CameraShake              { get; set; }
        public bool   VisualAids               { get; set; }
        public bool   MapContentFriendly       { get; set; }
        public bool   MapContentEnemy          { get; set; }
        public bool   MapContentMines          { get; set; }
        public string Commands                 { get; set; } = "Never";
        public bool   VonId                    { get; set; }
        public bool   KilledBy                 { get; set; }
        public string Waypoints                { get; set; } = "Never";
        public bool   Crosshair                { get; set; }
        public bool   AutoReporting            { get; set; }
        public bool   ScoreTable               { get; set; }
        public bool   TacticalPing             { get; set; }
        public double AiAccuracy               { get; set; } = 0.95;
        public double AiSkill                  { get; set; } = 0.55;
        public int    AiPreset                 { get; set; } = 3;
        public bool   MaxPacketLossEnabled     { get; set; }
        public int    MaxPacketLoss            { get; set; }
        public bool   DisconnectTimeoutEnabled { get; set; }
        public int    DisconnectTimeout        { get; set; } = 90;
        public bool   KickOnSlowNetworkEnabled { get; set; }
        public string KickOnSlowNetwork        { get; set; } = string.Empty;
        public int    TerrainGrid              { get; set; } = 10;
        public int    ViewDistance             { get; set; } = 2500;
        public bool   MaxPingEnabled           { get; set; }
        public int    MaxPing                  { get; set; } = 60;
        public bool   MaxDesyncEnabled         { get; set; }
        public int    MaxDesync                { get; set; }
        public int    MaxCustomFileSize        { get; set; } = 160;
        public int    MaxPacketSize            { get; set; } = 1400;
        public double MinBandwidth             { get; set; } = 131072;
        public double MaxBandwidth             { get; set; } = 10000000000;
        public int    MaxMessagesSend          { get; set; } = 128;
        public int    MaxSizeNonguaranteed     { get; set; } = 256;
        public int    MaxSizeGuaranteed        { get; set; } = 512;
        public double MinErrorToSend           { get; set; } = 0.001;
        public double MinErrorToSendNear       { get; set; } = 0.01;
        public string CpuCount                 { get; set; } = string.Empty;
        public string MaxMem                   { get; set; } = string.Empty;
        public string ExtraParams              { get; set; } = string.Empty;
        public string AdminUids                { get; set; } = string.Empty;
        public bool   EnableHyperThreading     { get; set; }
        public bool   FilePatching             { get; set; }
        public string ServerCommandPassword    { get; set; } = string.Empty;
        public string DoubleIdDetected         { get; set; } = string.Empty;
        public string OnUserConnected          { get; set; } = string.Empty;
        public string OnUserDisconnected       { get; set; } = string.Empty;
        public string OnHackedData             { get; set; } = string.Empty;
        public string OnDifferentData          { get; set; } = string.Empty;
        public string OnUnsignedData           { get; set; } = string.Empty;
        public string RegularCheck             { get; set; } = string.Empty;
        public bool   BattleEye                { get; set; } = true;
        public bool   enableAdditionalParams   { get; set; } = false;
        public string additionalParams         { get; set; } = string.Empty;
    }
}
