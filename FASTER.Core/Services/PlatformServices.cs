using System.Diagnostics;

namespace FASTER.Services;

public interface IPlatformServices
{
    string ServerBinaryName { get; }
    string ServerExecutableExtensionFilter { get; }
    string SteamCmdBinaryName { get; }
    bool IsServerExecutable(string? path);
    void OpenFolder(string path);
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

    public void OpenFolder(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (s_isWindows)
        {
            // Resolve explorer outside of PATH so a planted binary earlier on PATH cannot get picked up instead.
            string explorer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            Process.Start(new ProcessStartInfo { FileName = explorer, Arguments = path });
            return;
        }
        if (OperatingSystem.IsMacOS())
        {
            Process.Start("/usr/bin/open", path);
            return;
        }
        // xdg-utils installs here on every mainstream desktop distro.
        Process.Start("/usr/bin/xdg-open", path);
    }
}
