using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FASTER.Properties;
using Microsoft.AppCenter.Analytics;

namespace FASTER.ViewModel
{
    public class DeploymentViewModel
    {
        public DeploymentViewModel()
        { 
            if(Settings.Default.Deployments == null)
            {
                Settings.Default.Deployments = new ArmaDeployment();
                Settings.Default.Save();
            }

            Deployment = Settings.Default.Deployments;
        }

        public ArmaDeployment Deployment { get; set; }
        

        /// <summary>
        /// Unload data
        /// </summary>
        public void UnloadData()
        {
            Settings.Default.Deployments = Deployment;
            Settings.Default.Save();
        }

        /// <summary>
        /// Load data on focus
        /// </summary>
        public void LoadData()
        {
            Deployment = Settings.Default.Deployments;
            foreach (var mod in Settings.Default.armaMods.ArmaMods)
            {
                if (Deployment.DeployMods.Any(m => m.WorkshopId == mod.WorkshopId))
                    continue;
                Deployment.DeployMods.Add(new DeploymentMod(mod));
                Settings.Default.Save();
            }
            foreach (var mod in Deployment.DeployMods.ToArray())
            {
                if (Settings.Default.armaMods.ArmaMods.All(m => m.WorkshopId != mod.WorkshopId))
                {
                    Deployment.DeployMods.Remove(mod);
                    Settings.Default.Save();
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
            Analytics.TrackEvent("Deployment - Clicked DeployMod", new Dictionary<string, string>
            {
                {"Name", Settings.Default.steamUserName},
                {"Mod", mod.Name}
            });

            if(!Directory.Exists(Deployment.InstallPath))
            {
                DisplayMessage("Arma Install Path is empty.\nMake sure you have entered a valid path before deploying mods.");
                return;
            }

            mod.Marked = !mod.Marked;
            var linkPath = Path.Combine(Deployment.InstallPath, $"@{Functions.SafeName(mod.Name)}");
            if (mod.Marked)
            {
                //LINK MOD
                LinkMod(mod, linkPath);
            }
            else
            {
                //UNLINK MOD
                var links = Directory.EnumerateDirectories(Deployment.InstallPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
                if (links.Any(l => l.Name == $"@{Functions.SafeName(mod.Name)}"))
                    DeleteLink(linkPath);
            }

            Settings.Default.Deployments = Deployment;
            Settings.Default.Save();
        }


        /// <summary>
        /// Deploy all mods to Deployment
        /// </summary>
        public void DeployAll()
        {
            Analytics.TrackEvent("Deployment - Clicked DeployAll", new Dictionary<string, string>
            {
                {"Name", Settings.Default.steamUserName}
            });

            foreach (var mod in Deployment.DeployMods)
            {
                var linkPath = Path.Combine(Deployment.InstallPath, $"@{Functions.SafeName(mod.Name)}");
                mod.Marked = true;
                LinkMod(mod, linkPath);
            }
        }

        /// <summary>
        /// Clear all symlinks from the deployment
        /// </summary>
        public void ClearAll()
        {
            foreach (var mod in Deployment.DeployMods)
            { mod.Marked = false; }

            var links = Directory.EnumerateDirectories(Deployment.InstallPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
            foreach (var link in links)
            { DeleteLink(link.FullName); }
        }

        /// <summary>
        /// Set the current steam deployment location
        /// </summary>
        public void InstallFolderClick()
        {
            string path = MainWindow.Instance.SelectFolder(Deployment.InstallPath);

            if (path == null)
                return;

            Deployment.InstallPath = path;

            foreach (var mod in Deployment.DeployMods)
            {
                var links = Directory.EnumerateDirectories(Deployment.InstallPath).Select(d => new DirectoryInfo(d)).Where(d => d.Attributes.HasFlag(FileAttributes.ReparsePoint));
                mod.Marked = links.Any(l => l.Name == $"@{Functions.SafeName(mod.Name)}");
            }
            Settings.Default.Deployments = Deployment;
            Settings.Default.Save();
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

            ProcessStartInfo startInfo = new()
            {
                Arguments = mod.Path,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }


        /// <summary>
        /// Open a steam mod's page
        /// </summary>
        /// <param name="mod"></param>
        public void OpenModPage(DeploymentMod mod)
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

        /// <summary>
        /// Create a specific Symlink
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="linkPath"></param>
        private void LinkMod(DeploymentMod mod, string linkPath)
        {
            try
            {
                if(Directory.Exists(linkPath))
                    Directory.Delete(linkPath, true);

                Directory.CreateSymbolicLink(linkPath ?? throw new ArgumentNullException(nameof(linkPath)), mod.Path);
            }
            catch (Exception ex)
            { DisplayMessage("An exception occurred: \n\n" + ex.Message); }
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
                    Directory.Delete(linkPath, true);
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
            MainWindow.Instance.IFlyout.IsOpen = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

    }
}
