using System;
using System.Collections.Generic;
using System.Xml.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable once CheckNamespace
namespace FASTER.Models
{
    [Serializable]
    public class SteamModCollection
    {
        [XmlElement(Order = 1)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CollectionName { get; set; } = "Main";

        [XmlElement(Order = 2, ElementName = "SteamMod")]
        public List<SteamMod> SteamMods = new List<SteamMod>();
    }

    [Serializable]
    public class SteamMod
    {
        private SteamMod()
        { }

        public SteamMod(uint workshopId, string name, string author, int steamLastUpdated, bool privateMod = false)
        {
            WorkshopId = workshopId;
            Name = name;
            Author = author;
            SteamLastUpdated = steamLastUpdated;
            PrivateMod = privateMod;
        }

        public uint WorkshopId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int SteamLastUpdated { get; set; }
        public int LocalLastUpdated { get; set; }
        public bool PrivateMod { get; set; }
        public string Status { get; set; } = "Not Installed";
        public string Size { get; set; } = "Unknown";
    }
}
