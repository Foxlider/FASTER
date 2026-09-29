using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FASTER.Services;

public interface ISystemMetrics
{
    float GetTotalCpuUsage();
    ulong GetTotalMemoryBytes();
    ulong GetAvailableMemoryBytes();
}

[SupportedOSPlatform("windows")]
public sealed class WindowsSystemMetrics : ISystemMetrics
{
    private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
    private readonly PerformanceCounter _ramCounter = new("Memory", "Available Bytes");

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

    public float GetTotalCpuUsage() => _cpuCounter.NextValue();

    public ulong GetTotalMemoryBytes()
    {
        GetPhysicallyInstalledSystemMemory(out long kilobytes);
        return (ulong)kilobytes * 1024;
    }

    public ulong GetAvailableMemoryBytes() => (ulong)_ramCounter.NextValue();
}

public sealed class LinuxSystemMetrics : ISystemMetrics
{
    private ulong _lastIdle;
    private ulong _lastTotal;

    public float GetTotalCpuUsage()
    {
        var parts = File.ReadAllText("/proc/stat").Split('\n')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        ulong idle = ulong.Parse(parts[4]);
        ulong total = 0;
        for (int i = 1; i < parts.Length; i++)
            total += ulong.Parse(parts[i]);

        ulong idleDelta = idle - _lastIdle;
        ulong totalDelta = total - _lastTotal;
        _lastIdle = idle;
        _lastTotal = total;

        if (totalDelta == 0) return 0;
        return (1 - idleDelta / (float)totalDelta) * 100;
    }

    public ulong GetTotalMemoryBytes() => ReadMemInfo("MemTotal:") * 1024;

    public ulong GetAvailableMemoryBytes() => ReadMemInfo("MemAvailable:") * 1024;

    private static ulong ReadMemInfo(string key)
    {
        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (!line.StartsWith(key, StringComparison.Ordinal)) continue;
            var digits = new string(line.Where(char.IsDigit).ToArray());
            if (ulong.TryParse(digits, out ulong kilobytes))
                return kilobytes;
        }
        return 0;
    }
}
