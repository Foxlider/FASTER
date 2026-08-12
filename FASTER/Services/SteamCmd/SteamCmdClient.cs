using System.IO;

namespace FASTER.Services.SteamCmd;

public sealed class SteamCmdClient : IDisposable, IAsyncDisposable
{
    private const int MaximumWorkshopDownloadAttempts = 3;
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(45);

    private readonly SteamCmdInstaller _installer;
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly object _stateGate = new();

    private SteamCmdSession? _activeSession;
    private CancellationTokenSource? _activeCancellation;
    private bool _operationActive;
    private bool _disposed;

    public SteamCmdClient(string rootDirectory)
    {
        RootDirectory = SteamCmdCommandBuilder.ValidateAndNormalizePath(
            rootDirectory,
            nameof(rootDirectory));
        _installer = new SteamCmdInstaller(RootDirectory);
    }

    public string RootDirectory { get; }

    public bool IsInstalled => _installer.IsInstalled;

    public bool IsRunning
    {
        get
        {
            lock (_stateGate)
                return _operationActive || _activeSession is { HasExited: false };
        }
    }

    public async Task EnsureInstalledAsync(
        IProgress<SteamCmdProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        using CancellationTokenSource operationCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        SetActiveCancellation(operationCancellation);
        SetOperationActive(true);
        try
        {
            await _installer.EnsureInstalledAsync(progress, operationCancellation.Token).ConfigureAwait(false);
        }
        finally
        {
            ClearActiveState(null, operationCancellation);
            SetOperationActive(false);
            _operationGate.Release();
        }
    }

    public async Task<SteamCmdWorkshopBatchResult> DownloadWorkshopItemsAsync(
        string username,
        string password,
        IEnumerable<ulong> workshopIds,
        Func<SteamCmdGuardChallenge, CancellationToken, Task<string>>? guardCodeProvider = null,
        IProgress<SteamCmdProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new SteamCmdAuthenticationException(
                "Workshop downloads require a Steam account that owns Arma 3; an API key is not a Steam login.");
        }

        ArgumentNullException.ThrowIfNull(workshopIds);
        List<ulong> ids = NormalizeWorkshopIds(workshopIds);
        if (ids.Count == 0)
            throw new ArgumentException("At least one Workshop ID is required.", nameof(workshopIds));

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        using CancellationTokenSource operationCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        SetActiveCancellation(operationCancellation);
        SetOperationActive(true);

        List<SteamCmdWorkshopItemResult> results = new(ids.Count);
        SteamCmdSession? session = null;
        int? exitCode = null;
        try
        {
            await _installer.EnsureInstalledAsync(progress, operationCancellation.Token).ConfigureAwait(false);
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Starting,
                "Starting SteamCMD..."));

            session = StartSession(password, progress);
            SetActiveSession(session);
            await AuthenticateAsync(
                session,
                username,
                password,
                guardCodeProvider,
                progress,
                operationCancellation.Token).ConfigureAwait(false);

            for (int index = 0; index < ids.Count; index++)
            {
                operationCancellation.Token.ThrowIfCancellationRequested();
                ulong workshopId = ids[index];
                progress?.Report(new SteamCmdProgress(
                    SteamCmdProgressKind.DownloadingWorkshopItem,
                    $"Downloading Workshop item {workshopId} ({index + 1}/{ids.Count})...",
                    0,
                    workshopId));

                SteamCmdWorkshopItemResult itemResult = await DownloadWorkshopItemWithRetriesAsync(
                    session,
                    workshopId,
                    progress,
                    operationCancellation.Token).ConfigureAwait(false);
                results.Add(itemResult);
            }

            exitCode = await session.QuitAndWaitAsync(operationCancellation.Token).ConfigureAwait(false);
            string? exitError = exitCode is not 0
                ? $"SteamCMD exited with code {exitCode}."
                : null;

            progress?.Report(new SteamCmdProgress(
                results.All(item => item.Success) && exitError == null
                    ? SteamCmdProgressKind.Completed
                    : SteamCmdProgressKind.Warning,
                results.All(item => item.Success) && exitError == null
                    ? "SteamCMD completed all Workshop downloads."
                    : "SteamCMD completed with one or more Workshop failures.",
                100));

            return new SteamCmdWorkshopBatchResult(results, false, exitCode, exitError);
        }
        catch (OperationCanceledException)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            AddUnfinishedResults(ids, results, "Cancelled.");
            return new SteamCmdWorkshopBatchResult(results, true, session?.ExitCode, "Cancelled.");
        }
        catch (SteamCmdAuthenticationException)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            string error = SanitizeExceptionMessage(exception.Message, password);
            AddUnfinishedResults(ids, results, error);
            return new SteamCmdWorkshopBatchResult(results, false, session?.ExitCode, error);
        }
        finally
        {
            await DisposeSessionSafelyAsync(session).ConfigureAwait(false);
            ClearActiveState(session, operationCancellation);
            SetOperationActive(false);
            _operationGate.Release();
        }
    }

    public async Task<SteamCmdServerUpdateResult> UpdateServerAsync(
        string username,
        string password,
        string installDirectory,
        SteamCmdServerBranch branch,
        Func<SteamCmdGuardChallenge, CancellationToken, Task<string>>? guardCodeProvider = null,
        IProgress<SteamCmdProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        string forceInstallCommand = SteamCmdCommandBuilder.BuildForceInstallDirectoryCommand(installDirectory);
        string normalizedInstallDirectory = SteamCmdCommandBuilder.ValidateAndNormalizePath(
            installDirectory,
            nameof(installDirectory));
        string updateCommand = SteamCmdCommandBuilder.BuildServerUpdateCommand(branch);
        Directory.CreateDirectory(normalizedInstallDirectory);

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        using CancellationTokenSource operationCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        SetActiveCancellation(operationCancellation);
        SetOperationActive(true);

        SteamCmdSession? session = null;
        try
        {
            await _installer.EnsureInstalledAsync(progress, operationCancellation.Token).ConfigureAwait(false);
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Starting,
                "Starting SteamCMD..."));

            session = StartSession(password, progress);
            SetActiveSession(session);

            // SteamCMD requires force_install_dir before login for reliable app
            // placement. Sending it afterward is ignored by some app-update flows.
            await WaitForPromptAsync(session, operationCancellation.Token).ConfigureAwait(false);
            await session.SendCommandAsync(forceInstallCommand, operationCancellation.Token).ConfigureAwait(false);
            await WaitForPromptAfterCommandAsync(session, operationCancellation.Token).ConfigureAwait(false);

            await AuthenticateAsync(
                session,
                username,
                password,
                guardCodeProvider,
                progress,
                operationCancellation.Token,
                waitForInitialPrompt: false).ConfigureAwait(false);

            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.UpdatingServer,
                $"Updating Arma 3 Dedicated Server ({branch})...",
                0));
            await session.SendCommandAsync(updateCommand, operationCancellation.Token).ConfigureAwait(false);
            string? commandError = await WaitForServerUpdateAsync(
                session,
                progress,
                operationCancellation.Token).ConfigureAwait(false);

            int? exitCode = await session.QuitAndWaitAsync(operationCancellation.Token).ConfigureAwait(false);
            string? error = commandError;
            if (error == null && exitCode is not 0)
                error = $"SteamCMD exited with code {exitCode}.";

            bool success = error == null;
            if (success && !ServerInstallHasContent(normalizedInstallDirectory))
            {
                success = false;
                error = "SteamCMD reported success, but the Arma 3 server executable is missing.";
            }

            progress?.Report(new SteamCmdProgress(
                success ? SteamCmdProgressKind.Completed : SteamCmdProgressKind.Error,
                success ? "Arma 3 Dedicated Server update completed." : error!,
                success ? 100 : null));
            return new SteamCmdServerUpdateResult(success, false, exitCode, error);
        }
        catch (OperationCanceledException)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            return new SteamCmdServerUpdateResult(false, true, session?.ExitCode, "Cancelled.");
        }
        catch (SteamCmdAuthenticationException)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            await StopSessionSafelyAsync(session).ConfigureAwait(false);
            return new SteamCmdServerUpdateResult(
                false,
                false,
                session?.ExitCode,
                SanitizeExceptionMessage(exception.Message, password));
        }
        finally
        {
            await DisposeSessionSafelyAsync(session).ConfigureAwait(false);
            ClearActiveState(session, operationCancellation);
            SetOperationActive(false);
            _operationGate.Release();
        }
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenSource? operationCancellation;
        SteamCmdSession? session;
        lock (_stateGate)
        {
            operationCancellation = _activeCancellation;
            session = _activeSession;
        }

        operationCancellation?.Cancel();
        if (session != null)
            await session.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Reset()
    {
        ThrowIfDisposed();
        lock (_stateGate)
        {
            if (_operationActive || _activeSession is { HasExited: false })
                throw new InvalidOperationException("SteamCMD cannot be reset while an operation is running.");

            _activeSession = null;
            _activeCancellation = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        CancelAsync(CancellationToken.None).GetAwaiter().GetResult();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await CancelAsync(CancellationToken.None).ConfigureAwait(false);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private SteamCmdSession StartSession(string password, IProgress<SteamCmdProgress>? progress)
    {
        IEnumerable<string> secrets = string.IsNullOrEmpty(password) ? [] : [password];
        return SteamCmdSession.Start(
            _installer.ExecutablePath,
            RootDirectory,
            secrets,
            progress);
    }

    private static async Task AuthenticateAsync(
        SteamCmdSession session,
        string username,
        string password,
        Func<SteamCmdGuardChallenge, CancellationToken, Task<string>>? guardCodeProvider,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken,
        bool waitForInitialPrompt = true)
    {
        progress?.Report(new SteamCmdProgress(
            SteamCmdProgressKind.Authenticating,
            string.IsNullOrWhiteSpace(username)
                ? "Logging in to Steam anonymously..."
                : "Logging in to Steam..."));

        if (waitForInitialPrompt)
            await WaitForPromptAsync(session, cancellationToken).ConfigureAwait(false);
        await session.SendCommandAsync(
            SteamCmdCommandBuilder.BuildLoginCommand(username),
            cancellationToken).ConfigureAwait(false);

        bool loggedIn = false;
        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                PromptTimeout,
                cancellationToken).ConfigureAwait(false);
            switch (outputEvent.Kind)
            {
                case SteamCmdOutputEventKind.PasswordPrompt:
                    if (string.IsNullOrEmpty(password))
                    {
                        throw new SteamCmdAuthenticationException(
                            "SteamCMD requested a password. Enter the account password and try again, or complete a cached SteamCMD login first.");
                    }
                    await session.SendSecretAsync(password, cancellationToken).ConfigureAwait(false);
                    break;

                case SteamCmdOutputEventKind.SteamGuardPrompt:
                    await AnswerGuardChallengeAsync(
                        session,
                        CreateGuardChallenge(outputEvent.Text),
                        guardCodeProvider,
                        progress,
                        cancellationToken).ConfigureAwait(false);
                    break;

                case SteamCmdOutputEventKind.MobileConfirmationPrompt:
                    await AnswerGuardChallengeAsync(
                        session,
                        new SteamCmdGuardChallenge(
                            SteamCmdGuardChallengeKind.MobileConfirmation,
                            outputEvent.Text),
                        guardCodeProvider,
                        progress,
                        cancellationToken).ConfigureAwait(false);
                    break;

                case SteamCmdOutputEventKind.LoggedIn:
                    loggedIn = true;
                    break;

                case SteamCmdOutputEventKind.LoginFailed:
                    throw new SteamCmdAuthenticationException(
                        string.IsNullOrWhiteSpace(outputEvent.Text)
                            ? "Steam rejected the login."
                            : outputEvent.Text);

                // "Logged in OK" is followed by a user-info/licence refresh.
                // A timeout or error in that phase is still an authentication
                // failure even though the first acknowledgement was received.
                case SteamCmdOutputEventKind.Timeout:
                case SteamCmdOutputEventKind.Error:
                    throw new SteamCmdAuthenticationException(outputEvent.Text);

                case SteamCmdOutputEventKind.Prompt:
                    if (loggedIn)
                        return;
                    throw new SteamCmdAuthenticationException(
                        "SteamCMD returned to its prompt without confirming login.");
            }
        }
    }

    private static async Task AnswerGuardChallengeAsync(
        SteamCmdSession session,
        SteamCmdGuardChallenge challenge,
        Func<SteamCmdGuardChallenge, CancellationToken, Task<string>>? guardCodeProvider,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (guardCodeProvider == null)
            throw new SteamCmdAuthenticationException("Steam Guard approval is required to log in.");

        progress?.Report(new SteamCmdProgress(
            SteamCmdProgressKind.WaitingForGuard,
            challenge.Prompt));
        // A UI callback may itself expose no cancellation API. WaitAsync keeps
        // the SteamCMD operation cancellable even if that callback is still
        // waiting for a dialog response.
        string? response = await guardCodeProvider(challenge, cancellationToken)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        response ??= string.Empty;
        if (challenge.Kind != SteamCmdGuardChallengeKind.MobileConfirmation && string.IsNullOrWhiteSpace(response))
            throw new SteamCmdAuthenticationException("A Steam Guard code was not provided.");

        // Mobile confirmation is completed out-of-band in Steam's app. There
        // is no code to write; continue consuming output until login completes.
        if (challenge.Kind == SteamCmdGuardChallengeKind.MobileConfirmation && response.Length == 0)
            return;

        await session.SendSecretAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static SteamCmdGuardChallenge CreateGuardChallenge(string prompt)
    {
        SteamCmdGuardChallengeKind kind;
        if (prompt.Contains("email", StringComparison.OrdinalIgnoreCase))
            kind = SteamCmdGuardChallengeKind.EmailCode;
        else if (prompt.Contains("two-factor", StringComparison.OrdinalIgnoreCase) ||
                 prompt.Contains("authenticator", StringComparison.OrdinalIgnoreCase))
            kind = SteamCmdGuardChallengeKind.TwoFactorCode;
        else
            kind = SteamCmdGuardChallengeKind.Unknown;

        return new SteamCmdGuardChallenge(kind, prompt);
    }

    private async Task<SteamCmdWorkshopItemResult> DownloadWorkshopItemAsync(
        SteamCmdSession session,
        ulong workshopId,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        await session.SendCommandAsync(
            SteamCmdCommandBuilder.BuildWorkshopDownloadCommand(workshopId),
            cancellationToken).ConfigureAwait(false);

        bool successSignal = false;
        string? failure = null;
        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                DownloadTimeout,
                cancellationToken).ConfigureAwait(false);
            switch (outputEvent.Kind)
            {
                case SteamCmdOutputEventKind.WorkshopDownloadSucceeded
                    when outputEvent.WorkshopId == workshopId:
                    successSignal = true;
                    break;

                case SteamCmdOutputEventKind.WorkshopDownloadFailed
                    when outputEvent.WorkshopId == workshopId:
                    failure = outputEvent.Text;
                    break;

                case SteamCmdOutputEventKind.Progress when outputEvent.ProgressPercent is double percentage:
                    progress?.Report(new SteamCmdProgress(
                        SteamCmdProgressKind.DownloadingWorkshopItem,
                        $"Downloading Workshop item {workshopId}: {percentage:0.00}%",
                        Math.Clamp(percentage, 0, 100),
                        workshopId));
                    break;

                case SteamCmdOutputEventKind.Timeout
                    when outputEvent.WorkshopId is null || outputEvent.WorkshopId == workshopId:
                case SteamCmdOutputEventKind.Error
                    when outputEvent.WorkshopId is null || outputEvent.WorkshopId == workshopId:
                    failure ??= outputEvent.Text;
                    break;

                case SteamCmdOutputEventKind.LoginFailed:
                    if (IsLikelyTransientWorkshopFailure(outputEvent.Text))
                    {
                        failure ??= outputEvent.Text;
                        break;
                    }
                    throw new SteamCmdAuthenticationException(outputEvent.Text);

                case SteamCmdOutputEventKind.Prompt:
                    string sourcePath = GetWorkshopContentPath(workshopId);
                    bool hasContent = successSignal && DirectoryHasContent(sourcePath);
                    if (!hasContent && failure == null)
                    {
                        failure = successSignal
                            ? "SteamCMD reported success, but the Workshop content directory is missing or empty."
                            : "SteamCMD returned no success result for this Workshop item.";
                    }

                    bool success = successSignal && hasContent && failure == null;
                    return new SteamCmdWorkshopItemResult(workshopId, success, sourcePath, failure);
            }
        }
    }

    private async Task<SteamCmdWorkshopItemResult> DownloadWorkshopItemWithRetriesAsync(
        SteamCmdSession session,
        ulong workshopId,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        SteamCmdWorkshopItemResult? result = null;
        for (int attempt = 1; attempt <= MaximumWorkshopDownloadAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result = await DownloadWorkshopItemAsync(
                session,
                workshopId,
                progress,
                cancellationToken).ConfigureAwait(false);

            if (result.Success)
            {
                ReportWorkshopItemResult(result, progress);
                return result;
            }

            if (attempt == MaximumWorkshopDownloadAttempts ||
                !IsLikelyTransientWorkshopFailure(result.Error))
            {
                ReportWorkshopItemResult(result, progress);
                return result;
            }

            TimeSpan delay = GetWorkshopRetryDelay(attempt);
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Warning,
                $"Workshop item {workshopId} failed transiently. Retrying in {delay.TotalSeconds:0} second(s) " +
                $"(attempt {attempt + 1}/{MaximumWorkshopDownloadAttempts})...",
                WorkshopId: workshopId));
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        // The bounded loop always returns, but retaining a defensive result keeps
        // future changes from turning an exhausted retry into a false success.
        result ??= new SteamCmdWorkshopItemResult(
            workshopId,
            false,
            GetWorkshopContentPath(workshopId),
            "SteamCMD did not complete the Workshop download.");
        ReportWorkshopItemResult(result, progress);
        return result;
    }

    /// <summary>
    /// Returns whether a completed Workshop command failed for a reason that is
    /// safe to retry in the same authenticated SteamCMD session.
    /// </summary>
    public static bool IsLikelyTransientWorkshopFailure(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return false;

        string message = error.Trim();
        string[] permanentMarkers =
        [
            "access denied",
            "account logon denied",
            "authentication",
            "configuration",
            "decryption key",
            "disk",
            "does not own",
            "file system",
            "disk full",
            "disk write",
            "file permission",
            "i/o operation",
            "invalid parameter",
            "invalid password",
            "license",
            "login failed",
            "logon failed",
            "missing file privileges",
            "missing subscription",
            "no match",
            "no subscription",
            "not owned",
            "not authorized",
            "ownership",
            "password",
            "permission",
            "read-only",
            "steam guard",
            "subscription",
            "two-factor",
            "unauthorized",
            "write failure"
        ];
        if (permanentMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            return false;

        string[] transientMarkers =
        [
            "failure",
            "busy",
            "connection",
            "content server",
            "network",
            "rate limit",
            "service unavailable",
            "temporar",
            "timed out",
            "timeout",
            "try again"
        ];
        return transientMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static TimeSpan GetWorkshopRetryDelay(int completedAttempts) =>
        TimeSpan.FromSeconds(Math.Min(4, 1 << Math.Clamp(completedAttempts - 1, 0, 2)));

    private static void ReportWorkshopItemResult(
        SteamCmdWorkshopItemResult result,
        IProgress<SteamCmdProgress>? progress)
    {
        progress?.Report(new SteamCmdProgress(
            result.Success ? SteamCmdProgressKind.Completed : SteamCmdProgressKind.Error,
            result.Success
                ? $"Workshop item {result.WorkshopId} downloaded successfully."
                : $"Workshop item {result.WorkshopId} failed: {result.Error}",
            result.Success ? 100 : null,
            result.WorkshopId));
    }

    private static async Task WaitForPromptAsync(
        SteamCmdSession session,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                PromptTimeout,
                cancellationToken).ConfigureAwait(false);
            if (outputEvent.Kind == SteamCmdOutputEventKind.Prompt)
                return;
            if (outputEvent.Kind == SteamCmdOutputEventKind.Error)
                throw new SteamCmdException(outputEvent.Text);
        }
    }

    private static async Task WaitForPromptAfterCommandAsync(
        SteamCmdSession session,
        CancellationToken cancellationToken)
    {
        string? error = null;
        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                PromptTimeout,
                cancellationToken).ConfigureAwait(false);
            switch (outputEvent.Kind)
            {
                case SteamCmdOutputEventKind.Error:
                case SteamCmdOutputEventKind.Timeout:
                    error ??= outputEvent.Text;
                    break;
                case SteamCmdOutputEventKind.Prompt:
                    if (error != null)
                        throw new SteamCmdException(error);
                    return;
            }
        }
    }

    private static async Task<string?> WaitForServerUpdateAsync(
        SteamCmdSession session,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        bool successSignal = false;
        string? error = null;
        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                DownloadTimeout,
                cancellationToken).ConfigureAwait(false);
            switch (outputEvent.Kind)
            {
                case SteamCmdOutputEventKind.AppUpdateSucceeded
                    when outputEvent.AppId == SteamCmdCommandBuilder.ServerAppId:
                    successSignal = true;
                    break;
                case SteamCmdOutputEventKind.Progress when outputEvent.ProgressPercent is double percentage:
                    progress?.Report(new SteamCmdProgress(
                        SteamCmdProgressKind.UpdatingServer,
                        $"Updating Arma 3 Dedicated Server: {percentage:0.00}%",
                        Math.Clamp(percentage, 0, 100)));
                    break;
                case SteamCmdOutputEventKind.Error:
                case SteamCmdOutputEventKind.Timeout:
                    error ??= outputEvent.Text;
                    break;
                case SteamCmdOutputEventKind.LoginFailed:
                    throw new SteamCmdAuthenticationException(outputEvent.Text);
                case SteamCmdOutputEventKind.Prompt:
                    return error ?? (successSignal
                        ? null
                        : "SteamCMD returned no success result for app 233780.");
            }
        }
    }

    private string GetWorkshopContentPath(ulong workshopId) => Path.Combine(
        RootDirectory,
        "steamapps",
        "workshop",
        "content",
        SteamCmdCommandBuilder.WorkshopAppId.ToString(),
        workshopId.ToString());

    private static bool DirectoryHasContent(string path)
    {
        try
        {
            return Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool ServerInstallHasContent(string installDirectory) =>
        File.Exists(Path.Combine(installDirectory, "arma3server_x64.exe")) ||
        File.Exists(Path.Combine(installDirectory, "arma3server.exe"));

    private static List<ulong> NormalizeWorkshopIds(IEnumerable<ulong> workshopIds)
    {
        HashSet<ulong> seen = new();
        List<ulong> result = new();
        foreach (ulong workshopId in workshopIds)
        {
            if (workshopId == 0)
                throw new ArgumentOutOfRangeException(nameof(workshopIds), "Workshop IDs must be greater than zero.");
            if (seen.Add(workshopId))
                result.Add(workshopId);
        }
        return result;
    }

    private void AddUnfinishedResults(
        IEnumerable<ulong> requestedIds,
        ICollection<SteamCmdWorkshopItemResult> results,
        string error)
    {
        HashSet<ulong> completedIds = results.Select(result => result.WorkshopId).ToHashSet();
        foreach (ulong workshopId in requestedIds.Where(id => !completedIds.Contains(id)))
        {
            results.Add(new SteamCmdWorkshopItemResult(
                workshopId,
                false,
                GetWorkshopContentPath(workshopId),
                error));
        }
    }

    private static string SanitizeExceptionMessage(string message, string secret)
    {
        if (string.IsNullOrEmpty(secret))
            return message;
        return message.Replace(secret, "[REDACTED]", StringComparison.Ordinal);
    }

    private static async Task StopSessionSafelyAsync(SteamCmdSession? session)
    {
        if (session == null)
            return;
        try
        {
            await session.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is SteamCmdException or IOException or InvalidOperationException or ObjectDisposedException)
        {
        }
    }

    private static async Task DisposeSessionSafelyAsync(SteamCmdSession? session)
    {
        if (session == null)
            return;
        try
        {
            await session.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is SteamCmdException or IOException or InvalidOperationException or ObjectDisposedException)
        {
        }
    }

    private void SetActiveSession(SteamCmdSession session)
    {
        lock (_stateGate)
            _activeSession = session;
    }

    private void SetActiveCancellation(CancellationTokenSource cancellation)
    {
        lock (_stateGate)
            _activeCancellation = cancellation;
    }

    private void SetOperationActive(bool active)
    {
        lock (_stateGate)
            _operationActive = active;
    }

    private void ClearActiveState(
        SteamCmdSession? session,
        CancellationTokenSource operationCancellation)
    {
        lock (_stateGate)
        {
            if (ReferenceEquals(_activeSession, session))
                _activeSession = null;
            if (ReferenceEquals(_activeCancellation, operationCancellation))
                _activeCancellation = null;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
