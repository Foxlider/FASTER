using FASTER.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;

namespace FASTER.Services;

public interface ITelemetryBackend
{
    bool IsSupported { get; }
    Task InitializeAsync();
    Task SetEnabledAsync(bool enabled);
    void Send(string eventName);
}

public interface ITelemetryService
{
    string Status { get; }
    Task SetEnabledAsync(bool enabled);
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null);
    void TrackError(Exception exception, IDictionary<string, string>? properties = null);
}

public sealed class TelemetryService(ITelemetryBackend backend) : ITelemetryService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _enabled;
    private bool _initialized;
    public string Status { get; private set; } = "Disabled";

    public async Task SetEnabledAsync(bool enabled)
    {
        _enabled = enabled;
        await _gate.WaitAsync();
        try
        {
            if (!backend.IsSupported) { Status = enabled ? "Unavailable on this platform" : "Disabled"; return; }
            if (!_enabled)
            {
                if (_initialized) await backend.SetEnabledAsync(false);
                Status = "Disabled";
                return;
            }
            if (!_initialized) { await backend.InitializeAsync(); _initialized = true; }
            await backend.SetEnabledAsync(_enabled);
            Status = _enabled ? "App Center initialized; delivery is unverified (retired service)" : "Disabled";
        }
        catch (Exception ex)
        {
            _enabled = false;
            Status = "Unavailable: App Center initialization failed";
            Logger.LogCritical("Telemetry unavailable: " + ex.GetType().Name);
        }
        finally { _gate.Release(); }
    }

    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
    {
        if (!_enabled || !_initialized) return;
        try
        {
            // Event names are fixed at call sites. Legacy payloads can contain credentials or paths.
            backend.Send(eventName);
        }
        catch (Exception ex)
        {
            _enabled = false;
            Status = "Unavailable: App Center sending failed";
            Logger.LogCritical("Telemetry unavailable: " + ex.GetType().Name);
        }
    }

    public void TrackError(Exception exception, IDictionary<string, string>? properties = null)
    {
        Logger.LogCritical(exception.ToString());
        TrackEvent("Application error");
    }
}

public sealed class AppCenterTelemetryBackend : ITelemetryBackend
{
    public bool IsSupported => OperatingSystem.IsWindows();
    public Task InitializeAsync()
    {
        // Automatic crash uploads include exception messages and paths; keep crash details local.
        AppCenter.SetUserId(null);
        AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics));
        if (!AppCenter.Configured) throw new InvalidOperationException("App Center is not configured.");
        return Task.CompletedTask;
    }
    public Task SetEnabledAsync(bool enabled) => Analytics.SetEnabledAsync(enabled);
    public void Send(string eventName) => Analytics.TrackEvent(eventName);
}

public static class Telemetry
{
    public static ITelemetryService Current { get; set; } = new TelemetryService(new AppCenterTelemetryBackend());
    public static Task SetEnabledAsync(bool enabled) => Current.SetEnabledAsync(enabled);
    public static void TrackEvent(string name, IDictionary<string, string>? properties = null) => Current.TrackEvent(name, properties);
    public static void TrackError(Exception exception, IDictionary<string, string>? properties = null) => Current.TrackError(exception, properties);
}
