using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FASTER.Models
{
    [Serializable]
    internal class LocalMod
    {
        private LocalMod()
        { }

        private LocalMod(string name, string path, string author = "Unknown", string website = null)
        {
            Name = name;
            Path = path;
            Author = author;
            Website = website;
        }

        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        public static List<LocalMod> GetLocalMods(bool serverPathOnly = false)
        {
            var localMods = new List<LocalMod>();

            List<string> foldersToSearch = new();

            if (serverPathOnly && !string.IsNullOrEmpty(Properties.Settings.Default.serverPath))
            { foldersToSearch.Add(Properties.Settings.Default.serverPath); }

            if (!serverPathOnly && Properties.Settings.Default.localModFolders != null)
            { foldersToSearch.AddRange(Properties.Settings.Default.localModFolders.Where(folder => folder != null && folder != Properties.Settings.Default.serverPath)); }

            if (foldersToSearch.Count <= 0) return localMods;

            foreach (var localModFolder in foldersToSearch)
            {
                try
                {
                    var modFolders = Directory.GetDirectories(localModFolder, "@*");

                    localMods.AddRange(from modFolder in modFolders
                                       let name = modFolder[(modFolder.LastIndexOf("@", StringComparison.Ordinal) + 1)..]
                                       let author = "Unknown"
                                       let website = "Unknown"
                                       select new LocalMod(name, modFolder, author, website));
                }
                catch (Exception e)
                { Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } }); }
            }

            return localMods;
        }
    }
}
