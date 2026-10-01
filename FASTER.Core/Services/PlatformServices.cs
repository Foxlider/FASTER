using System.Diagnostics;

namespace FASTER.Services;

public interface IPlatformServices
{
    string ServerBinaryName { get; }
    string ServerExecutableExtensionFilter { get; }
    string SteamCmdBinaryName { get; }
    bool IsServerExecutable(string? path);
    void OpenFolder(string path);
    void OpenFile(string path);
    void OpenUrl(string url);
    void PrepareServerExecutables(string directory);
}

public static class Platform
{
    public static IPlatformServices Current { get; set; } = DefaultPlatformServices.Instance;
}

public sealed class DefaultPlatformServices : IPlatformServices
{
    public static readonly DefaultPlatformServices Instance = new();

    private static readonly bool s_isWindows = OperatingSystem.IsWindows();

    public string ServerBinaryName => s_isWindows ? "arma3server_x64.exe" : "arma3server";

    public string ServerExecutableExtensionFilter => s_isWindows ? ".exe" : string.Empty;

    public string SteamCmdBinaryName => s_isWindows ? "steamcmd.exe" : "steamcmd.sh";

    public bool IsServerExecutable(string? path)
    {
        var name = Path.GetFileName(path);
        if (string.IsNullOrEmpty(name) || !name.Contains("arma3server", StringComparison.OrdinalIgnoreCase))
            return false;
        return !s_isWindows || name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
    }

    public void OpenFolder(string path) => OpenFile(path);

    public void OpenFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Process.Start(CreateOpenStartInfo(path));
    }

    public void OpenUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new ArgumentException("Only HTTP and HTTPS links are supported.", nameof(url));
        OpenFile(url);
    }

    internal static ProcessStartInfo CreateOpenStartInfo(string target)
    {
        if (OperatingSystem.IsWindows())
            return new ProcessStartInfo(target) { UseShellExecute = true };
        var start = new ProcessStartInfo(OperatingSystem.IsMacOS() ? "/usr/bin/open" : "/usr/bin/xdg-open");
        start.ArgumentList.Add(target);
        return start;
    }

    public static uint ServerDepot(bool windows, bool profiling) =>
        (windows, profiling) switch { (true, false) => 233782, (false, false) => 233783,
            (true, true) => 233784, (false, true) => 233785 };

    public void PrepareServerExecutables(string directory)
    {
        if (!OperatingSystem.IsLinux() || !Directory.Exists(directory)) return;
        foreach (var path in Directory.EnumerateFiles(directory, "arma3server*"))
        {
            if (Path.GetExtension(path).Length != 0) continue;
            File.SetUnixFileMode(path, File.GetUnixFileMode(path) | UnixFileMode.UserExecute |
                UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }
    }
}
