using System.Collections.ObjectModel;

namespace FASTER.Services.SteamCmd;

public enum SteamCmdServerBranch
{
    Public,
    Contact,
    CreatorDlc,
    Profiling
}

public enum SteamCmdGuardChallengeKind
{
    EmailCode,
    TwoFactorCode,
    MobileConfirmation,
    Unknown
}

public sealed record SteamCmdGuardChallenge(
    SteamCmdGuardChallengeKind Kind,
    string Prompt);

public enum SteamCmdProgressKind
{
    Installing,
    Starting,
    Authenticating,
    WaitingForGuard,
    DownloadingWorkshopItem,
    UpdatingServer,
    Completed,
    Warning,
    Error,
    Output
}

public sealed record SteamCmdProgress(
    SteamCmdProgressKind Kind,
    string Message,
    double? Percentage = null,
    ulong? WorkshopId = null);

public sealed record SteamCmdWorkshopItemResult(
    ulong WorkshopId,
    bool Success,
    string SourcePath,
    string? Error = null);

public sealed class SteamCmdWorkshopBatchResult
{
    public SteamCmdWorkshopBatchResult(
        IEnumerable<SteamCmdWorkshopItemResult> items,
        bool cancelled,
        int? exitCode,
        string? error = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = new ReadOnlyCollection<SteamCmdWorkshopItemResult>(items.ToList());
        Cancelled = cancelled;
        ExitCode = exitCode;
        Error = error;
    }

    public IReadOnlyList<SteamCmdWorkshopItemResult> Items { get; }

    public bool Cancelled { get; }

    public int? ExitCode { get; }

    public string? Error { get; }

    public bool Success => !Cancelled && Error == null && Items.Count > 0 && Items.All(item => item.Success);
}

public sealed record SteamCmdServerUpdateResult(
    bool Success,
    bool Cancelled,
    int? ExitCode,
    string? Error = null);

public class SteamCmdException : Exception
{
    public SteamCmdException(string message)
        : base(message)
    {
    }

    public SteamCmdException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class SteamCmdAuthenticationException : SteamCmdException
{
    public SteamCmdAuthenticationException(string message)
        : base(message)
    {
    }
}
