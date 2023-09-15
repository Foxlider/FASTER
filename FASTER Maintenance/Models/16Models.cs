using FASTER_Maintenance.Models._17Models;
using System.Collections.Generic;
using System.Xml.Serialization;

// ReSharper disable once CheckNamespace
namespace FASTER_Maintenance.Models._16Models
{
    [XmlRoot(ElementName = "setting")]
    [XmlInclude(typeof(ArrayOfServerProfile))]
    [XmlInclude(typeof(Value))]
    public class Setting
    {
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "serializeAs")]
        public string SerializeAs { get; set; }

        [XmlElement(ElementName = "value")] public object Value { get; set; }
    }

    [XmlRoot(ElementName = "SteamMod")]
    public class SteamMod
    {
        [XmlElement(ElementName = "WorkshopId")]
        public string WorkshopId { get; set; }

        [XmlElement(ElementName = "Name")] public string Name { get; set; }
        [XmlElement(ElementName = "Author")] public string Author { get; set; }

        [XmlElement(ElementName = "SteamLastUpdated")]
        public string SteamLastUpdated { get; set; }

        [XmlElement(ElementName = "LocalLastUpdated")]
        public string LocalLastUpdated { get; set; }

        [XmlElement(ElementName = "PrivateMod")]
        public string PrivateMod { get; set; }

        [XmlElement(ElementName = "Status")] public string Status { get; set; }
        [XmlElement(ElementName = "Size")] public string Size { get; set; }
    }

    [XmlRoot(ElementName = "SteamModCollection")]
    public class SteamModCollection
    {
        [XmlElement(ElementName = "CollectionName")]
        public string CollectionName { get; set; }

        [XmlElement(ElementName = "SteamMod")] public List<SteamMod> SteamMod { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
    }

    [XmlRoot(ElementName = "ServerProfile")]
    public class ServerProfile
    {
        [XmlElement(ElementName = "SafeName")] public string SafeName { get; set; }

        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }

        [XmlElement(ElementName = "ServerName")]
        public string ServerName { get; set; }

        [XmlElement(ElementName = "Executable")]
        public string Executable { get; set; }

        [XmlElement(ElementName = "Password")] public string Password { get; set; }

        [XmlElement(ElementName = "AdminPassword")]
        public string AdminPassword { get; set; }

        [XmlElement(ElementName = "MaxPlayers")]
        public string MaxPlayers { get; set; }

        [XmlElement(ElementName = "Port")] public string Port { get; set; }

        [XmlElement(ElementName = "HeadlessClientEnabled")]
        public string HeadlessClientEnabled { get; set; }

        [XmlElement(ElementName = "HeadlessIps")]
        public string HeadlessIps { get; set; }

        [XmlElement(ElementName = "LocalClients")]
        public string LocalClients { get; set; }

        [XmlElement(ElementName = "NoOfHeadlessClients")]
        public string NoOfHeadlessClients { get; set; }

        [XmlElement(ElementName = "Loopback")] public string Loopback { get; set; }
        [XmlElement(ElementName = "Upnp")] public string Upnp { get; set; }
        [XmlElement(ElementName = "Netlog")] public string Netlog { get; set; }

        [XmlElement(ElementName = "AutoRestartEnabled")]
        public string AutoRestartEnabled { get; set; }

        [XmlElement(ElementName = "DailyRestartAEnabled")]
        public string DailyRestartAEnabled { get; set; }

        [XmlElement(ElementName = "DailyRestartA")]
        public string DailyRestartA { get; set; }

        [XmlElement(ElementName = "DailyRestartBEnabled")]
        public string DailyRestartBEnabled { get; set; }

        [XmlElement(ElementName = "DailyRestartB")]
        public string DailyRestartB { get; set; }

        [XmlElement(ElementName = "VotingEnabled")]
        public string VotingEnabled { get; set; }

        [XmlElement(ElementName = "VotingMinPlayers")]
        public string VotingMinPlayers { get; set; }

        [XmlElement(ElementName = "VotingThreshold")]
        public string VotingThreshold { get; set; }

        [XmlElement(ElementName = "AllowFilePatching")]
        public string AllowFilePatching { get; set; }

        [XmlElement(ElementName = "VerifySignatures")]
        public string VerifySignatures { get; set; }

        [XmlElement(ElementName = "RequiredBuildEnabled")]
        public string RequiredBuildEnabled { get; set; }

        [XmlElement(ElementName = "RequiredBuild")]
        public string RequiredBuild { get; set; }

        [XmlElement(ElementName = "KickDuplicates")]
        public string KickDuplicates { get; set; }

        [XmlElement(ElementName = "VonEnabled")]
        public string VonEnabled { get; set; }

        [XmlElement(ElementName = "CodecQuality")]
        public string CodecQuality { get; set; }

        [XmlElement(ElementName = "ServerConsoleLogEnabled")]
        public string ServerConsoleLogEnabled { get; set; }

        [XmlElement(ElementName = "PidEnabled")]
        public string PidEnabled { get; set; }

        [XmlElement(ElementName = "RankingEnabled")]
        public string RankingEnabled { get; set; }

        [XmlElement(ElementName = "RptTimestamp")]
        public string RptTimestamp { get; set; }

        [XmlElement(ElementName = "Motd")] public string Motd { get; set; }

        [XmlElement(ElementName = "MotdDelay")]
        public string MotdDelay { get; set; }

        [XmlElement(ElementName = "ServerMods")]
        public string ServerMods { get; set; }

        [XmlElement(ElementName = "ClientMods")]
        public string ClientMods { get; set; }

        [XmlElement(ElementName = "HeadlessMods")]
        public string HeadlessMods { get; set; }

        [XmlElement(ElementName = "ManualMissions")]
        public string ManualMissions { get; set; }

        [XmlElement(ElementName = "MissionsClass")]
        public string MissionsClass { get; set; }

        [XmlElement(ElementName = "Missions")] public string Missions { get; set; }

        [XmlElement(ElementName = "PersistentBattlefield")]
        public string PersistentBattlefield { get; set; }

        [XmlElement(ElementName = "AutoInit")] public string AutoInit { get; set; }

        [XmlElement(ElementName = "DifficultyPreset")]
        public string DifficultyPreset { get; set; }

        [XmlElement(ElementName = "ReducedDamage")]
        public string ReducedDamage { get; set; }

        [XmlElement(ElementName = "GroupIndicators")]
        public string GroupIndicators { get; set; }

        [XmlElement(ElementName = "FriendlyNameTags")]
        public string FriendlyNameTags { get; set; }

        [XmlElement(ElementName = "EnemyNameTags")]
        public string EnemyNameTags { get; set; }

        [XmlElement(ElementName = "DetectedMines")]
        public string DetectedMines { get; set; }

        [XmlElement(ElementName = "MultipleSaves")]
        public string MultipleSaves { get; set; }

        [XmlElement(ElementName = "ThirdPerson")]
        public string ThirdPerson { get; set; }

        [XmlElement(ElementName = "WeaponInfo")]
        public string WeaponInfo { get; set; }

        [XmlElement(ElementName = "StanceIndicator")]
        public string StanceIndicator { get; set; }

        [XmlElement(ElementName = "StaminaBar")]
        public string StaminaBar { get; set; }

        [XmlElement(ElementName = "CameraShake")]
        public string CameraShake { get; set; }

        [XmlElement(ElementName = "VisualAids")]
        public string VisualAids { get; set; }

        [XmlElement(ElementName = "MapContentFriendly")]
        public string MapContentFriendly { get; set; }

        [XmlElement(ElementName = "MapContentEnemy")]
        public string MapContentEnemy { get; set; }

        [XmlElement(ElementName = "MapContentMines")]
        public string MapContentMines { get; set; }

        [XmlElement(ElementName = "Commands")] public string Commands { get; set; }
        [XmlElement(ElementName = "VonId")] public string VonId { get; set; }
        [XmlElement(ElementName = "KilledBy")] public string KilledBy { get; set; }

        [XmlElement(ElementName = "Waypoints")]
        public string Waypoints { get; set; }

        [XmlElement(ElementName = "Crosshair")]
        public string Crosshair { get; set; }

        [XmlElement(ElementName = "AutoReporting")]
        public string AutoReporting { get; set; }

        [XmlElement(ElementName = "ScoreTable")]
        public string ScoreTable { get; set; }

        [XmlElement(ElementName = "TacticalPing")]
        public string TacticalPing { get; set; }

        [XmlElement(ElementName = "AiAccuracy")]
        public string AiAccuracy { get; set; }

        [XmlElement(ElementName = "AiSkill")] public string AiSkill { get; set; }
        [XmlElement(ElementName = "AiPreset")] public string AiPreset { get; set; }

        [XmlElement(ElementName = "MaxPacketLossEnabled")]
        public string MaxPacketLossEnabled { get; set; }

        [XmlElement(ElementName = "MaxPacketLoss")]
        public string MaxPacketLoss { get; set; }

        [XmlElement(ElementName = "DisconnectTimeoutEnabled")]
        public string DisconnectTimeoutEnabled { get; set; }

        [XmlElement(ElementName = "DisconnectTimeout")]
        public string DisconnectTimeout { get; set; }

        [XmlElement(ElementName = "KickOnSlowNetworkEnabled")]
        public string KickOnSlowNetworkEnabled { get; set; }

        [XmlElement(ElementName = "KickOnSlowNetwork")]
        public string KickOnSlowNetwork { get; set; }

        [XmlElement(ElementName = "TerrainGrid")]
        public string TerrainGrid { get; set; }

        [XmlElement(ElementName = "ViewDistance")]
        public string ViewDistance { get; set; }

        [XmlElement(ElementName = "MaxPingEnabled")]
        public string MaxPingEnabled { get; set; }

        [XmlElement(ElementName = "MaxPing")] public string MaxPing { get; set; }

        [XmlElement(ElementName = "MaxDesyncEnabled")]
        public string MaxDesyncEnabled { get; set; }

        [XmlElement(ElementName = "MaxDesync")]
        public string MaxDesync { get; set; }

        [XmlElement(ElementName = "MaxCustomFileSize")]
        public string MaxCustomFileSize { get; set; }

        [XmlElement(ElementName = "MaxPacketSize")]
        public string MaxPacketSize { get; set; }

        [XmlElement(ElementName = "MinBandwidth")]
        public string MinBandwidth { get; set; }

        [XmlElement(ElementName = "MaxBandwidth")]
        public string MaxBandwidth { get; set; }

        [XmlElement(ElementName = "MaxMessagesSend")]
        public string MaxMessagesSend { get; set; }

        [XmlElement(ElementName = "MaxSizeNonguaranteed")]
        public string MaxSizeNonguaranteed { get; set; }

        [XmlElement(ElementName = "MaxSizeGuaranteed")]
        public string MaxSizeGuaranteed { get; set; }

        [XmlElement(ElementName = "MinErrorToSend")]
        public string MinErrorToSend { get; set; }

        [XmlElement(ElementName = "MinErrorToSendNear")]
        public string MinErrorToSendNear { get; set; }

        [XmlElement(ElementName = "CpuCount")] public string CpuCount { get; set; }
        [XmlElement(ElementName = "MaxMem")] public string MaxMem { get; set; }

        [XmlElement(ElementName = "ExtraParams")]
        public string ExtraParams { get; set; }

        [XmlElement(ElementName = "AdminUids")]
        public string AdminUids { get; set; }

        [XmlElement(ElementName = "EnableHyperThreading")]
        public string EnableHyperThreading { get; set; }

        [XmlElement(ElementName = "FilePatching")]
        public string FilePatching { get; set; }

        [XmlElement(ElementName = "ServerCommandPassword")]
        public string ServerCommandPassword { get; set; }

        [XmlElement(ElementName = "DoubleIdDetected")]
        public string DoubleIdDetected { get; set; }

        [XmlElement(ElementName = "OnUserConnected")]
        public string OnUserConnected { get; set; }

        [XmlElement(ElementName = "OnUserDisconnected")]
        public string OnUserDisconnected { get; set; }

        [XmlElement(ElementName = "OnHackedData")]
        public string OnHackedData { get; set; }

        [XmlElement(ElementName = "OnDifferentData")]
        public string OnDifferentData { get; set; }

        [XmlElement(ElementName = "OnUnsignedData")]
        public string OnUnsignedData { get; set; }

        [XmlElement(ElementName = "RegularCheck")]
        public string RegularCheck { get; set; }

        [XmlElement(ElementName = "BattleEye")]
        public string BattleEye { get; set; }

        [XmlElement(ElementName = "enableAdditionalParams")]
        public string EnableAdditionalParams { get; set; }

        [XmlElement(ElementName = "additionalParams")]
        public string AdditionalParams { get; set; }
    }

    [XmlRoot(ElementName = "ServerCollection")]
    public class ServerCollection
    {
        [XmlElement(ElementName = "CollectionName")]
        public string CollectionName { get; set; }

        [XmlElement(ElementName = "ServerProfile")]
        public List<ServerProfile> ServerProfile { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
    }

    [XmlRoot(ElementName = "ArrayOfString")]
    public class ArrayOfString
    {
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
    }

    [XmlRoot(ElementName = "configuration")]
    public class Configuration
    {
        [XmlElement(ElementName = "userSettings")]
        public UserSettings UserSettings { get; set; }
    }

    [XmlRoot(ElementName = "FASTER.Properties.Settings")]
    public class Settings
    {
        [XmlElement(ElementName = "setting")] public List<Setting> Setting { get; set; }
    }

    [XmlRoot(ElementName = "userSettings")]
    public class UserSettings
    {
        [XmlElement(ElementName = "FASTER.Properties.Settings")]
        public Settings Settings { get; set; }
    }
}