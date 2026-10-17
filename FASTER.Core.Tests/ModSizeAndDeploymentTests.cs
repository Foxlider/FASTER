using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture, NonParallelizable]
public class ModSizeAndDeploymentTests
{
    private string _root = null!;
    private AppSettings _settings = null!;
    private IFilePickerService _previousFiles = null!;
    private string? _previousSettingsPath;

    private sealed class FolderPicker(string? folder) : IFilePickerService
    {
        public int Calls { get; private set; }
        public Task<string?> PickFolderAsync(string current)
        {
            Calls++;
            return Task.FromResult(folder);
        }
        public Task<string?> PickModPresetFileAsync() => throw new NotSupportedException();
        public Task<string?> PickServerExecutableAsync() => throw new NotSupportedException();
    }

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), $"faster-mod-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        _previousSettingsPath = AppSettings.PathOverrideForTests;
        AppSettings.PathOverrideForTests = Path.Combine(_root, "faster.json");
        _settings = AppSettings.Current;
        _settings.Reset();
        _settings.ArmaMods = new ArmaModCollection();
        _settings.Deployments = new ArmaDeployment { InstallPath = string.Empty };
        _settings.CheckForModUpdates = false;
        _previousFiles = AppServices.Files;
    }

    [TearDown]
    public void TearDown()
    {
        AppServices.Files = _previousFiles;
        _settings.Reset();
        AppSettings.PathOverrideForTests = _previousSettingsPath;
        Directory.Delete(_root, true);
    }

    private ArmaMod AddMod(bool local = false)
    {
        var folder = Path.Combine(_root, "mods", "42");
        Directory.CreateDirectory(Path.Combine(folder, "addons"));
        File.WriteAllBytes(Path.Combine(folder, "addons", "test.pbo"), new byte[2048]);
        File.WriteAllBytes(Path.Combine(folder, "LICENSE"), new byte[64]);
        var mod = new ArmaMod { WorkshopId = 42, Name = "Test Mod", Path = folder,
            IsLocal = local, Status = ArmaModStatus.UpToDate, Size = 0 };
        _settings.ArmaMods!.ArmaMods.Add(mod);
        return mod;
    }

    private string ServerDirectory()
    {
        var server = Path.Combine(_root, "Arma Server");
        Directory.CreateDirectory(server);
        return server;
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task OpeningModsRefreshesSavedZeroSizesWithoutSteam(bool local)
    {
        AddMod(local);
        _settings.Save();
        _settings.Reload();
        var mod = _settings.ArmaMods!.ArmaMods.Single();
        var sizeChanged = false;
        mod.PropertyChanged += (_, e) => sizeChanged |= e.PropertyName == nameof(ArmaMod.Size);

        await new ModsViewModel().RefreshSizesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(mod.Size, Is.EqualTo(2112));
            Assert.That(sizeChanged, Is.True);
            Assert.That(mod.IsLoading, Is.False);
            Assert.That(mod.Status, Is.EqualTo(ArmaModStatus.UpToDate));
        });
    }

    [Test]
    public async Task SizeRefreshTracksChangedAndRemovedFiles()
    {
        var mod = AddMod();
        var vm = new ModsViewModel();
        await vm.RefreshSizesAsync();
        File.WriteAllBytes(Path.Combine(mod.Path, "addons", "test.pbo"), new byte[4096]);
        await vm.RefreshSizesAsync();
        Assert.That(mod.Size, Is.EqualTo(4160));
        Directory.Delete(mod.Path, true);
        await vm.RefreshSizesAsync();
        Assert.That(mod.Size, Is.Zero);
        Assert.That(mod.IsLoading, Is.False);
    }

    [Test]
    public async Task OpeningDeploymentRefreshesSizesAndUsesConfiguredServerPath()
    {
        AddMod();
        _settings.ServerPath = ServerDirectory();
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        Assert.That(vm.Deployment.DeployMods.Single().Size, Is.EqualTo(2112));
        Assert.That(vm.Deployment.InstallPath, Is.EqualTo(_settings.ServerPath));
    }

    [Test]
    public async Task LinkedLocalModReportsTheTargetFolderSize()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var mod = AddMod(local: true);
        var link = Path.Combine(_root, "local-link");
        Directory.CreateSymbolicLink(link, mod.Path);
        mod.Path = link;
        await new ModsViewModel().RefreshSizesAsync();
        Assert.That(mod.Size, Is.EqualTo(2112));
    }

    [Test]
    public async Task UnreadableModDoesNotPreventOtherSizeUpdates()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux directory permissions."); return; }
        var mod = AddMod();
        var blocked = Path.Combine(_root, "blocked");
        Directory.CreateDirectory(blocked);
        var blockedMod = new ArmaMod { WorkshopId = 1, Path = blocked };
        _settings.ArmaMods!.ArmaMods.Insert(0, blockedMod);
        var permissions = File.GetUnixFileMode(blocked);
        try
        {
            File.SetUnixFileMode(blocked, UnixFileMode.None);
            await new ModsViewModel().RefreshSizesAsync();
            Assert.That(blockedMod.IsLoading, Is.False);
            Assert.That(mod.Size, Is.EqualTo(2112));
        }
        finally { File.SetUnixFileMode(blocked, permissions); }
    }

    [Test]
    public async Task DeployAllPromptsForMissingInstallDirectoryAndCreatesLinks()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var mod = AddMod();
        var server = ServerDirectory();
        var picker = new FolderPicker(server);
        AppServices.Files = picker;
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        var target = Directory.ResolveLinkTarget(Path.Combine(server, "@Test_Mod"), true);
        Assert.That(target?.FullName, Is.EqualTo(mod.Path));
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.True);
        Assert.That(picker.Calls, Is.EqualTo(1));
        _settings.Reload();
        Assert.That(_settings.Deployments!.InstallPath, Is.EqualTo(server));
        Assert.That(_settings.Deployments.DeployMods.Single().Marked, Is.True);
    }

    [Test]
    public async Task DeploySingleUsesServerPathAndCanRemoveTheLink()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var original = AddMod();
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        _settings.ServerPath = ServerDirectory();
        var mod = vm.Deployment.DeployMods.Single();
        await vm.DeployModAsync(mod);
        var link = Path.Combine(_settings.ServerPath, "@Test_Mod");
        Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(original.Path));
        await vm.DeployModAsync(mod);
        Assert.That(Directory.Exists(link), Is.False);
        Assert.That(Directory.Exists(original.Path), Is.True);
        Assert.That(mod.Marked, Is.False);
    }

    [Test]
    public async Task CancellingFolderSelectionLeavesModsUndeployed()
    {
        AddMod();
        AppServices.Files = new FolderPicker(null);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        await vm.DeployModAsync(vm.Deployment.DeployMods.Single());
        Assert.That(vm.Deployment.InstallPath, Is.Empty);
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.False);
    }

    [Test]
    public async Task PickingDirectoryWithExistingLinkDoesNotUndoTheRequestedDeployment()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var original = AddMod();
        var server = ServerDirectory();
        var link = Path.Combine(server, "@Test_Mod");
        Directory.CreateSymbolicLink(link, original.Path);
        AppServices.Files = new FolderPicker(server);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployModAsync(vm.Deployment.DeployMods.Single());
        Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(original.Path));
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.True);
    }

    [Test]
    public async Task DeploymentNeverOverwritesAnExistingRealFolder()
    {
        AddMod();
        _settings.ServerPath = ServerDirectory();
        var existing = Path.Combine(_settings.ServerPath, "@Test_Mod");
        Directory.CreateDirectory(existing);
        var keep = Path.Combine(existing, "keep.txt");
        File.WriteAllText(keep, "keep");
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.False);
        Assert.That(File.ReadAllText(keep), Is.EqualTo("keep"));
    }

    [Test]
    public async Task MissingModFolderDoesNotCreateABrokenDeploymentLink()
    {
        var mod = AddMod();
        _settings.ServerPath = ServerDirectory();
        Directory.Delete(mod.Path, true);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.False);
        Assert.That(Directory.GetFileSystemEntries(_settings.ServerPath), Is.Empty);
    }
}
