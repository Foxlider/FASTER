using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FASTER.Services.SteamCmd;

using NUnit.Framework;

namespace FASTERTests.Services.SteamCmd;

[TestFixture]
[Category("SteamCmdLive")]
public sealed class SteamCmdLiveSmokeTests
{
    [Test]
    public async Task UpdateServer_ReachesAuthenticatedPromptBeforeAnyDownload()
    {
        string steamCmdRoot = Environment.GetEnvironmentVariable("FASTER_STEAMCMD_SMOKE_ROOT");
        if (string.IsNullOrWhiteSpace(steamCmdRoot))
            Assert.Ignore("Set FASTER_STEAMCMD_SMOKE_ROOT to run the opt-in SteamCMD handshake test.");

        string installDirectory = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"steamcmd-smoke-{Guid.NewGuid():N}");
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(45));
        using SteamCmdClient client = new(steamCmdRoot);
        CancelBeforeDownloadProgress progress = new(cancellation);

        try
        {
            SteamCmdServerUpdateResult result = await client.UpdateServerAsync(
                string.Empty,
                string.Empty,
                installDirectory,
                SteamCmdServerBranch.Profiling,
                progress: progress,
                cancellationToken: cancellation.Token);

            Assert.Multiple(() =>
            {
                Assert.That(progress.ReachedServerUpdate, Is.True);
                Assert.That(result.Cancelled, Is.True);
                Assert.That(result.Success, Is.False);
            });
        }
        finally
        {
            if (Directory.Exists(installDirectory) && !Directory.EnumerateFileSystemEntries(installDirectory).Any())
                Directory.Delete(installDirectory);
        }
    }

    private sealed class CancelBeforeDownloadProgress : IProgress<SteamCmdProgress>
    {
        private readonly CancellationTokenSource _cancellation;

        public CancelBeforeDownloadProgress(CancellationTokenSource cancellation)
        {
            _cancellation = cancellation;
        }

        public bool ReachedServerUpdate { get; private set; }

        public void Report(SteamCmdProgress value)
        {
            TestContext.Progress.WriteLine($"{value.Kind}: {value.Message}");
            if (value.Kind != SteamCmdProgressKind.UpdatingServer)
                return;

            ReachedServerUpdate = true;
            _cancellation.Cancel();
        }
    }
}
