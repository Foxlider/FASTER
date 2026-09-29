using System.Text.RegularExpressions;
using System.Xml;

namespace FASTER.Models;

public static class ModUtilities
{
    public static string GetCompareString(string input)
    {
        input = input.Replace("@", "");
        input = Regex.Replace(input, "[^a-zA-Z0-9]", string.Empty);
        return input;
    }

    // We're parsing to ArmaMod instead of ProfileMod because there's more info on the ArmaMod object and we can thus re-use this function
    public static List<ArmaMod> ParseModsFromArmaProfileFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<ArmaMod>();
        var lines = File.ReadAllText(filePath);

        List<ArmaMod> extractedModlist = new();
        XmlDocument doc = new();
        doc.LoadXml(lines);
        var modNodes = doc.SelectNodes("//tr[@data-type=\"ModContainer\"]");
        for (int i = 0; i < modNodes?.Count; i++)
        {
            var modNode = modNodes.Item(i);
            if (modNode == null) continue;
            var modName = modNode.SelectSingleNode("td[@data-type='DisplayName']")?.InnerText ?? "Unknown";
            var modIdNode = modNode.SelectSingleNode("td/a[@data-type='Link']");
            Random r = new();
            var modId = (uint)(uint.MaxValue - r.Next(ushort.MaxValue / 2));
            var modIdS = modId.ToString();
            if (modIdNode != null)
            {
                modIdS = modIdNode.Attributes?.GetNamedItem("href")?.Value.Split("?id=")[1].Split('"')[0] ?? modIdS;
                uint.TryParse(modIdS, out modId);
            }

            ArmaMod mod = new()
            {
                WorkshopId = modId,
                Path = Path.Combine(AppSettings.Current.ModStagingDirectory, modIdS),
                Name = modName,
                IsLocal = modIdNode == null,
                Status = modIdNode == null ? ArmaModStatus.Local : ArmaModStatus.UpToDate,
            };
            extractedModlist.Add(mod);
        }

        return extractedModlist;
    }
}
