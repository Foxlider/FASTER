using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture, NonParallelizable]
public class ProfileAndMaintenanceTests
{
    private string _root = null!;
    private IDialogService _dialogs = null!;
    private AppSettings _settings = null!;
    private sealed class Dialogs(string? answer) : IDialogService
    {
        public Task<string?> ShowInputAsync(object context, string title, string message) => Task.FromResult(answer);
        public Task<bool> ShowConfirmationAsync(object context, string title, string message) => Task.FromResult(answer == "yes");
        public Task ShowMessageAsync(object context, string title, string message) => Task.CompletedTask;
        public Task<IProgressDialog> ShowProgressAsync(object context, string title, string message) => throw new NotSupportedException();
    }
    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        AppSettings.PathOverrideForTests = Path.Combine(_root, "faster.json");
        _settings = AppSettings.Current;
        _settings.Reset();
        _settings.InitializeForStartup();
        _settings.ModStagingDirectory = _root;
        _settings.ArmaMods = new ArmaModCollection();
        _dialogs = AppServices.Dialogs;
        AppServices.Dialogs = new Dialogs("yes");
    }
    [TearDown]
    public void TearDown()
    {
        AppServices.Dialogs = _dialogs;
        _settings.Reset();
        AppSettings.PathOverrideForTests = null;
        Directory.Delete(_root, true);
    }
    private ArmaMod AddMod(uint id, bool local)
    {
        var path = Path.Combine(_root, id.ToString());
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "fixture"), "keep");
        var mod = new ArmaMod { WorkshopId = id, IsLocal = local, Path = path, Name = "fixture" };
        _settings.ArmaMods!.ArmaMods.Add(mod);
        return mod;
    }
    [Test]
    public async Task PurgeAllPreservesLocalModsAndResetsWorkshopState()
    {
        var local = AddMod(1, true);
        var workshop = AddMod(2, false);
        var vm = new ModsViewModel();
        await vm.PurgeAndReinstallAll();
        Assert.That(File.Exists(Path.Combine(local.Path, "fixture")), Is.True);
        Assert.That(Directory.Exists(workshop.Path), Is.False);
        Assert.That(workshop.Status, Is.EqualTo(ArmaModStatus.UpdateRequired));
        Assert.That(vm.IsBusy, Is.False);
    }
    [Test]
    public async Task DeclinedOrCancelledMaintenanceDoesNotDelete()
    {
        var workshop = AddMod(2, false);
        AppServices.Dialogs = new Dialogs(null);
        var vm = new ModsViewModel();
        await vm.PurgeAndReinstallAll();
        Assert.That(Directory.Exists(workshop.Path), Is.True);
        AppServices.Dialogs = new Dialogs("yes");
        Assert.ThrowsAsync<OperationCanceledException>(async () => await vm.PurgeAndReinstallAll(new CancellationToken(true)));
        Assert.That(Directory.Exists(workshop.Path), Is.True);
        Assert.That(vm.IsBusy, Is.False);
    }
    [Test]
    public async Task UnusedPurgeKeepsLocalAndReferencedMods()
    {
        var local = AddMod(1, true);
        var used = AddMod(2, false);
        AddMod(3, false);
        _settings.Profiles = new ServerProfileCollection { new ServerProfile("fixture", false) };
        _settings.Profiles[0].ProfileMods = [new ProfileMod { Id = 2, HeadlessChecked = true }];
        await new ModsViewModel().PurgeUnusedMods();
        Assert.That(_settings.ArmaMods!.ArmaMods.Select(m => m.WorkshopId), Is.EquivalentTo(new uint[] { 1, 2 }));
        Assert.That(Directory.Exists(local.Path), Is.True);
        Assert.That(Directory.Exists(used.Path), Is.True);
    }
    [Test]
    public void ProfileOrderingPersistsAndRejectsOutOfRangeMoves()
    {
        _settings.Profiles = new ServerProfileCollection { new("first", false), new("second", false) };
        string id = _settings.Profiles[1].Id;
        Assert.That(_settings.Profiles.MoveProfile(id, -1), Is.True);
        Assert.That(_settings.Profiles.MoveProfile(id, -1), Is.False);
        _settings.Save();
        _settings.Reload();
        Assert.That(_settings.Profiles![0].Id, Is.EqualTo(id));
    }
    [Test]
    public void UnknownProfileValuesSurviveSavingAndCloning()
    {
        var profile = System.Text.Json.JsonSerializer.Deserialize<ServerProfile>("{\"Name\":\"future\",\"FutureSetting\":{\"enabled\":true}}")!;
        _settings.Profiles = new ServerProfileCollection { profile };
        _settings.Save();
        _settings.Reload();
        Assert.That(_settings.Profiles![0].AdditionalSettings!["FutureSetting"].GetProperty("enabled").GetBoolean(), Is.True);
        Assert.That(profile.Clone().AdditionalSettings!["FutureSetting"].GetProperty("enabled").GetBoolean(), Is.True);
    }

    [Test]
    public void RestoredOptionsReachConfigurationAndCommandLine()
    {
        var profile = new ServerProfile("options", false) {
            HugePages = true, LoadMissionToMemory = true, EnableSteamLogs = true,
            ExThreads = 7, LimitFPS = 80, BePath = "/a path/be", KeysFolder = "/a path/keys"
        };
        profile.BasicCfg.Language = "German";
        profile.ServerCfg.AntiFloodEnabled = true;
        profile.ServerCfg.AntiFloodCycleTime = 10;
        profile.ServerCfg.MissionHTTPDownloadBaseURL = "https://example.org/missions";
        Assert.That(profile.BasicCfg.BasicContent, Does.Contain("German"));
        Assert.That(profile.ServerCfg.ServerCfgContent, Does.Contain("class AntiFlood").And.Contain("cycleTime = 10").And.Contain("https://example.org/missions"));
        foreach (var argument in new[] { "-hugePages", "-loadMissionToMemory", "-enableSteamLogs", "-exThreads=7", "-limitFPS=80", "-bepath=/a path/be", "-keysFolder=/a path/keys" })
            Assert.That(profile.CommandLine, Does.Contain(argument).IgnoreCase);
    }
}
