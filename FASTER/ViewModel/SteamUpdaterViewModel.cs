using FASTER.Models;
using FASTER.Services.SteamCmd;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.AppCenter.Analytics;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace FASTER.ViewModel
{
    public sealed class SteamUpdaterViewModel : INotifyPropertyChanged, IDisposable
    {
        private const int MaximumConsoleCharacters = 200_000;
        private static readonly Lazy<SteamUpdaterViewModel> Lazy =
            new(() => new SteamUpdaterViewModel(new SteamUpdaterModel()));

        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FASTER", "Logs", "SteamCmd.log");

        private readonly SemaphoreSlim _operationGate = new(1, 1);
        private readonly WorkshopContentMirror _contentMirror = new();
        private readonly DispatcherTimer _statusTimer;
        private readonly object _logGate = new();

        private CancellationTokenSource _tokenSource = new();
        private SteamCmdClient? _steamCmdClient;
        private StreamWriter? _logWriter;
        private string _sessionPassword = string.Empty;
        private string? _lastProgressMessage;
        private bool _isBusy;
        private bool _updaterOnline;
        private bool _updaterFaulted;
        private bool _disposed;

        public SteamUpdaterViewModel()
            : this(new SteamUpdaterModel())
        {
        }

        private SteamUpdaterViewModel(SteamUpdaterModel model)
        {
            Parameters = model;
            MigrateLegacyPasswordToSession();

            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            _statusTimer.Tick += Timer_Tick;
        }

        public static SteamUpdaterViewModel Instance => Lazy.Value;

        public SteamUpdaterModel Parameters { get; }

        public IDialogCoordinator DialogCoordinator { get; set; } = null!;

        public bool IsDownloading => _isBusy;

        public bool CanConfigureUpdater => !_isBusy;

        public bool UpdaterOnline
        {
            get => _updaterOnline;
            private set
            {
                if (_updaterOnline == value)
                    return;

                _updaterOnline = value;
                RaisePropertyChanged(nameof(UpdaterOnline));
            }
        }

        public bool UpdaterFaulted
        {
            get => _updaterFaulted;
            private set
            {
                if (_updaterFaulted == value)
                    return;

                _updaterFaulted = value;
                RaisePropertyChanged(nameof(UpdaterFaulted));
            }
        }

        public void PasswordChanged(string password)
        {
            _sessionPassword = password ?? string.Empty;
        }

        internal string GetPw() => _sessionPassword;

        public async Task UpdateClick()
        {
            Analytics.TrackEvent("Updater - Clicked Update", new Dictionary<string, string>
            {
                {"Name", Parameters.Username ?? string.Empty},
                {"Branch", Parameters.ServerBranch}
            });

            Parameters.Output = "Starting the Arma 3 server update through SteamCMD...";
            int result = await RunServerUpdater(Parameters.InstallDirectory, Parameters.ServerBranch);

            if (result == UpdateState.Success)
                AppendOutput("Server update completed.");
        }

        public void UpdateCancelClick()
        {
            if (!_isBusy)
                return;

            AppendOutput("Cancellation requested. SteamCMD will exit after a short grace period.");
            _tokenSource.Cancel();

            SteamCmdClient? client = _steamCmdClient;
            if (client != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await client.CancelAsync();
                    }
                    catch (Exception exception)
                    {
                        AppendOutput($"SteamCMD cancellation warning: {exception.Message}");
                    }
                });
            }
        }

        public void ModStagingDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.ModStagingDirectory);
            if (!string.IsNullOrWhiteSpace(path))
                Parameters.ModStagingDirectory = path;
        }

        public void ServerDirClick()
        {
            string path = MainWindow.Instance.SelectFolder(Parameters.InstallDirectory);
            if (!string.IsNullOrWhiteSpace(path))
                Parameters.InstallDirectory = path;
        }

        public void SteamCmdDirClick()
        {
            if (_isBusy)
            {
                AppendOutput("SteamCMD's directory cannot be changed while an update is running.");
                return;
            }

            string path = MainWindow.Instance.SelectFolder(Parameters.SteamCmdDirectory);
            if (string.IsNullOrWhiteSpace(path))
                return;

            Parameters.SteamCmdDirectory = path;
            DisposeSteamCmdClient();
            RefreshSteamCmdStatus();
        }

        public async Task<bool> PrepareSteamCmdAsync()
        {
            if (!await TryBeginOperationAsync())
                return false;

            try
            {
                await EnsureSteamCmdInstalledAsync(CreateProgressReporter(), _tokenSource.Token);
                AppendOutput("SteamCMD is ready.");
                return true;
            }
            catch (OperationCanceledException)
            {
                AppendOutput("SteamCMD preparation was cancelled.");
                return false;
            }
            catch (Exception exception)
            {
                UpdaterFaulted = true;
                AppendOutput($"SteamCMD preparation failed: {exception.Message}");
                return false;
            }
            finally
            {
                EndOperation();
            }
        }

        public bool ResetSteamCmd()
        {
            if (_isBusy || _steamCmdClient?.IsRunning == true)
            {
                AppendOutput("Cancel the active update before resetting SteamCMD.");
                return false;
            }

            try
            {
                _steamCmdClient?.Reset();
                DisposeSteamCmdClient();
                UpdaterFaulted = false;
                RefreshSteamCmdStatus();
                AppendOutput("SteamCMD process state was reset. Cached login and downloaded content were kept.");
                return true;
            }
            catch (Exception exception)
            {
                UpdaterFaulted = true;
                AppendOutput($"SteamCMD reset failed: {exception.Message}");
                return false;
            }
        }

        internal async Task<int> RunServerUpdater(string path, string branchName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                AppendOutput("Select a server install directory before updating.");
                return UpdateState.Cancelled;
            }

            if (!TryParseServerBranch(branchName, out SteamCmdServerBranch branch))
            {
                AppendOutput($"Unsupported SteamCMD server branch: {branchName}");
                return UpdateState.Error;
            }

            if (!await TryBeginOperationAsync())
                return UpdateState.Cancelled;

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                SteamCmdClient client = await EnsureSteamCmdInstalledAsync(CreateProgressReporter(), _tokenSource.Token);
                AppendOutput($"Updating Arma 3 Dedicated Server ({branchName})...");

                string username = Parameters.Username?.Trim() ?? string.Empty;
                string password = username.Length == 0 ? string.Empty : _sessionPassword;

                // App 233780 can be updated anonymously, but prefer the configured
                // account whenever one is supplied. Besides honoring the updater UI,
                // this keeps the authenticated SteamCMD session ready for Workshop
                // content that requires the account's Arma 3 entitlement. A blank
                // username deliberately retains SteamCMD's anonymous fallback.
                SteamCmdServerUpdateResult result = await client.UpdateServerAsync(
                    username,
                    password,
                    path,
                    branch,
                    RequestGuardResponseAsync,
                    CreateProgressReporter(),
                    _tokenSource.Token);

                if (result.Cancelled)
                {
                    AppendOutput("Server update cancelled.");
                    return UpdateState.Cancelled;
                }

                if (!result.Success)
                {
                    AppendOutput(FormatSteamCmdFailure("Server update failed", result.Error, result.ExitCode));
                    return UpdateState.Error;
                }

                Parameters.Progress = 100;
                AppendOutput($"Server files verified in {FormatElapsed(stopwatch.Elapsed)}.");
                return UpdateState.Success;
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Server update cancelled.");
                return UpdateState.Cancelled;
            }
            catch (SteamCmdAuthenticationException exception)
            {
                AppendOutput($"Steam login failed: {exception.Message}");
                return UpdateState.LoginFailed;
            }
            catch (Exception exception)
            {
                UpdaterFaulted = true;
                AppendOutput($"Server update failed: {exception.Message}");
                return UpdateState.Error;
            }
            finally
            {
                stopwatch.Stop();
                EndOperation();
            }
        }

        public async Task<int> RunModUpdater(ulong modId, string path)
        {
            if (modId == 0)
                return UpdateState.Error;

            if (string.IsNullOrWhiteSpace(Parameters.Username))
            {
                AppendOutput("A Steam account that owns Arma 3 is required for Workshop downloads. An API key is not a download login.");
                return UpdateState.LoginFailed;
            }

            if (!await TryBeginOperationAsync())
                return UpdateState.Cancelled;

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                SteamCmdClient client = await EnsureSteamCmdInstalledAsync(CreateProgressReporter(), _tokenSource.Token);
                AppendOutput($"Downloading Workshop item {modId} through SteamCMD...");

                SteamCmdWorkshopBatchResult batch = await client.DownloadWorkshopItemsAsync(
                    Parameters.Username,
                    _sessionPassword,
                    new[] {modId},
                    RequestGuardResponseAsync,
                    CreateProgressReporter(),
                    _tokenSource.Token);

                if (batch.Cancelled)
                    return UpdateState.Cancelled;

                SteamCmdWorkshopItemResult? item = batch.Items.FirstOrDefault(result => result.WorkshopId == modId);
                if (item == null || !item.Success)
                {
                    string? error = item?.Error ?? batch.Error;
                    AppendOutput(FormatSteamCmdFailure($"Workshop item {modId} failed", error, batch.ExitCode));
                    return IsLoginFailure(error) ? UpdateState.LoginFailed : UpdateState.Error;
                }

                string stagingRoot = GetStagingRoot(path, modId);
                await _contentMirror.MirrorAsync(item.SourcePath, stagingRoot, modId, _tokenSource.Token);

                Parameters.Progress = 100;
                AppendOutput($"Workshop item {modId} was staged in {FormatElapsed(stopwatch.Elapsed)}.");
                return UpdateState.Success;
            }
            catch (OperationCanceledException)
            {
                AppendOutput($"Workshop item {modId} was cancelled.");
                return UpdateState.Cancelled;
            }
            catch (SteamCmdAuthenticationException exception)
            {
                AppendOutput($"Steam login failed: {exception.Message}");
                return UpdateState.LoginFailed;
            }
            catch (Exception exception)
            {
                AppendOutput($"Workshop item {modId} failed: {exception.Message}");
                return UpdateState.Error;
            }
            finally
            {
                stopwatch.Stop();
                EndOperation();
            }
        }

        public async Task<int> RunModsUpdater(ObservableCollection<ArmaMod> mods)
        {
            ArgumentNullException.ThrowIfNull(mods);

            List<ArmaMod> candidates = mods.Where(mod => !mod.IsLocal).ToList();
            if (candidates.Count == 0)
            {
                Parameters.Progress = 0;
                AppendOutput("No downloadable Workshop mods were selected.");
                return UpdateState.Cancelled;
            }

            List<ArmaMod> pending = new();

            foreach (ArmaMod mod in candidates)
            {
                if (mod.LocalLastUpdated > mod.SteamLastUpdated && mod.Size > 0)
                {
                    mod.Status = ArmaModStatus.UpToDate;
                    AppendOutput($"Workshop item {mod.WorkshopId} is already up to date.");
                }
                else
                {
                    pending.Add(mod);
                }
            }

            if (pending.Count == 0)
            {
                Parameters.Progress = 100;
                AppendOutput("All selected Workshop mods are already up to date.");
                return UpdateState.Success;
            }

            if (string.IsNullOrWhiteSpace(Parameters.Username))
            {
                foreach (ArmaMod mod in pending)
                    mod.Status = ArmaModStatus.NotComplete;

                AppendOutput("A Steam account that owns Arma 3 is required for Workshop downloads. An API key is not a download login.");
                return UpdateState.LoginFailed;
            }

            string stagingRoot;
            try
            {
                stagingRoot = SteamCmdCommandBuilder.ValidateAndNormalizePath(
                    Parameters.ModStagingDirectory,
                    nameof(Parameters.ModStagingDirectory));
                Directory.CreateDirectory(stagingRoot);
            }
            catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
            {
                foreach (ArmaMod mod in pending)
                    mod.Status = ArmaModStatus.NotComplete;

                AppendOutput($"The mod staging directory is unavailable: {exception.Message}");
                return UpdateState.Error;
            }

            if (!await TryBeginOperationAsync())
                return UpdateState.Cancelled;

            try
            {
                SteamCmdClient client = await EnsureSteamCmdInstalledAsync(CreateProgressReporter(), _tokenSource.Token);
                List<ulong> workshopIds = pending.Select(mod => (ulong)mod.WorkshopId).Distinct().ToList();
                Dictionary<ulong, int> positions = workshopIds
                    .Select((id, index) => (id, index))
                    .ToDictionary(entry => entry.id, entry => entry.index);

                AppendOutput($"Downloading {workshopIds.Count} Workshop item(s) in one SteamCMD session...");

                IProgress<SteamCmdProgress> progress = new Progress<SteamCmdProgress>(update =>
                {
                    ReportSteamCmdProgress(update);
                    if (update.WorkshopId is ulong id && update.Percentage is double itemPercentage &&
                        positions.TryGetValue(id, out int index))
                    {
                        Parameters.Progress = Math.Clamp(
                            (index + Math.Clamp(itemPercentage, 0, 100) / 100d) / workshopIds.Count * 100d,
                            0,
                            100);
                    }
                });

                SteamCmdWorkshopBatchResult batch = await client.DownloadWorkshopItemsAsync(
                    Parameters.Username,
                    _sessionPassword,
                    workshopIds,
                    RequestGuardResponseAsync,
                    progress,
                    _tokenSource.Token);

                Dictionary<ulong, SteamCmdWorkshopItemResult> results = batch.Items
                    .GroupBy(item => item.WorkshopId)
                    .ToDictionary(group => group.Key, group => group.Last());

                int completed = 0;
                int failed = 0;
                foreach (ArmaMod mod in pending)
                {
                    if (!results.TryGetValue(mod.WorkshopId, out SteamCmdWorkshopItemResult? item) || !item.Success)
                    {
                        mod.Status = ArmaModStatus.NotComplete;
                        failed++;
                        string? error = item?.Error ?? batch.Error ?? "SteamCMD returned no result for this item.";
                        if (!batch.Cancelled || !string.Equals(error, "Cancelled.", StringComparison.OrdinalIgnoreCase))
                            AppendOutput($"Workshop item {mod.WorkshopId} failed: {error}");
                        Parameters.Progress = (completed + failed) / (double)pending.Count * 100d;
                        continue;
                    }

                    try
                    {
                        // Once SteamCMD has reported a complete item, promote it even if
                        // cancellation stopped the rest of the batch. The mirror swaps the
                        // old target only after the new copy is complete, so this preserves
                        // every successful download without exposing partial staged content.
                        string target = await _contentMirror.MirrorAsync(
                            item.SourcePath,
                            stagingRoot,
                            mod.WorkshopId,
                            CancellationToken.None);

                        mod.Path = target;
                        mod.Status = ArmaModStatus.UpToDate;
                        mod.LocalLastUpdated = (ulong)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
                        await Task.Run(mod.CheckModSize, CancellationToken.None);
                        completed++;
                        Parameters.Progress = (completed + failed) / (double)pending.Count * 100d;
                        AppendOutput($"Workshop item {mod.WorkshopId} downloaded and staged ({completed + failed}/{pending.Count}).");
                    }
                    catch (Exception exception)
                    {
                        mod.Status = ArmaModStatus.NotComplete;
                        failed++;
                        Parameters.Progress = (completed + failed) / (double)pending.Count * 100d;
                        AppendOutput($"Workshop item {mod.WorkshopId} downloaded, but staging failed: {exception.Message}");
                    }
                }

                if (batch.Cancelled || _tokenSource.IsCancellationRequested)
                {
                    AppendOutput(
                        $"Mod update cancelled. {completed} completed item(s) were staged; " +
                        $"{failed} item(s) were unfinished or failed.");
                    return UpdateState.Cancelled;
                }

                if (failed > 0 || !batch.Success)
                {
                    string? error = batch.Error ?? results.Values.FirstOrDefault(item => !item.Success)?.Error;
                    AppendOutput($"Mod update finished with {failed} failure(s). Successful items were kept.");
                    return IsLoginFailure(error) && completed == 0 ? UpdateState.LoginFailed : UpdateState.Error;
                }

                Parameters.Progress = 100;
                AppendOutput($"All {completed} mod(s) were updated successfully.");
                return UpdateState.Success;
            }
            catch (OperationCanceledException)
            {
                foreach (ArmaMod mod in pending.Where(mod => mod.Status != ArmaModStatus.UpToDate))
                    mod.Status = ArmaModStatus.NotComplete;

                AppendOutput("Mod update cancelled before the batch completed.");
                return UpdateState.Cancelled;
            }
            catch (SteamCmdAuthenticationException exception)
            {
                foreach (ArmaMod mod in pending)
                    mod.Status = ArmaModStatus.NotComplete;

                AppendOutput($"Steam login failed: {exception.Message}");
                return UpdateState.LoginFailed;
            }
            catch (Exception exception)
            {
                foreach (ArmaMod mod in pending.Where(mod => mod.Status != ArmaModStatus.UpToDate))
                    mod.Status = ArmaModStatus.NotComplete;

                AppendOutput($"Mod update failed: {exception.Message}");
                return UpdateState.Error;
            }
            finally
            {
                EndOperation();
            }
        }

        public async Task<string> SteamGuardInput()
        {
            EnsureDialogCoordinator();
            return await DialogCoordinator.ShowInputAsync(this, "Steam Guard", "Enter the Steam Guard code") ?? string.Empty;
        }

        public async Task<MessageDialogResult> SteamGuardInputPhone()
        {
            EnsureDialogCoordinator();
            return await DialogCoordinator.ShowMessageAsync(
                this,
                "Steam Guard",
                "Approve the login in the Steam mobile app, then press OK. Press Cancel to enter a code instead.",
                MessageDialogStyle.AffirmativeAndNegative);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _statusTimer.Stop();
            _statusTimer.Tick -= Timer_Tick;
            _tokenSource.Cancel();
            DisposeSteamCmdClient();
            _tokenSource.Dispose();

            lock (_logGate)
            {
                _logWriter?.Dispose();
                _logWriter = null;
            }
        }

        private void MigrateLegacyPasswordToSession()
        {
            string encryptedPassword = Properties.Settings.Default.steamPassword;
            if (string.IsNullOrWhiteSpace(encryptedPassword))
                return;

            _sessionPassword = Encryption.Instance.DecryptData(encryptedPassword) ?? string.Empty;
            Properties.Settings.Default.steamPassword = string.Empty;
            Properties.Settings.Default.Save();
        }

        private async Task<bool> TryBeginOperationAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SteamUpdaterViewModel));

            if (!await _operationGate.WaitAsync(0))
            {
                AppendOutput("Another SteamCMD operation is already running.");
                return false;
            }

            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            _lastProgressMessage = null;
            _isBusy = true;
            Parameters.IsUpdating = true;
            Parameters.Progress = 0;
            UpdaterFaulted = false;
            RaisePropertyChanged(nameof(IsDownloading));
            RaisePropertyChanged(nameof(CanConfigureUpdater));
            return true;
        }

        private void EndOperation()
        {
            Parameters.IsUpdating = false;
            _isBusy = false;
            RaisePropertyChanged(nameof(IsDownloading));
            RaisePropertyChanged(nameof(CanConfigureUpdater));
            RefreshSteamCmdStatus();
            _operationGate.Release();
        }

        private async Task<SteamCmdClient> EnsureSteamCmdInstalledAsync(
            IProgress<SteamCmdProgress> progress,
            CancellationToken cancellationToken)
        {
            SteamCmdClient client = GetOrCreateSteamCmdClient();
            if (!client.IsInstalled)
                AppendOutput($"Installing Valve SteamCMD in {client.RootDirectory}...");

            await client.EnsureInstalledAsync(progress, cancellationToken);
            UpdaterOnline = client.IsInstalled;
            UpdaterFaulted = false;
            return client;
        }

        private SteamCmdClient GetOrCreateSteamCmdClient()
        {
            string root = SteamCmdCommandBuilder.ValidateAndNormalizePath(
                Parameters.SteamCmdDirectory,
                nameof(Parameters.SteamCmdDirectory));

            if (_steamCmdClient != null &&
                !string.Equals(_steamCmdClient.RootDirectory, root, StringComparison.OrdinalIgnoreCase))
            {
                if (_steamCmdClient.IsRunning)
                    throw new InvalidOperationException("SteamCMD's directory cannot be changed during an active operation.");

                DisposeSteamCmdClient();
            }

            return _steamCmdClient ??= new SteamCmdClient(root);
        }

        private IProgress<SteamCmdProgress> CreateProgressReporter() =>
            new Progress<SteamCmdProgress>(ReportSteamCmdProgress);

        private void ReportSteamCmdProgress(SteamCmdProgress progress)
        {
            // Every event is written here first, unfiltered, so the raw SteamCMD
            // chatter suppressed from the UI below is still available for troubleshooting.
            LogSteamCmdProgress(progress);

            if (progress.Percentage is double percentage)
                Parameters.Progress = Math.Clamp(percentage, 0, 100);

            // Raw SteamCMD output and per-chunk percentage lines can generate
            // thousands of UI updates during a large Workshop collection. The
            // structured status/error messages retain the useful information;
            // percentages are represented by the progress bar.
            if (progress.Kind == SteamCmdProgressKind.Output ||
                (progress.Percentage is > 0 &&
                 progress.Kind is SteamCmdProgressKind.DownloadingWorkshopItem or SteamCmdProgressKind.UpdatingServer))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(progress.Message) && progress.Message != _lastProgressMessage)
            {
                _lastProgressMessage = progress.Message;
                AppendOutput(progress.Message);
            }
        }

        private async Task<string> RequestGuardResponseAsync(
            SteamCmdGuardChallenge challenge,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher? dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                return await dispatcher
                    .InvokeAsync(() => ShowGuardPromptAsync(challenge, cancellationToken))
                    .Task
                    .Unwrap();
            }

            return await ShowGuardPromptAsync(challenge, cancellationToken);
        }

        private async Task<string> ShowGuardPromptAsync(
            SteamCmdGuardChallenge challenge,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureDialogCoordinator();

            if (challenge.Kind == SteamCmdGuardChallengeKind.MobileConfirmation)
            {
                MessageDialogResult response = await SteamGuardInputPhone();
                if (response == MessageDialogResult.Affirmative)
                    return string.Empty;
            }

            return await DialogCoordinator.ShowInputAsync(
                       this,
                       "Steam Guard",
                       string.IsNullOrWhiteSpace(challenge.Prompt) ? "Enter the Steam Guard code" : challenge.Prompt)
                   ?? string.Empty;
        }

        private void EnsureDialogCoordinator()
        {
            if (DialogCoordinator == null)
                throw new SteamCmdAuthenticationException("Steam Guard input is required, but the updater dialog is not available.");
        }

        private string GetStagingRoot(string requestedTarget, ulong workshopId)
        {
            if (!string.IsNullOrWhiteSpace(requestedTarget))
            {
                string fullTarget = Path.TrimEndingDirectorySeparator(Path.GetFullPath(requestedTarget));
                if (string.Equals(
                        Path.GetFileName(fullTarget),
                        workshopId.ToString(),
                        StringComparison.OrdinalIgnoreCase) &&
                    Path.GetDirectoryName(fullTarget) is string parent)
                {
                    return parent;
                }
            }

            return Parameters.ModStagingDirectory;
        }

        private static bool TryParseServerBranch(string? branchName, out SteamCmdServerBranch branch)
        {
            switch (branchName?.Trim().ToLowerInvariant())
            {
                case "public":
                case "stable":
                    branch = SteamCmdServerBranch.Public;
                    return true;
                case "contact":
                    branch = SteamCmdServerBranch.Contact;
                    return true;
                case "creatordlc":
                    branch = SteamCmdServerBranch.CreatorDlc;
                    return true;
                case "profiling":
                    branch = SteamCmdServerBranch.Profiling;
                    return true;
                default:
                    branch = default;
                    return false;
            }
        }

        private static bool IsLoginFailure(string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return false;

            return error.Contains("login", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("Steam Guard", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("account", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatSteamCmdFailure(string prefix, string? error, int? exitCode)
        {
            string detail = string.IsNullOrWhiteSpace(error) ? "SteamCMD did not report success." : error;
            return exitCode.HasValue
                       ? $"{prefix}: {detail} (exit code {exitCode.Value})"
                       : $"{prefix}: {detail}";
        }

        private static string FormatElapsed(TimeSpan elapsed) =>
            $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_disposed)
                RefreshSteamCmdStatus();
        }

        private void RefreshSteamCmdStatus()
        {
            try
            {
                // IsInstalled also checks the readiness marker written after
                // SteamCMD has completed its first self-update. An executable
                // left behind by an interrupted bootstrap is not ready yet.
                UpdaterOnline = GetOrCreateSteamCmdClient().IsInstalled;
            }
            catch
            {
                UpdaterOnline = false;
            }
        }

        private void DisposeSteamCmdClient()
        {
            _steamCmdClient?.Dispose();
            _steamCmdClient = null;
        }

        private void LogSteamCmdProgress(SteamCmdProgress progress)
        {
            string percentageSuffix = progress.Percentage is double percentage
                ? $" ({percentage:0.##}%)"
                : string.Empty;
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{progress.Kind}] {progress.Message}{percentageSuffix}";

            lock (_logGate)
            {
                try
                {
                    bool firstWrite = _logWriter == null;
                    _logWriter ??= CreateLogWriter();
                    if (firstWrite)
                        AppendOutput($"Detailed SteamCMD logging is being written to {LogFilePath}");
                    _logWriter.WriteLine(line);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        private static StreamWriter CreateLogWriter()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
            return new StreamWriter(LogFilePath, append: true) { AutoFlush = true };
        }

        private void AppendOutput(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Dispatcher? dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                _ = dispatcher.BeginInvoke(() => AppendOutput(message));
                return;
            }

            string cleanMessage = message.Replace("\r\n", "\n").Replace('\r', '\n').Trim('\n');
            string output = string.IsNullOrEmpty(Parameters.Output)
                ? cleanMessage
                : $"{Parameters.Output}\n{cleanMessage}";
            if (output.Length > MaximumConsoleCharacters)
            {
                int firstLineBreak = output.IndexOf(
                    '\n',
                    output.Length - MaximumConsoleCharacters);
                output = "[Earlier SteamCMD output was trimmed]\n" +
                         output[(firstLineBreak >= 0 ? firstLineBreak + 1 : output.Length - MaximumConsoleCharacters)..];
            }

            Parameters.Output = output;
        }

        private void RaisePropertyChanged(string property) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public static class UpdateState
    {
        public const int Success = 0;
        public const int Error = 1;
        public const int LoginFailed = 2;
        public const int Cancelled = 3;
    }
}
