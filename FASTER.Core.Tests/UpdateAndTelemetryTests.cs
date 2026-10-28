using FASTER.Services;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class UpdateAndTelemetryTests
{
    private sealed class UpdateBackend : IApplicationUpdateBackend
    {
        public bool IsInstalled { get; set; } = true;
        public bool Available { get; set; }
        public bool Offline { get; set; }
        public int Downloads, Restarts, Checks;
        public TaskCompletionSource? Wait;
        public async Task<bool> CheckAsync(CancellationToken token)
        {
            Checks++;
            if (Wait != null) await Wait.Task.WaitAsync(token);
            if (Offline) throw new IOException("offline");
            return Available;
        }
        public Task DownloadAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); Downloads++; return Task.CompletedTask; }
        public void ApplyAndRestart() => Restarts++;
    }

    [Test]
    public async Task UpdateChecksHandleNoUpdateOfflineAndUnpackaged()
    {
        var backend = new UpdateBackend();
        var service = new ApplicationUpdates(backend, () => false);
        Assert.That(await service.CheckAsync(), Is.EqualTo(UpdateResult.NoUpdate));
        backend.Offline = true;
        Assert.That(await service.CheckAsync(), Is.EqualTo(UpdateResult.Failed));
        Assert.That(service.Status, Does.Contain("offline"));
        backend.IsInstalled = false;
        Assert.That(await service.CheckAsync(), Is.EqualTo(UpdateResult.Unpackaged));
        Assert.That(backend.Downloads, Is.Zero);
    }

    [Test]
    public async Task RestartWaitsForOperationsAndCheckIsNotConcurrent()
    {
        var backend = new UpdateBackend { Available = true, Wait = new() };
        bool busy = true;
        var service = new ApplicationUpdates(backend, () => busy);
        var first = service.CheckAsync();
        Assert.That(await service.CheckAsync(), Is.EqualTo(UpdateResult.Busy));
        backend.Wait.SetResult();
        Assert.That(await first, Is.EqualTo(UpdateResult.Ready));
        Assert.That(service.TryRestart(), Is.False);
        Assert.That(backend.Restarts, Is.Zero);
        busy = false;
        Assert.That(service.TryRestart(), Is.True);
        Assert.That(backend.Restarts, Is.EqualTo(1));
        Assert.That(backend.Downloads, Is.EqualTo(1));
    }

    [Test]
    public async Task CancelledCheckCanBeRetried()
    {
        var backend = new UpdateBackend { Wait = new() };
        var service = new ApplicationUpdates(backend, () => false);
        using var cancel = new CancellationTokenSource();
        var check = service.CheckAsync(cancel.Token);
        cancel.Cancel();
        Assert.That(await check, Is.EqualTo(UpdateResult.Cancelled));
        backend.Wait = null;
        Assert.That(await service.CheckAsync(), Is.EqualTo(UpdateResult.NoUpdate));
    }

    private sealed class TelemetryBackend : ITelemetryBackend
    {
        public bool IsSupported { get; set; } = true;
        public bool Fail { get; set; }
        public int Initializations, Sends;
        public bool Enabled;
        public Task InitializeAsync() { Initializations++; if (Fail) throw new IOException(); return Task.CompletedTask; }
        public Task SetEnabledAsync(bool enabled) { Enabled = enabled; return Task.CompletedTask; }
        public void Send(string name) { if (Fail) throw new IOException(); Sends++; }
    }

    [Test]
    public async Task OptOutPreventsInitializationAndSending()
    {
        var backend = new TelemetryBackend();
        var service = new TelemetryService(backend);
        await service.SetEnabledAsync(false);
        service.TrackEvent("Example", new Dictionary<string, string> { ["Name"] = "private" });
        Assert.That(backend.Initializations, Is.Zero);
        Assert.That(backend.Sends, Is.Zero);
        await service.SetEnabledAsync(true);
        service.TrackEvent("Example");
        Assert.That(backend.Sends, Is.EqualTo(1));
        await service.SetEnabledAsync(false);
        service.TrackEvent("Example");
        Assert.That(backend.Enabled, Is.False);
        Assert.That(backend.Sends, Is.EqualTo(1));
    }

    [Test]
    public async Task BackendFailureAndUnsupportedPlatformAreUnavailable()
    {
        var backend = new TelemetryBackend { Fail = true };
        var service = new TelemetryService(backend);
        await service.SetEnabledAsync(true);
        Assert.That(service.Status, Does.StartWith("Unavailable"));
        backend.Fail = false;
        await service.SetEnabledAsync(true);
        backend.Fail = true;
        Assert.DoesNotThrow(() => service.TrackEvent("Example"));
        Assert.That(service.Status, Does.StartWith("Unavailable"));
        backend.IsSupported = false;
        await service.SetEnabledAsync(true);
        Assert.That(service.Status, Does.Contain("platform"));
    }
}
