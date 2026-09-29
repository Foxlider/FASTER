using FASTER.Models;
using FASTER.Services;
using Microsoft.AppCenter.Analytics;
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
        private const string WorkshopFileDetailsUrl = "https://steamcommunity.com/workshop/filedetails/?id=";

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

            Analytics.TrackEvent("Mods - Clicked AddSteamMod", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName},
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

            Analytics.TrackEvent("Mods - Clicked DeleteAllMods", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName}
            });
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

            var url = WorkshopFileDetailsUrl + mod.WorkshopId;

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
        public async Task CheckForUpdates()
        {
            Logger.Log("CheckForUpdates started.");
            foreach (ArmaMod mod in ModsCollection.ArmaMods)
            {
                Logger.Log($"  Checking mod {mod.WorkshopId} ({mod.Name})...");
                await Task.Run(() => mod.UpdateInfos());
                await Task.Delay(300);
            }
            Logger.Log("CheckForUpdates finished.");
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
            Analytics.TrackEvent("Mods - Clicked UpdateAll", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName}
            });

            Ui.Current.NavigateToConsole();
            var ans = await Ui.Current.RunModsUpdaterAsync(ModsCollection.ArmaMods);
            if (ans == UpdateState.LoginFailed)
                DisplayMessage("Steam Login Failed");
        }

        public void PurgeAndReinstallMod(ArmaMod mod)
        {
            if (mod == null) return;

            Logger.Log($"PurgeAndReinstallMod: {mod.WorkshopId} ({mod.Name}) path={mod.Path}");
            try
            {
                if (Directory.Exists(mod.Path))
                {
                    Directory.Delete(mod.Path, true);
                    Logger.Log($"  Deleted folder: {mod.Path}");
                }
                else
                    Logger.Log($"  Folder not found, skipping delete: {mod.Path}");
            }
            catch (Exception ex)
            {
                Logger.Log($"  ERROR deleting folder: {ex.Message}");
                DisplayMessage($"Could not delete folder for mod {mod.WorkshopId}");
            }

            mod.Status           = ArmaModStatus.UpdateRequired;
            mod.LocalLastUpdated = 0;
            mod.Size             = 0;
            AppSettings.Current.Save();
        }

        public void PurgeAndReinstallSelectedMods()
        {
            var selectedMods = new List<ArmaMod>(ModsCollection.ArmaMods.Where(m => m.IsSelected && !m.IsLocal));
            foreach (var mod in selectedMods)
                PurgeAndReinstallMod(mod);
        }

        public async Task PurgeAndReinstallAll()
        {
            var answer = await AppServices.Dialogs.ShowInputAsync(this, "Are you sure you want to purge all mods?", "Write \"yes\" and press OK to delete all folders in the Mod Staging Directory and re-download everything.");

            if (string.IsNullOrEmpty(answer?.Trim()) || !answer.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
                return;

            Analytics.TrackEvent("Mods - Clicked PurgeAndReinstallAll", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName}
            });

            var stagingDir = AppSettings.Current.ModStagingDirectory;
            Logger.Log($"PurgeAndReinstallAll: staging dir={stagingDir}");
            if (Directory.Exists(stagingDir))
            {
                var localModFolderNames = ModsCollection.ArmaMods.Where(m => m.IsLocal).Select(m => m.WorkshopId.ToString()).ToHashSet();

                foreach (var dir in Directory.GetDirectories(stagingDir))
                {
                    if (localModFolderNames.Contains(Path.GetFileName(dir)))
                    {
                        Logger.Log($"  Skipped (local mod): {dir}");
                        continue;
                    }

                    try
                    {
                        Directory.Delete(dir, true);
                        Logger.Log($"  Deleted: {dir}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"  ERROR deleting {dir}: {ex.Message}");
                        DisplayMessage($"Could not delete folder: {dir}");
                    }
                }
            }
            else
                Logger.Log("  Staging dir does not exist, nothing deleted.");

            foreach (var mod in ModsCollection.ArmaMods.Where(m => !m.IsLocal).ToList())
            {
                mod.Status           = ArmaModStatus.UpdateRequired;
                mod.LocalLastUpdated = 0;
                mod.Size             = 0;
                Logger.Log($"  Reset mod {mod.WorkshopId} ({mod.Name})");
            }
            AppSettings.Current.Save();

            Logger.Log("PurgeAndReinstallAll: launching UpdateAll...");
            Ui.Current.NavigateToConsole();
            var ans = await Ui.Current.RunModsUpdaterAsync(ModsCollection.ArmaMods);
            if (ans == UpdateState.LoginFailed)
                DisplayMessage("Steam Login Failed");
        }

        public async Task PurgeUnusedMods()
        {
            var usedIds = AppSettings.Current.Profiles
                .SelectMany(p => p.ProfileMods ?? Enumerable.Empty<ProfileMod>())
                .Where(m => m.ServerSideChecked || m.ClientSideChecked || m.HeadlessChecked || m.OptChecked)
                .Select(m => m.Id)
                .ToHashSet();

            var unusedMods = ModsCollection.ArmaMods
                .Where(m => !m.IsLocal && !usedIds.Contains(m.WorkshopId))
                .ToList();

            if (unusedMods.Count == 0)
            {
                DisplayMessage("No unused mods found.");
                return;
            }

            var result = await AppServices.Dialogs.ShowInputAsync(this,
                "Purge Unused Mods",
                $"Found {unusedMods.Count} unused mod(s). Type \"yes\" to confirm deletion.");
            if (result?.ToLower() != "yes") return;

            foreach (var mod in unusedMods)
                DeleteMod(mod);
        }
    }
}
