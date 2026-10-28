using FASTER.Models;
using FASTER.Services;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
[NonParallelizable]
public class StartupParityTests
{
    private string _directory = null!;
    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        AppSettings.PathOverrideForTests = Path.Combine(_directory, "faster.json");
    }
    [TearDown]
    public void TearDown()
    {
        AppSettings.PathOverrideForTests = null;
        Directory.Delete(_directory, true);
    }
    [Test]
    public void SetupDoesNotCompleteUntilSubmittedAndRetainsUnchangedPassword()
    {
        var settings = new AppSettings { SteamPassword = "saved ciphertext" };
        settings.InitializeForStartup();
        Assert.That(settings.FirstRun, Is.True);
        settings.CompleteSetup("", false);
        Assert.Multiple(() => {
            Assert.That(settings.FirstRun, Is.False);
            Assert.That(settings.SetupRun, Is.False);
            Assert.That(settings.SteamPassword, Is.EqualTo("saved ciphertext"));
        });
    }
    [Test]
    public void LegacyModMigrationKeepsExistingPaths()
    {
        var settings = new AppSettings { SteamCMDPath = _directory,
            SteamMods = new SteamModCollection { SteamMods = [new SteamMod { WorkshopId = 42, Name = "legacy" }] },
            LocalMods = [new LocalMod { Name = "local", Path = Path.Combine(_directory, "@local") }] };
        settings.InitializeForStartup();
        Assert.That(settings.ArmaMods!.ArmaMods.Count, Is.EqualTo(2));
        Assert.That(settings.ArmaMods.ArmaMods[0].Path, Is.EqualTo(Path.Combine(_directory, "steamapps", "workshop", "content", "107410", "42")));
        Assert.That(settings.ArmaMods.ArmaMods[1].IsLocal, Is.True);
        settings.InitializeForStartup();
        Assert.That(settings.ArmaMods.ArmaMods.Count, Is.EqualTo(2));
    }
    [Test]
    public void ChangedPasswordIsEncryptedAndReloadable()
    {
        var settings = new AppSettings();
        settings.CompleteSetup("new password", true);
        Assert.That(settings.SteamPassword, Is.Not.EqualTo("new password"));
        Assert.That(Encryption.Instance.DecryptData(settings.SteamPassword), Is.EqualTo("new password"));
    }

    [Test]
    public void CompletedInstallationIsNormalizedWithoutLosingValues()
    {
        var settings = new AppSettings { FirstRun = false, SetupRun = true, SteamUserName = "saved" };
        settings.InitializeForStartup();
        Assert.That(settings.SetupRun, Is.False);
        Assert.That(settings.SteamUserName, Is.EqualTo("saved"));
    }
    [Test]
    public void PendingResetPreservesInstallationFiles()
    {
        var file = Path.Combine(_directory, "server.cfg");
        File.WriteAllText(file, "keep");
        var settings = new AppSettings { ClearSettings = true, ServerPath = _directory, FirstRun = false };
        settings.InitializeForStartup();
        Assert.That(settings.FirstRun, Is.True);
        Assert.That(settings.ClearSettings, Is.False);
        Assert.That(settings.ServerPath, Is.Empty);
        Assert.That(File.ReadAllText(file), Is.EqualTo("keep"));
    }
    [TestCase(true, false, 233782u)]
    [TestCase(false, false, 233783u)]
    [TestCase(true, true, 233784u)]
    [TestCase(false, true, 233785u)]
    public void SelectsPlatformDepot(bool windows, bool profiling, uint expected) =>
        Assert.That(DefaultPlatformServices.ServerDepot(windows, profiling), Is.EqualTo(expected));

    [Test]
    public void LinuxExecutablePermissionsPreserveOtherBits()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux permissions"); return; }
        var file = Path.Combine(_directory, "arma3server_x64");
        File.WriteAllText(file, "fixture");
        File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        DefaultPlatformServices.Instance.PrepareServerExecutables(_directory);
        Assert.That(File.GetUnixFileMode(file), Is.EqualTo(UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute));
    }
    [Test]
    public void DesktopOpenerKeepsSpecialCharactersInOneArgument()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Unix desktop opener");
        const string target = "/tmp/a folder & another/file";
        var start = DefaultPlatformServices.CreateOpenStartInfo(target);
        Assert.That(start.ArgumentList, Is.EqualTo(new[] { target }));
        Assert.That(start.UseShellExecute, Is.False);
    }
}
