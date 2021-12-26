using FASTER.Models;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FASTER.ViewModel
{
    public class ModsViewModel
    {
        public ModsViewModel()
        { ModsCollection = Properties.Settings.Default.armaMods ?? new ArmaModCollection(); }

        public ArmaModCollection ModsCollection { get; set; }

        public IDialogCoordinator DialogCoordinator { get; set; }


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
            var modID = await DialogCoordinator.ShowInputAsync(this, "Add Steam Mod", "Please enter the mod ID or mod URL");

            if (string.IsNullOrEmpty(modID))
                return;

            Analytics.TrackEvent("Mods - Clicked AddSteamMod", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName},
                {"Mod", modID}
            });

            //Cast link to mod ID
            if (modID.Contains("steamcommunity.com") && modID.Contains("id="))
            {
                var uri = new Uri(modID);
                modID = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("id");
            }

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

        public async Task AddLocalModAsync()
        {
            var localPath = MainWindow.Instance.SelectFolder(Properties.Settings.Default.modStagingDirectory);

            if (string.IsNullOrEmpty(localPath))
                return;

            Random r = new();
            var modID   = (uint) (uint.MaxValue - r.Next(ushort.MaxValue / 2));
            var newPath = Path.Combine(Properties.Settings.Default.modStagingDirectory, modID.ToString());
            var oldPath = localPath;
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            if (Directory.Exists(oldPath))
            {
                var progress = await DialogCoordinator.ShowProgressAsync(this, "Local Mod", "Copying mod...");
                var files = Directory.EnumerateFiles(oldPath, "*", SearchOption.AllDirectories).ToList();
                progress.Maximum = files.Count;
                await Task.Factory.StartNew(() =>
                {
                    foreach (var file in files)
                    {
                        var newFile = file.Replace(oldPath, newPath);
                        if (!Directory.Exists(Path.GetDirectoryName(newFile)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newFile));

                        File.Copy(file, newFile, true);
                        var progressDone = files.IndexOf(file);
                        progress.SetMessage($"Copying mod from {oldPath}\n{progressDone} / {progress.Maximum}");
                        progress.SetProgress(progressDone);
                    }
                });
                
                await progress.CloseAsync();
            }

            var newMod = new ArmaMod
            {
                WorkshopId = modID,
                Name       = localPath.Contains("@") 
                                ? localPath[(localPath.LastIndexOf("@", StringComparison.Ordinal) + 1)..]
                                : Path.GetFileName(localPath),
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
            using StreamReader dataReader = new(modsFile);
            
            var lines = (await File.ReadAllLinesAsync(modsFile))
                                   .AsEnumerable()
                                   .Where(l => l.Contains("steamcommunity.com/sharedfiles/filedetails"));

            foreach (string line in lines)
            {
                var link = XElement.Parse(line).Attribute("href").Value;
                if (link == null)
                    continue;
                var modIdS = System.Web.HttpUtility.ParseQueryString(new Uri(link).Query).Get("id");
                if (!uint.TryParse(modIdS, out uint modId)) continue; 

                if(ModsCollection.ArmaMods.FirstOrDefault(m => m.WorkshopId == modId) != null)
                    continue;

                var mod = new ArmaMod
                {
                    WorkshopId = modId,
                    Path = Path.Combine(Properties.Settings.Default.modStagingDirectory, modIdS),
                    IsLocal = false, 
                    Status = ArmaModStatus.UpdateRequired
                };

                if(!await Task.Run(() => mod.IsOnWorkshop()))
                    continue;

                ModsCollection.AddSteamMod(mod);
            }
        }

        public void OpenModFolder(ArmaMod mod)
        {
            if (mod == null || !Directory.Exists(mod.Path))
            {
                DisplayMessage($"Could not open folder \"{mod?.Path}\"");
                return;
            }

            ProcessStartInfo startInfo = new()
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
            Analytics.TrackEvent("Mods - Clicked UpdateAll", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName}
            });

            MainWindow.Instance.NavigateToConsole();
            var ans = await MainWindow.Instance.SteamUpdaterViewModel.RunModsUpdater(ModsCollection.ArmaMods);
            if(ans == UpdateState.LoginFailed) 
                DisplayMessage("Steam Login Failed");
        }
    }
}
