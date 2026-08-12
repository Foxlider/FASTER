using FASTER.Services.SteamCmd;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FASTERTests.Services.SteamCmd;

[TestFixture]
public sealed class SteamCmdOutputParserTests
{
    [Test]
    public void Feed_RecognizesSplitSteamPromptOnceAndStripsSplitAnsiSequence()
    {
        SteamCmdOutputParser parser = new();
        List<SteamCmdOutputEvent> events = [];

        events.AddRange(parser.Feed("\u001b["));
        events.AddRange(parser.Feed("32mSte"));
        events.AddRange(parser.Feed("am>"));
        events.AddRange(parser.Feed(string.Empty));

        Assert.That(
            events.Count(item => item.Kind == SteamCmdOutputEventKind.Prompt),
            Is.EqualTo(1));

        events.AddRange(parser.Complete());
        SteamCmdOutputEvent output = events.Single(item => item.Kind == SteamCmdOutputEventKind.Output);
        Assert.Multiple(() =>
        {
            Assert.That(output.Text, Is.EqualTo("Steam>"));
            Assert.That(events.All(item => !item.Text.Contains('\u001b')), Is.True);
        });
    }

    [Test]
    public void Feed_SuppressesConPtyPromptRepaintUntilCommandActivity()
    {
        SteamCmdOutputParser parser = new();
        List<SteamCmdOutputEvent> events = [];

        events.AddRange(parser.Feed("Steam>\r\n\u001b[38;5;15m\r\nSteam>"));
        Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.Prompt), Is.EqualTo(1));

        events.AddRange(parser.Feed("login anonymous\r\nConnecting anonymously...OK\r\nSteam>"));
        Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.Prompt), Is.EqualTo(2));
    }

    [Test]
    public void Feed_PublishesInlineLoginResultBeforeConPtyPrompt()
    {
        SteamCmdOutputParser parser = new();

        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            "Waiting for user info...OKSteam>");

        Assert.Multiple(() =>
        {
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.LoggedIn), Is.True);
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.Prompt), Is.True);
            Assert.That(
                events.ToList().FindIndex(item => item.Kind == SteamCmdOutputEventKind.LoggedIn),
                Is.LessThan(events.ToList().FindIndex(item => item.Kind == SteamCmdOutputEventKind.Prompt)));
        });
    }

    [Test]
    public void Feed_TreatsCrLfCrAndLfAsLogicalRecordDelimiters()
    {
        SteamCmdOutputParser parser = new();
        List<SteamCmdOutputEvent> events = [.. parser.Feed("one\rtwo\nthree\r\nfour")];
        events.AddRange(parser.Complete());

        Assert.That(
            events.Where(item => item.Kind == SteamCmdOutputEventKind.Output).Select(item => item.Text),
            Is.EqualTo(new[] { "one", "two", "three", "four" }));
    }

    [Test]
    public void Feed_RecognizesPasswordPromptBeforeNewlineAndRedactsSplitSecret()
    {
        const string password = "correct horse battery staple";
        SteamCmdOutputParser parser = new([password]);
        List<SteamCmdOutputEvent> events = [.. parser.Feed("password:")];

        Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.PasswordPrompt), Is.EqualTo(1));

        events.AddRange(parser.Feed(" correct horse "));
        events.AddRange(parser.Feed("battery staple\r\n"));

        Assert.Multiple(() =>
        {
            Assert.That(events.All(item => !item.Text.Contains(password, StringComparison.Ordinal)), Is.True);
            Assert.That(
                events.Single(item => item.Kind == SteamCmdOutputEventKind.Output).Text,
                Is.EqualTo("password: [REDACTED]"));
            Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.PasswordPrompt), Is.EqualTo(1));
        });
    }

    [Test]
    public void AddSecret_RedactsGuardCodeAddedAfterParserConstruction()
    {
        const string guardCode = "R4C9Q";
        SteamCmdOutputParser parser = new();
        List<SteamCmdOutputEvent> events = [.. parser.Feed("Steam Guard code:")];

        parser.AddSecret(guardCode);
        events.AddRange(parser.Feed($" {guardCode}\n"));

        Assert.Multiple(() =>
        {
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.SteamGuardPrompt), Is.True);
            Assert.That(events.All(item => !item.Text.Contains(guardCode, StringComparison.Ordinal)), Is.True);
            Assert.That(events.Single(item => item.Kind == SteamCmdOutputEventKind.Output).Text, Does.Contain("[REDACTED]"));
        });
    }

    [Test]
    public void Feed_DistinguishesEmailAuthenticatorAndMobileApprovalPrompts()
    {
        SteamCmdOutputParser parser = new();
        List<SteamCmdOutputEvent> events = [];

        events.AddRange(parser.Feed("Please enter the authentication code sent to your email address:\n"));
        events.AddRange(parser.Feed("Please enter the current code from your Steam Guard Mobile Authenticator app:\r"));
        events.AddRange(parser.Feed("Approve the sign in request in your Steam Mobile App"));

        Assert.Multiple(() =>
        {
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.SteamGuardPrompt
                    && item.GuardPromptKind == SteamCmdGuardPromptKind.EmailCode),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.SteamGuardPrompt
                    && item.GuardPromptKind == SteamCmdGuardPromptKind.AuthenticatorCode),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.MobileConfirmationPrompt
                    && item.GuardPromptKind == SteamCmdGuardPromptKind.MobileApproval),
                Is.True);
        });
    }

    [Test]
    public void Feed_MobileAuthenticatorTranscript_EmitsOnlyTheLiteralCodePrompt()
    {
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            "Enter the current code from your Steam Guard Mobile Authenticator app\r\n"
            + "Two-factor code:");

        SteamCmdOutputEvent prompt = events.Single(
            item => item.Kind == SteamCmdOutputEventKind.SteamGuardPrompt);
        Assert.That(prompt.GuardPromptKind, Is.EqualTo(SteamCmdGuardPromptKind.AuthenticatorCode));
        Assert.That(prompt.Text, Is.EqualTo("Two-factor code:"));
    }

    [Test]
    public void Feed_UserInfoTimeoutAfterLoggedIn_IsStillALoginFailure()
    {
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            "Logged in OK\r\n"
            + "Waiting for user info...FAILED. Timed out.\r\n"
            + "Steam>");

        Assert.Multiple(() =>
        {
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.LoggedIn), Is.True);
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.LoginFailed), Is.True);
            Assert.That(events.Any(item => item.Kind == SteamCmdOutputEventKind.Timeout), Is.True);
        });
    }

    [Test]
    public void Feed_RecognizesSuccessfulAndFailedAuthentication()
    {
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            "Logging in user 'test' to Steam Public...FAILED (Invalid Password)\n"
            + "Logged in OK\n"
            + "Waiting for user info...OK\n");

        Assert.Multiple(() =>
        {
            Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.LoginFailed), Is.EqualTo(1));
            Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.LoggedIn), Is.EqualTo(2));
        });
    }

    [Test]
    public void Feed_ParsesUlongWorkshopIdsAcrossRequestSuccessAndFailure()
    {
        const ulong firstId = ulong.MaxValue;
        const ulong secondId = 450814997;
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            $"workshop_download_item 107410 {firstId} validate\r"
            + $"Success. Downloaded item {firstId} to C:\\workshop\r\n"
            + $"Downloading item {secondId} ...\n"
            + $"ERROR! Download item {secondId} failed (Failure).\n");

        Assert.Multiple(() =>
        {
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.WorkshopDownloadRequested
                    && item.WorkshopId == firstId),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.WorkshopDownloadSucceeded
                    && item.WorkshopId == firstId),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.WorkshopDownloadRequested
                    && item.WorkshopId == secondId),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.WorkshopDownloadFailed
                    && item.WorkshopId == secondId),
                Is.True);
        });
    }

    [Test]
    public void Feed_PreservesFullProgressDecimalAndAssociatesActiveWorkshopItem()
    {
        const ulong workshopId = 12345678901234567890;
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            $"Downloading item {workshopId} ...\r"
            + "Update state (0x61) downloading, progress: 12.345678 (123 / 1000)\r");

        SteamCmdOutputEvent progress = events.Single(item => item.Kind == SteamCmdOutputEventKind.Progress);
        Assert.Multiple(() =>
        {
            Assert.That(progress.ProgressPercent, Is.EqualTo(12.345678d));
            Assert.That(progress.WorkshopId, Is.EqualTo(workshopId));
        });
    }

    [Test]
    public void Feed_RecognizesAppSuccessGenericErrorsAndTimeouts()
    {
        SteamCmdOutputParser parser = new();
        IReadOnlyList<SteamCmdOutputEvent> events = parser.Feed(
            "Success! App '233780' fully installed.\n"
            + "ERROR! Something unexpected happened.\n"
            + "ERROR! Timeout downloading item 42\n");

        Assert.Multiple(() =>
        {
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.AppUpdateSucceeded
                    && item.AppId == 233780),
                Is.True);
            Assert.That(events.Count(item => item.Kind == SteamCmdOutputEventKind.Error), Is.EqualTo(2));
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.Timeout && item.WorkshopId == 42),
                Is.True);
            Assert.That(
                events.Any(item => item.Kind == SteamCmdOutputEventKind.WorkshopDownloadFailed
                    && item.WorkshopId == 42),
                Is.True);
        });
    }

    [Test]
    public void Complete_FlushesUnterminatedOutputOnceAndPreventsFurtherFeed()
    {
        SteamCmdOutputParser parser = new();
        Assert.That(parser.Feed("final record"), Is.Empty);

        IReadOnlyList<SteamCmdOutputEvent> firstCompletion = parser.Complete();
        IReadOnlyList<SteamCmdOutputEvent> secondCompletion = parser.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(
                firstCompletion.Single(item => item.Kind == SteamCmdOutputEventKind.Output).Text,
                Is.EqualTo("final record"));
            Assert.That(secondCompletion, Is.Empty);
            Assert.Throws<ObjectDisposedException>(() => parser.Feed("too late"));
        });
    }
}
