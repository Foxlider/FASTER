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
using Microsoft.AppCenter.Analytics;

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

        private bool _isLoggingIn;
        private bool _isDlOverride;

        public SteamUpdaterModel            Parameters        { get; set; }

        public bool ProfilingBranch   { get; set; }
        public bool ContactDLCChecked { get; set; }
        public bool GMDLCChecked      { get; set; }
        public bool WSDLCChecked      { get; set; }
        public bool CLSADLCChecked    { get; set; }
        public bool PFDLCChecked      { get; set; }

        public  IDialogCoordinator      DialogCoordinator { get; set; }
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public bool IsDownloading => DownloadTasks.Count > 0 || IsLoggingIn || IsDlOverride;

        private BindingList<Task> DownloadTasks { get; } = new BindingList<Task>();

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(nameof(IsLoggingIn));
                RaisePropertyChanged(nameof(IsDownloading));
            }
        }

        public bool IsDlOverride
        {
            get => _isDlOverride;
            set
            {
                _isDlOverride = value;
                RaisePropertyChanged(nameof(IsDlOverride));
                RaisePropertyChanged(nameof(IsDownloading));
            }
        }

        private SteamClient _steamClient;
        private SteamContentClient _steamContentClient;
        private SteamCredentials   _steamCredentials;

        public void PasswordChanged(string password)
        {
            Parameters.Password = Encryption.Instance.EncryptData(password);
        }

        internal string GetPw()
        {
            return Encryption.Instance.DecryptData(Parameters.Password);
        }

        public async Task UpdateClick()
        {
            Analytics.TrackEvent("Updater - Clicked Update", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName},
                {"DLCs", $"{(GMDLCChecked ? "GM " : "")}{(CLSADLCChecked? "CLSA " : "")}{(PFDLCChecked ? "SOG " : "")}{(WSDLCChecked ? "WS " : "")}"},
                {"Branch", $"{(ProfilingBranch? "Profiling" : "Public")}"}
            });

            Parameters.IsUpdating = true;
            Parameters.Output     = "Starting Update...";
            Parameters.Output += "\nPlease don't quit this page or cancel the download\nThis might take a while...";
            
            uint   appId      = 233780;

            Parameters.Output += "\nDownloading Shared Content...";
            //Downloading Depot 233781 from either branch contact or public
            await RunServerUpdater(Parameters.InstallDirectory, appId, 233781, ContactDLCChecked? "contact" : "public", null);

            Parameters.Output += "\nDownloading Executables...";
            //Either downloading depot 233782 fow Windows from branch public or 233784 for windows in branch profiling
            if (ProfilingBranch)
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233784, "profiling", "CautionSpecialProfilingAndTestingBranchArma3");
            else
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233782, "public", null);

            //Downloading mods
            if (GMDLCChecked)
            {
                Parameters.Output += "\nDownloading Arma 3 Server Creator DLC - GM...";
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233787, "creatordlc", null);
            }

            if (CLSADLCChecked)
            {
                Parameters.Output += "\nDownloading Arma 3 Server Creator DLC - CSLA...";
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233789, "creatordlc", null);
            }

            if (PFDLCChecked)
            {
                Parameters.Output += "\nDownloading Arma 3 Server Creator DLC - SOGPF...";
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233790, "creatordlc", null);
            }

            if (WSDLCChecked)
            {
                Parameters.Output += "\nDownloading Arma 3 Server Creator DLC - Western Sahara...";
                await RunServerUpdater(Parameters.InstallDirectory, appId, 233786, "creatordlc", null);
            }

            Parameters.Output += "\n\nAll Done ! ";
        }


        public void UpdateCancelClick()
        {
            Parameters.Output     += "\nUpdate Cancelled.";
            Parameters.IsUpdating =  false;

            tokenSource.Cancel();
        }

        public void ModStagingDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.ModStagingDirectory);

            if (path == null) 
                return;

            Parameters.ModStagingDirectory = path;
        }

        public void ServerDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.InstallDirectory);

            if (path == null)
                return;

            Parameters.InstallDirectory = path;
        }


        public async Task<int> RunServerUpdater(string path, uint appId, uint depotId, string branch, string branchPass)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            tokenSource = new CancellationTokenSource();

            if (!await SteamLogin())
                return UpdateState.LoginFailed; 

            var _OS = _steamClient?.GetSteamOs().Identifier;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    SteamOs steamOs      = new SteamOs(_OS);
                    ManifestId manifestId;

                    manifestId = await _steamContentClient.GetDepotDefaultManifestIdAsync(appId, depotId, branch, branchPass);

                    Parameters.Output += $"\nAttempting to start download of app {appId}, depot {depotId}... ";

                    var downloadHandler = await _steamContentClient.GetAppDataAsync(appId, depotId, manifestId,branch, branchPass, steamOs);

                    await Download(downloadHandler, path);
                }
                catch (Exception ex)
                {
                    Parameters.Output += $"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}";
                    return UpdateState.Error;
                } 
                
                sw.Stop(); 
                Parameters.Output += $"\nDone in {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms";

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
            tokenSource = new CancellationTokenSource();

            try
            {
                if (!await SteamLogin())
                    return UpdateState.LoginFailed;
            }
            catch(Exception)
            {
                return UpdateState.LoginFailed;
            }

            var _OS = _steamClient.GetSteamOs().Identifier;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                SteamOs    steamOs    = new SteamOs(_OS);
                ManifestId manifestId = default;

                Parameters.Output += $"\nFetching mod {modId} infos... ";

                if (!_steamClient.Credentials.IsAnonymous) //IS SYNC ENABLED
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

                await Download(downloadHandler, path);
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
            Parameters.Output += $"\nDownload completed, it took {sw.Elapsed.TotalMinutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms";
            _steamClient?.Shutdown();
            return UpdateState.Success;
        }


        public async Task<int> RunModsUpdater(ObservableCollection<ArmaMod> mods)
        {
            
            tokenSource = new CancellationTokenSource();
            if(!await SteamLogin())
            { 
                IsLoggingIn = false;
                return UpdateState.LoginFailed;
            }

            var _OS = _steamClient.GetSteamOs().Identifier;

            Parameters.Output += "\nAdding mods to download list...";

            SemaphoreSlim maxThread = new SemaphoreSlim(3);
            var  ml = mods.Where(m => !m.IsLocal);
            uint finished = 0;
            IsDlOverride = true;

            foreach (ArmaMod mod in ml)
            {
                await maxThread.WaitAsync();

                _ = Task.Factory.StartNew(() =>
                {
                    if (!Directory.Exists(mod.Path))
                        Directory.CreateDirectory(mod.Path);

                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        mod.Status = ArmaModStatus.NotComplete;
                        return;
                    }
                    Parameters.Output += $"\n   Starting {mod.WorkshopId}";

                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        SteamOs steamOs = new SteamOs(_OS);
                        ManifestId manifestId = default;

                        if (!_steamClient.Credentials.IsAnonymous) //IS SYNC NEABLED
                        {
                            Parameters.Output += $"\n   Getting manifest for {mod.WorkshopId}";
                            manifestId = _steamContentClient.GetPublishedFileDetailsAsync(mod.WorkshopId).Result.hcontent_file;
                            Manifest manifest = _steamContentClient.GetManifestAsync(107410, 107410, manifestId).Result;
                            Parameters.Output += $"\n   Manifest retreived {mod.WorkshopId}";
                            SyncDeleteRemovedFiles(mod.Path, manifest);
                        }

                        Parameters.Output += $"\n    Attempting to start download of item {mod.WorkshopId}... ";

                        var downloadHandler = _steamContentClient.GetPublishedFileDataAsync(mod.WorkshopId,
                                                                                            manifestId,
                                                                                            null,
                                                                                            null,
                                                                                            steamOs);

                        DownloadForMultiple(downloadHandler.Result, mod.Path).Wait();
                    }
                    catch (TaskCanceledException)
                    {
                        sw.Stop();
                        mod.Status = ArmaModStatus.NotComplete;
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        mod.Status = ArmaModStatus.NotComplete;
                        Parameters.Output += $"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}";
                    }

                    sw.Stop();
                    mod.Status = ArmaModStatus.UpToDate;
                    var nx = new DateTime(1970, 1, 1);
                    var ts = DateTime.UtcNow - nx;
                    mod.LocalLastUpdated = (ulong) ts.TotalSeconds;

                    Parameters.Output += $"\n    Download {mod.WorkshopId} completed, it took {sw.Elapsed.TotalMinutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms";


                }, TaskCreationOptions.LongRunning).ContinueWith((task) =>
                {
                    finished += 1;
                    Parameters.Output += $"\n   Thread {mod.WorkshopId} complete  ({finished} / {ml.Count()})";
                    Parameters.Progress = finished * ml.Count() / 100.00;
                    maxThread.Release();
                });
            }

            Parameters.Output += "\nAlmost there...";
            await maxThread.WaitAsync();

            
            _steamClient?.Shutdown();
            Parameters.Output += "\nMods updated !";
            IsDlOverride = false;
            return UpdateState.Success;
        }
        

        private async Task<bool> SteamLogin()
        {
            IsLoggingIn = true;
            SteamAuthenticationFilesProvider sentryFileProvider = new DirectorySteamAuthenticationFilesProvider(".\\sentries");

            if (string.IsNullOrEmpty(Parameters.Username)
             || Parameters.Username == "anonymous"
             || string.IsNullOrEmpty(Parameters.Password)
             || string.IsNullOrEmpty(Encryption.Instance.DecryptData(Parameters.Password)))
            { _steamCredentials = SteamCredentials.Anonymous; }
            else
            { _steamCredentials = new SteamCredentials(Parameters.Username, Encryption.Instance.DecryptData(Parameters.Password), Parameters.ApiKey); }

            _steamClient        = new SteamClient(_steamCredentials, new AuthCodeProvider(), sentryFileProvider);
            _steamContentClient = new SteamContentClient(_steamClient);

            Parameters.Output += $"\nConnecting to Steam as {(_steamCredentials.IsAnonymous ? "anonymous" : _steamCredentials.Username)}";

            try
            { await _steamClient.ConnectAsync(tokenSource.Token); }
            catch (Exception ex)
            {
                
                Parameters.Output += $"\nFailed! Error: {ex.Message}";
                _steamClient.Shutdown();

                if (ex.GetBaseException() is SteamLogonException logonEx && logonEx.Result == SteamKit2.EResult.InvalidPassword)
                {
                    Parameters.Output += "\nWarning: The logon may have failed due to expired sentry-data." 
                                       + $"\nIf you are sure that the provided username and password are correct, consider deleting the .bin and .key file for the user \"{_steamClient.Credentials.Username}\" in the sentries directory."
                                       + $"{AppDomain.CurrentDomain.BaseDirectory}\\sentries";
                }
                IsLoggingIn = false;
                return false;
            }

            Parameters.Output += "\nOK.";
            IsLoggingIn = false;
            return _steamClient.IsConnected;
        }


        private void SyncDeleteRemovedFiles(string targetDir, Manifest manifest)
        {
            Console.WriteLine("Checking for unnecessary files in target directory...");

            foreach (var localFilePath in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            {
                var relativeLocalPath = Path.GetRelativePath(targetDir, localFilePath);

                if (manifest.Files.Any(x => string.Equals(x.FileName, relativeLocalPath, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                Console.WriteLine($"Deleting local file {relativeLocalPath}");
                File.Delete(localFilePath);
            }
        }


        private async Task Download(IDownloadHandler downloadHandler, string targetDir)
        {
            ulong downloadedSize = 0;
            downloadHandler.VerificationCompleted += (sender, args) => Parameters.Output += $"\nVerification completed, {args.QueuedFiles.Count} files queued for download. ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)";
            downloadHandler.FileDownloaded        += (sender, args) =>
                                                     {
                                                         downloadedSize    += args.TotalSize;
                                                         Parameters.Output += $"\nProgress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)} / {Functions.ParseFileSize(downloadHandler.TotalFileSize)})";
                                                         Parameters.Progress = downloadHandler.TotalProgress * 100;
                                                     };
            downloadHandler.DownloadComplete      += (sender, args) => Parameters.Output += "\nDownload completed";

            if (tokenSource.IsCancellationRequested)
                tokenSource = new CancellationTokenSource();

            Task downloadTask = downloadHandler.DownloadToFolderAsync(targetDir, tokenSource.Token);
            

            Parameters.Output += "\nOK.";

            DownloadTasks.Add(downloadTask);
            
            Parameters.Output += $"\nDownloading {downloadHandler.TotalFileCount} files with total size of {Functions.ParseFileSize(downloadHandler.TotalFileSize)}...";
            Parameters.Progress = 0;
            while (!downloadTask.IsCompleted && !downloadTask.IsCanceled && !tokenSource.Token.IsCancellationRequested)
            {

                var delayTask = Task.Delay(500, tokenSource.Token);
                await Task.WhenAny(delayTask, downloadTask);

                if (tokenSource.Token.IsCancellationRequested)
                    Parameters.Output += "\nTask cancellation requested";
                Parameters.Output += $"\nProgress {downloadHandler.TotalProgress * 100:00.00}%";
                Parameters.Progress = downloadHandler.TotalProgress * 100;
            }

            if (downloadTask.IsCanceled)
            {
                Parameters.Output += "\nTask Cancelled";
                Parameters.Progress = 0;
                DownloadTasks.Remove(downloadTask);
                return;
            }

            try
            { await downloadTask; }
            catch (TaskCanceledException)
            {
                Parameters.Output += "\nTask Cancelled";
                Parameters.Progress = 0;
                throw;
            }
            finally
            {
                DownloadTasks.Remove(downloadTask);
                downloadTask.Dispose();
            }
        }

        private async Task DownloadForMultiple(IDownloadHandler downloadHandler, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            tokenSource.Token.ThrowIfCancellationRequested();
            ulong downloadedSize = 0;
            downloadHandler.VerificationCompleted += (sender, args) => Parameters.Output += $"\n    Verification completed, {args.QueuedFiles.Count} files queued for download. ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)";
            downloadHandler.FileDownloaded        += (sender, args) =>
                                                     {
                                                         downloadedSize    += args.TotalSize;
                                                         Parameters.Output += $"\n    Progress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)} / {Functions.ParseFileSize(downloadHandler.TotalFileSize)})";
                                                     };
            downloadHandler.DownloadComplete      += (sender, args) => Parameters.Output += "\n    Download completed";

            Task downloadTask = downloadHandler.DownloadToFolderAsync(targetDir, tokenSource.Token);
            
            Parameters.Output += "\n    OK.";

            DownloadTasks.Add(downloadTask);
            
            Parameters.Output += $"\n    Downloading {downloadHandler.TotalFileCount} files with total size of {Functions.ParseFileSize(downloadHandler.TotalFileSize)}...";
            Parameters.Progress = 0;
            while (!downloadTask.IsCompleted && !downloadTask.IsCanceled && !tokenSource.IsCancellationRequested)
            {

                var delayTask = Task.Delay(500, tokenSource.Token);
                await Task.WhenAny(delayTask, downloadTask);

                if (tokenSource.IsCancellationRequested)
                    Parameters.Output += "\n    Task cancellation requested";
            }

            if (downloadTask.IsCanceled)
            {
                Parameters.Output += "\n    Task Cancelled";
                DownloadTasks.Remove(downloadTask);
                return;
            }

            try
            { await downloadTask; }
            catch (TaskCanceledException)
            {
                Parameters.Output += "\n    Task Cancelled";
                throw;
            }
            finally
            {
                DownloadTasks.Remove(downloadTask);
                downloadTask.Dispose();
            }
        }

        public async Task<string> SteamGuardInput()
        { return await DialogCoordinator.ShowInputAsync(this, "Steam Guard", "Please enter your 2FA code"); }

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
