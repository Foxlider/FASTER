using System.Globalization;
using System.IO;

namespace FASTER.Services.SteamCmd;

/// <summary>
/// Promotes a SteamCMD Workshop download into FASTER's managed mod staging directory.
/// The existing staged mod is left untouched until the replacement has been copied in full.
/// </summary>
public sealed class WorkshopContentMirror
{
    private const int CopyBufferSize = 128 * 1024;

    public async Task<string> MirrorAsync(
        string sourceDirectory,
        string stagingDirectory,
        ulong workshopId,
        CancellationToken cancellationToken = default)
    {
        if (workshopId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workshopId), "A Workshop id must be greater than zero.");
        }

        string sourcePath = NormalizeDirectoryPath(sourceDirectory, nameof(sourceDirectory));
        string stagingPath = NormalizeDirectoryPath(stagingDirectory, nameof(stagingDirectory));

        if (!Directory.Exists(sourcePath))
        {
            throw new DirectoryNotFoundException($"The SteamCMD Workshop directory does not exist: {sourcePath}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.EnumerateFileSystemEntries(sourcePath).Any())
        {
            throw new InvalidDataException($"The SteamCMD Workshop directory is empty: {sourcePath}");
        }

        // An incoming directory is created below staging. If staging were inside the
        // source, recursively copying the source could discover its own output.
        if (IsSamePath(sourcePath, stagingPath) || IsStrictChildPath(stagingPath, sourcePath))
        {
            throw new InvalidOperationException("The staging directory cannot be the source directory or a child of it.");
        }

        string workshopDirectoryName = workshopId.ToString(CultureInfo.InvariantCulture);
        string targetPath = GetDirectChildPath(stagingPath, workshopDirectoryName);
        string operationId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        string incomingPath = GetDirectChildPath(stagingPath, $"{workshopDirectoryName}.incoming-{operationId}");
        string backupPath = GetDirectChildPath(stagingPath, $"{workshopDirectoryName}.backup-{operationId}");

        Directory.CreateDirectory(stagingPath);

        if (File.Exists(targetPath))
        {
            throw new IOException($"The managed mod target is a file, not a directory: {targetPath}");
        }

        Directory.CreateDirectory(incomingPath);

        try
        {
            await CopyDirectoryAsync(sourcePath, incomingPath, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            bool oldTargetMoved = false;
            try
            {
                if (Directory.Exists(targetPath))
                {
                    Directory.Move(targetPath, backupPath);
                    oldTargetMoved = true;
                }

                // Incoming and target are siblings, so this promotion is a same-volume
                // directory rename rather than another partial copy.
                Directory.Move(incomingPath, targetPath);
            }
            catch (Exception swapException)
            {
                if (oldTargetMoved && Directory.Exists(backupPath) &&
                    !Directory.Exists(targetPath) && !File.Exists(targetPath))
                {
                    try
                    {
                        Directory.Move(backupPath, targetPath);
                    }
                    catch (Exception rollbackException)
                    {
                        throw new IOException(
                            $"Could not promote Workshop item {workshopId}, and restoring the previous staged copy also failed. " +
                            $"The previous copy remains at '{backupPath}'.",
                            new AggregateException(swapException, rollbackException));
                    }
                }

                throw new IOException($"Could not promote Workshop item {workshopId} into the staging directory.", swapException);
            }

            // Promotion succeeded. A failed cleanup is harmless and the uniquely named
            // backup can be removed on a later maintenance pass.
            DeleteDirectChildBestEffort(stagingPath, backupPath);
            return targetPath;
        }
        catch
        {
            DeleteDirectChildBestEffort(stagingPath, incomingPath);
            throw;
        }
    }

    private static async Task CopyDirectoryAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        foreach (string sourceEntry in Directory.EnumerateFileSystemEntries(sourcePath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileAttributes attributes = File.GetAttributes(sourceEntry);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException($"Workshop content contains an unsupported reparse point: {sourceEntry}");
            }

            string destinationEntry = Path.Combine(destinationPath, Path.GetFileName(sourceEntry));
            if ((attributes & FileAttributes.Directory) != 0)
            {
                Directory.CreateDirectory(destinationEntry);
                await CopyDirectoryAsync(sourceEntry, destinationEntry, cancellationToken).ConfigureAwait(false);
                Directory.SetLastWriteTimeUtc(destinationEntry, Directory.GetLastWriteTimeUtc(sourceEntry));
                continue;
            }

            await using FileStream sourceStream = new(
                sourceEntry,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using FileStream destinationStream = new(
                destinationEntry,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await sourceStream.CopyToAsync(destinationStream, CopyBufferSize, cancellationToken).ConfigureAwait(false);
            await destinationStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            File.SetLastWriteTimeUtc(destinationEntry, File.GetLastWriteTimeUtc(sourceEntry));
        }
    }

    private static string NormalizeDirectoryPath(string path, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A directory path is required.", parameterName);
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    private static string GetDirectChildPath(string stagingPath, string childName)
    {
        if (string.IsNullOrWhiteSpace(childName) ||
            !string.Equals(childName, Path.GetFileName(childName), StringComparison.Ordinal) ||
            childName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException("A managed staging entry must have a single valid directory name.");
        }

        string childPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Combine(stagingPath, childName)));
        string? parentPath = Path.GetDirectoryName(childPath);
        if (parentPath is null || !IsSamePath(parentPath, stagingPath) ||
            !string.Equals(Path.GetFileName(childPath), childName, PathComparison))
        {
            throw new InvalidOperationException("A managed mod target must be a direct child of the staging directory.");
        }

        return childPath;
    }

    private static bool IsStrictChildPath(string candidatePath, string parentPath)
    {
        string relativePath = Path.GetRelativePath(parentPath, candidatePath);
        return !Path.IsPathRooted(relativePath) &&
               !string.Equals(relativePath, ".", StringComparison.Ordinal) &&
               !string.Equals(relativePath, "..", StringComparison.Ordinal) &&
               !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static bool IsSamePath(string left, string right) =>
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            PathComparison);

    private static void DeleteDirectChildBestEffort(string stagingPath, string candidatePath)
    {
        try
        {
            string childPath = GetDirectChildPath(stagingPath, Path.GetFileName(candidatePath));
            if (!IsSamePath(childPath, candidatePath) || !Directory.Exists(childPath))
            {
                return;
            }

            DeleteDirectoryTreeWithoutFollowingReparsePoints(childPath);
        }
        catch
        {
            // Cleanup must never hide the result of the copy, promotion, or rollback.
        }
    }

    private static void DeleteDirectoryTreeWithoutFollowingReparsePoints(string directoryPath)
    {
        DirectoryInfo directory = new(directoryPath);
        if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            directory.Delete();
            return;
        }

        foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos())
        {
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                entry.Delete();
            }
            else if (entry is DirectoryInfo childDirectory)
            {
                DeleteDirectoryTreeWithoutFollowingReparsePoints(childDirectory.FullName);
            }
            else
            {
                entry.Attributes = FileAttributes.Normal;
                entry.Delete();
            }
        }

        directory.Attributes = FileAttributes.Directory;
        directory.Delete();
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
