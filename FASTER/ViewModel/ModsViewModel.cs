using FASTER.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FASTER.ViewModel
{
    public class ModsViewModel
    {
        public ModsViewModel()
        { ModsCollection = Properties.Settings.Default.armaMods ?? new ArmaModCollection(); }

        public ArmaModCollection ModsCollection { get; set; }


        internal void DisplayMessage(string msg)
        {
            MainWindow.Instance.IFlyout.IsOpen         = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

        public void UnloadData()
        { Properties.Settings.Default.armaMods = ModsCollection; }

        public void AddSteamMod()
        {
            Random r = new Random();
            ModsCollection.ArmaMods.Add(new ArmaMod
            {
                Name       = "Yeet",
                Path       = "oui",
                Author     = "Honhon",
                WorkshopId = Convert.ToUInt32(r.Next(0, int.MaxValue)),
                IsLocal    = false,
                Size       = Convert.ToInt64(r.Next(0, int.MaxValue))
            });
        }

        public void AddLocalMod()
        {
            Random r = new Random();
            ModsCollection.ArmaMods.Add(new ArmaMod
            {
                Name       = "Yeet",
                Path       = "oui",
                Author     = "Honhon",
                WorkshopId = Convert.ToUInt32(r.Next(0, int.MaxValue)),
                IsLocal    = true,
                Size       = Convert.ToInt64(r.Next(0, int.MaxValue))
            });
        }

        internal void DeleteMod(ArmaMod mod)
        {
            if (mod == null)
                return;

            //TODO delete folder
            ModsCollection.ArmaMods.Remove(mod);
        }
        public void OpenModPage(ArmaMod mod)
        {
            if (mod == null)
                return;

            var url = "https://steamcommunity.com/workshop/filedetails/?id=" + mod.WorkshopId;

            try
            { Process.Start(url); }
            catch
            {
                try
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                catch
                { DisplayMessage($"Could not open \"{url}\""); }
            }
        }

        internal async Task OpenLauncherFile()
        {
            string modsFile = Functions.SelectFile("Arma 3 Launcher File|*.html");

            if (string.IsNullOrEmpty(modsFile)) return;
            using StreamReader dataReader = new StreamReader(modsFile);
            
            var tasklist = new List<Task<ArmaMod>>();

            var lines = (await File.ReadAllLinesAsync(modsFile)).ToList();

            var extractedModlist = from line in lines
                                   where line.Contains("steamcommunity.com/sharedfiles/filedetails")
                                   select line.Split("?id=")
                                   into extract
                                   select extract[1].Split('"')[0];

            foreach (string modIdS in extractedModlist)
            {
                if (!uint.TryParse(modIdS, out uint modId)) continue; 

                if(ModsCollection.ArmaMods.FirstOrDefault(m => m.WorkshopId == modId) != null)
                    continue;
                var mod = new ArmaMod
                    {WorkshopId = modId};

                if(!await Task.Run(() => mod.IsOnWorkshop()))
                    continue;

                ModsCollection.ArmaMods.Add(mod);

                _ = Task.Run(() => ModsCollection.ArmaMods.FirstOrDefault(m => m.WorkshopId == modId)?.UpdateInfos());
            }
        }

        public void OpenModFolder(ArmaMod mod)
        {
            if (mod == null || !Directory.Exists(mod.Path))
            {
                DisplayMessage($"Could not open folder \"{mod?.Path}\"");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = mod.Path,
                FileName  = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        public void CheckForUpdates()
        {
            foreach (ArmaMod mod in ModsCollection.ArmaMods)
            { Task.Run(() => mod.UpdateInfos()); }
        }
        public void UpdateAll()
        {
            foreach (ArmaMod mod in ModsCollection.ArmaMods)
            { Task.Run(() => mod.UpdateMod()); }
        }
        public void ImportFromSteam()
        {
            throw new NotImplementedException();
        }
    }
}
