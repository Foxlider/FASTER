using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace FASTER.Models;

public sealed class AppSettings
{
    private static readonly object s_lock = new();
    private static readonly JsonSerializerOptions s_json = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    private static AppSettings? s_current;
    public static AppSettings Current
    {
        get
        {
            lock (s_lock)
            {
                // Model constructors run while the file is being deserialized and read settings through here. Those nested reads get a throwaway so they can never poison the real instance or trigger a save of half-loaded data.
                if (s_loading) return new AppSettings();
                return s_current ??= Load();
            }
        }
    }

    public static string SettingsPath => PathOverrideForTests ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
        "FoxliCorp", "FASTER", "faster.json");

    internal static string? PathOverrideForTests { get; set; }

    public string ServerPath { get; set; } = string.Empty;
    public string SteamCMDPath { get; set; } = string.Empty;
    public string ModStagingDirectory { get; set; } = string.Empty;
    public string SteamUserName { get; set; } = string.Empty;
    public string SteamPassword { get; set; } = string.Empty;
    public bool FirstRun { get; set; } = true;
    public bool CheckForAppUpdates { get; set; } = false;
    public bool ClearSettings { get; set; } = false;
    public bool SetupRun { get; set; } = true;
    public string ServerBranch { get; set; } = "Stable";
    public ArmaModCollection? ArmaMods { get; set; }
    public SteamModCollection? SteamMods { get; set; }
    public List<LocalMod>? LocalMods { get; set; }
    public List<string>? LocalModFolders { get; set; }
    public bool ExcludeServerFolder { get; set; } = false;
    public bool CheckForModUpdates { get; set; } = true;
    public string SteamAPIKey { get; set; } = string.Empty;
    public ServerProfileCollection? Profiles { get; set; }
    public string Theme { get; set; } = "Dark.Blue";
    public string Font { get; set; } = "Segoe UI";
    public ArmaDeployment? Deployments { get; set; }
    public ushort CliWorkers { get; set; } = 10;
    public bool UsingPerfBinaries { get; set; } = false;
    public bool UsingContactDlc { get; set; } = false;
    public bool UsingGMDlc { get; set; } = false;
    public bool UsingPFDlc { get; set; } = false;
    public bool UsingCSLADlc { get; set; } = false;
    public bool UsingWSDlc { get; set; } = false;
    public bool UsingSPEDlc { get; set; } = false;
    public bool UsingRFDlc { get; set; } = false;
    public bool UsingEFDlc { get; set; } = false;
    public bool EnableDebugLog { get; set; } = false;
    public bool EnableAnalytics { get; set; } = true;

    public void InitializeForStartup()
    {
        if (ClearSettings)
            Reset();
        if (!FirstRun)
            SetupRun = false;
        SteamMods ??= new SteamModCollection();
        LocalMods ??= new List<LocalMod>();
        LocalModFolders ??= new List<string>();
        if (ArmaMods == null)
        {
            ArmaMods = new ArmaModCollection();
            foreach (var mod in SteamMods.SteamMods)
                ArmaMods.ArmaMods.Add(new ArmaMod {
                    WorkshopId = mod.WorkshopId, Name = mod.Name, Author = mod.Author,
                    Path = Path.Combine(SteamCMDPath, "steamapps", "workshop", "content", "107410", mod.WorkshopId.ToString()),
                    SteamLastUpdated = (ulong)Math.Max(0, mod.SteamLastUpdated),
                    LocalLastUpdated = (ulong)Math.Max(0, mod.LocalLastUpdated), PrivateMod = mod.PrivateMod, Status = mod.Status
                });
            uint localId = uint.MaxValue;
            foreach (var mod in LocalMods)
            {
                while (ArmaMods.ArmaMods.Any(m => m.WorkshopId == localId)) localId--;
                ArmaMods.ArmaMods.Add(new ArmaMod { WorkshopId = localId--, Name = mod.Name,
                    Author = mod.Author, Path = mod.Path, IsLocal = true, Status = ArmaModStatus.Local });
            }
        }
        Save();
    }

    public void CompleteSetup(string password, bool passwordChanged)
    {
        if (passwordChanged)
            SteamPassword = Encryption.Instance.EncryptData(password) ?? string.Empty;
        FirstRun = false;
        SetupRun = false;
        Save();
    }

    public void Save()
    {
        lock (s_lock)
        {
            // Saving while the file is still being loaded would persist half-loaded defaults over good data, so ignore it.
            if (s_loading) return;
            var path = SettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path, JsonSerializer.Serialize(this, s_json));
        }
    }

    public void Reload()
    {
        var fresh = Load();
        ServerPath = fresh.ServerPath;
        SteamCMDPath = fresh.SteamCMDPath;
        ModStagingDirectory = fresh.ModStagingDirectory;
        SteamUserName = fresh.SteamUserName;
        SteamPassword = fresh.SteamPassword;
        FirstRun = fresh.FirstRun;
        CheckForAppUpdates = fresh.CheckForAppUpdates;
        ClearSettings = fresh.ClearSettings;
        SetupRun = fresh.SetupRun;
        ServerBranch = fresh.ServerBranch;
        ArmaMods = fresh.ArmaMods;
        SteamMods = fresh.SteamMods;
        LocalMods = fresh.LocalMods;
        LocalModFolders = fresh.LocalModFolders;
        ExcludeServerFolder = fresh.ExcludeServerFolder;
        CheckForModUpdates = fresh.CheckForModUpdates;
        SteamAPIKey = fresh.SteamAPIKey;
        Profiles = fresh.Profiles;
        Theme = fresh.Theme;
        Font = fresh.Font;
        Deployments = fresh.Deployments;
        CliWorkers = fresh.CliWorkers;
        UsingPerfBinaries = fresh.UsingPerfBinaries;
        UsingContactDlc = fresh.UsingContactDlc;
        UsingGMDlc = fresh.UsingGMDlc;
        UsingPFDlc = fresh.UsingPFDlc;
        UsingCSLADlc = fresh.UsingCSLADlc;
        UsingWSDlc = fresh.UsingWSDlc;
        UsingSPEDlc = fresh.UsingSPEDlc;
        UsingRFDlc = fresh.UsingRFDlc;
        UsingEFDlc = fresh.UsingEFDlc;
        EnableDebugLog = fresh.EnableDebugLog;
        EnableAnalytics = fresh.EnableAnalytics;
    }

    public void Reset()
    {
        var defaults = new AppSettings();
        ReloadFrom(defaults);
    }

    public void Upgrade()
    {
        if (File.Exists(SettingsPath)) return;
        var imported = ImportLegacyUserConfig();
        if (imported != null)
            ReloadFrom(imported);
        Save();
    }

    private void ReloadFrom(AppSettings source)
    {
        ServerPath = source.ServerPath;
        SteamCMDPath = source.SteamCMDPath;
        ModStagingDirectory = source.ModStagingDirectory;
        SteamUserName = source.SteamUserName;
        SteamPassword = source.SteamPassword;
        FirstRun = source.FirstRun;
        CheckForAppUpdates = source.CheckForAppUpdates;
        ClearSettings = source.ClearSettings;
        SetupRun = source.SetupRun;
        ServerBranch = source.ServerBranch;
        ArmaMods = source.ArmaMods;
        SteamMods = source.SteamMods;
        LocalMods = source.LocalMods;
        LocalModFolders = source.LocalModFolders;
        ExcludeServerFolder = source.ExcludeServerFolder;
        CheckForModUpdates = source.CheckForModUpdates;
        SteamAPIKey = source.SteamAPIKey;
        Profiles = source.Profiles;
        Theme = source.Theme;
        Font = source.Font;
        Deployments = source.Deployments;
        CliWorkers = source.CliWorkers;
        UsingPerfBinaries = source.UsingPerfBinaries;
        UsingContactDlc = source.UsingContactDlc;
        UsingGMDlc = source.UsingGMDlc;
        UsingPFDlc = source.UsingPFDlc;
        UsingCSLADlc = source.UsingCSLADlc;
        UsingWSDlc = source.UsingWSDlc;
        UsingSPEDlc = source.UsingSPEDlc;
        UsingRFDlc = source.UsingRFDlc;
        UsingEFDlc = source.UsingEFDlc;
        EnableDebugLog = source.EnableDebugLog;
        EnableAnalytics = source.EnableAnalytics;
    }

    private static bool s_loading;

    private static AppSettings Load()
    {
        lock (s_lock)
        {
            // Model constructors must never trigger a nested load while deserializing, so bail out with defaults instead of recursing.
            if (s_loading) return new AppSettings();
            s_loading = true;
            try
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        var parsed = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), s_json);
                        if (parsed != null) return parsed;
                    }
                }
                catch
                { /* Corrupt or unreadable file, fall through to defaults and the legacy import. */ }

                var fresh = new AppSettings();
                var imported = ImportLegacyUserConfig();
                if (imported != null)
                    fresh.ReloadFrom(imported);
                return fresh;
            }
            finally
            { s_loading = false; }
        }
    }

    private static AppSettings? ImportLegacyUserConfig()
    {
        try
        {
            var localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            if (string.IsNullOrEmpty(localAppData)) return null;
            var foxliCorp = Path.Combine(localAppData, "FoxliCorp");
            if (!Directory.Exists(foxliCorp)) return null;

            var config = Directory.GetFiles(foxliCorp, "user.config", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
            if (config == null) return null;

            var doc = new XmlDocument();
            doc.Load(config);
            var result = new AppSettings();
            var type = typeof(AppSettings);

            foreach (XmlNode setting in doc.GetElementsByTagName("setting"))
            {
                try
                {
                    var name = setting.Attributes?.GetNamedItem("name")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;
                    // Legacy setting names are camelCase, ours are PascalCase, so match case-insensitively.
                    var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property == null || !property.CanWrite) continue;
                    var valueNode = setting.SelectSingleNode("value");
                    if (valueNode == null) continue;

                    if (setting.Attributes?.GetNamedItem("serializeAs")?.Value == "Xml")
                    {
                        var serializer = new XmlSerializer(property.PropertyType);
                        using var reader = new StringReader(valueNode.InnerXml);
                        property.SetValue(result, serializer.Deserialize(reader));
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(result, valueNode.InnerText);
                    }
                    else if (property.PropertyType == typeof(bool) && bool.TryParse(valueNode.InnerText, out bool b))
                    {
                        property.SetValue(result, b);
                    }
                    else if (property.PropertyType == typeof(ushort) && ushort.TryParse(valueNode.InnerText, out ushort u))
                    {
                        property.SetValue(result, u);
                    }
                }
                catch
                { /* One bad value must not kill the whole import, skip it. */ }
            }

            return result;
        }
        catch
        { return null; }
    }
}
