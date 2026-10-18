using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FASTER.Models;

public static class ModDeployment
{
    private const string Marker = "FASTER lowercase mod view v1";
    private static readonly object s_sync = new();

    public static string FolderName(string name, uint id)
    {
        if (name.Any(c => c > 127) || !name.Any(char.IsAsciiLetterOrDigit))
            return "@mod_" + Key(name, id);
        return "@" + Functions.SafeName(name);
    }

    private static string Key(string name, uint id) => id != 0
        ? id.ToString(CultureInfo.InvariantCulture)
        : Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..16];

    private static string ViewPath(string installPath, string name, uint id)
        => Path.Combine(installPath, ".faster-linux-mods", Key(name, id));

    // Arma's Linux server needs lowercase paths inside mods. Link to the original
    // files through a separate view so Steam verification and local mods stay intact.
    public static string PrepareSource(string source, string installPath, string name, uint id)
    {
        source = Path.GetFullPath(source);
        if (!OperatingSystem.IsLinux()) return source;
        lock (s_sync)
        {
            var view = Path.GetFullPath(ViewPath(installPath, name, id));
            if (view.StartsWith(source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new IOException("The deployment directory must be outside the mod's source folder.");

            var entries = new List<(string Source, string Relative, bool Directory)>();
            var directories = new Stack<string>();
            directories.Push(source);
            var hasDirectoryLinks = false;
            while (directories.TryPop(out var directory))
            {
                foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
                {
                    var isDirectory = entry.Attributes.HasFlag(FileAttributes.Directory);
                    entries.Add((entry.FullName, Path.GetRelativePath(source, entry.FullName), isDirectory));
                    if (!isDirectory) continue;
                    if (entry.LinkTarget != null) hasDirectoryLinks = true;
                    else directories.Push(entry.FullName);
                }
            }

            if (!entries.Any(e => e.Relative != e.Relative.ToLowerInvariant()) && !Directory.Exists(view))
                return source;
            if (hasDirectoryLinks)
                throw new IOException("Cannot create a lowercase mod view containing nested directory symlinks.");

            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
                if (!paths.Add(entry.Relative.ToLowerInvariant()))
                    throw new IOException($"Mod paths differ only by case: {entry.Relative}. They cannot both be deployed on Linux.");

            var marker = view + ".managed";
            if (Directory.Exists(view) && (new DirectoryInfo(view).LinkTarget != null ||
                !File.Exists(marker) || File.ReadAllText(marker) != Marker))
                throw new IOException($"Refusing to replace an unmanaged directory: {view}");

            Directory.CreateDirectory(Path.GetDirectoryName(view)!);
            var staging = view + ".new-" + Guid.NewGuid().ToString("N");
            var backup = view + ".old-" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(staging);
            try
            {
                foreach (var entry in entries)
                {
                    var target = Path.Combine(staging, entry.Relative.ToLowerInvariant());
                    if (entry.Directory) Directory.CreateDirectory(target);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                        File.CreateSymbolicLink(target, entry.Source);
                    }
                }

                if (Directory.Exists(view)) Directory.Move(view, backup);
                try { Directory.Move(staging, view); }
                catch
                {
                    if (Directory.Exists(backup)) Directory.Move(backup, view);
                    throw;
                }
                File.WriteAllText(marker, Marker);
            }
            finally
            {
                if (Directory.Exists(staging)) Directory.Delete(staging, true);
                if (Directory.Exists(backup)) Directory.Delete(backup, true);
            }
            return view;
        }
    }

    public static void RemoveLegacyLink(string installPath, string source, string name, uint id)
    {
        var legacy = Path.Combine(installPath, "@" + Functions.SafeName(name));
        if (Path.GetFileName(legacy) == FolderName(name, id)) return;
        var link = new DirectoryInfo(legacy);
        var sourceTarget = Directory.ResolveLinkTarget(source, true)?.FullName ?? Path.GetFullPath(source);
        if (link.LinkTarget != null && link.ResolveLinkTarget(true)?.FullName == sourceTarget)
            Directory.Delete(legacy);
    }

    public static void Link(string source, string installPath, string name, uint id)
    {
        lock (s_sync)
        {
            var path = Path.Combine(installPath, FolderName(name, id));
            var link = new DirectoryInfo(path);
            if (Directory.Exists(path) && link.LinkTarget == null)
                throw new IOException($"Refusing to replace a real directory: {path}");
            var target = PrepareSource(source, installPath, name, id);
            if (link.LinkTarget != null) Directory.Delete(path);
            Directory.CreateSymbolicLink(path, target);
            try { RemoveLegacyLink(installPath, source, name, id); }
            catch (IOException ex) { Logger.Log($"Could not remove the old deployment link: {ex.Message}"); }
            catch (UnauthorizedAccessException ex) { Logger.Log($"Could not remove the old deployment link: {ex.Message}"); }
        }
    }

    public static void RefreshDeployed(ArmaMod mod)
    {
        lock (s_sync)
        {
            var deployment = AppSettings.Current.Deployments;
            if (!OperatingSystem.IsLinux() || deployment == null ||
                !deployment.DeployMods.Any(m => m.WorkshopId == mod.WorkshopId && m.Marked)) return;
            var linkPath = Path.Combine(deployment.InstallPath, FolderName(mod.Name, mod.WorkshopId));
            var link = new DirectoryInfo(linkPath);
            var target = link.LinkTarget == null ? null : link.ResolveLinkTarget(true)?.FullName;
            if (target != Path.GetFullPath(mod.Path) && target != Path.GetFullPath(ViewPath(deployment.InstallPath, mod.Name, mod.WorkshopId))) return;
            var source = PrepareSource(mod.Path, deployment.InstallPath, mod.Name, mod.WorkshopId);
            if (target == source) return;
            Directory.Delete(linkPath);
            Directory.CreateSymbolicLink(linkPath, source);
        }
    }
}
