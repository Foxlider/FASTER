using FASTER.Models;

using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class AppSettingsTests
{
    private string _tempDir = string.Empty;

    [OneTimeSetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "faster-core-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        AppSettings.PathOverrideForTests = Path.Combine(_tempDir, "faster.json");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        AppSettings.PathOverrideForTests = null;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test]
    public void DefaultsMatchLegacyStore()
    {
        Assert.That(AppSettings.Current.FirstRun, Is.True);
        Assert.That(AppSettings.Current.SetupRun, Is.True);
        Assert.That(AppSettings.Current.CheckForModUpdates, Is.True);
        Assert.That(AppSettings.Current.CheckForAppUpdates, Is.False);
        Assert.That(AppSettings.Current.ServerBranch, Is.EqualTo("Stable"));
        Assert.That(AppSettings.Current.Theme, Is.EqualTo("Dark.Blue"));
        Assert.That(AppSettings.Current.Font, Is.EqualTo("Segoe UI"));
        Assert.That(AppSettings.Current.CliWorkers, Is.EqualTo(10));
        Assert.That(AppSettings.Current.Profiles, Is.Null);
    }

    [Test]
    public void SaveAndReloadRoundTrip()
    {
        var serverDir = Path.Combine(_tempDir, "server");
        Directory.CreateDirectory(serverDir);
        AppSettings.Current.ServerPath = serverDir;
        AppSettings.Current.SteamUserName = "tester";
        AppSettings.Current.Profiles = new ServerProfileCollection();
        AppSettings.Current.Profiles.Add(new ServerProfile("roundtrip") { Port = 2302 });
        AppSettings.Current.Save();

        Assert.That(File.Exists(AppSettings.SettingsPath), Is.True);

        AppSettings.Current.ServerPath = "changed";
        AppSettings.Current.Reload();

        Assert.That(AppSettings.Current.ServerPath, Is.EqualTo(serverDir));
        Assert.That(AppSettings.Current.SteamUserName, Is.EqualTo("tester"));
        Assert.That(AppSettings.Current.Profiles?.Count, Is.EqualTo(1));
        Assert.That(AppSettings.Current.Profiles?[0].Port, Is.EqualTo(2302));
    }

    [Test]
    public void CollectionsRoundTrip()
    {
        AppSettings.Current.SteamMods = new SteamModCollection();
        AppSettings.Current.SteamMods.SteamMods.Add(new SteamMod(123, "TestMod", "Author", 999));
        AppSettings.Current.LocalModFolders = new List<string> { "/mods" };
        AppSettings.Current.ArmaMods = new ArmaModCollection();
        AppSettings.Current.ArmaMods.ArmaMods.Add(new ArmaMod { WorkshopId = 1, Name = "m", Path = "/tmp" });
        AppSettings.Current.Deployments = new ArmaDeployment();
        AppSettings.Current.Save();
        AppSettings.Current.Reload();

        Assert.That(AppSettings.Current.SteamMods?.SteamMods.Count, Is.EqualTo(1));
        Assert.That(AppSettings.Current.SteamMods?.SteamMods[0].Name, Is.EqualTo("TestMod"));
        Assert.That(AppSettings.Current.LocalModFolders?.Count, Is.EqualTo(1));
        Assert.That(AppSettings.Current.ArmaMods?.ArmaMods.Count, Is.EqualTo(1));
        Assert.That(AppSettings.Current.Deployments, Is.Not.Null);
    }

    [Test]
    public void ResetRestoresDefaults()
    {
        AppSettings.Current.ServerBranch = "changed";
        AppSettings.Current.Reset();

        Assert.That(AppSettings.Current.ServerBranch, Is.EqualTo("Stable"));
        Assert.That(AppSettings.Current.FirstRun, Is.True);
    }

    [Test]
    public void UpgradeCreatesFileWhenMissing()
    {
        if (File.Exists(AppSettings.SettingsPath))
            File.Delete(AppSettings.SettingsPath);

        Assert.DoesNotThrow(() => AppSettings.Current.Upgrade());
        Assert.That(File.Exists(AppSettings.SettingsPath), Is.True);
    }
}
