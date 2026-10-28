namespace FASTER.Services;

public enum UpdateResult { NoUpdate, Ready, Unpackaged, Busy, Cancelled, Failed }
public interface IApplicationUpdateBackend
{
    bool IsInstalled { get; }
    Task<bool> CheckAsync(CancellationToken cancellationToken);
    Task DownloadAsync(CancellationToken cancellationToken);
    void ApplyAndRestart();
}
public interface IApplicationUpdates
{
    string Status { get; }
    Task<UpdateResult> CheckAsync(CancellationToken cancellationToken = default);
    bool TryRestart();
}

public sealed class ApplicationUpdates(IApplicationUpdateBackend backend, Func<bool> operationsBusy) : IApplicationUpdates
{
    private readonly SemaphoreSlim _checkGate = new(1, 1);
    private bool _ready;
    public string Status { get; private set; } = "Not checked";

    public async Task<UpdateResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        if (!await _checkGate.WaitAsync(0, CancellationToken.None)) return UpdateResult.Busy;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!backend.IsInstalled)
            { Status = "Development installation: download releases from GitHub."; return UpdateResult.Unpackaged; }
            if (_ready) return UpdateResult.Ready;
            Status = "Checking for updates...";
            if (!await backend.CheckAsync(cancellationToken))
            { Status = "No update available."; return UpdateResult.NoUpdate; }
            Status = "Downloading application update...";
            await backend.DownloadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _ready = true;
            Status = "Update ready. Restart when server and mod maintenance has finished.";
            return UpdateResult.Ready;
        }
        catch (OperationCanceledException) { Status = "Update check cancelled."; return UpdateResult.Cancelled; }
        catch (Exception ex) { Status = "Update check failed: " + ex.Message; return UpdateResult.Failed; }
        finally { _checkGate.Release(); }
    }

    public bool TryRestart()
    {
        if (!_ready) return false;
        if (operationsBusy()) { Status = "Restart deferred until downloads and maintenance finish."; return false; }
        try { backend.ApplyAndRestart(); return true; }
        catch (Exception ex) { Status = "Could not apply update: " + ex.Message; return false; }
    }
}
