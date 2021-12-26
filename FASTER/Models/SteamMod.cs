
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FASTER.Models
{
    [Serializable]
    public class SteamModCollection
    {
        [XmlElement(Order = 1)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CollectionName { get; set; } = "Main";

        [XmlElement(Order = 2, ElementName = "SteamMod")]
        public List<SteamMod> SteamMods = new();
    }

    [Serializable]
    public class SteamMod
    {
        private SteamMod()
        {}

        public SteamMod(uint workshopId, string name, string author, int steamLastUpdated, bool privateMod = false)
        {
            WorkshopId = workshopId;
            Name = name;
            Author = author;
            SteamLastUpdated = steamLastUpdated;
            PrivateMod = privateMod;

            Task.Factory.StartNew(InitModSize);
        }

        private static string GetModSize(uint workshopId)
        {
            var modFolder = Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410", workshopId.ToString());
            if (!Directory.Exists(modFolder)) return "Unknown";

            double fullSize = GetDirectorySize(modFolder);

            string[] sizes = { " B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (fullSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fullSize /= 1024.0;
            }
            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{fullSize,7:F} {sizes[order],-2}";
        }

        private void InitModSize()
        { Size = GetModSize(WorkshopId); }

        static long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);
            return a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }

        public uint WorkshopId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int SteamLastUpdated { get; set; }
        public int LocalLastUpdated { get; set; }
        public bool PrivateMod { get; set; }
        public string Status { get; set; } = "Not Installed";
        public string Size { get; set; } = "Unknown";


        public static List<SteamMod> GetSteamMods()
        {
            List<SteamMod> currentSteamMods = new();

            if (Properties.Settings.Default.steamMods == null) return currentSteamMods;

            Properties.Settings.Default.Reload();
            currentSteamMods = Properties.Settings.Default.steamMods?.SteamMods;

            return currentSteamMods;
        }

        public static uint SteamIdFromUrl(string modUrl)
        {
            var uri = new Uri(modUrl);
            var modID = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("id");
            return uint.Parse(modID);
        }

        public static Tuple<string, string, int> GetModInfo(uint modId)
        {
            var modInfo = SteamWebApi.GetSingleFileDetails(modId);
            string author = null;
            int steamUpdateTime = 0;
            try
            {
                if (modInfo != null) 
                { author = SteamWebApi.GetPlayerSummaries(modInfo.SelectToken("creator").ToString())?.SelectToken("personaname")?.ToString(); }
            }
            catch 
            { author = "Unknown"; }

            try
            {
                if (modInfo?.SelectToken("time_updated") != null) 
                { steamUpdateTime = int.Parse(modInfo.SelectToken("time_updated").ToString()); }
            }
            catch 
            { steamUpdateTime = 0; }

            var modName = modInfo?.SelectToken("title").ToString();
            
            return modInfo?.SelectToken("creator_appid").ToString() == "107410" ? new Tuple<string, string, int>(modName, author, steamUpdateTime) : null;
        }
    }
}
