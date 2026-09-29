using System.Runtime.InteropServices;

namespace FASTER.Services;

public static class AnalyticsInfo
{
    public static Dictionary<string, string> GetEnvironmentProperties()
    {
        var props = new Dictionary<string, string>
        {
            { "OS", RuntimeInformation.OSDescription.Trim() }
        };

        if (OperatingSystem.IsLinux())
        {
            props["Session Type"] = GetLinuxSessionType();
            props["Desktop"] = GetEnvFirst("XDG_CURRENT_DESKTOP", "DESKTOP_SESSION");
            props["Kernel"] = GetLinuxKernel();
            props["Distro"] = GetLinuxDistro();
        }

        return props;
    }

    private static string GetLinuxSessionType()
    {
        var session = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        if (!string.IsNullOrWhiteSpace(session))
            return session.Trim();
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
            return "wayland";
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
            return "x11";
        return "unknown";
    }

    private static string GetEnvFirst(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return "unknown";
    }

    private static string GetLinuxKernel()
    {
        try
        {
            var release = File.ReadLines("/proc/sys/kernel/osrelease").FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(release))
                return release;
        }
        catch
        {
            // Procfs may be unavailable in containers, fall through to the runtime value.
        }
        return Environment.OSVersion.VersionString;
    }

    private static string GetLinuxDistro()
    {
        try
        {
            foreach (var line in File.ReadLines("/etc/os-release"))
            {
                if (!line.StartsWith("PRETTY_NAME=", StringComparison.Ordinal))
                    continue;
                var name = line["PRETTY_NAME=".Length..].Trim().Trim('"');
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
        }
        catch
        {
            // Missing or unreadable os-release, fall through to the runtime description.
        }
        return RuntimeInformation.OSDescription.Trim();
    }
}
