using System.Globalization;
using System.Management;
using System.Runtime.Versioning;

namespace FASTER.Services;

public static class CpuTemperature
{
    public static double? Read()
    {
        try
        {
            if (OperatingSystem.IsWindows()) return ReadWindows();
            if (OperatingSystem.IsLinux()) return ReadLinux("/sys/class/hwmon", "/sys/class/thermal");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ManagementException or System.Runtime.InteropServices.COMException) { }
        return null;
    }

    internal static double? ReadLinux(string hwmon, string thermal)
    {
        var readings = new List<double>();
        foreach (var root in new[] { hwmon, thermal })
        {
            if (!Directory.Exists(root)) continue;
            foreach (var directory in Directory.EnumerateDirectories(root))
            {
                try
                {
                    var namePath = Path.Combine(directory, root == hwmon ? "name" : "type");
                    if (!File.Exists(namePath)) continue;
                    var name = File.ReadAllText(namePath).Trim();
                    if (!new[] { "coretemp", "k10temp", "zenpower", "cpu_thermal", "x86_pkg_temp", "cpu-thermal" }.Contains(name)) continue;
                    foreach (var file in Directory.EnumerateFiles(directory, root == hwmon ? "temp*_input" : "temp"))
                    {
                        if (double.TryParse(File.ReadAllText(file).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && Valid(value / 1000))
                            readings.Add(value / 1000);
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        }
        return readings.Count > 0 ? readings.Max() : null;
    }

    private static bool Valid(double value) => double.IsFinite(value) && value > 0 && value < 150;

    [SupportedOSPlatform("windows")]
    private static double? ReadWindows()
    {
        using var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
        using var results = searcher.Get();
        foreach (ManagementObject result in results)
        {
            using (result)
            {
                double value = Convert.ToDouble(result["CurrentTemperature"], CultureInfo.InvariantCulture) / 10 - 273.15;
                if (Valid(value)) return value;
            }
        }
        return null;
    }
}
