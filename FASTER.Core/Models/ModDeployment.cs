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
        var folder = "@" + Functions.SafeName(name);
        return OperatingSystem.IsLinux() ? folder.ToLowerInvariant() : folder;
    }

    public static void ValidateFolderNames(IEnumerable<(string Name, uint Id)> mods)
    {
        if (!OperatingSystem.IsLinux()) return;
        var collision = mods.GroupBy(m => FolderName(m.Name, m.Id), StringComparer.Ordinal)
            .FirstOrDefault(group => group.Select(m => m.Id).Distinct().Skip(1).Any());
        if (collision != null)
            throw new IOException($"Multiple mods use the Linux folder '{collision.Key}'. Give these mods distinct names before deploying or launching them.");
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
        source = Directory.ResolveLinkTarget(source, true)?.FullName ?? source;
        lock (s_sync)
        {
            var view = Path.GetFullPath(ViewPath(installPath, name, id));
            if (view == source ||
                view.StartsWith(source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                source.StartsWith(view.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
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
        var legacyTarget = link.LinkTarget == null ? null : link.ResolveLinkTarget(true)?.FullName;
        var view = Path.GetFullPath(ViewPath(installPath, name, id));
        if (link.LinkTarget != null && (legacyTarget == sourceTarget ||
            (legacyTarget == view && File.Exists(view + ".managed") && File.ReadAllText(view + ".managed") == Marker)))
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

    // Profiles may still point at links created by an older version, or the source
    // may have been changed by Steam or copied manually since the last deployment.
    public static void PrepareForLaunch(string installPath, string name, uint id, string? registeredSource)
    {
        if (!OperatingSystem.IsLinux()) return;
        lock (s_sync)
        {
            string path = Path.Combine(installPath, FolderName(name, id));
            string legacy = Path.Combine(installPath, "@" + Functions.SafeName(name));
            string? source = registeredSource;
            if (string.IsNullOrWhiteSpace(source))
            {
                var existing = Directory.Exists(path) ? path : legacy;
                if (!Directory.Exists(existing)) throw new IOException($"Mod '{name}' is not deployed. Download and deploy it first.");
                source = Directory.ResolveLinkTarget(existing, true)?.FullName ?? Path.GetFullPath(existing);
                if (source == Path.GetFullPath(ViewPath(installPath, name, id)))
                    throw new IOException($"Cannot refresh '{name}' without its source folder. Add the mod in Mods and deploy it again.");
            }
            if (!Directory.Exists(source)) throw new IOException($"The source folder for '{name}' does not exist. Download or add the mod again.");
            // A manually installed, already-lowercase folder can be used directly.
            if (Directory.Exists(path) && new DirectoryInfo(path).LinkTarget == null)
            {
                if (PrepareSource(path, installPath, name, id) == Path.GetFullPath(path)) return;
                throw new IOException($"Mod '{name}' contains uppercase paths in a real deployment folder. Move it outside the server directory, add it as a local mod, and deploy it again.");
            }
            Link(source, installPath, name, id);
        }
    }

    public static void RefreshDeployed(ArmaMod mod)
    {
        lock (s_sync)
        {
            var deployment = AppSettings.Current.Deployments;
            if (!OperatingSystem.IsLinux() || deployment == null ||
                !deployment.DeployMods.Any(m => m.WorkshopId == mod.WorkshopId && m.Marked)) return;
            string source = Directory.ResolveLinkTarget(mod.Path, true)?.FullName ?? Path.GetFullPath(mod.Path);
            string view = Path.GetFullPath(ViewPath(deployment.InstallPath, mod.Name, mod.WorkshopId));
            string[] candidates = [Path.Combine(deployment.InstallPath, FolderName(mod.Name, mod.WorkshopId)),
                Path.Combine(deployment.InstallPath, "@" + Functions.SafeName(mod.Name))];
            if (!candidates.Any(path => new DirectoryInfo(path).LinkTarget != null &&
                (Directory.ResolveLinkTarget(path, true)?.FullName is { } target && (target == source || target == view)))) return;
            PrepareForLaunch(deployment.InstallPath, mod.Name, mod.WorkshopId, mod.Path);
        }
    }
}
