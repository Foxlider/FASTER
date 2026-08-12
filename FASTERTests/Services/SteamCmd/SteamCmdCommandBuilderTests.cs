using FASTER.Services.SteamCmd;
using NUnit.Framework;
using System;
using System.IO;

namespace FASTERTests.Services.SteamCmd;

[TestFixture]
public sealed class SteamCmdCommandBuilderTests
{
    [Test]
    public void BuildWorkshopDownloadCommand_IncludesAppIdItemIdAndValidate()
    {
        string command = SteamCmdCommandBuilder.BuildWorkshopDownloadCommand(450814997);

        Assert.That(command, Is.EqualTo("workshop_download_item 107410 450814997 validate"));
    }

    [Test]
    public void BuildWorkshopDownloadCommand_RejectsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SteamCmdCommandBuilder.BuildWorkshopDownloadCommand(0));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void BuildLoginCommand_UsesAnonymousForBlankAccount(string accountName)
    {
        Assert.That(SteamCmdCommandBuilder.BuildLoginCommand(accountName), Is.EqualTo("login anonymous"));
    }

    [Test]
    public void BuildLoginCommand_UsesConfiguredAccountInsteadOfAnonymous()
    {
        Assert.That(
            SteamCmdCommandBuilder.BuildLoginCommand("  arma_owner  "),
            Is.EqualTo("login arma_owner"));
    }

    [Test]
    public void BuildLoginCommand_DoesNotAcceptCommandInjection()
    {
        Assert.Throws<ArgumentException>(() =>
            SteamCmdCommandBuilder.BuildLoginCommand("owner\nquit"));
    }

    [Test]
    public void BuildLoginCommand_NeverContainsPassword()
    {
        const string password = "do-not-log-this";

        string command = SteamCmdCommandBuilder.BuildLoginCommand("workshop_owner");

        Assert.Multiple(() =>
        {
            Assert.That(command, Is.EqualTo("login workshop_owner"));
            Assert.That(command, Does.Not.Contain(password));
        });
    }

    [Test]
    public void BuildForceInstallDirectoryCommand_QuotesNormalizedPathWithSpaces()
    {
        string input = Path.Combine(Path.GetTempPath(), "Arma Server");
        string expected = Path.TrimEndingDirectorySeparator(Path.GetFullPath(input));

        string command = SteamCmdCommandBuilder.BuildForceInstallDirectoryCommand(input);

        Assert.That(command, Is.EqualTo($"force_install_dir \"{expected}\""));
    }

    [Test]
    public void BuildForceInstallDirectoryCommand_RejectsQuoteAndNewlineInjection()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                SteamCmdCommandBuilder.BuildForceInstallDirectoryCommand("C:\\server\\\" +quit"));
            Assert.Throws<ArgumentException>(() =>
                SteamCmdCommandBuilder.BuildForceInstallDirectoryCommand("C:\\server\nquit"));
        });
    }

    [Test]
    public void BuildServerUpdateCommand_MapsEverySupportedBranch()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                SteamCmdCommandBuilder.BuildServerUpdateCommand(SteamCmdServerBranch.Public),
                Is.EqualTo("app_update 233780 -beta public validate"));
            Assert.That(
                SteamCmdCommandBuilder.BuildServerUpdateCommand(SteamCmdServerBranch.Contact),
                Is.EqualTo("app_update 233780 -beta contact validate"));
            Assert.That(
                SteamCmdCommandBuilder.BuildServerUpdateCommand(SteamCmdServerBranch.CreatorDlc),
                Is.EqualTo("app_update 233780 -beta creatordlc validate"));
            Assert.That(
                SteamCmdCommandBuilder.BuildServerUpdateCommand(SteamCmdServerBranch.Profiling),
                Is.EqualTo("app_update 233780 -beta profiling validate"));
        });
    }

    [Test]
    public void BuildServerUpdateCommand_RejectsUnknownEnumValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SteamCmdCommandBuilder.BuildServerUpdateCommand((SteamCmdServerBranch)999));
    }
}
