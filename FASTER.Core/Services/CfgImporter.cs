using System.Globalization;
using System.Reflection;
using FASTER.Models;

namespace FASTER.Services;

internal static class CfgImporter
{
    public static List<Action> Read(object model, string text) => new ImportPlan(model, text).Build();

    // One plan owns validation and pending assignments for a single paste. Building
    // the plan never changes the model; callers apply it only after validation succeeds.
    private sealed class ImportPlan
    {
        private readonly object _model;
        private readonly CfgDocument _document;
        private readonly HashSet<string> _known;
        private readonly List<Action> _changes = [];
        private bool _headlessConfigured;

        public ImportPlan(object model, string text)
        {
            _model = model;
            _document = new CfgDocument(text);
            // The generator defines the settings represented by the form. Optional
            // settings with a different form representation are listed below.
            string schema = model is ServerCfg ? new ServerCfg().ProcessFile() : new BasicCfg().ProcessFile();
            _known = new CfgDocument(schema).Entries.Where(e => e.Children == null)
                .Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            _known.UnionWith(["requiredBuild", "missionHTTPDownloadBaseURL", "headlessClients[]", "localClient[]"]);
        }

        public List<Action> Build()
        {
            ReadEntries(_document.Entries, "");
            return _changes;
        }

        private void Set(string propertyName, object value)
        {
            var property = _model.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw new InvalidOperationException($"No form property for {propertyName}");
            _changes.Add(() => property.SetValue(_model, value));
        }

        private void ReadEntries(List<CfgDocument.Entry> entries, string scope)
        {
            foreach (var entry in entries)
            {
                try { ReadEntry(entry, scope); }
                catch (Exception ex) when (ex is FormatException or OverflowException)
                {
                    // Do not include the literal: it may contain a password.
                    throw new FormatException($"Cannot apply {scope + (scope.Length == 0 ? "" : ".")}{entry.Name}: {ex.Message}", ex);
                }
            }
        }

        private void ReadEntry(CfgDocument.Entry entry, string scope)
        {
            string key = entry.Name.ToLowerInvariant();
            if (entry.Children != null) { ReadClass(entry, scope, key); return; }
            string? propertyName = FormPropertyName(entry.Name, scope, key);
            if (propertyName == null) return;
            if (_model is ServerCfg && scope.Length == 0 && ReadServerSetting(entry, key)) return;
            if (_model is BasicCfg && key == "maxcustomfilesize") { ReadCustomFileSize(entry); return; }
            ReadProperty(entry, propertyName);
        }

        private void ReadClass(CfgDocument.Entry entry, string scope, string key)
        {
            if (scope.Length != 0) return;
            if (_model is ServerCfg && key == "missions")
            {
                Set(nameof(ServerCfg.MissionChecked), true);
                Set(nameof(ServerCfg.MissionContentOverride), _document.Raw(entry));
            }
            else if ((_model is ServerCfg && key is "advancedoptions" or "antiflood") ||
                     (_model is BasicCfg && key == "sockets"))
            {
                if (key == "antiflood") Set(nameof(ServerCfg.AntiFloodEnabled), true);
                ReadEntries(entry.Children!, key);
            }
        }

        private string? FormPropertyName(string name, string scope, string key) => scope switch
        {
            "" => _known.Contains(name) ? name : null,
            "advancedoptions" => key is "logobjectnotfound" or "skipdescriptionparsing" or "ignoremissionloaderrors" ? name : null,
            "antiflood" => key is "cycletime" or "cyclelimit" or "cyclehardlimit" or "enablekick" ? "AntiFlood" + name : null,
            "sockets" => key == "maxpacketsize" ? name : null,
            _ => null
        };

        private bool ReadServerSetting(CfgDocument.Entry entry, string key)
        {
            switch (key)
            {
                case "admins[]": case "motd[]": case "headlessclients[]": case "localclient[]":
                    ReadStringList(entry, key); return true;
                case "kickduplicate":
                    Set(nameof(ServerCfg.KickDuplicates), ReadBool(Scalar(entry))); return true;
                case "disablevon":
                    Set(nameof(ServerCfg.VonActivated), !ReadBool(Scalar(entry))); return true;
                case "verifysignatures":
                    Set(nameof(ServerCfg.VerifySignatures), ReadEnum(entry, ServerCfgArrays.VerifySignaturesStrings)); return true;
                case "allowedfilepatching":
                    Set(nameof(ServerCfg.AllowedFilePatching), ReadEnum(entry, ServerCfgArrays.AllowFilePatchingStrings)); return true;
                case "voncodec":
                    Set(nameof(ServerCfg.VonCodec), ReadEnum(entry, ServerCfgArrays.VonCodecStrings)); return true;
                case "kickclientsonslownetwork[]":
                    ReadNetworkFlags(entry); return true;
                case "requiredbuild":
                    Set(nameof(ServerCfg.RequiredBuildChecked), true); break;
                case "votethreshold":
                    double threshold = double.Parse(Scalar(entry), CultureInfo.InvariantCulture);
                    ValidateFinite(threshold);
                    Set(nameof(ServerCfg.VotingEnabled), threshold > 0); break;
            }
            return false;
        }

        private void ReadStringList(CfgDocument.Entry entry, string key)
        {
            string list = string.Join("\n", ReadArray(entry).Select(CfgDocument.StringValue));
            if (!_headlessConfigured && key is "headlessclients[]" or "localclient[]")
            {
                Set(nameof(ServerCfg.HeadlessClientEnabled), true);
                _headlessConfigured = true;
            }
            Set(entry.Name[..^2], list);
        }

        private void ReadNetworkFlags(CfgDocument.Entry entry)
        {
            var flags = ReadArray(entry).Select(ReadBool).ToArray();
            if (flags.Length != 4) throw new FormatException("Expected four network flags");
            // The form has one switch; retain mixed flags in the output unchanged.
            Set(nameof(ServerCfg.KickClientOnSlowNetwork), flags.All(f => f));
        }

        private void ReadCustomFileSize(CfgDocument.Entry entry)
        {
            ulong bytes = ulong.Parse(Scalar(entry), CultureInfo.InvariantCulture);
            if (bytes % 1000 != 0) throw new FormatException("The form supports whole KB (1000 bytes) for MaxCustomFileSize");
            Set(nameof(BasicCfg.MaxCustomFileSize), checked((ushort)(bytes / 1000)));
        }

        private void ReadProperty(CfgDocument.Entry entry, string propertyName)
        {
            var property = _model.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null || !property.CanWrite) return; // e.g. missionWhitelist has no form control.
            string literal = Scalar(entry);
            object value = ConvertLiteral(literal, property.PropertyType);
            if (value is double number) ValidateFinite(number);
            Set(property.Name, value);
        }
    }

    private static object ConvertLiteral(string literal, Type type)
    {
        if (type == typeof(string)) return CfgDocument.StringValue(literal);
        if (type == typeof(bool)) return ReadBool(literal);
        return Convert.ChangeType(literal, type, CultureInfo.InvariantCulture);
    }

    private static void ValidateFinite(double number)
    {
        if (!double.IsFinite(number)) throw new FormatException("Expected a finite number");
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
