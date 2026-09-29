using System.Diagnostics;
using FASTER.Services;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class ProcessMonitoringTests
{
    private string _output = null!;
    [SetUp] public void SetUp() => _output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    [TearDown] public void TearDown() { if (Directory.Exists(_output)) Directory.Delete(_output, true); }

    [Test]
    public void CpuIsNormalizedToMachineCapacity() => Assert.That(
        ProcessMonitor.CalculateCpu(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 4), Is.EqualTo(50));

    [Test]
    public async Task CapturesOutputAndTracksExitWithBoundedHistory()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux fixture process"); return; }
        using var monitor = new ProcessMonitor(3, 128, _output);
        var start = new ProcessStartInfo("/bin/sh");
        start.Environment.Clear();
        start.Environment["PATH"] = "/usr/bin:/bin";
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("echo stdout; echo stderr >&2; sleep 2; i=0; while [ $i -lt 80 ]; do echo line$i; i=$((i+1)); done");
        var identity = monitor.Launch(start);
        for (int i = 0; i < 6; i++) { await Task.Delay(50); monitor.Sample(); }
        var sample = monitor.Sample().Single();
        Assert.That(sample.History.Count, Is.EqualTo(3));
        Assert.That(sample.MemoryBytes, Is.GreaterThan(0));
        Assert.That(monitor.ReadOutput(identity), Does.Contain("stdout").And.Contain("stderr"));
        monitor.Pause(identity, true);
        var pausedTime = monitor.Sample().Single().History.Last().Time;
        await Task.Delay(50);
        Assert.That(monitor.Sample().Single().History.Last().Time, Is.EqualTo(pausedTime));
        monitor.Pause(identity, false);
        Assert.That(monitor.Sample().Single().History.Last().Time, Is.GreaterThan(pausedTime));
        monitor.IsPaused = true;
        Assert.That(monitor.Sample().Single().Paused, Is.True);
        monitor.IsPaused = false;
        for (int i = 0; i < 100 && !monitor.Sample().Single().Exited; i++) await Task.Delay(50);
        Assert.That(monitor.Sample().Single().Exited, Is.True);
        Assert.That(monitor.ReadOutput(identity).Length, Is.LessThanOrEqualTo(128));
    }

    [Test]
    public async Task DisposingMonitorDoesNotTerminateServer()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux fixture process"); return; }
        var monitor = new ProcessMonitor(outputDirectory: _output);
        var start = new ProcessStartInfo("/bin/sh");
        start.Environment.Clear();
        start.Environment["PATH"] = "/usr/bin:/bin";
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("while true; do echo alive; sleep 0.05; done");
        var identity = monitor.Launch(start);
        using var process = Process.GetProcessById(identity.Pid);
        try
        {
            monitor.Dispose();
            await Task.Delay(200);
            Assert.That(process.HasExited, Is.False);
        }
        finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
    }

    [Test]
    public void LaunchArgumentsPreserveSpacesAndShellCharacters()
    {
        Assert.That(ProcessMonitor.SplitArguments("-port=2302 \"-profiles=/a path/servers\" -mod=a;b&c"),
            Is.EqualTo(new[] { "-port=2302", "-profiles=/a path/servers", "-mod=a;b&c" }));
    }

    [Test]
    public void UnknownOutputIsExplicitlyUnavailable()
    {
        using var monitor = new ProcessMonitor(outputDirectory: _output);
        Assert.That(monitor.ReadOutput(new ProcessIdentity(0, DateTime.MinValue)), Does.Contain("unavailable"));
    }

    [Test]
    public void LinuxTemperatureIgnoresInvalidSensors()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sensor = Path.Combine(root, "hwmon", "hwmon0");
        Directory.CreateDirectory(sensor);
        try
        {
            File.WriteAllText(Path.Combine(sensor, "name"), "coretemp");
            File.WriteAllText(Path.Combine(sensor, "temp1_input"), "52000");
            File.WriteAllText(Path.Combine(sensor, "temp2_input"), "NaN");
            Assert.That(CpuTemperature.ReadLinux(Path.Combine(root, "hwmon"), Path.Combine(root, "thermal")), Is.EqualTo(52));
            File.WriteAllText(Path.Combine(sensor, "temp1_input"), "0");
            Assert.That(CpuTemperature.ReadLinux(Path.Combine(root, "hwmon"), Path.Combine(root, "thermal")), Is.Null);
        }
        finally { Directory.Delete(root, true); }
    }
}
