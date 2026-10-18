using System.Globalization;
using System.Reflection;
using FASTER.Models;

namespace FASTER.Services;

internal static class CfgImporter
{
    public static List<Action> Read(object model, string text)
    {
        var document = new CfgDocument(text);
        // The generator defines the settings represented by the form. Optional settings
        // and settings with a different form representation are listed explicitly below.
        string schema = model is ServerCfg ? new ServerCfg().ProcessFile() : new BasicCfg().ProcessFile();
        var known = new CfgDocument(schema).Entries.Where(e => e.Children == null)
            .Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        known.UnionWith(["requiredBuild", "missionHTTPDownloadBaseURL", "headlessClients[]", "localClient[]"]);
        List<Action> changes = [];
        bool headlessConfigured = false;
        ReadEntries(document.Entries, "");
        return changes;

        void Set(string propertyName, object value)
        {
            var property = model.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw new InvalidOperationException($"No form property for {propertyName}");
            changes.Add(() => property.SetValue(model, value));
        }

        void ReadEntries(List<CfgDocument.Entry> entries, string scope)
        {
            foreach (var entry in entries)
            {
                string key = entry.Name.ToLowerInvariant();
                try
                {
                    if (entry.Children != null)
                    {
                        if (model is ServerCfg && scope.Length == 0 && key == "missions")
                        {
                            Set(nameof(ServerCfg.MissionChecked), true);
                            Set(nameof(ServerCfg.MissionContentOverride), document.Raw(entry));
                        }
                        else if (scope.Length == 0 && ((model is ServerCfg && key is "advancedoptions" or "antiflood") ||
                                                      (model is BasicCfg && key == "sockets")))
                        {
                            if (key == "antiflood") Set(nameof(ServerCfg.AntiFloodEnabled), true);
                            ReadEntries(entry.Children, key);
                        }
                        continue;
                    }
                    string propertyName = entry.Name;
                    if (scope == "advancedoptions" && key is not ("logobjectnotfound" or "skipdescriptionparsing" or "ignoremissionloaderrors")) continue;
                    if (scope == "antiflood")
                    {
                        if (key is not ("cycletime" or "cyclelimit" or "cyclehardlimit" or "enablekick")) continue;
                        propertyName = "AntiFlood" + entry.Name;
                    }
                    if (scope == "sockets" && key != "maxpacketsize") continue;
                    if (scope.Length == 0 && !known.Contains(entry.Name)) continue;
                    if (model is ServerCfg && scope.Length == 0)
                    {
                        switch (key)
                        {
                            case "admins[]": case "motd[]": case "headlessclients[]": case "localclient[]":
                                string list = string.Join("\n", ReadArray(entry).Select(CfgDocument.StringValue));
                                if (!headlessConfigured && key is "headlessclients[]" or "localclient[]")
                                {
                                    Set(nameof(ServerCfg.HeadlessClientEnabled), true);
                                    headlessConfigured = true;
                                }
                                Set(entry.Name[..^2], list);
                                continue;
                            case "kickduplicate":
                                Set(nameof(ServerCfg.KickDuplicates), ReadBool(Scalar(entry))); continue;
                            case "disablevon":
                                Set(nameof(ServerCfg.VonActivated), !ReadBool(Scalar(entry))); continue;
                            case "verifysignatures":
                                Set(nameof(ServerCfg.VerifySignatures), ReadEnum(entry, ServerCfgArrays.VerifySignaturesStrings)); continue;
                            case "allowedfilepatching":
                                Set(nameof(ServerCfg.AllowedFilePatching), ReadEnum(entry, ServerCfgArrays.AllowFilePatchingStrings)); continue;
                            case "voncodec":
                                Set(nameof(ServerCfg.VonCodec), ReadEnum(entry, ServerCfgArrays.VonCodecStrings)); continue;
                            case "kickclientsonslownetwork[]":
                                var flags = ReadArray(entry).Select(ReadBool).ToArray();
                                if (flags.Length != 4) throw new FormatException("Expected four network flags");
                                // The form has one switch; retain mixed flags in the output unchanged.
                                Set(nameof(ServerCfg.KickClientOnSlowNetwork), flags.All(f => f)); continue;
                            case "requiredbuild": Set(nameof(ServerCfg.RequiredBuildChecked), true); break;
                            case "votethreshold":
                                double threshold = double.Parse(Scalar(entry), CultureInfo.InvariantCulture);
                                if (!double.IsFinite(threshold)) throw new FormatException("Expected a finite number");
                                Set(nameof(ServerCfg.VotingEnabled), threshold > 0); break;
                        }
                    }
                    if (model is BasicCfg && key == "maxcustomfilesize")
                    {
                        ulong bytes = ulong.Parse(Scalar(entry), CultureInfo.InvariantCulture);
                        if (bytes % 1000 != 0) throw new FormatException("The form supports whole KB (1000 bytes) for MaxCustomFileSize");
                        Set(nameof(BasicCfg.MaxCustomFileSize), checked((ushort)(bytes / 1000)));
                        continue;
                    }
                    var property = model.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property == null || !property.CanWrite) continue; // e.g. missionWhitelist has no form control.
                    string literal = Scalar(entry);
                    object value = property.PropertyType == typeof(string) ? CfgDocument.StringValue(literal)
                        : property.PropertyType == typeof(bool) ? ReadBool(literal)
                        : Convert.ChangeType(literal, property.PropertyType, CultureInfo.InvariantCulture);
                    if (value is double number && !double.IsFinite(number)) throw new FormatException("Expected a finite number");
                    Set(property.Name, value);
                }
                catch (Exception ex) when (ex is FormatException or OverflowException)
                {
                    // Do not include the literal: it may contain a password.
                    throw new FormatException($"Cannot apply {scope + (scope.Length == 0 ? "" : ".")}{entry.Name}: {ex.Message}", ex);
                }
            }
        }
    }

    private static string Scalar(CfgDocument.Entry entry) => entry.Values.Count == 1
        ? entry.Values[0] : throw new FormatException("Expected a single literal value");

    private static bool ReadBool(string value) => value.ToLowerInvariant() switch
    {
        "true" or "1" => true,
        "false" or "0" => false,
        _ => throw new FormatException("Expected 0, 1, true or false")
    };

    private static string ReadEnum(CfgDocument.Entry entry, string[] options)
    {
        if (!int.TryParse(Scalar(entry), NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) ||
            index < 0 || index >= options.Length) throw new FormatException($"Expected an index from 0 to {options.Length - 1}");
        return options[index];
    }

    private static List<string> ReadArray(CfgDocument.Entry entry)
    {
        var values = entry.Values;
        if (values.Count < 2 || values[0] != "{" || values[^1] != "}") throw new FormatException("Expected an array in braces");
        List<string> result = [];
        for (int i = 1; i < values.Count - 1; i++)
        {
            if ((i & 1) == 0)
            {
                if (values[i] != ",") throw new FormatException("Expected a comma between array values");
            }
            else result.Add(values[i]);
        }
        return result;
    }
}
