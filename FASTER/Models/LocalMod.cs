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
        {
        }

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

            List<string> foldersToSearch = new List<string>();

            if (serverPathOnly)
            {
                if (Properties.Options.Default.serverPath != string.Empty)
                    foldersToSearch.Add(Properties.Options.Default.serverPath);
            }

            if (!serverPathOnly)
            {
                if (Properties.Options.Default.localModFolders != null)
                {
                    foreach (var folder in Properties.Options.Default.localModFolders)
                    {
                        if (folder != null && folder != Properties.Options.Default.serverPath) foldersToSearch.Add(folder);
                    }
                }
            }

            if (foldersToSearch.Count > 0)
            {
                foreach (var localModFolder in foldersToSearch)
                {
                    try
                    {
                        var modFolders = Directory.GetDirectories(localModFolder, "@*");

                        localMods.AddRange(from modFolder in modFolders
                                           let name = modFolder.Substring(modFolder.LastIndexOf("@", StringComparison.Ordinal) + 1)
                                           let author = "Unknown"
                                           let website = "Unknown"
                                           select new LocalMod(name, modFolder, author, website));
                    }
                    catch (Exception) { /* ignored */ }
                }
            }

            return localMods;
        }
    }
}
