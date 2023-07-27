using FASTER_Maintenance.Models._16Models;
using System.Collections.Generic;
using System.Xml.Serialization;

// ReSharper disable once CheckNamespace
namespace FASTER_Maintenance.Models._17Models
{
    [XmlRoot(ElementName = "ArrayOfString")]
    public class ArrayOfString
    {
        [XmlElement(ElementName = "string")] public string String { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
    }

    [XmlRoot(ElementName = "value")]
    public class Value
    {
        [XmlElement(ElementName = "ArrayOfString")]
        public ArrayOfString ArrayOfString { get; set; }

        [XmlElement(ElementName = "ArrayOfServerProfile")]
        public ArrayOfServerProfile ArrayOfServerProfile { get; set; }

        [XmlElement(ElementName = "SteamModCollection")]
        public SteamModCollection SteamModCollection { get; set; }
    }

    [XmlRoot(ElementName = "ProfileMod")]
    public class ProfileMod
    {
        [XmlElement(ElementName = "ServerSideChecked")]
        public string ServerSideChecked { get; set; }

        [XmlElement(ElementName = "ClientSideChecked")]
        public string ClientSideChecked { get; set; }

        [XmlElement(ElementName = "HeadlessChecked")]
        public string HeadlessChecked { get; set; }

        [XmlElement(ElementName = "Id")]   public string Id   { get; set; }
        [XmlElement(ElementName = "Name")] public string Name { get; set; }
    }

    [XmlRoot(ElementName = "ProfileMods")]
    public class ProfileMods
    {
        [XmlElement(ElementName = "ProfileMod")]
        public List<ProfileMod> ProfileMod { get; set; }
    }

    [XmlRoot(ElementName = "ProfileMission")]
    public class ProfileMission
    {
        [XmlElement(ElementName = "MissionChecked")]
        public string MissionChecked { get; set; }

        [XmlElement(ElementName = "Name")] public string Name { get; set; }
        [XmlElement(ElementName = "Path")] public string Path { get; set; }
    }

    [XmlRoot(ElementName = "Missions")]
    public class Missions
    {
        [XmlElement(ElementName = "ProfileMission")]
        public List<ProfileMission> ProfileMission { get; set; }
    }

    [XmlRoot(ElementName = "ServerCfg")]
    public class ServerCfg
    {
        [XmlElement(ElementName = "PasswordAdmin")]
        public string PasswordAdmin { get; set; }

        [XmlElement(ElementName = "Password")] public string Password { get; set; }
        [XmlElement(ElementName = "Hostname")] public string Hostname { get; set; }

        [XmlElement(ElementName = "MaxPlayers")]
        public string MaxPlayers { get; set; }

        [XmlElement(ElementName = "Motd")] public string Motd { get; set; }

        [XmlElement(ElementName = "MotdInterval")]
        public string MotdInterval { get; set; }

        [XmlElement(ElementName = "Admins")] public string Admins { get; set; }

        [XmlElement(ElementName = "HeadlessClients")]
        public string HeadlessClients { get; set; }

        [XmlElement(ElementName = "LocalClient")]
        public string LocalClient { get; set; }

        [XmlElement(ElementName = "HeadlessClientEnabled")]
        public string HeadlessClientEnabled { get; set; }

        [XmlElement(ElementName = "VotingEnabled")]
        public string VotingEnabled { get; set; }

        [XmlElement(ElementName = "NetLogEnabled")]
        public string NetLogEnabled { get; set; }

        [XmlElement(ElementName = "VoteThreshold")]
        public string VoteThreshold { get; set; }

        [XmlElement(ElementName = "VoteMissionPlayers")]
        public string VoteMissionPlayers { get; set; }

        [XmlElement(ElementName = "KickDuplicates")]
        public string KickDuplicates { get; set; }

        [XmlElement(ElementName = "Loopback")] public string Loopback { get; set; }
        [XmlElement(ElementName = "Upnp")]     public string Upnp     { get; set; }

        [XmlElement(ElementName = "AllowedFilePatching")]
        public string AllowedFilePatching { get; set; }

        [XmlElement(ElementName = "DisconnectTimeout")]
        public string DisconnectTimeout { get; set; }

        [XmlElement(ElementName = "MaxDesync")]
        public string MaxDesync { get; set; }

        [XmlElement(ElementName = "MaxPing")]
        public string MaxPing { get; set; }

        [XmlElement(ElementName = "MaxPacketLoss")]
        public string MaxPacketLoss { get; set; }

        [XmlElement(ElementName = "KickClientOnSlowNetwork")]
        public string KickClientOnSlowNetwork { get; set; }

        [XmlElement(ElementName = "LobbyIdleTimeout")]
        public string LobbyIdleTimeout { get; set; }

        [XmlElement(ElementName = "AutoSelectMission")]
        public string AutoSelectMission { get; set; }

        [XmlElement(ElementName = "RandomMissionOrder")]
        public string RandomMissionOrder { get; set; }

        [XmlElement(ElementName = "BriefingTimeOut")]
        public string BriefingTimeOut { get; set; }

        [XmlElement(ElementName = "RoleTimeOut")]
        public string RoleTimeOut { get; set; }

        [XmlElement(ElementName = "VotingTimeOut")]
        public string VotingTimeOut { get; set; }

        [XmlElement(ElementName = "DebriefingTimeOut")]
        public string DebriefingTimeOut { get; set; }

        [XmlElement(ElementName = "LogObjectNotFound")]
        public string LogObjectNotFound { get; set; }

        [XmlElement(ElementName = "SkipDescriptionParsing")]
        public string SkipDescriptionParsing { get; set; }

        [XmlElement(ElementName = "IgnoreMissionLoadErrors")]
        public string IgnoreMissionLoadErrors { get; set; }

        [XmlElement(ElementName = "VerifySignatures")]
        public string VerifySignatures { get; set; }

        [XmlElement(ElementName = "DrawingInMap")]
        public string DrawingInMap { get; set; }

        [XmlElement(ElementName = "VonActivated")]
        public string VonActivated { get; set; }

        [XmlElement(ElementName = "VonCodecQuality")]
        public string VonCodecQuality { get; set; }

        [XmlElement(ElementName = "VonCodec")] public string VonCodec { get; set; }

        [XmlElement(ElementName = "SkipLobby")]
        public string SkipLobby { get; set; }

        [XmlElement(ElementName = "LogFile")]  public string LogFile  { get; set; }
        [XmlElement(ElementName = "BattlEye")] public string BattlEye { get; set; }

        [XmlElement(ElementName = "TimeStampFormat")]
        public string TimeStampFormat { get; set; }

        [XmlElement(ElementName = "Persistent")]
        public string Persistent { get; set; }

        [XmlElement(ElementName = "RequiredBuildChecked")]
        public string RequiredBuildChecked { get; set; }

        [XmlElement(ElementName = "RequiredBuild")]
        public string RequiredBuild { get; set; }

        [XmlElement(ElementName = "SteamProtocolMaxDataSize")]
        public string SteamProtocolMaxDataSize { get; set; }

        [XmlElement(ElementName = "OnHackedData")]
        public string OnHackedData { get; set; }

        [XmlElement(ElementName = "OnUnsignedData")]
        public string OnUnsignedData { get; set; }

        [XmlElement(ElementName = "MissionChecked")]
        public string MissionChecked { get; set; }

        [XmlElement(ElementName = "MissionContentOverride")]
        public string MissionContentOverride { get; set; }

        [XmlElement(ElementName = "AutoInit")] public string AutoInit { get; set; }

        [XmlElement(ElementName = "Difficulty")]
        public string Difficulty { get; set; }

        [XmlElement(ElementName = "Missions")] public Missions Missions { get; set; }

        [XmlElement(ElementName = "MaxMemOverride")]
        public string MaxMemOverride { get; set; }

        [XmlElement(ElementName = "CpuCountOverride")]
        public string CpuCountOverride { get; set; }

        [XmlElement(ElementName = "MaxMem")]   public string MaxMem   { get; set; }
        [XmlElement(ElementName = "CpuCount")] public string CpuCount { get; set; }

        [XmlElement(ElementName = "ServerCfgContent")]
        public string ServerCfgContent { get; set; }
    }

    [XmlRoot(ElementName = "ArmaProfile")]
    public class ArmaProfile
    {
        [XmlElement(ElementName = "ArmaProfileContent")]
        public string ArmaProfileContent { get; set; }

        [XmlElement(ElementName = "ReducedDamage")]
        public string ReducedDamage { get; set; }

        [XmlElement(ElementName = "GroupIndicators")]
        public string GroupIndicators { get; set; }

        [XmlElement(ElementName = "FriendlyTags")]
        public string FriendlyTags { get; set; }

        [XmlElement(ElementName = "EnemyTags")]
        public string EnemyTags { get; set; }

        [XmlElement(ElementName = "DetectedMines")]
        public string DetectedMines { get; set; }

        [XmlElement(ElementName = "Commands")] public string Commands { get; set; }

        [XmlElement(ElementName = "Waypoints")]
        public string Waypoints { get; set; }

        [XmlElement(ElementName = "WeaponInfo")]
        public string WeaponInfo { get; set; }

        [XmlElement(ElementName = "StanceIndicator")]
        public string StanceIndicator { get; set; }

        [XmlElement(ElementName = "StaminaBar")]
        public string StaminaBar { get; set; }

        [XmlElement(ElementName = "WeaponCrosshair")]
        public string WeaponCrosshair { get; set; }

        [XmlElement(ElementName = "VisionAid")]
        public string VisionAid { get; set; }

        [XmlElement(ElementName = "ThirdPersonView")]
        public string ThirdPersonView { get; set; }

        [XmlElement(ElementName = "CameraShake")]
        public string CameraShake { get; set; }

        [XmlElement(ElementName = "ScoreTable")]
        public string ScoreTable { get; set; }

        [XmlElement(ElementName = "DeathMessages")]
        public string DeathMessages { get; set; }

        [XmlElement(ElementName = "VonID")] public string VonID { get; set; }

        [XmlElement(ElementName = "MapContentFriendly")]
        public string MapContentFriendly { get; set; }

        [XmlElement(ElementName = "MapContentEnemy")]
        public string MapContentEnemy { get; set; }

        [XmlElement(ElementName = "MapContentMines")]
        public string MapContentMines { get; set; }

        [XmlElement(ElementName = "AutoReport")]
        public string AutoReport { get; set; }

        [XmlElement(ElementName = "MultipleSaves")]
        public string MultipleSaves { get; set; }

        [XmlElement(ElementName = "TacticalPing")]
        public string TacticalPing { get; set; }

        [XmlElement(ElementName = "AiLevelPreset")]
        public string AiLevelPreset { get; set; }

        [XmlElement(ElementName = "SkillAi")] public string SkillAi { get; set; }

        [XmlElement(ElementName = "PrecisionAi")]
        public string PrecisionAi { get; set; }
    }

    [XmlRoot(ElementName = "BasicCfg")]
    public class BasicCfg
    {
        [XmlElement(ElementName = "BasicContent")]
        public string BasicContent { get; set; }

        [XmlElement(ElementName = "MaxSizeGuaranteed")]
        public string MaxSizeGuaranteed { get; set; }

        [XmlElement(ElementName = "MaxSizeNonGuaranteed")]
        public string MaxSizeNonGuaranteed { get; set; }

        [XmlElement(ElementName = "MaxMsgSend")]
        public string MaxMsgSend { get; set; }

        [XmlElement(ElementName = "MinBandwidth")]
        public string MinBandwidth { get; set; }

        [XmlElement(ElementName = "MaxBandwidth")]
        public string MaxBandwidth { get; set; }

        [XmlElement(ElementName = "MaxPacketSize")]
        public string MaxPacketSize { get; set; }

        [XmlElement(ElementName = "MinErrorToSend")]
        public string MinErrorToSend { get; set; }

        [XmlElement(ElementName = "MinErrorToSendNear")]
        public string MinErrorToSendNear { get; set; }

        [XmlElement(ElementName = "MaxCustomFileSize")]
        public string MaxCustomFileSize { get; set; }

        [XmlElement(ElementName = "PerfPreset")]
        public string PerfPreset { get; set; }
    }


    [XmlRoot(ElementName = "ArrayOfServerProfile")]
    public class ArrayOfServerProfile
    {
        [XmlElement(ElementName = "ServerProfile")]
        public List<FASTER.Models.ServerProfile> ServerProfile { get; set; }
    }

    [XmlRoot(ElementName = "SteamMod")]
    public class SteamMod
    {
        [XmlElement(ElementName = "WorkshopId")]
        public string WorkshopId { get; set; }

        [XmlElement(ElementName = "Name")]   public string Name   { get; set; }
        [XmlElement(ElementName = "Author")] public string Author { get; set; }

        [XmlElement(ElementName = "SteamLastUpdated")]
        public string SteamLastUpdated { get; set; }

        [XmlElement(ElementName = "LocalLastUpdated")]
        public string LocalLastUpdated { get; set; }

        [XmlElement(ElementName = "PrivateMod")]
        public string PrivateMod { get; set; }

        [XmlElement(ElementName = "Status")] public string Status { get; set; }
        [XmlElement(ElementName = "Size")]   public string Size   { get; set; }
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

    [XmlRoot(ElementName = "configuration")]
    public class Configuration
    {
        [XmlElement(ElementName = "userSettings")]
        public UserSettings UserSettings { get; set; }
    }
}
