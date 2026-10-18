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
    public class ModsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private const string WorkshopFileDetailsUrl = "https://steamcommunity.com/workshop/filedetails/?id="; // NOSONAR - stable public service endpoint, intentionally compiled in

        public ModsViewModel()
        {
            ModsCollection = AppSettings.Current.ArmaMods ??= new ArmaModCollection();
            SteamUpdaterViewModel.Instance.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(SteamUpdaterViewModel.IsDownloading))
                    PropertyChanged?.Invoke(this, new(nameof(IsBusy)));
            };
        }

        private bool _maintaining;
        public bool IsBusy => _maintaining || SteamUpdaterViewModel.Instance.IsDownloading;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private async Task MaintainAsync(Func<Task> action)
        {
            if (IsBusy) return;
            _maintaining = true;
            SteamUpdaterViewModel.Instance.IsMaintaining = true;
            PropertyChanged?.Invoke(this, new(nameof(IsBusy)));
            try { await action(); }
            finally { SteamUpdaterViewModel.Instance.IsMaintaining = false; _maintaining = false; PropertyChanged?.Invoke(this, new(nameof(IsBusy))); }
        }

        public Task PurgeAndReinstallAll(CancellationToken cancellationToken = default) => MaintainAsync(() => PurgeAllCore(cancellationToken));
        public Task PurgeUnusedMods(CancellationToken cancellationToken = default) => MaintainAsync(() => PurgeUnusedCore(cancellationToken));
        public Task PurgeSelectedAsync(CancellationToken cancellationToken = default) => MaintainAsync(async () =>
        {
            if (!await AppServices.Dialogs.ShowConfirmationAsync(this, "Purge selected mods",
                "Delete and download the selected Workshop mods again? Local mods will be kept.")) return;
            cancellationToken.ThrowIfCancellationRequested();
            PurgeAndReinstallSelectedMods();
            await UpdateSelectedMods();
        });

        public ArmaModCollection ModsCollection { get; set; }

        public Task RefreshSizesAsync() => ModsCollection.RefreshSizesAsync();

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

            FASTER.Services.Telemetry.TrackEvent("Mods - Clicked AddSteamMod", new Dictionary<string, string>
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

        public Task AddLocalModAsync() => MaintainAsync(AddLocalModCore);

        private async Task AddLocalModCore()
        {
            var localPath = await AppServices.Files.PickFolderAsync(AppSettings.Current.ModStagingDirectory);
            if (string.IsNullOrWhiteSpace(localPath)) return;

            IProgressDialog? progress = null;
            var errors = new List<string>();
            try
            {
                localPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(localPath));
                if (!Directory.Exists(localPath))
                    throw new DirectoryNotFoundException($"The selected mod folder no longer exists: {localPath}");
                var children = Directory.GetDirectories(localPath).Where(path => Path.GetFileName(path).StartsWith('@')).ToList();
                var oldPaths = !Path.GetFileName(localPath).StartsWith('@') && children.Count > 0 ? children : [localPath];
                var total = oldPaths.Count;
                progress = await AppServices.Dialogs.ShowProgressAsync(this, "Local Mod", "Adding local mod(s)...");
                progress.Maximum = total;

                if (string.IsNullOrWhiteSpace(AppSettings.Current.ModStagingDirectory))
                    throw new IOException("Set a mod staging directory in Settings before adding local mods.");
                var staging = ResolveDirectoryPath(AppSettings.Current.ModStagingDirectory);
                await Task.Run(() => Directory.CreateDirectory(staging));
                var completed = 0;
                foreach (var oldPath in oldPaths)
                {
                    var name = Path.GetFileName(oldPath).TrimStart('@');
                    try
                    {
                        var source = ResolveDirectoryPath(oldPath);
                        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                        if (staging.Equals(source, comparison) ||
                            staging.StartsWith(source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison))
                            throw new IOException("The selected folder contains the mod staging directory. Select an individual mod folder to avoid a directory loop.");

                        uint modId;
                        string newPath;
                        do
                        {
                            modId = (uint)Random.Shared.NextInt64(uint.MaxValue / 2L, uint.MaxValue);
                            newPath = Path.Combine(staging, modId.ToString());
                        } while (ModsCollection.ArmaMods.Any(mod => mod.WorkshopId == modId) ||
                            File.Exists(newPath) || Directory.Exists(newPath) || new DirectoryInfo(newPath).LinkTarget != null);

                        progress.SetMessage($"Adding {name}: {completed + 1} / {total}");
                        await Task.Run(() =>
                        {
                            if (!Directory.Exists(source))
                                throw new DirectoryNotFoundException($"The mod folder no longer exists: {source}");
                            Directory.CreateSymbolicLink(newPath, source);
                        });

                        ModsCollection.AddSteamMod(new ArmaMod
                        {
                            WorkshopId = modId, Name = string.IsNullOrEmpty(name) ? "Local mod" : name,
                            Path = newPath, Author = "Unknown", IsLocal = true, Status = ArmaModStatus.Local
                        });
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
                    {
                        errors.Add($"{name}: {ex.Message}");
                    }
                    finally { progress.SetProgress(++completed); }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                errors.Add(ex.Message);
            }
            finally
            {
                if (progress != null) await progress.CloseAsync();
            }

            if (errors.Count > 0)
                await AppServices.Dialogs.ShowMessageAsync(this, "Could not add local mods", string.Join("\n", errors));
        }

        private static string ResolveDirectoryPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath)!;
            var resolved = root;
            foreach (var part in fullPath[root.Length..].Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
            {
                resolved = Path.Combine(resolved, part);
                if (Directory.Exists(resolved))
                    resolved = Directory.ResolveLinkTarget(resolved, true)?.FullName ?? resolved;
            }
            return Path.TrimEndingDirectorySeparator(resolved);
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

        internal Task DeleteAllMods() => MaintainAsync(DeleteAllModsCore);

        private async Task DeleteAllModsCore()
        {
            var answer = await AppServices.Dialogs.ShowInputAsync(this, "Are you sure you want to delete all mods?", "Write \"yes\" and press OK if you wish to continue.");

            if (string.IsNullOrEmpty(answer) || !answer.Equals("yes"))
                return;

            FASTER.Services.Telemetry.TrackEvent("Mods - Clicked DeleteAllMods", new Dictionary<string, string>
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
            { Platform.Current.OpenUrl(url); }
            catch
            { DisplayMessage($"Could not open \"{url}\""); }
        }

        internal Task OpenLauncherFile() => MaintainAsync(OpenLauncherFileCore);

        private async Task OpenLauncherFileCore()
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
            FASTER.Services.Telemetry.TrackEvent("Mods - Clicked UpdateAll", new Dictionary<string, string>
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

        private async Task PurgeAllCore(CancellationToken cancellationToken)
        {
            var answer = await AppServices.Dialogs.ShowInputAsync(this, "Are you sure you want to purge all mods?", "Write \"yes\" and press OK to delete all folders in the Mod Staging Directory and re-download everything.");

            if (string.IsNullOrEmpty(answer?.Trim()) || !answer.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
                return;

            FASTER.Services.Telemetry.TrackEvent("Mods - Clicked PurgeAndReinstallAll", new Dictionary<string, string>
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
                    cancellationToken.ThrowIfCancellationRequested();
                    if (localModFolderNames.Contains(Path.GetFileName(dir)) || ModsCollection.ArmaMods.Any(m => m.IsLocal && Path.GetFullPath(m.Path) == Path.GetFullPath(dir)))
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

        private async Task PurgeUnusedCore(CancellationToken cancellationToken)
        {
            // Without profiles there is no usage data, so bail out instead of treating every mod as unused.
            var profiles = AppSettings.Current.Profiles;
            if (profiles == null)
            {
                DisplayMessage("No profiles found, cannot determine unused mods.");
                return;
            }

            var usedIds = profiles
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
            { cancellationToken.ThrowIfCancellationRequested(); DeleteMod(mod); }
        }
    }
}
