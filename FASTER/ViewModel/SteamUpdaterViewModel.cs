using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.ContentDelivery.Exceptions;
using BytexDigital.Steam.ContentDelivery.Models;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using BytexDigital.Steam.Core;
using BytexDigital.Steam.Core.Enumerations;
using BytexDigital.Steam.Core.Structs;

using FASTER.Models;

using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.GC.Artifact.Internal;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;

namespace FASTER.ViewModel
{
    public sealed class SteamUpdaterViewModel : INotifyPropertyChanged
    {
        public SteamUpdaterViewModel()
        {
            Parameters = new SteamUpdaterModel();
        }


        private static readonly Lazy<SteamUpdaterViewModel>
            lazy =
                new Lazy<SteamUpdaterViewModel>
                    (() => new SteamUpdaterViewModel(new SteamUpdaterModel()));

        public static SteamUpdaterViewModel Instance => lazy.Value;

        private SteamUpdaterViewModel(SteamUpdaterModel model)
        {
            Parameters                =  model;
            DownloadTasks.ListChanged += (sender, args) => RaisePropertyChanged(nameof(IsDownloading));
        }
    

        public SteamUpdaterModel            Parameters        { get; set; }
        public ObservableCollection<string> ServerBranches    { get; }      = new ObservableCollection<string> {"Stable", "Contact", "Creator DLC", "LegacyPorts", "Development", "Performance / Profiling"};
        public string                       ServerBranch      { get; set; } = "Stable";
        public IDialogCoordinator           dialogCoordinator { get; set; }
        public CancellationTokenSource      tokenSource = new CancellationTokenSource();

        public bool IsDownloading => DownloadTasks.Count > 0;

        public BindingList<Task> DownloadTasks { get; } = new BindingList<Task>();


        private static SteamClient _steamClient;
        private static SteamContentClient _steamContentClient;
        private static SteamCredentials   _steamCredentials;

        public void PasswordChanged(string password)
        {
            Parameters.Password = Encryption.Instance.EncryptData(password);
            var pw = Encryption.Instance.DecryptData(Parameters.Password);
        }


        public async Task UpdateClick()
        {
            Parameters.IsUpdating = true;
            Parameters.Output     = "Starting Update...";

            string branch     = "public";
            string branchPass = null;
            uint   appId      = 0;
            switch (ServerBranch)
            {
                case "Stable":
                    //Arma3 Server main branch
                    appId  = 233780;
                    break;
                case "Contact": //Arma 3 server Contact DLC
                    appId  = 233780;
                    branch = "contact";
                    break;
                case "Creator DLC": //Arma 3 server Creator DLC
                    appId  = 233780;
                    branch = "creatordlc";
                    break;
                case "LegacyPorts": //Arma 3 server Legacy Ports branch for linux
                    appId      = 233780;
                    branch     = "legacyPorts";
                    branchPass = "Arma3LegacyPorts";
                    break;
                case "Developpment": //Arma 3 Developpment branch, only for developpment clients
                    appId = 107410;
                    branch = "development";
                    break;
                case "Performance / Profiling":
                    appId  = 233780;
                    branch = "profiling"; 
                    branchPass = "CautionSpecialProfilingAndTestingBranchArma3";
                    break;
                default:
                    Console.WriteLine("Nothing to see here");
                    break;
            }

            //TODO Change this when binding is done
            await RunServerUpdater(@".\download", appId, branch, branchPass);
        }


        public void UpdateCancelClick()
        {
            Parameters.Output     += "\nUpdate Cancelled.";
            Parameters.IsUpdating =  false;

            tokenSource.Cancel();
        }
        public void SteamCmdDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.InstallDirectory);

            if (path == null) 
                return;

            Parameters.InstallDirectory = path;
        }
        public void ModStagingDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.ModStagingDirectory);

            if (path == null) 
                return;

            Parameters.ModStagingDirectory = path;
        }
        public void ServerDirClick(object sender)
        {
            throw new NotImplementedException();
        }


        public async Task<int> RunServerUpdater(string path, uint appId, string branch, string branchPass)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!await SteamLogin())
                return UpdateState.LoginFailed; 

            var _OS = _steamClient?.GetSteamOs().Identifier;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    SteamOs steamOs      = new SteamOs(_OS);

                    var        publicDepots = await _steamContentClient.GetDepotsOfBranchAsync(appId, "public");
                    var        depotId      = publicDepots[0].Id;
                    ManifestId manifestId;


                    if (true) //IS SYNC ENABLED
                    {
                        manifestId = await _steamContentClient.GetDepotDefaultManifestIdAsync(appId, depotId, branch, branchPass);
                        Manifest manifest = await _steamContentClient.GetManifestAsync(appId, depotId, manifestId);

                        SyncDeleteRemovedFiles(path, manifest);
                    }

                    Parameters.Output += $"\nAttempting to start download of app {appId}, depot {depotId}... ";

                    var downloadHandler = await _steamContentClient.GetAppDataAsync(appId, depotId, manifestId,branch, branchPass, steamOs);

                    await Download(downloadHandler, path, true);
                }
                catch (Exception ex)
                {
                    Parameters.Output += $"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}";
                    return UpdateState.Error;
                } 
                
                sw.Stop(); 
                Parameters.Output += $"\nDone in {sw.Elapsed}";

                return UpdateState.Success;
            }
            catch (TaskCanceledException)
            { return UpdateState.Cancelled; }
            catch (Exception ex)
            {
                Parameters.Output += $"\nDownload failed: {ex.Message}";
                return UpdateState.Error;
            }
            finally
            { _steamClient?.Shutdown(); }
        }

        public async Task<int> RunModUpdater(ulong modId, string path)
        {
            if (!await SteamLogin())
                return UpdateState.LoginFailed;

            var _OS = _steamClient.GetSteamOs().Identifier;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                SteamOs    steamOs    = new SteamOs(_OS);
                ManifestId manifestId = default;

                if (!_steamClient.Credentials.IsAnonymous) //IS SYNC NEABLED
                {
                    manifestId = (await _steamContentClient.GetPublishedFileDetailsAsync(modId)).hcontent_file;
                    Manifest manifest = await _steamContentClient.GetManifestAsync(107410, 107410, manifestId);

                    SyncDeleteRemovedFiles(path, manifest);
                }

                Parameters.Output += $"\nAttempting to start download of item {modId}... ";

                var downloadHandler = await _steamContentClient.GetPublishedFileDataAsync(
                                                                                          modId,
                                                                                          manifestId,
                                                                                          null,
                                                                                          null,
                                                                                          steamOs);

                await Download(downloadHandler, path, true);
            }
            catch (TaskCanceledException)
            {
                sw.Stop();
                _steamClient?.Shutdown();
                return UpdateState.Cancelled;
            }
            catch (Exception ex)
            {
                sw.Stop();
                Parameters.Output += $"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}";
                _steamClient?.Shutdown();
                return UpdateState.Error;
            }

            sw.Stop();
            Parameters.Output += $"\nDownload completed, it took {sw.Elapsed:hh\\:mm\\:ss}";
            _steamClient?.Shutdown();
            return UpdateState.Success;
        }


        public async Task<int> RunModsUpdater(ObservableCollection<ArmaMod> mods)
        {
            if (!await SteamLogin())
                return UpdateState.LoginFailed;

            var _OS = _steamClient.GetSteamOs().Identifier;

            List<Task> dls = new List<Task>();

            foreach (Task t in mods.Select(mod => DownloadSingleMod(mod, _OS)))
            {
                dls.Add(t);
                t.Start();
            }

            await Task.WhenAll(dls.ToArray());

            _steamClient?.Shutdown();
            return UpdateState.Success;
        }

        private async Task DownloadSingleMod(ArmaMod mod, string _OS)
        {
            if (mod.IsLocal)
                return;

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                SteamOs    steamOs    = new SteamOs(_OS);
                ManifestId manifestId = default;

                if (!_steamClient.Credentials.IsAnonymous) //IS SYNC NEABLED
                {
                    manifestId = (await _steamContentClient.GetPublishedFileDetailsAsync(mod.WorkshopId)).hcontent_file;
                    Manifest manifest = await _steamContentClient.GetManifestAsync(107410, 107410, manifestId);

                    SyncDeleteRemovedFiles(mod.Path, manifest);
                }

                Parameters.Output += $"\nAttempting to start download of item {mod.WorkshopId}... ";

                var downloadHandler = await _steamContentClient.GetPublishedFileDataAsync(
                                                                                          mod.WorkshopId,
                                                                                          manifestId,
                                                                                          null,
                                                                                          null,
                                                                                          steamOs);

                await Download(downloadHandler, mod.Path, true);
            }
            catch (TaskCanceledException)
            {
                sw.Stop();
                mod.Status = ArmaModStatus.NotComplete;
            }
            catch (Exception ex)
            {
                sw.Stop();
                mod.Status        =  ArmaModStatus.NotComplete;
                Parameters.Output += $"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}";
            }

            sw.Stop();
            mod.Status        =  ArmaModStatus.UpToDate;
            Parameters.Output += $"\nDownload completed, it took {sw.Elapsed:hh\\:mm\\:ss}";
        }


        private async Task<bool> SteamLogin()
        {
            SteamAuthenticationFilesProvider sentryFileProvider = new DirectorySteamAuthenticationFilesProvider(".\\sentries");

            if (string.IsNullOrEmpty(Parameters.Username)
             || Parameters.Username == "anonymous"
             || string.IsNullOrEmpty(Parameters.Password)
             || string.IsNullOrEmpty(Encryption.Instance.DecryptData(Parameters.Password)))
            {
                _steamCredentials = SteamCredentials.Anonymous;
            }
            else
            {
                _steamCredentials = new SteamCredentials(Parameters.Username, Encryption.Instance.DecryptData(Parameters.Password), Parameters.ApiKey);
            }

            _steamClient        = new SteamClient(_steamCredentials, new AuthCodeProvider(), sentryFileProvider);
            _steamContentClient = new SteamContentClient(_steamClient);

            Parameters.Output += $"\nConnecting to Steam as {(_steamCredentials.IsAnonymous ? "anonymous" : _steamCredentials.Username)}";

            try
            { await _steamClient.ConnectAsync(); }
            catch (Exception ex)
            {
                
                Parameters.Output += $"\nFailed! Error: {ex.Message}";
                _steamClient.Shutdown();
                if (!(ex is SteamLogonException logonEx))
                    return false;

                if (logonEx.Result == SteamKit2.EResult.InvalidPassword)
                {
                    Parameters.Output += "\nWarning: The logon may have failed due to expired sentry-data."
                                       + $"\nIf you are sure that the provided username and password are correct, consider deleting the .bin and .key file for the user \"{_steamClient.Credentials.Username}\" in the sentries directory.";
                }

                return false;
            }

            Parameters.Output += "\nOK.";
            return true;
        }


        private void SyncDeleteRemovedFiles(string targetDir, Manifest manifest)
        {
            Console.WriteLine("Checking for unnecessary files in target directory...");

            foreach (var localFilePath in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            {
                var relativeLocalPath = Path.GetRelativePath(targetDir, localFilePath);

                if (manifest.Files.Count(x => x.FileName.ToLowerInvariant() == relativeLocalPath.ToLowerInvariant()) != 0)
                    continue;

                Console.WriteLine($"Deleting local file {relativeLocalPath}");
                File.Delete(localFilePath);
            }
        }


        private async Task Download(IDownloadHandler downloadHandler, string targetDir, bool sync)
        {
            ulong downloadedSize = 0;
            downloadHandler.VerificationCompleted += (sender, args) => Parameters.Output += $"\nVerification completed, {args.QueuedFiles.Count} files queued for download. ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)";
            downloadHandler.FileDownloaded        += (sender, args) =>
                                                     {
                                                         downloadedSize    += args.TotalSize;
                                                         Parameters.Output += $"\nProgress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)} / {Functions.ParseFileSize(downloadHandler.TotalFileSize)})";
                                                     };
            downloadHandler.DownloadComplete      += (sender, args) => Parameters.Output += "\nDownload completed";

            if (tokenSource.IsCancellationRequested)
                tokenSource = new CancellationTokenSource();

            Task downloadTask = downloadHandler.DownloadToFolderAsync(targetDir, tokenSource.Token);
            

            Parameters.Output += "\nOK.";

            DownloadTasks.Add(downloadTask);
            
            Parameters.Output += $"\nDownloading {downloadHandler.TotalFileCount} files with total size of {Functions.ParseFileSize(downloadHandler.TotalFileSize)}...";

            while (!downloadTask.IsCompleted && !downloadTask.IsCanceled && !tokenSource.Token.IsCancellationRequested)
            {

                var delayTask = Task.Delay(500, tokenSource.Token);
                await Task.WhenAny(delayTask, downloadTask);

                if (tokenSource.Token.IsCancellationRequested)
                    Parameters.Output += "\nTask cancellation requested";
                Parameters.Output += $"\nProgress {downloadHandler.TotalProgress * 100:00.00}%";
            }

            if (downloadTask.IsCanceled)
            {
                Parameters.Output += "\nTask Cancelled";
                DownloadTasks.Remove(downloadTask);
                return;
            }

            try
            { await downloadTask; }
            catch (TaskCanceledException)
            {
                Parameters.Output += "\nTask Cancelled";
                throw;
            }
            finally
            {
                DownloadTasks.Remove(downloadTask);
                downloadTask.Dispose();
            }
        }

        public async Task<string> SteamGuardInput()
        { return await dialogCoordinator.ShowInputAsync(this, "Steam Guard", "Please enter your 2FA code"); }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public static class UpdateState
    {
        public const int Success     = 0;
        public const int Error       = 1;
        public const int LoginFailed = 2;
        public const int Cancelled   = 3;
    }
}
