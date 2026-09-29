using FASTER.Models;
using FASTER.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FASTER.ViewModel
{
    public class ModsViewModel
    {
        public ModsViewModel()
        {
            ModsCollection = AppSettings.Current.ArmaMods ?? new ArmaModCollection();
        }

        public ArmaModCollection ModsCollection { get; set; }

        internal void DisplayMessage(string msg)
        {
            Ui.Current.DisplayMessage(msg);
        }

        public void UnloadData()
        {
            AppSettings.Current.ArmaMods = ModsCollection;
            AppSettings.Current.Save();
        }

        public async Task AddSteamMod()
        {
            var modID = await AppServices.Dialogs.ShowInputAsync(this, "Add Steam Mod", "Please enter the mod ID or mod URL");

            if (string.IsNullOrEmpty(modID))
                return;

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
                Path = Path.Combine(AppSettings.Current.ModStagingDirectory, modID),
                IsLocal = false
            };

            ModsCollection.AddSteamMod(mod);
        }

        public async Task AddLocalModAsync()
        {
            var localPath = await AppServices.Files.PickFolderAsync(AppSettings.Current.ModStagingDirectory);

            if (string.IsNullOrEmpty(localPath))
                return;

            if (!Directory.Exists(localPath))
                return;

            var oldPaths = new List<string>();
            if (!Path.GetFileName(localPath).StartsWith('@') && Directory.GetDirectories(localPath).Where((file) => Path.GetFileName(file).StartsWith('@')).ToList().Count > 0)
            {
                oldPaths = Directory.GetDirectories(localPath).Where((file) => Path.GetFileName(file).StartsWith('@')).ToList();
            }
            else
            {
                oldPaths.Add(localPath);
            }

            var progress = await AppServices.Dialogs.ShowProgressAsync(this, "Local Mod", "Copying mod(s)...");
            progress.Maximum = oldPaths.Count;
            foreach (var oldPath in oldPaths.Where((path) => Directory.Exists(path)))
            {
                uint modID = 0;
                var existingIds = ModsCollection.ArmaMods.Select(mod => mod.WorkshopId);
                while (modID == 0 || existingIds.Contains(modID))
                {
                    Random r = new();
                    modID = (uint)(uint.MaxValue - r.Next(ushort.MaxValue / 2));
                }
                var newPath = Path.Combine(AppSettings.Current.ModStagingDirectory, modID.ToString());
                if (Directory.Exists(newPath))
                {
                    await AppServices.Dialogs.ShowMessageAsync(this, "Warning", $"Directory already exists for {oldPath[(oldPath.LastIndexOf("@", StringComparison.Ordinal) + 1)..]}.");
                    continue;
                }

                await Task.Factory.StartNew(() =>
                {
                    Directory.CreateSymbolicLink(newPath, oldPath);
                    var progressDone = oldPaths.IndexOf(oldPath);
                    progress.SetMessage($"Copying mod from {oldPath}\n{progressDone} / {progress.Maximum}");
                    progress.SetProgress(progressDone);
                });

                var newMod = new ArmaMod
                {
                    WorkshopId = modID,
                    Name = oldPath[(oldPath.LastIndexOf("@", StringComparison.Ordinal) + 1)..],
                    Path = newPath,
                    Author = "Unknown",
                    IsLocal = true,
                    Status = ArmaModStatus.Local
                };

                ModsCollection.AddSteamMod(newMod);
            }
            await progress.CloseAsync();
        }

        internal void DeleteMod(ArmaMod mod)
        {
            if (mod == null)
                return;

            ModsCollection.DeleteSteamMod(mod.WorkshopId);
        }

        internal void DeleteSelectedMods()
        {
            var selectedArmaMods = new List<ArmaMod>(ModsCollection.ArmaMods.Where(m => m.IsSelected));
            foreach (var mod in selectedArmaMods)
            {
                DeleteMod(mod);
            }
        }

        internal async Task DeleteAllMods()
        {
            var answer = await AppServices.Dialogs.ShowInputAsync(this, "Are you sure you want to delete all mods?", "Write \"yes\" and press OK if you wish to continue.");

            if (string.IsNullOrEmpty(answer) || !answer.Equals("yes"))
                return;

            var copyArmaMods = new List<ArmaMod>(ModsCollection.ArmaMods);
            foreach (var mod in copyArmaMods)
            {
                DeleteMod(mod);
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
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                catch
                { DisplayMessage($"Could not open \"{url}\""); }
            }
        }

        internal async Task OpenLauncherFile()
        {
            string? modsFile = await AppServices.Files.PickModPresetFileAsync();

            if (string.IsNullOrEmpty(modsFile)) return;

            var extractedModList = ModUtilities.ParseModsFromArmaProfileFile(modsFile);

            foreach (var extractedMod in extractedModList)
            {
                var mod = ModsCollection.ArmaMods.FirstOrDefault(m => m.WorkshopId == extractedMod.WorkshopId || ModUtilities.GetCompareString(extractedMod.Name) == ModUtilities.GetCompareString(m.Name));
                if (mod != null)
                    continue;

                if (extractedMod.IsLocal)
                {
                    ModsCollection.ArmaMods.Add(extractedMod);
                    continue;
                }

                if (!await Task.Run(() => extractedMod.IsOnWorkshop()))
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

            Platform.Current.OpenFolder(mod.Path);
        }
        public void CheckForUpdates()
        {
            foreach (ArmaMod mod in ModsCollection.ArmaMods)
            { Task.Run(() => mod.UpdateInfos()); }
        }

        public async Task UpdateSelectedMods()
        {
            Ui.Current.NavigateToConsole();
            var ans = await Ui.Current.RunModsUpdaterAsync(new ObservableCollection<ArmaMod>(ModsCollection.ArmaMods.Where(m => m.IsSelected)));
            if (ans == UpdateState.LoginFailed)
                DisplayMessage("Steam Login Failed");
        }

        public async Task UpdateAll()
        {
            Ui.Current.NavigateToConsole();
            var ans = await Ui.Current.RunModsUpdaterAsync(ModsCollection.ArmaMods);
            if (ans == UpdateState.LoginFailed)
                DisplayMessage("Steam Login Failed");
        }
    }
}
