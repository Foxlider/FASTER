using FASTER.Models;
using FASTER.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FASTER.ViewModel
{
    public class DeploymentViewModel
    {
        private const string WorkshopFileDetailsUrl = "https://steamcommunity.com/workshop/filedetails/?id="; // NOSONAR - stable public service endpoint, intentionally compiled in

        public DeploymentViewModel()
        {
            if (AppSettings.Current.Deployments == null)
            {
                AppSettings.Current.Deployments = new ArmaDeployment();
                AppSettings.Current.Save();
            }

            Deployment = AppSettings.Current.Deployments;
        }

        public ArmaDeployment Deployment { get; set; }

        public async Task LoadDataAsync()
        {
            if (AppSettings.Current.ArmaMods is { } mods)
                await mods.RefreshSizesAsync();
            LoadData();
        }

        public async Task DeployModAsync(DeploymentMod mod)
        {
            var shouldDeploy = !mod.Marked;
            if (await EnsureInstallPathAsync() && mod.Marked != shouldDeploy)
                await Task.Run(() => DeployMod(mod));
        }

        public async Task DeployAllAsync()
        {
            if (await EnsureInstallPathAsync())
                await Task.Run(DeployAll);
        }

        private async Task<bool> EnsureInstallPathAsync()
        {
            if (Directory.Exists(Deployment.InstallPath)) return true;
            if (string.IsNullOrWhiteSpace(Deployment.InstallPath) && Directory.Exists(AppSettings.Current.ServerPath))
                Deployment.InstallPath = AppSettings.Current.ServerPath;
            else
                await InstallFolderClick();
            return Directory.Exists(Deployment.InstallPath);
        }

        /// <summary>
        /// Unload data
        /// </summary>
        public void UnloadData()
        {
            AppSettings.Current.Deployments = Deployment;
            AppSettings.Current.Save();
        }

        /// <summary>
        /// Load data on focus
        /// </summary>
        public void LoadData()
        {
            Deployment = AppSettings.Current.Deployments ?? new ArmaDeployment();
            if (string.IsNullOrWhiteSpace(Deployment.InstallPath) && Directory.Exists(AppSettings.Current.ServerPath))
                Deployment.InstallPath = AppSettings.Current.ServerPath;
            var armaMods = AppSettings.Current.ArmaMods;
            if (armaMods == null)
                return;
            foreach (var mod in armaMods.ArmaMods)
            {
                if (Deployment.DeployMods.Any(m => m.WorkshopId == mod.WorkshopId))
                    continue;
                Deployment.DeployMods.Add(new DeploymentMod(mod));
                AppSettings.Current.Save();
            }
            foreach (var mod in Deployment.DeployMods.ToArray())
            {
                if (armaMods.ArmaMods.All(m => m.WorkshopId != mod.WorkshopId))
                {
                    Deployment.DeployMods.Remove(mod);
                    AppSettings.Current.Save();
                    continue;
                }
                mod.UpdateInfos();
            }
        }


        /// <summary>
        /// Deploy a single mod
        /// </summary>
        /// <param name="mod"></param>
        public void DeployMod(DeploymentMod mod)
        {
            FASTER.Services.Telemetry.TrackEvent("Deployment - Clicked DeployMod", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName},
                {"Mod", mod.Name}
            });

            if (!Directory.Exists(Deployment.InstallPath))
            {
                DisplayMessage("Arma Install Path is empty.\nMake sure you have entered a valid path before deploying mods.");
                return;
            }

            var linkPath = Path.Combine(Deployment.InstallPath, ModDeployment.FolderName(mod.Name, mod.WorkshopId));
            if (!mod.Marked)
            {
                //LINK MOD
                mod.Marked = LinkMod(mod, linkPath);
            }
            else
            {
                //UNLINK MOD
                var links = Directory.EnumerateDirectories(Deployment.InstallPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
                if (links.Any(l => l.Name == ModDeployment.FolderName(mod.Name, mod.WorkshopId)))
                    DeleteLink(linkPath);
                mod.Marked = false;
            }

            AppSettings.Current.Deployments = Deployment;
            AppSettings.Current.Save();
        }


        /// <summary>
        /// Deploy all mods to Deployment
        /// </summary>
        public void DeployAll()
        {
            FASTER.Services.Telemetry.TrackEvent("Deployment - Clicked DeployAll", new Dictionary<string, string>
            {
                {"Name", AppSettings.Current.SteamUserName}
            });

            Logger.Log($"DeployAll: installPath={Deployment.InstallPath}, mods={Deployment.DeployMods.Count}");

            if (!Directory.Exists(Deployment.InstallPath))
            {
                Logger.Log("DeployAll: install path not found, aborting.");
                DisplayMessage("Arma Install Path is empty.\nMake sure you have entered a valid path before deploying mods.");
                return;
            }

            foreach (var mod in Deployment.DeployMods)
            {
                var linkPath = Path.Combine(Deployment.InstallPath, ModDeployment.FolderName(mod.Name, mod.WorkshopId));
                Logger.Log($"  Linking {mod.Name}: {mod.Path} -> {linkPath}");
                mod.Marked = LinkMod(mod, linkPath);
            }
            AppSettings.Current.Deployments = Deployment;
            AppSettings.Current.Save();
            Logger.Log("DeployAll: done.");
        }

        /// <summary>
        /// Clear all symlinks from the deployment
        /// </summary>
        public void ClearAll()
        {
            if (!Directory.Exists(Deployment.InstallPath))
            {
                DisplayMessage("Arma Install Path is empty.\nMake sure you have entered a valid path before deploying mods.");
                return;
            }

            foreach (var mod in Deployment.DeployMods)
            {
                var linkPath = Path.Combine(Deployment.InstallPath, ModDeployment.FolderName(mod.Name, mod.WorkshopId));
                if (Directory.Exists(linkPath) && new DirectoryInfo(linkPath).Attributes.HasFlag(FileAttributes.ReparsePoint))
                { DeleteLink(linkPath); }

                mod.Marked = false;
            }
        }

        /// <summary>
        /// Set the current steam deployment location
        /// </summary>
        public async Task InstallFolderClick()
        {
            string? path = await AppServices.Files.PickFolderAsync(Deployment.InstallPath);

            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!Directory.Exists(path))
            {
                await AppServices.Dialogs.ShowMessageAsync(this, "Deployment directory", "Choose an existing Arma installation directory.");
                return;
            }

            Deployment.InstallPath = path;

            foreach (var mod in Deployment.DeployMods)
            {
                var links = Directory.EnumerateDirectories(Deployment.InstallPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
                mod.Marked = links.Any(l => l.Name == ModDeployment.FolderName(mod.Name, mod.WorkshopId));
            }
            AppSettings.Current.Deployments = Deployment;
            AppSettings.Current.Save();
        }

        /// <summary>
        /// Open a mod's folder
        /// </summary>
        /// <param name="mod"></param>
        public void OpenModFolder(DeploymentMod mod)
        {
            if (mod == null || !Directory.Exists(mod.Path))
            {
                DisplayMessage($"Could not open folder \"{mod?.Path}\"");
                return;
            }

            Platform.Current.OpenFolder(mod.Path);
        }


        /// <summary>
        /// Open a steam mod's page
        /// </summary>
        /// <param name="mod"></param>
        public void OpenModPage(DeploymentMod mod)
        {
            if (mod == null)
                return;

            var url = WorkshopFileDetailsUrl + mod.WorkshopId;

            try
            { Platform.Current.OpenUrl(url); }
            catch
            { DisplayMessage($"Could not open \"{url}\""); }
        }

        /// <summary>
        /// Create a specific Symlink
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="linkPath"></param>
        private bool LinkMod(DeploymentMod mod, string linkPath)
        {
            Logger.Log($"LinkMod: {mod.Name} ({mod.WorkshopId}) -> {linkPath}");
            if (!Directory.Exists(mod.Path))
            {
                DisplayMessage($"Could not deploy \"{mod.Name}\": the mod folder does not exist. Download the mod first.\n{mod.Path}");
                return false;
            }
            try
            {
                if(Directory.Exists(linkPath))
                {
                    if (new DirectoryInfo(linkPath).Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        Logger.Log($"  Replacing existing symlink: {linkPath}");
                    }
                    else
                    {
                        Logger.Log($"  Skipped: a real folder already exists at {linkPath}. Not deleting it.");
                        DisplayMessage($"Skipped \"{mod.Name}\": a real folder already exists at\n{linkPath}\n\nRename or remove it yourself, then deploy again.");
                        return false;
                    }
                }

                ModDeployment.Link(mod.Path, Deployment.InstallPath, mod.Name, mod.WorkshopId);
                Logger.Log($"  Symlink created OK.");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Log($"  ERROR: UnauthorizedAccessException creating symlink.");
                DisplayMessage(OperatingSystem.IsWindows()
                    ? "Could not create symlink: Access denied.\n\nTo deploy mods, enable Windows Developer Mode in Settings → Update & Security → For Developers, or run FASTER as Administrator."
                    : "Could not deploy mod: Access denied. Check read access to the mod folder and write access to the Arma installation directory.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"  ERROR: {ex.Message}");
                DisplayMessage("An exception occurred: \n\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete a specific Symlink
        /// </summary>
        /// <param name="linkPath"></param>
        private void DeleteLink(string linkPath)
        {
            try
            {
                if (Directory.Exists(linkPath))
                {
                    if (new DirectoryInfo(linkPath).Attributes.HasFlag(FileAttributes.ReparsePoint))
                        Directory.Delete(linkPath);
                    else
                        Directory.Delete(linkPath, true);
                }
            }
            catch (Exception ex)
            { DisplayMessage("An exception occurred: \n\n" + ex.Message); }
        }

        /// <summary>
        /// Display a message on the UI
        /// </summary>
        /// <param name="msg"></param>
        internal void DisplayMessage(string msg)
        {
            Ui.Current.DisplayMessage(msg);
        }

    }
}
