using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace FASTER.Services.SteamCmd;

internal sealed class SteamCmdInstaller
{
    private const long MaximumArchiveBytes = 64L * 1024 * 1024;
    private const long MaximumExpandedBytes = 512L * 1024 * 1024;
    private static readonly Uri DownloadUri = new("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
    private static readonly HttpClient HttpClient = new();

    private readonly string _rootDirectory;

    public SteamCmdInstaller(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public string ExecutablePath => Path.Combine(_rootDirectory, "steamcmd.exe");

    private string ReadinessMarkerPath => Path.Combine(_rootDirectory, ".faster-steamcmd-ready");

    public bool IsInstalled => File.Exists(ExecutablePath) && File.Exists(ReadinessMarkerPath);

    public async Task EnsureInstalledAsync(
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (IsInstalled)
            return;

        // Accept an existing user-supplied or interrupted install, but do not
        // call it ready until SteamCMD has successfully completed self-update.
        if (File.Exists(ExecutablePath))
        {
            await BootstrapAsync(progress, cancellationToken).ConfigureAwait(false);
            await WriteReadinessMarkerAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"faster-steamcmd-{Guid.NewGuid():N}");
        string archivePath = Path.Combine(temporaryDirectory, "steamcmd.zip");
        string extractionPath = Path.Combine(temporaryDirectory, "extracted");

        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Installing,
                "Downloading SteamCMD from Valve...",
                0));

            await DownloadArchiveAsync(archivePath, progress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Installing,
                "Validating and extracting SteamCMD..."));

            ExtractArchiveSafely(archivePath, extractionPath, cancellationToken);
            string extractedExecutable = Path.Combine(extractionPath, "steamcmd.exe");
            if (!File.Exists(extractedExecutable))
                throw new SteamCmdException("Valve's SteamCMD archive did not contain steamcmd.exe.");

            Directory.CreateDirectory(_rootDirectory);
            CopyDirectory(extractionPath, _rootDirectory, cancellationToken);

            if (!File.Exists(ExecutablePath))
                throw new SteamCmdException("SteamCMD could not be installed in the selected directory.");

            await BootstrapAsync(progress, cancellationToken).ConfigureAwait(false);
            await WriteReadinessMarkerAsync(cancellationToken).ConfigureAwait(false);
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Completed,
                "SteamCMD is installed and up to date.",
                100));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SteamCmdException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new SteamCmdException("SteamCMD installation failed.", exception);
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectory);
        }
    }

    private async Task WriteReadinessMarkerAsync(CancellationToken cancellationToken)
    {
        string temporaryMarker = ReadinessMarkerPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllTextAsync(
                temporaryMarker,
                "SteamCMD completed bootstrap successfully.",
                cancellationToken).ConfigureAwait(false);
            File.Move(temporaryMarker, ReadinessMarkerPath, true);
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryMarker))
                    File.Delete(temporaryMarker);
            }
            catch (IOException)
            {
            }
        }
    }

    private static async Task DownloadArchiveAsync(
        string destinationPath,
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(
            DownloadUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        long? contentLength = response.Content.Headers.ContentLength;
        if (contentLength > MaximumArchiveBytes)
            throw new SteamCmdException("The SteamCMD archive is unexpectedly large.");

        await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using FileStream destination = new(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        byte[] buffer = new byte[81920];
        long downloaded = 0;
        while (true)
        {
            int read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;

            downloaded += read;
            if (downloaded > MaximumArchiveBytes)
                throw new SteamCmdException("The SteamCMD archive exceeded the safe download limit.");

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            double? percentage = contentLength is > 0
                ? Math.Min(100, downloaded * 100d / contentLength.Value)
                : null;
            progress?.Report(new SteamCmdProgress(
                SteamCmdProgressKind.Installing,
                "Downloading SteamCMD from Valve...",
                percentage));
        }
    }

    private static void ExtractArchiveSafely(
        string archivePath,
        string destinationDirectory,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destinationDirectory);
        string destinationRoot = Path.GetFullPath(destinationDirectory) + Path.DirectorySeparatorChar;
        long expandedBytes = 0;

        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            expandedBytes = checked(expandedBytes + entry.Length);
            if (expandedBytes > MaximumExpandedBytes)
                throw new SteamCmdException("The SteamCMD archive exceeded the safe extraction limit.");

            string targetPath = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));
            if (!targetPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                throw new SteamCmdException("The SteamCMD archive contains an unsafe path.");

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(targetPath);
                continue;
            }

            string? targetParent = Path.GetDirectoryName(targetPath);
            if (targetParent != null)
                Directory.CreateDirectory(targetParent);

            using Stream source = entry.Open();
            using FileStream destination = new(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            source.CopyTo(destination);
        }
    }

    private static void CopyDirectory(
        string sourceDirectory,
        string destinationDirectory,
        CancellationToken cancellationToken)
    {
        foreach (string directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (string file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string relativePath = Path.GetRelativePath(sourceDirectory, file);
            string destination = Path.Combine(destinationDirectory, relativePath);
            string? parent = Path.GetDirectoryName(destination);
            if (parent != null)
                Directory.CreateDirectory(parent);
            File.Copy(file, destination, true);
        }
    }

    private async Task BootstrapAsync(
        IProgress<SteamCmdProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new SteamCmdProgress(
            SteamCmdProgressKind.Installing,
            "SteamCMD is applying its self-update..."));

        // SteamCMD initializes as an interactive console application even when
        // it only needs to self-update. Ordinary redirected pipes can hang (or
        // crash recent builds), so bootstrap it through the same ConPTY-backed
        // session used for downloads.
        await using SteamCmdSession session = SteamCmdSession.Start(
            ExecutablePath,
            _rootDirectory,
            [],
            progress);

        while (true)
        {
            SteamCmdOutputEvent outputEvent = await session.NextEventAsync(
                TimeSpan.FromMinutes(5),
                cancellationToken).ConfigureAwait(false);
            if (outputEvent.Kind == SteamCmdOutputEventKind.Prompt)
                break;
            if (outputEvent.Kind is SteamCmdOutputEventKind.Error or SteamCmdOutputEventKind.Timeout)
                throw new SteamCmdException(outputEvent.Text);
        }

        int? exitCode = await session.QuitAndWaitAsync(cancellationToken).ConfigureAwait(false);
        if (exitCode is not 0)
            throw new SteamCmdException($"SteamCMD self-update exited with code {exitCode}.");
    }

    private static void DeleteTemporaryDirectory(string temporaryDirectory)
    {
        try
        {
            string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
            string target = Path.GetFullPath(temporaryDirectory);
            if (target.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(target))
                Directory.Delete(target, true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
