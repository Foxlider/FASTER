using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace FASTER.Services.SteamCmd;

public static partial class SteamCmdCommandBuilder
{
    public const uint WorkshopAppId = 107410;
    public const uint ServerAppId = 233780;

    public static string BuildLoginCommand(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "login anonymous";

        string accountName = username.Trim();
        if (!AccountNamePattern().IsMatch(accountName))
        {
            throw new ArgumentException(
                "The Steam account name may contain only letters, numbers, underscores, and hyphens.",
                nameof(username));
        }

        return $"login {accountName}";
    }

    public static string BuildWorkshopDownloadCommand(ulong workshopId)
    {
        if (workshopId == 0)
            throw new ArgumentOutOfRangeException(nameof(workshopId), "A Workshop ID must be greater than zero.");

        return string.Create(
            CultureInfo.InvariantCulture,
            $"workshop_download_item {WorkshopAppId} {workshopId} validate");
    }

    public static string BuildForceInstallDirectoryCommand(string installDirectory)
    {
        string normalizedPath = ValidateAndNormalizePath(installDirectory, nameof(installDirectory));
        return $"force_install_dir \"{normalizedPath}\"";
    }

    public static string BuildServerUpdateCommand(SteamCmdServerBranch branch)
    {
        string branchName = branch switch
        {
            SteamCmdServerBranch.Public => "public",
            SteamCmdServerBranch.Contact => "contact",
            SteamCmdServerBranch.CreatorDlc => "creatordlc",
            SteamCmdServerBranch.Profiling => "profiling",
            _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, "Unsupported Steam server branch.")
        };

        return $"app_update {ServerAppId} -beta {branchName} validate";
    }

    public static string BuildQuitCommand() => "quit";

    public static string ValidateAndNormalizePath(string path, string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A directory path is required.", parameterName ?? nameof(path));

        if (path.IndexOfAny(['\0', '\r', '\n', '\"']) >= 0)
            throw new ArgumentException("The directory path contains characters SteamCMD cannot safely accept.", parameterName ?? nameof(path));

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path.Trim());
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException("The directory path is invalid.", parameterName ?? nameof(path), exception);
        }

        return Path.TrimEndingDirectorySeparator(fullPath);
    }

    [GeneratedRegex("^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AccountNamePattern();
}
