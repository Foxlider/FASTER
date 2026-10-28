using FASTER.Models;

using NUnit.Framework;

using System.Reflection;

namespace FASTER.Core.Tests;

[TestFixture]
public class ColdLoadTests
{
    private string _tempDir = string.Empty;
    private string? _previousOverride;

    [SetUp]
    public void SetUp()
    {
        _previousOverride = AppSettings.PathOverrideForTests;
        _tempDir = Path.Combine(Path.GetTempPath(), "faster-coldload-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        AppSettings.PathOverrideForTests = Path.Combine(_tempDir, "faster.json");
        ResetSingleton();
    }

    [TearDown]
    public void TearDown()
    {
        ResetSingleton();
        AppSettings.PathOverrideForTests = _previousOverride;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static void ResetSingleton()
        => typeof(AppSettings).GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, null);

    [Test]
    public void ColdLoadWithDeploymentsKeepsEveryProfileAndNeverRewritesTheFile()
    {
        var serverDir = Path.Combine(_tempDir, "server");
        Directory.CreateDirectory(serverDir);

        AppSettings.Current.ServerPath = serverDir;
        AppSettings.Current.Deployments = new ArmaDeployment();
        AppSettings.Current.Profiles = new ServerProfileCollection();
        var profile = new ServerProfile("coldload", createFolder: false);
        profile.ProfileMods = new List<ProfileMod>
        {
            new() { Id = 1, Name = "CBA_A3" },
            new() { Id = 2, Name = "ACE3" }
        };
        AppSettings.Current.Profiles.Add(profile);
        AppSettings.Current.Save();
        byte[] before = File.ReadAllBytes(AppSettings.SettingsPath);

        ResetSingleton();

        var loaded = AppSettings.Current;
        Assert.That(loaded.ServerPath, Is.EqualTo(serverDir));
        Assert.That(loaded.Deployments, Is.Not.Null);
        Assert.That(loaded.Profiles?.Count, Is.EqualTo(1));
        Assert.That(loaded.Profiles?[0].ProfileMods.Count, Is.EqualTo(2));
        Assert.That(loaded.Profiles?[0].FilteredProfileMods.Count, Is.EqualTo(2));
        Assert.That(File.ReadAllBytes(AppSettings.SettingsPath), Is.EqualTo(before));
    }

    [Test]
    public void ConstructingModelsNeverTouchesTheSettingsFile()
    {
        File.WriteAllText(AppSettings.SettingsPath, """{"ServerPath":"/tmp/kept"}""");
        byte[] before = File.ReadAllBytes(AppSettings.SettingsPath);

        ResetSingleton();
        _ = new ArmaDeployment();
        _ = new ServerProfile("transient", createFolder: false);
        _ = new SteamMod(123, "m", "a", 1);

        Assert.That(File.ReadAllBytes(AppSettings.SettingsPath), Is.EqualTo(before));
    }
}
