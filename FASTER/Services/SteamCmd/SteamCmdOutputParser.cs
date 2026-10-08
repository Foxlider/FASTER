using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FASTER.Services.SteamCmd;

/// <summary>
/// Identifies a logical event found in SteamCMD console output.
/// </summary>
public enum SteamCmdOutputEventKind
{
    Output,
    Prompt,
    PasswordPrompt,
    SteamGuardPrompt,
    MobileConfirmationPrompt,
    LoggedIn,
    LoginFailed,
    WorkshopDownloadRequested,
    WorkshopDownloadSucceeded,
    WorkshopDownloadFailed,
    Progress,
    AppUpdateSucceeded,
    Error,
    Timeout
}

/// <summary>
/// Describes the authentication action requested by a Steam Guard prompt.
/// </summary>
public enum SteamCmdGuardPromptKind
{
    Unknown,
    EmailCode,
    AuthenticatorCode,
    MobileApproval
}

/// <summary>
/// A sanitized event produced from SteamCMD's console stream.
/// </summary>
/// <param name="Kind">The kind of output that was recognized.</param>
/// <param name="Text">Sanitized console text or a canonical prompt description.</param>
/// <param name="WorkshopId">The workshop item associated with the event, when known.</param>
/// <param name="ProgressPercent">The reported percentage, without rounding, when present.</param>
/// <param name="AppId">The Steam application associated with the event, when present.</param>
/// <param name="GuardPromptKind">The kind of Steam Guard interaction, when present.</param>
public sealed record SteamCmdOutputEvent(
    SteamCmdOutputEventKind Kind,
    string Text,
    ulong? WorkshopId = null,
    double? ProgressPercent = null,
    uint? AppId = null,
    SteamCmdGuardPromptKind? GuardPromptKind = null);

/// <summary>
/// Incrementally converts the character stream written by SteamCMD into logical events.
/// </summary>
/// <remarks>
/// The parser accepts arbitrary chunk boundaries. Console records are delimited by CR, LF,
/// or CRLF; interactive prompts are recognized before a delimiter is received. Supplied
/// secrets are replaced in every event's text and are never returned as raw output.
/// </remarks>
public sealed class SteamCmdOutputParser
{
    private const string RedactionMarker = "[REDACTED]";

    private static readonly Regex SteamPromptRegex = new(
        @"Steam>\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex PasswordPromptRegex = new(
        @"(?:(?:^|\s)(?:password|passphrase)\s*:|please\s+enter(?:\s+your)?\s+password\s*:?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WorkshopSuccessRegex = new(
        @"\bSuccess[.!]?\s+Downloaded\s+item\s+(?<id>\d+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WorkshopFailureRegex = new(
        @"\bDownload(?:ing|ed)?\s+item\s+(?<id>\d+)\s+failed\b(?:\s*\((?<reason>[^)]*)\))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WorkshopTimeoutRegex = new(
        @"\b(?:timeout|timed\s+out)\b.*?\bdownload(?:ing)?\s+(?:workshop\s+)?item\s+(?<id>\d+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WorkshopDownloadingRegex = new(
        @"\bDownloading\s+(?:workshop\s+)?item\s+(?<id>\d+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WorkshopCommandRegex = new(
        @"\bworkshop_download_item\s+(?<appId>\d+)\s+(?<id>\d+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ProgressRegex = new(
        @"\bprogress\s*:\s*(?<progress>\d+(?:[.,]\d+)?)\s*%?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex AppSuccessRegex = new(
        @"\bSuccess!\s+App\s+['""]?(?<appId>\d+)['""]?\s+fully\s+installed\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex LoggedInRegex = new(
        @"\b(?:Logged\s+in\s+OK|Waiting\s+for\s+user\s+info\.{0,3}\s*OK)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex LoginFailedRegex = new(
        @"(?:\b(?:login|logon|logging\s+in)\b.*\bfailed\b|\bwaiting\s+for\s+user\s+info\b.*\bfailed\b|\bFAILED\s*\((?:Invalid\s+Password|Account\s+Logon\s+Denied|Invalid\s+Login\s+Auth\s+Code|Two[- ]Factor\s+Code\s+Mismatch|Rate\s+Limit\s+Exceeded)\)|\binvalid\s+(?:password|steam\s+guard\s+code|authentication\s+code)\b|\bunable\s+to\s+log\s+(?:in|on)\b)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ErrorRegex = new(
        @"(?:^|\s)(?:ERROR!|ERROR\s*:|\[ERROR\])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex TimeoutRegex = new(
        @"\b(?:timeout|timed\s+out)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly object syncRoot = new();
    private readonly StringBuilder recordBuffer = new();
    private readonly List<string> secrets = [];

    private AnsiState ansiState;
    private bool skipLineFeed;
    private bool completed;
    private bool steamPromptEmitted;
    private bool passwordPromptEmitted;
    private bool guardPromptEmitted;
    private bool steamPromptAwaitingActivity;
    private bool passwordPromptAwaitingActivity;
    private string? guardPromptAwaitingActivityText;
    private ulong? currentWorkshopId;

    public SteamCmdOutputParser(IEnumerable<string>? secretsToRedact = null)
    {
        if (secretsToRedact is null)
        {
            return;
        }

        foreach (string secret in secretsToRedact)
        {
            AddSecret(secret);
        }
    }

    /// <summary>
    /// Adds a value that must be removed from all subsequently produced event text.
    /// </summary>
    public void AddSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            return;
        }

        lock (syncRoot)
        {
            if (secrets.Contains(secret, StringComparer.Ordinal))
            {
                return;
            }

            secrets.Add(secret);
            secrets.Sort(static (left, right) => right.Length.CompareTo(left.Length));
        }
    }

    /// <summary>
    /// Parses the next arbitrary chunk of SteamCMD console characters.
    /// </summary>
    public IReadOnlyList<SteamCmdOutputEvent> Feed(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        lock (syncRoot)
        {
            ObjectDisposedException.ThrowIf(completed, this);

            if (chunk.Length == 0)
            {
                return Array.Empty<SteamCmdOutputEvent>();
            }

            List<SteamCmdOutputEvent> events = [];
            foreach (char character in chunk)
            {
                ProcessCharacter(character, events);
            }

            return events;
        }
    }

    /// <summary>
    /// Flushes the final unterminated console record. This method is idempotent.
    /// </summary>
    public IReadOnlyList<SteamCmdOutputEvent> Complete()
    {
        lock (syncRoot)
        {
            if (completed)
            {
                return Array.Empty<SteamCmdOutputEvent>();
            }

            completed = true;
            List<SteamCmdOutputEvent> events = [];
            CompleteRecord(events);
            return events;
        }
    }

    private void ProcessCharacter(char character, List<SteamCmdOutputEvent> events)
    {
        switch (ansiState)
        {
            case AnsiState.None:
                if (character == '\u001b')
                {
                    ansiState = AnsiState.Escape;
                }
                else if (character == '\u009b')
                {
                    ansiState = AnsiState.ControlSequence;
                }
                else if (character == '\u009d')
                {
                    ansiState = AnsiState.OperatingSystemCommand;
                }
                else
                {
                    ProcessVisibleCharacter(character, events);
                }

                break;

            case AnsiState.Escape:
                if (character == '[')
                {
                    ansiState = AnsiState.ControlSequence;
                }
                else if (character is ']' or 'P' or 'X' or '^' or '_')
                {
                    ansiState = AnsiState.OperatingSystemCommand;
                }
                else if (character is >= '\u0020' and <= '\u002f')
                {
                    ansiState = AnsiState.EscapeIntermediate;
                }
                else if (character == '\u001b')
                {
                    ansiState = AnsiState.Escape;
                }
                else
                {
                    ansiState = AnsiState.None;
                }

                break;

            case AnsiState.EscapeIntermediate:
                if (character is >= '\u0030' and <= '\u007e')
                {
                    ansiState = AnsiState.None;
                }
                else if (character == '\u001b')
                {
                    ansiState = AnsiState.Escape;
                }

                break;

            case AnsiState.ControlSequence:
                if (character is >= '\u0040' and <= '\u007e')
                {
                    ansiState = AnsiState.None;
                }
                else if (character == '\u001b')
                {
                    ansiState = AnsiState.Escape;
                }

                break;

            case AnsiState.OperatingSystemCommand:
                if (character == '\u0007')
                {
                    ansiState = AnsiState.None;
                }
                else if (character == '\u001b')
                {
                    ansiState = AnsiState.OperatingSystemCommandEscape;
                }

                break;

            case AnsiState.OperatingSystemCommandEscape:
                if (character == '\\')
                {
                    ansiState = AnsiState.None;
                }
                else if (character != '\u001b')
                {
                    ansiState = AnsiState.OperatingSystemCommand;
                }

                break;
        }
    }

    private void ProcessVisibleCharacter(char character, List<SteamCmdOutputEvent> events)
    {
        if (character == '\r')
        {
            CompleteRecord(events);
            skipLineFeed = true;
            return;
        }

        if (character == '\n')
        {
            if (skipLineFeed)
            {
                skipLineFeed = false;
            }
            else
            {
                CompleteRecord(events);
            }

            return;
        }

        skipLineFeed = false;

        if (character == '\0')
        {
            return;
        }

        if (character == '\b')
        {
            if (recordBuffer.Length > 0)
            {
                recordBuffer.Length--;
            }

            return;
        }

        recordBuffer.Append(character);
        DetectInteractivePrompt(events);
    }

    private void DetectInteractivePrompt(List<SteamCmdOutputEvent> events)
    {
        string currentRecord = recordBuffer.ToString();
        Match steamPromptMatch = SteamPromptRegex.Match(currentRecord);

        if (!steamPromptEmitted && !steamPromptAwaitingActivity && steamPromptMatch.Success)
        {
            if (steamPromptMatch.Index > 0)
            {
                // ConPTY frequently paints the next prompt directly after the
                // command result without CR/LF (for example
                // "Waiting for user info...OKSteam>"). Publish that result
                // first so command completion cannot overtake its success or
                // failure event in the channel.
                string precedingOutput = currentRecord[..steamPromptMatch.Index];
                if (!string.IsNullOrWhiteSpace(precedingOutput))
                {
                    ProcessRecord(precedingOutput, events);
                    passwordPromptAwaitingActivity = false;
                    guardPromptAwaitingActivityText = null;
                }

                recordBuffer.Clear();
                recordBuffer.Append("Steam>");
                currentRecord = "Steam>";
            }

            steamPromptEmitted = true;
            steamPromptAwaitingActivity = true;
            events.Add(CreateEvent(SteamCmdOutputEventKind.Prompt, "Steam>"));
        }

        if (!passwordPromptEmitted && !passwordPromptAwaitingActivity && PasswordPromptRegex.IsMatch(currentRecord))
        {
            passwordPromptEmitted = true;
            passwordPromptAwaitingActivity = true;
            events.Add(CreateEvent(SteamCmdOutputEventKind.PasswordPrompt, "Password prompt"));
        }

        if (guardPromptEmitted ||
            !TryGetGuardPromptKind(currentRecord, out SteamCmdGuardPromptKind guardKind) ||
            string.Equals(
                guardPromptAwaitingActivityText,
                currentRecord.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        guardPromptEmitted = true;
        guardPromptAwaitingActivityText = currentRecord.Trim();
        SteamCmdOutputEventKind eventKind = guardKind == SteamCmdGuardPromptKind.MobileApproval
            ? SteamCmdOutputEventKind.MobileConfirmationPrompt
            : SteamCmdOutputEventKind.SteamGuardPrompt;
        events.Add(CreateEvent(eventKind, currentRecord, guardPromptKind: guardKind));
    }

    private void CompleteRecord(List<SteamCmdOutputEvent> events)
    {
        if (recordBuffer.Length > 0)
        {
            string record = recordBuffer.ToString();
            bool recordIsPrompt = SteamPromptRegex.IsMatch(record) ||
                                  PasswordPromptRegex.IsMatch(record) ||
                                  TryGetGuardPromptKind(record, out _);
            ProcessRecord(record, events);
            if (!recordIsPrompt && !string.IsNullOrWhiteSpace(record))
            {
                // ConPTY may repaint the same prompt in a later terminal frame.
                // Do not emit it again until command/authentication activity has
                // appeared between the two frames.
                steamPromptAwaitingActivity = false;
                passwordPromptAwaitingActivity = false;
                guardPromptAwaitingActivityText = null;
            }
            recordBuffer.Clear();
        }

        steamPromptEmitted = false;
        passwordPromptEmitted = false;
        guardPromptEmitted = false;
    }

    private void ProcessRecord(string rawRecord, List<SteamCmdOutputEvent> events)
    {
        string record = rawRecord.Trim();
        if (record.Length == 0)
        {
            return;
        }

        // Sanitize before trimming so a secret that intentionally begins or ends with
        // whitespace cannot be partially exposed by normalization.
        string eventText = Sanitize(rawRecord).Trim();
        events.Add(CreateEvent(SteamCmdOutputEventKind.Output, eventText));

        if (LoggedInRegex.IsMatch(record))
        {
            events.Add(CreateEvent(SteamCmdOutputEventKind.LoggedIn, eventText));
        }

        bool loginFailed = LoginFailedRegex.IsMatch(record);
        if (loginFailed)
        {
            events.Add(CreateEvent(SteamCmdOutputEventKind.LoginFailed, eventText));
        }

        bool workshopFailure = TryParseWorkshopFailure(record, out ulong failedWorkshopId);
        if (workshopFailure)
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.WorkshopDownloadFailed,
                eventText,
                workshopId: failedWorkshopId));
            currentWorkshopId = null;
        }
        else if (TryParseWorkshopSuccess(record, out ulong successfulWorkshopId))
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.WorkshopDownloadSucceeded,
                eventText,
                workshopId: successfulWorkshopId));
            currentWorkshopId = null;
        }
        else if (TryParseWorkshopRequest(record, out ulong requestedWorkshopId))
        {
            if (currentWorkshopId != requestedWorkshopId)
            {
                events.Add(CreateEvent(
                    SteamCmdOutputEventKind.WorkshopDownloadRequested,
                    eventText,
                    workshopId: requestedWorkshopId));
            }

            currentWorkshopId = requestedWorkshopId;
        }

        Match progressMatch = ProgressRegex.Match(record);
        if (progressMatch.Success && TryParseProgress(progressMatch.Groups["progress"].Value, out double progress))
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.Progress,
                eventText,
                workshopId: currentWorkshopId,
                progressPercent: progress));
        }

        Match appSuccessMatch = AppSuccessRegex.Match(record);
        if (appSuccessMatch.Success
            && uint.TryParse(
                appSuccessMatch.Groups["appId"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out uint appId))
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.AppUpdateSucceeded,
                eventText,
                appId: appId));
        }

        bool timeout = TimeoutRegex.IsMatch(record);
        if (timeout)
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.Timeout,
                eventText,
                workshopId: failedWorkshopId == 0 ? currentWorkshopId : failedWorkshopId));
        }

        if (ErrorRegex.IsMatch(record) && !loginFailed)
        {
            events.Add(CreateEvent(
                SteamCmdOutputEventKind.Error,
                eventText,
                workshopId: failedWorkshopId == 0 ? currentWorkshopId : failedWorkshopId));
        }
    }

    private bool TryParseWorkshopFailure(string record, out ulong workshopId)
    {
        Match failureMatch = WorkshopFailureRegex.Match(record);
        if (TryParseId(failureMatch, out workshopId))
        {
            return true;
        }

        Match timeoutMatch = WorkshopTimeoutRegex.Match(record);
        return TryParseId(timeoutMatch, out workshopId);
    }

    private static bool TryParseWorkshopSuccess(string record, out ulong workshopId) =>
        TryParseId(WorkshopSuccessRegex.Match(record), out workshopId);

    private static bool TryParseWorkshopRequest(string record, out ulong workshopId)
    {
        Match downloadingMatch = WorkshopDownloadingRegex.Match(record);
        if (TryParseId(downloadingMatch, out workshopId))
        {
            return true;
        }

        return TryParseId(WorkshopCommandRegex.Match(record), out workshopId);
    }

    private static bool TryParseId(Match match, out ulong workshopId)
    {
        workshopId = 0;
        return match.Success
            && ulong.TryParse(
                match.Groups["id"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out workshopId);
    }

    private static bool TryParseProgress(string value, out double progress) =>
        double.TryParse(
            value.Replace(',', '.'),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out progress);

    private static bool TryGetGuardPromptKind(string value, out SteamCmdGuardPromptKind kind)
    {
        string prompt = value.Trim();
        if (prompt.Length == 0)
        {
            kind = default;
            return false;
        }

        bool mentionsCode = prompt.Contains("code", StringComparison.OrdinalIgnoreCase);
        // SteamCMD can print an explanatory sentence followed by the actual
        // input prompt on the next line (for example, a Mobile Authenticator
        // explanation followed by "Two-factor code:"). Only the terminal
        // prompt should cause FASTER to send a secret.
        bool asksForInput = prompt.EndsWith(':') || prompt.EndsWith('?');

        bool mentionsMobileApp = prompt.Contains("mobile app", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("Steam app", StringComparison.OrdinalIgnoreCase);
        bool requestsApproval = prompt.Contains("approve", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("confirm", StringComparison.OrdinalIgnoreCase)
            || prompt.Contains("sign in request", StringComparison.OrdinalIgnoreCase);
        if (mentionsMobileApp && requestsApproval)
        {
            kind = SteamCmdGuardPromptKind.MobileApproval;
            return true;
        }

        if (mentionsCode
            && asksForInput
            && (prompt.Contains("email", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("e-mail", StringComparison.OrdinalIgnoreCase)))
        {
            kind = SteamCmdGuardPromptKind.EmailCode;
            return true;
        }

        if (mentionsCode
            && asksForInput
            && (prompt.Contains("authenticator", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("two-factor", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("two factor", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("2fa", StringComparison.OrdinalIgnoreCase)))
        {
            kind = SteamCmdGuardPromptKind.AuthenticatorCode;
            return true;
        }

        if (mentionsCode
            && (prompt.EndsWith(':') || prompt.EndsWith('?'))
            && prompt.Contains("Steam Guard", StringComparison.OrdinalIgnoreCase))
        {
            kind = SteamCmdGuardPromptKind.Unknown;
            return true;
        }

        kind = default;
        return false;
    }

    private SteamCmdOutputEvent CreateEvent(
        SteamCmdOutputEventKind kind,
        string text,
        ulong? workshopId = null,
        double? progressPercent = null,
        uint? appId = null,
        SteamCmdGuardPromptKind? guardPromptKind = null) =>
        new(
            kind,
            Sanitize(text).Trim(),
            workshopId,
            progressPercent,
            appId,
            guardPromptKind);

    private string Sanitize(string value)
    {
        string sanitized = value;
        foreach (string secret in secrets)
        {
            sanitized = sanitized.Replace(secret, RedactionMarker, StringComparison.Ordinal);
        }

        return sanitized;
    }

    private enum AnsiState
    {
        None,
        Escape,
        EscapeIntermediate,
        ControlSequence,
        OperatingSystemCommand,
        OperatingSystemCommandEscape
    }
}
