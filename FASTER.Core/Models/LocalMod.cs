using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FASTER.Models
{
    public class LocalMod
    {
        public LocalMod()
        { }
        private LocalMod(string name, string path, string author = "Unknown", string? website = null)
        {
            Name = name;
            Path = path;
            Author = author;
            Website = website ?? string.Empty;
        }

        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        public static List<LocalMod> GetLocalMods(bool serverPathOnly = false)
        {
            var localMods = new List<LocalMod>();

            List<string> foldersToSearch = new();

            if (serverPathOnly && !string.IsNullOrEmpty(AppSettings.Current.ServerPath))
            { foldersToSearch.Add(AppSettings.Current.ServerPath); }

            if (!serverPathOnly && AppSettings.Current.LocalModFolders != null)
            { foldersToSearch.AddRange(AppSettings.Current.LocalModFolders.Where(folder => folder != null && folder != AppSettings.Current.ServerPath)); }

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
                {
                    FASTER.Services.Telemetry.TrackError(e, new Dictionary<string, string> { { "Name", AppSettings.Current.SteamUserName } });
                    Console.WriteLine($"Could not list local mods in {localModFolder}: {e.Message}");
                }
            }

            return localMods;
        }
    }
}
