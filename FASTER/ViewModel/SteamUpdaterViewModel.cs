using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.ContentDelivery.Exceptions;
using BytexDigital.Steam.ContentDelivery.Models;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using BytexDigital.Steam.Core;
using BytexDigital.Steam.Core.Exceptions;
using BytexDigital.Steam.Core.Structs;

using FASTER.Models;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.AppCenter.Analytics;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace FASTER.ViewModel
{
    public sealed class SteamUpdaterViewModel : INotifyPropertyChanged, IDisposable
    {
        public SteamUpdaterViewModel()
        {
            Parameters = new SteamUpdaterModel();
        }

        private static readonly Lazy<SteamUpdaterViewModel>
            lazy =
                new(() => new SteamUpdaterViewModel(new SteamUpdaterModel()));

        public static SteamUpdaterViewModel Instance => lazy.Value;

        private SteamUpdaterViewModel(SteamUpdaterModel model)
        {
            Parameters                =  model;
            DownloadTasks.ListChanged += (_, _) => RaisePropertyChanged(nameof(IsDownloading));
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;
        }

        private bool _isLoggingIn;
        private bool _isDlOverride;
        private bool _updaterOnline;
        private bool _updaterFaulted;

        public SteamUpdaterModel Parameters { get; set; }


        public IDialogCoordinator DialogCoordinator { get; set; }
        private CancellationTokenSource tokenSource = new();

        public bool IsDownloading => DownloadTasks.Count > 0 || IsLoggingIn || IsDlOverride;

        private bool _modsUpdating;

        // True for the whole of a server update or a mod update, including the gaps between
        // steps where IsDownloading alone briefly reads false.
        public bool UpdateInProgress => IsDownloading || Parameters.IsUpdating || _modsUpdating;

        private BindingList<Task> DownloadTasks { get; } = new BindingList<Task>();


        public bool UpdaterOnline
        {
            get => _updaterOnline;
            set
            {
                _updaterOnline = value;
                RaisePropertyChanged(nameof(UpdaterOnline));
            }
        }

        public bool UpdaterFaulted
        {
            get => _updaterFaulted;
            set
            {
                _updaterFaulted = value;
                RaisePropertyChanged(nameof(UpdaterFaulted));
            }
        }

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

        internal SteamClient SteamClient;
        internal SteamContentClient SteamContentClient;

        public void PasswordChanged(string password)
        {
            Parameters.Password = Encryption.Instance.EncryptData(password);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (SteamClient == null)
            {
                UpdaterFaulted = false;
                UpdaterOnline = false;
                return;
            }

            UpdaterFaulted = SteamClient.IsFaulted;
            UpdaterOnline = SteamClient.IsConnected;
        }

        internal string GetPw()
        { return Encryption.Instance.DecryptData(Parameters.Password); }

        public async Task UpdateClick()
        {
            if (UpdateInProgress)
            {
                Parameters.AppendOutput("\nAn update is already running.");
                return;
            }

            Analytics.TrackEvent("Updater - Clicked Update", new Dictionary<string, string>
            {
                {"Name", Properties.Settings.Default.steamUserName},
                {"DLCs", $"{(Parameters.UsingContactDlc ? "Contact " : "")}{(Parameters.UsingGMDlc ? "GM " : "")}{(Parameters.UsingCSLADlc? "CSLA " : "")}{(Parameters.UsingPFDlc ? "SOG " : "")}{(Parameters.UsingWSDlc ? "WS " : "")}{(Parameters.UsingSPEDlc ? "SPE " : "")}{(Parameters.UsingRFDlc ? "RF " : "")}{(Parameters.UsingEFDlc ? "EF " : "")}"},
                {"Branch", $"{(Parameters.UsingPerfBinaries? "Profiling" : "Public")}"}
            });

            Parameters.IsUpdating = true;
            Parameters.Output     = "Starting Update...";
            Parameters.AppendOutput("\nPlease don't quit this page or cancel the download\nThis might take a while...");

            uint appId = 233780;

            Dictionary<uint, string> depotsIDs = new()
            //Find a way to update automatically with depot changess
            {
                {233781, "Arma 3 Alpha Dedicated Server Content (internal)"},
                {233782, "Arma 3 Alpha Dedicated Server binary Windows (internal)"},
                {233783, "Arma 3 Alpha Dedicated Server binary Linux (internal)"},
                {233784, "Arma 3 Server Profiling - WINDOWS Depot"},
                {233785, "Arma 3 Server - Profiler - LINUX Depot"},
                {233792, "Arma 3 Server Creator DLC - GM"},
                {233788, "Arma 3 Server Creator DLC - SPE"},
                {233793, "Arma 3 Server Creator DLC - CSLA"},
                {233794, "Arma 3 Server Creator DLC - SOGPF"},
                {233795, "Arma 3 Server Creator DLC - WS"},
                {233799, "Arma 3 Server Creator DLC - RF"},
                {233798, "Arma 3 Server Creator DLC - EF"},
            };

            List<(uint id, string branch, string pass)> depotsDownload = new();

            Parameters.AppendOutput("\nChecking Shared Content...");
            //Downloading Depot 233781 from either branch contact or public
            depotsDownload.Add((
                depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Alpha Dedicated Server Content (internal)").Key, 
                Parameters.UsingContactDlc ? "contact" : "public", 
                null));

            Parameters.AppendOutput("\nChecking Executables...");


            // Downloading depot 233782 fow Windows from branch public
            depotsDownload.Add((
                depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Alpha Dedicated Server binary Windows (internal)").Key,
                "public",
                null));


            //Download depot 233784 for windows in branch profiling
            if (Parameters.UsingPerfBinaries)
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Profiling - WINDOWS Depot").Key,
                    "profiling",
                    "CautionSpecialProfilingAndTestingBranchArma3"));


            //Downloading mods
            if (Parameters.UsingGMDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - GM...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - GM").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingCSLADlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - CSLA...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - CSLA").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingPFDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - SOGPF...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - SOGPF").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingWSDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - Western Sahara...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - WS").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingSPEDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - SPE...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - SPE").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingRFDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - RF...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - RF").Key,
                    "creatordlc",
                    null));
            }

            if (Parameters.UsingEFDlc)
            {
                Parameters.AppendOutput("\nChecking Arma 3 Server Creator DLC - EF...");
                depotsDownload.Add((
                    depotsIDs.FirstOrDefault(d => d.Value == "Arma 3 Server Creator DLC - EF").Key,
                    "creatordlc",
                    null));
            }

            Logger.Log($"UpdateClick: updating {Parameters.InstallDirectory} ({(Parameters.UsingPerfBinaries ? "profiling" : "public")} binaries), depots: {string.Join(", ", depotsDownload.Select(d => $"{d.id}/{d.branch}"))}");

            int result;
            try
            { result = await RunServerUpdater(Parameters.InstallDirectory, appId, depotsDownload); }
            catch (Exception ex)
            {
                Logger.Log($"UpdateClick: RunServerUpdater threw: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                Parameters.AppendOutput($"\nError: {ex.Message}");
                result = UpdateState.Error;
            }
            finally
            { Parameters.IsUpdating = false; }
			Logger.Log($"UpdateClick: finished with result {result} (0 success, 1 error, 2 login failed, 3 cancelled)");
            Parameters.AppendOutput(result switch
            {
                UpdateState.Success     => "\n\nAll Done ! ",
                UpdateState.LoginFailed => "\n\nSteam login failed. Nothing was updated.",
                UpdateState.Cancelled   => "\n\nUpdate was cancelled.",
                _                       => "\n\nUpdate did not complete. Check the output above for errors."
            });
        }

        public void UpdateCancelClick()
        {
            Parameters.AppendOutput("\nUpdate Cancelled.");

            // IsUpdating is cleared by UpdateClick's finally block once the run has actually
            // stopped, so a new update can't start while this one is still unwinding.
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

        internal async Task<int> RunServerUpdater(string path, uint appId, List<(uint id, string branch, string pass)> depots)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Log("RunServerUpdater: no install directory set, nothing to do.");
                return UpdateState.Cancelled;
            }

            if (!Directory.Exists(path))
            {
                Logger.Log($"RunServerUpdater: creating install directory {path}");
                Directory.CreateDirectory(path);
            }
            tokenSource = new CancellationTokenSource();

            if (!await SteamLogin())
            {
                Logger.Log("RunServerUpdater: Steam login failed, nothing was updated.");
                return UpdateState.LoginFailed;
            }

            Stopwatch sw = Stopwatch.StartNew();

            foreach (var depot in depots)
            {
                if (tokenSource.IsCancellationRequested)
                {
                    Logger.Log($"RunServerUpdater: cancelled before depot {depot.id}.");
                    return UpdateState.Cancelled;
                }

                try
                {
                    Logger.Log($"RunServerUpdater: depot {depot.id} ({depots.IndexOf(depot) + 1}/{depots.Count}), branch {depot.branch}");
                    ManifestId manifestId;
                    manifestId = await SteamContentClient.GetDepotManifestIdAsync(appId, depot.id, depot.branch, depot.pass);

                    Parameters.AppendOutput($"\n\nFetching informations of app {appId}, depot {depot.id} from Steam ({depots.IndexOf(depot)+1}/{depots.Count})... ");
                    var downloadHandler = await SteamContentClient.GetAppDataAsync(appId, depot.id, manifestId, tokenSource.Token);

                    await Download(downloadHandler, path);
                }
                catch (ArgumentException ex) when (ex.Message.Contains("'tasks'"))
                {
                    Logger.Log($"RunServerUpdater: depot {depot.id} has nothing to download, skipped.");
                    Parameters.AppendOutput("\nSkipped...");
                }
                catch (OperationCanceledException)
                {
                    Logger.Log($"RunServerUpdater: cancelled during depot {depot.id}.");
                    return UpdateState.Cancelled;
                }
                catch (Exception ex)
                {
                    Logger.Log($"RunServerUpdater: depot {depot.id} failed: {ex.GetType().Name}: {ex.Message}{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "")}\n{ex.StackTrace}");
                    Parameters.AppendOutput($"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}");
                    return UpdateState.Error;
                }
            }
            sw.Stop();
            Logger.Log($"RunServerUpdater: all {depots.Count} depots done in {sw.Elapsed}.");
            Parameters.AppendOutput($"\nDone in {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms");

            return UpdateState.Success;
        }

        public async Task<int> RunModsUpdater(ObservableCollection<ArmaMod> mods)
        {
            if (UpdateInProgress)
            {
                Logger.Log("RunModsUpdater: refused, an update is already running.");
                Parameters.AppendOutput("\nAn update is already running.");
                return UpdateState.Busy;
            }

            _modsUpdating = true;
            try
            { return await RunModsUpdaterCore(mods); }
            finally
            { _modsUpdating = false; }
        }

        private async Task<int> RunModsUpdaterCore(ObservableCollection<ArmaMod> mods)
        {
            Logger.Log($"RunModsUpdater: starting, {mods.Count} mods total");

            tokenSource = new CancellationTokenSource();

            if (!await TryLoginAsync())
            {
                IsLoggingIn = false;
                return UpdateState.LoginFailed;
            }

            Parameters.AppendOutput("\nAdding mods to download list...");

            SemaphoreSlim maxThread = new(1);
            var  ml = mods.Where(m => !m.IsLocal).ToList();
            uint finished = 0;
            IsDlOverride = true;
            try
            {
                Logger.Log($"RunModsUpdater: {ml.Count} non-local mods to update");

                foreach (ArmaMod mod in ml)
                {
                    Logger.Log($"RunModsUpdater: waiting semaphore for mod {mod.WorkshopId} ({mod.Name})");
                    await maxThread.WaitAsync();

                    _ = Task.Factory.StartNew(() => DownloadModAsync(mod), TaskCreationOptions.LongRunning)
                        .Unwrap()
                        .ContinueWith((t) =>
                        {
                            if (t.IsFaulted)
                                Logger.Log($"  ContinueWith: task for {mod.WorkshopId} faulted: {t.Exception}");
                            finished += 1;
                            Parameters.AppendOutput($"\n   Thread {mod.WorkshopId} complete  ({finished} / {ml.Count})");
                            Parameters.Progress = finished * 100.0 / ml.Count;
                            Logger.Log($"  ContinueWith: mod {mod.WorkshopId} done ({finished}/{ml.Count}), releasing semaphore.");
                            maxThread.Release();
                        });
                }

                Logger.Log("RunModsUpdater: all tasks queued, waiting for last semaphore...");
                Parameters.AppendOutput("\nAlmost there...");
                await maxThread.WaitAsync();
            }
            finally
            {
                IsDlOverride = false;
            }

            if (tokenSource.IsCancellationRequested)
            {
                Logger.Log("RunModsUpdater: cancelled.");
                Parameters.AppendOutput("\nMods update cancelled.");
                return UpdateState.Cancelled;
            }

            var failed = ml.Count(m => m.Status == ArmaModStatus.NotComplete);
            if (failed > 0)
            {
                Logger.Log($"RunModsUpdater: finished with {failed} of {ml.Count} mods not complete.");
                Parameters.AppendOutput($"\n{failed} mod(s) did not finish. Check the output above.");
                return UpdateState.Error;
            }

            Logger.Log("RunModsUpdater: all done.");
            Parameters.AppendOutput("\nMods updated !");
            return UpdateState.Success;
        }

        private async Task<bool> TryLoginAsync()
        {
            Logger.Log("RunModsUpdater: calling SteamLogin...");
            bool loginOk = false;
            try { loginOk = await SteamLogin(); }
            catch (Exception ex) { Logger.Log($"RunModsUpdater: SteamLogin threw exception: {ex}"); }

            Logger.Log(loginOk
                ? "RunModsUpdater: SteamLogin OK"
                : "RunModsUpdater: SteamLogin failed, aborting.");

            return loginOk;
        }

        private async Task DownloadModAsync(ArmaMod mod)
        {
            Logger.Log($"  Task started: {mod.WorkshopId} ({mod.Name}), path={mod.Path}");
            try
            {
                if (!Directory.Exists(mod.Path))
                {
                    Logger.Log($"  Creating dir: {mod.Path}");
                    Directory.CreateDirectory(mod.Path);
                }

                if (tokenSource.Token.IsCancellationRequested)
                {
                    Logger.Log($"  Cancellation requested before {mod.WorkshopId}, skipping.");
                    return;
                }
                Parameters.AppendOutput($"\n   Starting {mod.WorkshopId}");

                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    await DownloadModContentAsync(mod);
                }
                catch (TaskCanceledException)
                {
                   Logger.Log($"  {mod.WorkshopId} task cancelled.");
                   sw.Stop();
                   mod.Status = ArmaModStatus.NotComplete;
                }
                catch (Exception ex)
                {
                    Logger.Log($"  ERROR downloading {mod.WorkshopId}: {ex.GetType().Name}: {ex.Message}{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "")}\n  StackTrace: {ex.StackTrace}");
                    sw.Stop();
                    mod.Status = ArmaModStatus.NotComplete;
                    Parameters.AppendOutput($"\nError: {ex.Message}{(ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : "")}");
                }
                sw.Stop();

                mod.CheckModSize();
                Parameters.AppendOutput($"\n    Download {mod.WorkshopId} completed, it took {sw.Elapsed.Minutes + sw.Elapsed.Hours*60}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.Log($"  UNHANDLED ERROR in task for {mod.WorkshopId}: {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}");
            }
        }

        private async Task DownloadModContentAsync(ArmaMod mod)
        {
            ManifestId manifestId = default;

            // Ask Steam for the current update time first. Without this, the check below
            // compares against whatever was saved the last time Check For Updates was pressed.
            await Task.Run(() => mod.UpdateInfos(false));

            if (mod.LocalLastUpdated > mod.SteamLastUpdated && mod.Size > 0)
            {
                mod.Status = ArmaModStatus.UpToDate;
                Parameters.AppendOutput($"\n   Mod{mod.WorkshopId} already up to date. Ignoring...");
                Logger.Log($"  {mod.WorkshopId} already up to date, skipping.");
                return;
            }

            if (!SteamClient.Credentials.IsAnonymous)
            {
                Logger.Log($"  Getting manifest for {mod.WorkshopId}");
                Parameters.AppendOutput($"\n   Getting manifest for {mod.WorkshopId}");
                manifestId = (await SteamContentClient.GetPublishedFileDetailsAsync(mod.WorkshopId)).hcontent_file;
                Manifest manifest = await SteamContentClient.GetManifestAsync(107410, 107410, manifestId);
                Parameters.AppendOutput($"\n   Manifest retrieved {mod.WorkshopId}");
                Logger.Log($"  Manifest retrieved for {mod.WorkshopId}, syncing deleted files...");
                SyncDeleteRemovedFiles(mod.Path, manifest);
            }

           Logger.Log($"  Requesting download handler for {mod.WorkshopId}");
           Parameters.AppendOutput($"\n    Attempting to start download of item {mod.WorkshopId}... ");

           var downloadHandler = await SteamContentClient.GetPublishedFileDataAsync(mod.WorkshopId, manifestId, tokenSource.Token);
           Logger.Log($"  Download handler obtained for {mod.WorkshopId}, starting download...");
           await DownloadForMultiple(downloadHandler, mod.Path);
           Logger.Log($"  Download complete for {mod.WorkshopId}");

           mod.Status = ArmaModStatus.UpToDate;
           var nx = DateTime.UnixEpoch;
           var ts = DateTime.UtcNow - nx;
           mod.LocalLastUpdated = (ulong)ts.TotalSeconds;
        }

        internal async Task<bool> SteamLogin()
        {
            Logger.Log("SteamLogin: start");
            if (tokenSource.IsCancellationRequested)
                tokenSource = new CancellationTokenSource();
            IsLoggingIn = true;
            var path = Path.Combine(Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) ?? string.Empty, "sentries");

            var password = Encryption.Instance.DecryptData(Parameters.Password);

            // A username with no usable password would otherwise be sent to Steam as-is.
            // Leaving the username empty is still the anonymous login, so that case is untouched.
            if (!string.IsNullOrWhiteSpace(Parameters.Username) && string.IsNullOrEmpty(password))
            {
                var unreadable = !string.IsNullOrEmpty(Parameters.Password);
                Logger.Log($"SteamLogin: {(unreadable ? "the stored password could not be decrypted" : "no password is stored")}, aborting.");
                Parameters.AppendOutput(unreadable
                    ? "\nThe saved Steam password could not be read. It only works for the Windows user and PC that saved it.\nPlease enter your Steam password again in the Password box."
                    : "\nNo Steam password is saved. Please enter it in the Password box.");
                IsLoggingIn = false;
                return false;
            }

            SteamCredentials _steamCredentials = new(Parameters.Username, password);

            if (SteamClient == null || SteamClient.Credentials.Username != _steamCredentials.Username || SteamClient.Credentials.Password != _steamCredentials.Password)
            { 
                SteamClient = new SteamClient(_steamCredentials, new AuthCodeProvider(_steamCredentials.Username, path));
                SteamClient.InternalClientAttemptingConnect += () => Parameters.AppendOutput("\n\tClient : Attempting connect..");
                SteamClient.InternalClientConnected         += () => Parameters.AppendOutput("\n\tClient : Connected");
                SteamClient.InternalClientDisconnected      += () => Parameters.AppendOutput("\n\tClient : Disconnected");
                SteamClient.InternalClientLoggedOn          += () => Parameters.AppendOutput("\n\tClient : Logged on");
                SteamClient.InternalClientLoggedOff         += () => Parameters.AppendOutput("\n\tClient : Logged off");
            }

            if (!SteamClient.IsConnected || SteamClient.IsFaulted)
            {
                Parameters.AppendOutput($"\nConnecting to Steam as {(_steamCredentials.IsAnonymous ? "anonymous" : _steamCredentials.Username)}");
                SteamClient.MaximumLogonAttempts = 5;
                try
                { await SteamClient.ConnectAsync(tokenSource.Token); }
                catch (SteamClientAlreadyRunningException)
                {
                    Logger.Log("SteamLogin: SteamClientAlreadyRunningException - client already running");
                    Parameters.AppendOutput($"\nClient already logged in.");
                    IsLoggingIn = false;
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Log($"SteamLogin: ConnectAsync failed: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    Parameters.AppendOutput($"\nFailed! Error: {ex.Message}");
                    var savedUsername = SteamClient.Credentials.Username;
                    SteamClient.Shutdown();
                    SteamClient.Dispose();
                    SteamClient = null;

                    if (ex.GetBaseException() is SteamAuthenticationException)
                    {
                        Parameters.AppendOutput("\nWarning: The logon may have failed due to expired sentry-data."
                                                + $"\nIf you are sure that the provided username and password are correct, consider deleting the token file for the user \"{savedUsername}\" in the sentries directory."
                                                + $"\n{path}");
                    }
                    IsLoggingIn = false;
                    return false;
                }
            }

            // A new content client per login is deliberate: it caches app info, and a fresh one is what
            // makes each update see the current manifests. The old one has to be disposed, or its CDN
            // server pools keep running in the background.
            Logger.Log($"SteamLogin: creating SteamContentClient with {Properties.Settings.Default.CliWorkers} workers");
            var previousContentClient = SteamContentClient;
            SteamContentClient = new SteamContentClient(SteamClient, Properties.Settings.Default.CliWorkers)
            {
                Logger = new FASTER.Models.FasterLoggerAdapter()
            };

            if (previousContentClient != null)
                await previousContentClient.DisposeAsync();
            Parameters.AppendOutput("\nConnected !");
            Logger.Log("SteamLogin: connected OK");
            IsLoggingIn = false;
            return SteamClient.IsConnected;
        }

        internal bool SteamReset()
        {
            Logger.Log($"SteamReset: disconnecting (connected: {SteamClient?.IsConnected}, faulted: {SteamClient?.IsFaulted})");
            Parameters.AppendOutput("\nDisconnecting...");
            if (SteamContentClient != null)
            {
                SteamContentClient.DisposeAsync().AsTask().GetAwaiter().GetResult();
                SteamContentClient = null;
            }
            SteamClient?.Shutdown();
            SteamClient?.Dispose();
            SteamClient = null;
            Parameters.AppendOutput("\nDisconnected.");
            return SteamClient == null;
        }

        private void SyncDeleteRemovedFiles(string targetDir, Manifest manifest)
        {
            Logger.Log($"SyncDeleteRemovedFiles: checking {targetDir} for files Steam no longer lists");

            // A set makes each lookup instant, instead of scanning the whole manifest per file
            var expected = new HashSet<string>(manifest.Files.Select(x => x.FileName), StringComparer.InvariantCultureIgnoreCase);
            var deleted  = 0;

            foreach (var localFilePath in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            {
                var relativeLocalPath = Path.GetRelativePath(targetDir, localFilePath);

                if (expected.Contains(relativeLocalPath))
                    continue;

                Logger.Log($"SyncDeleteRemovedFiles: deleting {relativeLocalPath}");
                File.Delete(localFilePath);
                deleted++;
            }

            Logger.Log($"SyncDeleteRemovedFiles: {deleted} file(s) deleted from {targetDir}");
        }


        private async Task Download(IDownloadHandler downloadHandler, string targetDir)
        {
            SteamDiagLogger.Attach(downloadHandler);

            ulong downloadedSize = 0;
            bool skipDownload = false;
            downloadHandler.FileVerified          += (_, args) => Parameters.AppendOutput($"{(args.RequiresDownload ? $"\nFile verified : {args.ManifestFile.FileName} ({Functions.ParseFileSize(args.ManifestFile.TotalSize)})" : "")}");
            downloadHandler.VerificationCompleted += (_, args) => {
                Parameters.AppendOutput($"\nVerification completed, {args.QueuedFiles.Count} files queued for download. ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)"); 
                if (args.QueuedFiles.Count == 0) 
                    {skipDownload = true; } 
                };
            downloadHandler.FileDownloaded        += (_, args) =>
                                                     {
                                                         downloadedSize    += args.TotalSize;
                                                         Parameters.AppendOutput($"\nProgress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)} / {Functions.ParseFileSize(downloadHandler.TotalFileSize)})");
                                                         Parameters.Progress = downloadHandler.TotalProgress * 100;
                                                     };
            downloadHandler.DownloadComplete      += (_, _) => Parameters.AppendOutput("\nDownload completed");

            if (tokenSource.IsCancellationRequested)
                tokenSource = new CancellationTokenSource();

            Task downloadTask = Task.Run(async () =>
            {
                await downloadHandler.SetupAsync(targetDir, file => true, tokenSource.Token);
                await downloadHandler.VerifyAsync(tokenSource.Token);
                await downloadHandler.DownloadAsync(tokenSource.Token);
            });

            Parameters.AppendOutput("\nOK.");

            DownloadTasks.Add(downloadTask);

            Parameters.Progress = 0;
            Logger.Log($"Download: {downloadHandler.TotalFileCount} files, {Functions.ParseFileSize(downloadHandler.TotalFileSize)}, into {targetDir}");
            Parameters.AppendOutput($"\nDownloading {downloadHandler.TotalFileCount} files with total size of {Functions.ParseFileSize(downloadHandler.TotalFileSize)}...");
            Parameters.AppendOutput($"\nVerifying Install...");

            while (!downloadTask.IsCompleted && !downloadTask.IsCanceled && !tokenSource.Token.IsCancellationRequested && !skipDownload)
            {

                var delayTask = Task.Delay(500, tokenSource.Token);
                await Task.WhenAny(delayTask, downloadTask);

                if (tokenSource.Token.IsCancellationRequested)
                    Parameters.AppendOutput("\nTask cancellation requested");
                Parameters.AppendOutput($"\nProgress {downloadHandler.TotalProgress * 100:00.00}%");
                Parameters.Progress = downloadHandler.TotalProgress * 100;
            }

            if (skipDownload)
            {
                Parameters.AppendOutput("\nSkipping Download...");
                Parameters.Progress = 0;

                // With nothing queued, DownloadAsync throws its "'tasks'" ArgumentException.
                // Wait for the task and swallow only that one, so it is never left unobserved.
                try
                { await downloadTask; }
                catch (ArgumentException ex) when (ex.Message.Contains("'tasks'"))
                { Logger.Log("Download: nothing to download (no tasks), skipped."); }
                finally
                {
                    DownloadTasks.Remove(downloadTask);
                    await downloadHandler.DisposeAsync();
                }
                return;
            }


            if (downloadTask.IsCanceled)
            {

                Parameters.AppendOutput("\nTask Cancelled");
                Parameters.Progress = 0;
                await downloadHandler.DisposeAsync();
                DownloadTasks.Remove(downloadTask);
                return;
            }

            try
            { await downloadTask; }
            catch (TaskCanceledException)
            {
                Parameters.AppendOutput("\nTask Cancelled");
                Parameters.Progress = 0;
                throw;
            }
            catch (ArgumentException)
            {
                Parameters.AppendOutput($"\nSkipping download : No tasks ");
                Parameters.Progress = 0;
            }
            finally
            {
                DownloadTasks.Remove(downloadTask);
                await downloadHandler.DisposeAsync();
                downloadTask.Dispose();
            }
        }

        private async Task DownloadForMultiple(IDownloadHandler downloadHandler, string targetDir)
        {
            SteamDiagLogger.Attach(downloadHandler);

            Logger.Log($"DownloadForMultiple: targetDir={targetDir}");
            if (targetDir == null)
            {
                Logger.Log("DownloadForMultiple: targetDir is null, aborting.");
                return;
            }

            if (!Directory.Exists(targetDir))
            {
                Logger.Log($"DownloadForMultiple: creating dir {targetDir}");
                Directory.CreateDirectory(targetDir);
            }

            tokenSource.Token.ThrowIfCancellationRequested();
            ulong downloadedSize = 0;
            downloadHandler.FileVerified          += (_, args) =>
            {
                if (args.RequiresDownload)
                {
                    Logger.Log($"  File verified (needs download): {args.ManifestFile.FileName} ({Functions.ParseFileSize(args.ManifestFile.TotalSize)})");
                    Parameters.AppendOutput($"\n    File verified : {args.ManifestFile.FileName} ({Functions.ParseFileSize(args.ManifestFile.TotalSize)})");
                }
            };
            downloadHandler.VerificationCompleted += (_, args) =>
            {
                Logger.Log($"  Verification completed: {args.QueuedFiles.Count} files queued ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)");
                Parameters.AppendOutput($"\n    Verification completed, {args.QueuedFiles.Count} files queued for download. ({args.QueuedFiles.Sum(f => (double)f.TotalSize)} bytes)");
            };
            downloadHandler.FileDownloaded        += (_, args) =>
            {
                downloadedSize    += args.TotalSize;
                Logger.Log($"  File downloaded: progress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)}/{Functions.ParseFileSize(downloadHandler.TotalFileSize)})");
                Parameters.AppendOutput($"\n    Progress {downloadHandler.TotalProgress * 100:00.00}% ({Functions.ParseFileSize(downloadedSize)} / {Functions.ParseFileSize(downloadHandler.TotalFileSize)})");
            };
            downloadHandler.DownloadComplete      += (_, _) =>
            {
                Logger.Log("  DownloadComplete event fired.");
                Parameters.AppendOutput("\n    Download completed");
            };

            Logger.Log($"DownloadForMultiple: starting Task.Run (Setup/Verify/Download), totalFiles={downloadHandler.TotalFileCount}, totalSize={Functions.ParseFileSize(downloadHandler.TotalFileSize)}");
            Task downloadTask = Task.Run(async () =>
            {
                try
                {
                    Logger.Log("  SetupAsync starting...");
                    await downloadHandler.SetupAsync(targetDir, file => true, tokenSource.Token);
                    Logger.Log("  SetupAsync done. VerifyAsync starting...");
                    await downloadHandler.VerifyAsync(tokenSource.Token);
                    Logger.Log("  VerifyAsync done. DownloadAsync starting...");
                    await downloadHandler.DownloadAsync(tokenSource.Token);
                    Logger.Log("  DownloadAsync done.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"  ERROR inside download Task.Run: {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}");
                    throw;
                }
            });

            Parameters.AppendOutput("\n    OK.");

            DownloadTasks.Add(downloadTask);

            Parameters.AppendOutput($"\n    Downloading {downloadHandler.TotalFileCount} files with total size of {Functions.ParseFileSize(downloadHandler.TotalFileSize)}...");
            Parameters.Progress = 0;
            while (!downloadTask.IsCompleted && !downloadTask.IsCanceled && !tokenSource.IsCancellationRequested)
            {
                var delayTask = Task.Delay(500, tokenSource.Token);
                await Task.WhenAny(delayTask, downloadTask);

                if (tokenSource.IsCancellationRequested)
                    Parameters.AppendOutput("\n    Task cancellation requested");
            }

            if (downloadTask.IsCanceled)
            {
                Logger.Log("DownloadForMultiple: task was cancelled.");
                Parameters.AppendOutput("\n    Task Cancelled");
                DownloadTasks.Remove(downloadTask);
                await downloadHandler.DisposeAsync();
                return;
            }

            try
            { await downloadTask; }
            catch (TaskCanceledException)
            {
                Logger.Log("DownloadForMultiple: TaskCanceledException caught on await.");
                Parameters.AppendOutput("\n    Task Cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Log($"DownloadForMultiple: exception on await downloadTask: {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                Logger.Log("DownloadForMultiple: finalizing, disposing handler.");
                DownloadTasks.Remove(downloadTask);
                await downloadHandler.DisposeAsync();
                downloadTask.Dispose();
            }
        }

        public async Task<string> SteamGuardInput()
        { return await DialogCoordinator.ShowInputAsync(this, "Steam Guard", "Please enter your 2FA code"); }

        public async Task<MessageDialogResult> SteamGuardInputPhone()
        { return await DialogCoordinator.ShowMessageAsync(this, "Steam Guard", "Press OK after accepting authentification on mobile\nOr press Cancel to enter a 2FA Code", MessageDialogStyle.AffirmativeAndNegative); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {
            tokenSource.Dispose();
        }
    }

    public static class UpdateState
    {
        public const int Success     = 0;
        public const int Error       = 1;
        public const int LoginFailed = 2;
        public const int Cancelled   = 3;
        public const int Busy        = 4;
    }
}
