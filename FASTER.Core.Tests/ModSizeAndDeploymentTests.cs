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

    private sealed class FolderPicker(string? folder, string? preset = null) : IFilePickerService
    {
        public int Calls { get; private set; }
        public Task<string?> PickFolderAsync(string current)
        {
            Calls++;
            return Task.FromResult(folder);
        }
        public Task<string?> PickModPresetFileAsync() => Task.FromResult(preset);
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
        File.WriteAllBytes(Path.Combine(folder, "license"), new byte[64]);
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

    [Test]
    public void NonLatinFolderNamesAreStableAndUseTheModId()
    {
        Assert.That(ModDeployment.FolderName("Догечня 0.55", 3125798174), Is.EqualTo("@mod_3125798174"));
        Assert.That(ModDeployment.FolderName("Новое название", 3125798174), Is.EqualTo("@mod_3125798174"));
        Assert.That(ModDeployment.FolderName("Другой мод 0.55", 42), Is.EqualTo("@mod_42"));
        Assert.That(ModDeployment.FolderName("CBA_A3", 450814997), Is.EqualTo(OperatingSystem.IsLinux() ? "@cba_a3" : "@CBA_A3"));
        Assert.That(ModDeployment.FolderName("Догечня", 0), Is.Not.EqualTo(ModDeployment.FolderName("Другой мод", 0)));
    }

    [Test]
    public void LegacyCleanupPreservesUnrelatedLinksAndRealDirectories()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux symlink cleanup."); return; }
        var mod = AddMod();
        mod.Name = "Догечня 0.55";
        var install = ServerDirectory();
        var legacy = Path.Combine(install, "@" + Functions.SafeName(mod.Name));
        var other = Path.Combine(_root, "other-mod");
        Directory.CreateDirectory(other);
        Directory.CreateSymbolicLink(legacy, other);
        ModDeployment.RemoveLegacyLink(install, mod.Path, mod.Name, mod.WorkshopId);
        Assert.That(Directory.ResolveLinkTarget(legacy, true)?.FullName, Is.EqualTo(other));
        Directory.Delete(legacy);
        Directory.CreateDirectory(legacy);
        File.WriteAllText(Path.Combine(legacy, "keep"), "keep");
        ModDeployment.RemoveLegacyLink(install, mod.Path, mod.Name, mod.WorkshopId);
        Assert.That(File.ReadAllText(Path.Combine(legacy, "keep")), Is.EqualTo("keep"));
    }

    [Test]
    public async Task CyrillicModDeploysLowercaseFilesWithoutChangingTheDownload()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux lowercase deployment."); return; }
        var original = AddMod();
        original.WorkshopId = 3125798174;
        original.Name = "Догечня 0.55";
        var addons = Path.Combine(original.Path, "addons");
        var upperFile = Path.Combine(addons, "Dogechnya_cfg.pbo");
        File.WriteAllText(upperFile, "config contents");
        _settings.ServerPath = ServerDirectory();
        var legacy = Path.Combine(_settings.ServerPath, "@" + Functions.SafeName(original.Name));
        Directory.CreateSymbolicLink(legacy, original.Path);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();

        var link = Path.Combine(_settings.ServerPath, "@mod_3125798174");
        Assert.That(File.ReadAllText(Path.Combine(link, "addons", "dogechnya_cfg.pbo")), Is.EqualTo("config contents"));
        Assert.That(File.Exists(upperFile), Is.True);
        Assert.That(File.Exists(Path.Combine(addons, "dogechnya_cfg.pbo")), Is.False);
        Assert.That(Directory.Exists(legacy), Is.False);
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.True);

        var profile = new ServerProfile("Cyrillic mod", createFolder: false)
        {
            ProfileMods = [new ProfileMod { Id = original.WorkshopId, Name = original.Name,
                ClientSideChecked = true, ServerSideChecked = true, HeadlessChecked = true }]
        };
        Assert.That(profile.CommandLine, Does.Contain("-mod=@mod_3125798174").And.Contain("-serverMod=@mod_3125798174"));
        var headless = typeof(ProfileViewModel).GetMethod("SetHCCommandLine",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(new ProfileViewModel(profile), [1]);
        Assert.That(headless, Does.Contain("-mod=@mod_3125798174"));

        File.Delete(upperFile);
        File.WriteAllText(Path.Combine(addons, "NewFile.PBO"), "updated contents");
        ModDeployment.RefreshDeployed(original);
        Assert.That(File.Exists(Path.Combine(link, "addons", "dogechnya_cfg.pbo")), Is.False);
        Assert.That(File.ReadAllText(Path.Combine(link, "addons", "newfile.pbo")), Is.EqualTo("updated contents"));
        await vm.DeployModAsync(vm.Deployment.DeployMods.Single());
        Assert.That(Directory.Exists(link), Is.False);
        Assert.That(File.ReadAllText(Path.Combine(addons, "NewFile.PBO")), Is.EqualTo("updated contents"));
    }

    [Test]
    public async Task LinuxDeploymentLowercasesDirectoriesAndRefreshesDirectLinksAfterUpdates()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux lowercase deployment."); return; }
        var mod = AddMod();
        _settings.ServerPath = ServerDirectory();
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        var link = Path.Combine(_settings.ServerPath, ModDeployment.FolderName("Test Mod", 42));
        Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(mod.Path));
        Directory.CreateDirectory(Path.Combine(mod.Path, "Optional", "Addons"));
        File.WriteAllText(Path.Combine(mod.Path, "Optional", "Addons", "Extra.PBO"), "extra");
        ModDeployment.RefreshDeployed(mod);
        Assert.That(File.ReadAllText(Path.Combine(link, "optional", "addons", "extra.pbo")), Is.EqualTo("extra"));
        Assert.That(File.Exists(Path.Combine(mod.Path, "Optional", "Addons", "Extra.PBO")), Is.True);
        vm.ClearAll();
        Assert.That(Directory.Exists(link), Is.False);
        Assert.That(Directory.Exists(mod.Path), Is.True);
    }

    [Test]
    public async Task CaseCollisionsKeepBothSourceFilesAndTheExistingDeployment()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Case-sensitive Linux paths."); return; }
        var mod = AddMod();
        var upper = Path.Combine(mod.Path, "addons", "TEST.pbo");
        File.WriteAllText(upper, "different file");
        _settings.ServerPath = ServerDirectory();
        var link = Path.Combine(_settings.ServerPath, ModDeployment.FolderName("Test Mod", 42));
        Directory.CreateSymbolicLink(link, mod.Path);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.False);
        Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(mod.Path));
        Assert.That(File.ReadAllText(upper), Is.EqualTo("different file"));
        Assert.That(new FileInfo(Path.Combine(mod.Path, "addons", "test.pbo")).Length, Is.EqualTo(2048));
    }

    [Test]
    public async Task LowercaseDeploymentRefusesToReplaceAnUnmanagedCacheDirectory()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux lowercase deployment."); return; }
        var mod = AddMod();
        File.WriteAllText(Path.Combine(mod.Path, "UPPER"), "source");
        _settings.ServerPath = ServerDirectory();
        var existing = Path.Combine(_settings.ServerPath, ".faster-linux-mods", "42");
        Directory.CreateDirectory(existing);
        File.WriteAllText(Path.Combine(existing, "keep"), "keep");
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        Assert.That(vm.Deployment.DeployMods.Single().Marked, Is.False);
        Assert.That(File.ReadAllText(Path.Combine(existing, "keep")), Is.EqualTo("keep"));
    }

    [Test]
    public void LaunchPreparationMigratesLegacyViewsAndIncludesNewFilesFromTheReportedMod()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux mod paths."); return; }
        var mod = AddMod();
        mod.Name = "MMM CSO";
        string[] paths = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "TestData", "LinuxMixedCaseModPaths.txt"));
        Assert.That(paths, Has.Length.GreaterThan(80));
        foreach (string relative in paths)
        {
            string file = Path.Combine(mod.Path, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, relative);
        }
        var server = ServerDirectory();
        // Simulate a view/link made by the previous version, then a manual update.
        string view = ModDeployment.PrepareSource(mod.Path, server, mod.Name, mod.WorkshopId);
        string legacy = Path.Combine(server, "@MMM_CSO");
        Directory.CreateSymbolicLink(legacy, view);
        File.WriteAllText(Path.Combine(mod.Path, "addons", "NewAfterDeployment.PBO"), "updated");
        ModDeployment.PrepareForLaunch(server, mod.Name, mod.WorkshopId, mod.Path);
        string deployed = Path.Combine(server, "@mmm_cso");
        Assert.That(Directory.Exists(legacy), Is.False);
        foreach (string relative in paths)
        {
            Assert.That(File.ReadAllText(Path.Combine(deployed, relative.ToLowerInvariant())), Is.EqualTo(relative));
            Assert.That(File.Exists(Path.Combine(mod.Path, relative)), Is.True);
        }
        Assert.That(File.ReadAllText(Path.Combine(deployed, "addons", "newafterdeployment.pbo")), Is.EqualTo("updated"));
        Assert.That(Directory.EnumerateFileSystemEntries(deployed, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(deployed, path)), Is.All.Matches<string>(path => path == path.ToLowerInvariant()));
        ModDeployment.PrepareForLaunch(server, mod.Name, mod.WorkshopId, mod.Path);
        Assert.That(File.ReadAllText(Path.Combine(deployed, "addons", "newafterdeployment.pbo")), Is.EqualTo("updated"));
    }

    [Test]
    public async Task ServerLaunchRepairsLegacyLinksBeforeStartingARealProcess()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux launch fixture."); return; }
        var mod = AddMod();
        mod.Name = "MMM CSO";
        File.WriteAllText(Path.Combine(mod.Path, "addons", "MMM_CSO_Vegetation.pbo"), "vegetation");
        string server = ServerDirectory();
        Directory.CreateSymbolicLink(Path.Combine(server, "@MMM_CSO"), mod.Path);
        string executable = Path.Combine(server, "arma3server_x64");
        File.WriteAllText(executable, """
            #!/bin/sh
            set -eu
            export LC_ALL=C
            for argument in "$@"; do
              case "$argument" in
                -mod=*|-serverMod=*)
                  list=${argument#*=}
                  oldIFS=$IFS
                  IFS=';'
                  for mod in $list; do
                    [ -z "$mod" ] && continue
                    case "$mod" in *[A-Z]*) echo "Uppercase mod argument: $mod"; exit 21;; esac
                    [ -r "$mod/addons/mmm_cso_vegetation.pbo" ] || { echo "Missing lowercase file in $mod"; exit 22; }
                    if find -L "$mod" -mindepth 1 -printf '%P\n' | LC_ALL=C grep '[A-Z]'; then exit 23; fi
                  done
                  IFS=$oldIFS
                  ;;
              esac
            done
            echo LINUX-MODS-OK
            sleep 0.2
            """ + "\n");
        File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        var profile = new ServerProfile("Linux fixture", createFolder: false)
        {
            Executable = executable,
            ProfileMods = [new ProfileMod { Id = mod.WorkshopId, Name = mod.Name,
                ClientSideChecked = true, ServerSideChecked = true, HeadlessChecked = true }]
        };
        var vm = new ProfileViewModel(profile);
        vm.SaveProfile();
        using var monitor = new ProcessMonitor(outputDirectory: Path.Combine(_root, "output"));
        var previousProcesses = AppServices.Processes;
        AppServices.Processes = monitor;
        try
        {
            await vm.LaunchServer();
            for (int i = 0; i < 100 && !monitor.Sample().Any(p => p.Exited); i++) await Task.Delay(20);
            var launched = monitor.Sample().Single();
            Assert.That(monitor.ReadOutput(launched.Identity), Does.Contain("LINUX-MODS-OK"));
            Assert.That(profile.CommandLine, Does.Contain("-mod=@mmm_cso").And.Contain("-serverMod=@mmm_cso"));
            string headless = (string)typeof(ProfileViewModel).GetMethod("SetHCCommandLine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(vm, [1])!;
            Assert.That(headless, Does.Contain("-mod=@mmm_cso"));
            Assert.That(File.Exists(Path.Combine(mod.Path, "addons", "MMM_CSO_Vegetation.pbo")), Is.True);

            // An external update after launch must be incorporated on the next launch.
            File.WriteAllText(Path.Combine(mod.Path, "addons", "Another.PBO"), "another");
            await vm.LaunchServer();
            Assert.That(File.ReadAllText(Path.Combine(server, "@mmm_cso", "addons", "another.pbo")), Is.EqualTo("another"));
        }
        finally { AppServices.Processes = previousProcesses; }
    }

    [Test]
    public async Task LaunchStopsWhenCaseCollisionsPreventSafePreparation()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux case collision."); return; }
        var mod = AddMod();
        File.WriteAllText(Path.Combine(mod.Path, "addons", "TEST.pbo"), "collision");
        string server = ServerDirectory();
        Directory.CreateSymbolicLink(Path.Combine(server, "@Test_Mod"), mod.Path);
        string executable = Path.Combine(server, "arma3server_x64");
        File.WriteAllText(executable, "#!/bin/sh\necho SHOULD-NOT-LAUNCH\n");
        File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        var profile = new ServerProfile("Collision", createFolder: false)
        {
            Executable = executable,
            ProfileMods = [new ProfileMod { Id = mod.WorkshopId, Name = mod.Name, ClientSideChecked = true }]
        };
        var vm = new ProfileViewModel(profile);
        vm.SaveProfile();
        using var monitor = new ProcessMonitor(outputDirectory: Path.Combine(_root, "output"));
        var previousProcesses = AppServices.Processes;
        AppServices.Processes = monitor;
        try
        {
            await vm.LaunchServer();
            Assert.That(monitor.Sample(), Is.Empty);
            Assert.That(Directory.ResolveLinkTarget(Path.Combine(server, "@Test_Mod"), true)?.FullName, Is.EqualTo(mod.Path));
            Assert.That(File.ReadAllText(Path.Combine(mod.Path, "addons", "TEST.pbo")), Is.EqualTo("collision"));
        }
        finally { AppServices.Processes = previousProcesses; }
    }

    [Test]
    public void LaunchPreparationCanUseAnUnregisteredLegacyDirectLink()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux legacy link."); return; }
        var mod = AddMod();
        File.WriteAllText(Path.Combine(mod.Path, "addons", "Mixed.PBO"), "mixed");
        string server = ServerDirectory();
        Directory.CreateSymbolicLink(Path.Combine(server, "@Test_Mod"), mod.Path);
        ModDeployment.PrepareForLaunch(server, mod.Name, mod.WorkshopId, null);
        Assert.That(File.ReadAllText(Path.Combine(server, "@test_mod", "addons", "mixed.pbo")), Is.EqualTo("mixed"));
        Assert.That(Directory.Exists(Path.Combine(server, "@Test_Mod")), Is.False);
    }

    [Test]
    public async Task LinuxFolderNameCollisionsStopDeployAllBeforeReplacingAnyLinks()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux folder names."); return; }
        var mod = AddMod();
        _settings.ArmaMods!.ArmaMods.Add(new ArmaMod { WorkshopId = 43, Name = "test mod", Path = mod.Path });
        string server = ServerDirectory();
        _settings.ServerPath = server;
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        await vm.DeployAllAsync();
        Assert.That(vm.Deployment.DeployMods.All(m => !m.Marked), Is.True);
        Assert.That(Directory.GetFileSystemEntries(server), Is.Empty);
        Assert.That(File.Exists(Path.Combine(mod.Path, "addons", "test.pbo")), Is.True);
    }

    [Test]
    public void LowercaseRealDeploymentFoldersArePreservedAndUnsafeOnesAreReported()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux real mod folders."); return; }
        string server = ServerDirectory();
        string folder = Path.Combine(server, "@test_mod");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "lower.pbo"), "lower");
        Assert.DoesNotThrow(() => ModDeployment.PrepareForLaunch(server, "Test Mod", 42, null));
        Assert.That(new DirectoryInfo(folder).LinkTarget, Is.Null);
        File.WriteAllText(Path.Combine(folder, "UPPER.PBO"), "upper");
        Assert.Throws<IOException>(() => ModDeployment.PrepareForLaunch(server, "Test Mod", 42, null));
        Assert.That(File.ReadAllText(Path.Combine(folder, "UPPER.PBO")), Is.EqualTo("upper"));
        Assert.That(new DirectoryInfo(folder).LinkTarget, Is.Null);
    }

    [Test]
    public async Task UndeployAndCleanAlsoRemoveOwnedLegacyMixedCaseLinks()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux legacy migration."); return; }
        var mod = AddMod();
        string server = ServerDirectory();
        _settings.Deployments!.InstallPath = server;
        string legacy = Path.Combine(server, "@Test_Mod");
        Directory.CreateSymbolicLink(legacy, mod.Path);
        var vm = new DeploymentViewModel();
        await vm.LoadDataAsync();
        var deployed = vm.Deployment.DeployMods.Single();
        deployed.Marked = true;
        await vm.DeployModAsync(deployed);
        Assert.That(Directory.Exists(legacy), Is.False);
        Directory.CreateSymbolicLink(legacy, mod.Path);
        deployed.Marked = true;
        vm.ClearAll();
        Assert.That(Directory.Exists(legacy), Is.False);
        Assert.That(File.Exists(Path.Combine(mod.Path, "addons", "test.pbo")), Is.True);
    }

    [Test]
    public void PreparingAViewFromItselfCannotCreateDanglingFileLinks()
    {
        if (!OperatingSystem.IsLinux()) { Assert.Ignore("Linux view safety."); return; }
        var mod = AddMod();
        File.WriteAllText(Path.Combine(mod.Path, "Mixed.PBO"), "mixed");
        string server = ServerDirectory();
        string view = ModDeployment.PrepareSource(mod.Path, server, mod.Name, mod.WorkshopId);
        Assert.Throws<IOException>(() => ModDeployment.PrepareSource(view, server, mod.Name, mod.WorkshopId));
        Assert.That(File.ReadAllText(Path.Combine(view, "mixed.pbo")), Is.EqualTo("mixed"));
    }

    [Test]
    public void ReplacingProfileModsPublishesTheNewFilteredListToBindings()
    {
        var profile = new ServerProfile("CSO Main", createFolder: false);
        Assert.That(profile.FilteredProfileMods, Is.Empty);
        List<ProfileMod>? observed = null;
        profile.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ServerProfile.FilteredProfileMods))
                observed = profile.FilteredProfileMods;
        };
        var mod = new ProfileMod { Id = 42, Name = "CBA_A3" };
        profile.ProfileMods = [mod];
        Assert.That(observed, Is.EqualTo(new[] { mod }));
    }

    [Test]
    public async Task PresetImportRefreshesTheProfileAndPreservesOtherModRoles()
    {
        var preset = Path.Combine(AppContext.BaseDirectory, "TestData", "MMM_CSO_20260922.html");
        var mods = ModUtilities.ParseModsFromArmaProfileFile(preset);
        Assert.That(mods.Count, Is.EqualTo(16));
        Assert.That(mods[0].WorkshopId, Is.EqualTo(450814997u));
        foreach (var mod in mods)
            _settings.ArmaMods!.ArmaMods.Add(mod);
        AppServices.Files = new FolderPicker(null, preset);
        var profile = new ServerProfile("CSO Main", createFolder: false);
        var existing = new ProfileMod { Id = mods[0].WorkshopId, Name = "old name",
            ServerSideChecked = true, HeadlessChecked = true, OptChecked = true };
        profile.ProfileMods = [existing];

        await new ProfileViewModel(profile).LoadModsFromFile();

        Assert.Multiple(() =>
        {
            Assert.That(profile.ProfileMods.Count, Is.EqualTo(16));
            Assert.That(profile.ClientModsChecked, Is.EqualTo(16));
            Assert.That(profile.ProfileMods.OrderBy(m => m.LoadPriority).Select(m => m.Id),
                Is.EqualTo(mods.Select(m => m.WorkshopId)));
            Assert.That(profile.ProfileMods[0], Is.SameAs(existing));
            Assert.That(existing.ServerSideChecked && existing.HeadlessChecked && existing.OptChecked, Is.True);
            Assert.That(existing.Name, Is.EqualTo("CBA_A3"));
        });
    }

    [Test]
    public async Task PresetImportStillSelectsAvailableModsWhenOneIsUnregistered()
    {
        var preset = Path.Combine(AppContext.BaseDirectory, "TestData", "MMM_CSO_20260922.html");
        var mods = ModUtilities.ParseModsFromArmaProfileFile(preset);
        foreach (var mod in mods.Skip(1))
            _settings.ArmaMods!.ArmaMods.Add(mod);
        AppServices.Files = new FolderPicker(null, preset);
        var profile = new ServerProfile("CSO Main", createFolder: false);

        await new ProfileViewModel(profile).LoadModsFromFile();

        Assert.That(profile.ClientModsChecked, Is.EqualTo(15));
        Assert.That(profile.ProfileMods.Select(m => m.Id), Is.EquivalentTo(mods.Skip(1).Select(m => m.WorkshopId)));
        Assert.That(profile.ProfileMods.Any(m => m.Id == mods[0].WorkshopId), Is.False);
    }

    [Test]
    public async Task CancellingPresetImportKeepsExistingSelections()
    {
        AppServices.Files = new FolderPicker(null);
        var selected = new ProfileMod { Id = 42, Name = "keep", ClientSideChecked = true, LoadPriority = 5 };
        var profile = new ServerProfile("CSO Main", createFolder: false) { ProfileMods = [selected] };
        await new ProfileViewModel(profile).LoadModsFromFile();
        Assert.That(profile.ProfileMods.Single(), Is.SameAs(selected));
        Assert.That(selected.ClientSideChecked, Is.True);
        Assert.That(selected.LoadPriority, Is.EqualTo(5));
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
        var target = Directory.ResolveLinkTarget(Path.Combine(server, ModDeployment.FolderName("Test Mod", 42)), true);
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
        var link = Path.Combine(_settings.ServerPath, ModDeployment.FolderName("Test Mod", 42));
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
        var link = Path.Combine(server, ModDeployment.FolderName("Test Mod", 42));
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
        var existing = Path.Combine(_settings.ServerPath, ModDeployment.FolderName("Test Mod", 42));
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
