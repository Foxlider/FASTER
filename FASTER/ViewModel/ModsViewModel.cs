using FASTER.Models;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Xml;

namespace FASTER.ViewModel
{
    public class ModsViewModel
    {
        public ModsViewModel()
        { ModsCollection = Properties.Settings.Default.armaMods ?? new ArmaModCollection(); }

        public ArmaModCollection ModsCollection { get; set; }

        public IDialogCoordinator dialogCoordinator { get; set; }


        internal void DisplayMessage(string msg)
        {
            MainWindow.Instance.IFlyout.IsOpen         = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

        public void UnloadData()
        {
            Properties.Settings.Default.armaMods = ModsCollection;
            Properties.Settings.Default.Save();
        }

        public async Task AddSteamMod()
        {
            var modID = await dialogCoordinator.ShowInputAsync(this, "Add Steam Mod", "Please enter the mod ID or mod URL");

            if (string.IsNullOrEmpty(modID))
                return;

            //Cast link to mod ID
            if (modID.Contains("steamcommunity.com"))
                modID = modID.Split("?id=")[1].Split('&')[0];

            if (!uint.TryParse(modID, out uint modIDOut))
                return;

            var mod = new ArmaMod
            {
                WorkshopId = modIDOut,
                Path       = Path.Combine(Properties.Settings.Default.modStagingDirectory, modID),
                IsLocal = false
            };

            ModsCollection.AddSteamMod(mod);
        }

        public void AddLocalMod()
        {
            var localPath = MainWindow.Instance.SelectFolder(Properties.Settings.Default.modStagingDirectory);

            if (string.IsNullOrEmpty(localPath))
                return;

            Random r = new Random();
            var modID   = (uint) (uint.MaxValue - r.Next(ushort.MaxValue / 2));
            var newPath = Path.Combine(Properties.Settings.Default.modStagingDirectory, modID.ToString());
            var oldPath = localPath;
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            if (Directory.Exists(oldPath))
            {
                foreach (var file in Directory.EnumerateFiles(oldPath, "*", SearchOption.AllDirectories))
                {
                    var newFile = file.Replace(oldPath, newPath);
                    if (!Directory.Exists(Path.GetDirectoryName(newFile)))
                       Directory.CreateDirectory(Path.GetDirectoryName(newFile));

                    File.Copy(file, newFile, true);
                }
            }

            var newMod = new ArmaMod
            {
                WorkshopId = modID,
                Name       = localPath.Substring(localPath.LastIndexOf("@", StringComparison.Ordinal) + 1),
                Path       = newPath,
                Author     = "Unknown",
                IsLocal    = true,
                Status     = ArmaModStatus.Local
            };

            ModsCollection.AddSteamMod(newMod);
        }

        internal void DeleteMod(ArmaMod mod)
        {
            if (mod == null)
                return;
            
            ModsCollection.DeleteSteamMod(mod.WorkshopId);
        }
        internal void DeleteAllMods()
        {
            var copyArmaMods = new List<ArmaMod>(ModsCollection.ArmaMods);
            foreach (var mod in copyArmaMods)
            {
                if (!mod.IsLocal)
                {
                    ModsCollection.DeleteSteamMod(mod.WorkshopId);
                }
            }
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
            
            var lines = File.ReadAllText(modsFile);

            var extractedModlist = new List<ArmaMod>();
            var doc = new XmlDocument();
            doc.LoadXml(lines);
            var modNodes = doc.SelectNodes("//tr[@data-type=\"ModContainer\"]");
            for (int i = 0; i < modNodes.Count; i++)
            {
                var modNode = modNodes.Item(i);
                var modName = modNode.SelectSingleNode("td[@data-type='DisplayName']").InnerText;
                var modIdNode = modNode.SelectSingleNode("td/a[@data-type='Link']");
                Random r = new Random();
                var modId = (uint)(uint.MaxValue - r.Next(ushort.MaxValue / 2));
                var modIdS = modId.ToString();
                if (modIdNode != null)
                {
                    modIdS = modIdNode.Attributes.GetNamedItem("href").Value.Split("?id=")[1].Split('"')[0];
                    uint.TryParse(modIdS, out modId);
                }

                var mod = new ArmaMod
                {
                    WorkshopId = modId,
                    Path = Path.Combine(Properties.Settings.Default.modStagingDirectory, modIdS),
                    Name = modName,
                    IsLocal = modIdNode == null,
                    Status = modIdNode == null ? ArmaModStatus.Local : ArmaModStatus.UpToDate,
                };
                extractedModlist.Add(mod);
            }

            foreach (var extractedMod in extractedModlist)
            {
                var mod = ModsCollection.ArmaMods.FirstOrDefault(m => m.WorkshopId == extractedMod.WorkshopId || ProfileViewModel.GetCompareString(extractedMod.Name) == ProfileViewModel.GetCompareString(m.Name));
                if (mod != null)
                    continue;

                if (extractedMod.IsLocal)
                {
                    ModsCollection.ArmaMods.Add(extractedMod);
                    continue;
                }

                if(!await Task.Run(() => extractedMod.IsOnWorkshop()))
                    continue;

                ModsCollection.AddSteamMod(extractedMod);
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

        public async Task UpdateAll()
        {
            MainWindow.Instance.NavigateToConsole();
            var ans = await MainWindow.Instance.SteamUpdaterViewModel.RunModsUpdater(ModsCollection.ArmaMods);
            if(ans == UpdateState.LoginFailed) 
                DisplayMessage("Steam Login Failed");
        }
        public void ImportFromSteam()
        {
            throw new NotImplementedException();
        }
    }
}
