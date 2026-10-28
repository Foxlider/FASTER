using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture, NonParallelizable]
public class LocalModImportTests
{
    private string _root = null!;
    private string? _previousSettingsPath;
    private IFilePickerService _previousFiles = null!;
    private IDialogService _previousDialogs = null!;
    private AppSettings _settings = null!;
    private Dialogs _dialogs = null!;

    private sealed class Picker(string? path) : IFilePickerService
    {
        public Task<string?> PickFolderAsync(string current) => Task.FromResult(path);
        public Task<string?> PickModPresetFileAsync() => throw new NotSupportedException();
        public Task<string?> PickServerExecutableAsync() => throw new NotSupportedException();
    }

    private sealed class Progress : IProgressDialog
    {
        public double Maximum { get => throw new InvalidOperationException("Progress controls must not be read by the import worker."); set => Total = value; }
        public double Total { get; private set; }
        public double Value { get; private set; }
        public bool Closed { get; private set; }
        public void SetMessage(string message) { }
        public void SetProgress(double value) => Value = value;
        public Task CloseAsync() { Closed = true; return Task.CompletedTask; }
    }

    private sealed class Dialogs : IDialogService
    {
        public Progress Progress { get; } = new();
        public int ProgressWindows { get; private set; }
        public List<string> Errors { get; } = new();
        public Action? OnShowProgress { get; set; }
        public Task<IProgressDialog> ShowProgressAsync(object context, string title, string message)
        {
            ProgressWindows++;
            OnShowProgress?.Invoke();
            return Task.FromResult<IProgressDialog>(Progress);
        }
        public Task ShowMessageAsync(object context, string title, string message)
        {
            Assert.That(ProgressWindows == 0 || Progress.Closed, Is.True, "Close progress before displaying an error.");
            Errors.Add(message);
            return Task.CompletedTask;
        }
        public Task<string?> ShowInputAsync(object context, string title, string message) => throw new NotSupportedException();
        public Task<bool> ShowConfirmationAsync(object context, string title, string message) => throw new NotSupportedException();
    }

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), $"faster-local-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        _previousSettingsPath = AppSettings.PathOverrideForTests;
        AppSettings.PathOverrideForTests = Path.Combine(_root, "faster.json");
        _settings = AppSettings.Current;
        _settings.Reset();
        _settings.ModStagingDirectory = Path.Combine(_root, "staging", "mods");
        _previousFiles = AppServices.Files;
        _previousDialogs = AppServices.Dialogs;
        _dialogs = new Dialogs();
        AppServices.Dialogs = _dialogs;
    }

    [TearDown]
    public void TearDown()
    {
        AppServices.Files = _previousFiles;
        AppServices.Dialogs = _previousDialogs;
        _settings.Reset();
        AppSettings.PathOverrideForTests = _previousSettingsPath;
        Directory.Delete(_root, true);
    }

    private string Source(string name)
    {
        var path = Path.Combine(_root, "sources", name);
        Directory.CreateDirectory(Path.Combine(path, "addons"));
        File.WriteAllBytes(Path.Combine(path, "addons", "local.pbo"), new byte[32]);
        return path;
    }

    private static async Task WaitForSizes(ModsViewModel vm)
    {
        for (var i = 0; i < 100 && vm.ModsCollection.ArmaMods.Any(m => m.Size == 0); i++)
            await Task.Delay(10);
        Assert.That(vm.ModsCollection.ArmaMods.All(m => m.Size == 32), Is.True);
    }

    [TestCase("Local mod", "Local mod")]
    [TestCase("@Local mod", "Local mod")]
    public async Task ImportCreatesStagingAndRegistersTheLocalFolder(string folder, string expectedName)
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var source = Source(folder);
        AppServices.Files = new Picker(source + Path.DirectorySeparatorChar);
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        await WaitForSizes(vm);
        var mod = vm.ModsCollection.ArmaMods.Single();
        Assert.Multiple(() =>
        {
            Assert.That(mod.Name, Is.EqualTo(expectedName));
            Assert.That(mod.IsLocal, Is.True);
            Assert.That(mod.Status, Is.EqualTo(ArmaModStatus.Local));
            Assert.That(Directory.ResolveLinkTarget(mod.Path, true)?.FullName, Is.EqualTo(source));
            Assert.That(_settings.ArmaMods, Is.SameAs(vm.ModsCollection));
            Assert.That(_dialogs.Progress.Value, Is.EqualTo(_dialogs.Progress.Total));
            Assert.That(_dialogs.Progress.Closed, Is.True);
            Assert.That(_dialogs.Errors, Is.Empty);
            Assert.That(vm.IsBusy, Is.False);
        });
    }

    [Test]
    public async Task BulkImportAddsEveryPrefixedModAndFinishesProgress()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        Source("@one");
        Source("@two");
        Source("unrelated");
        AppServices.Files = new Picker(Path.Combine(_root, "sources"));
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        await WaitForSizes(vm);
        Assert.That(vm.ModsCollection.ArmaMods.Select(m => m.Name), Is.EquivalentTo(new[] { "one", "two" }));
        Assert.That(_dialogs.Progress.Total, Is.EqualTo(2));
        Assert.That(_dialogs.Progress.Value, Is.EqualTo(2));
        Assert.That(_dialogs.Progress.Closed, Is.True);
        Assert.That(vm.IsBusy, Is.False);
    }

    [Test]
    public async Task StagingFailureClosesProgressAndAllowsAnotherImport()
    {
        var source = Source("@local");
        _settings.ModStagingDirectory = Path.Combine(_root, "staging-is-a-file");
        File.WriteAllText(_settings.ModStagingDirectory, "keep");
        AppServices.Files = new Picker(source);
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        Assert.That(_dialogs.Errors, Has.Count.EqualTo(1));
        Assert.That(_dialogs.Progress.Closed, Is.True);
        Assert.That(vm.ModsCollection.ArmaMods, Is.Empty);
        Assert.That(vm.IsBusy, Is.False);
        Assert.That(File.ReadAllText(_settings.ModStagingDirectory), Is.EqualTo("keep"));
        if (OperatingSystem.IsWindows()) return;
        _settings.ModStagingDirectory = Path.Combine(_root, "valid-staging");
        AppServices.Dialogs = _dialogs = new Dialogs();
        await vm.AddLocalModAsync();
        await WaitForSizes(vm);
        Assert.That(vm.ModsCollection.ArmaMods, Has.Count.EqualTo(1));
        Assert.That(_dialogs.Errors, Is.Empty);
        Assert.That(_dialogs.Progress.Closed, Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ImportRefusesDirectoryLoopsIncludingSymlinkAliases(bool alias)
    {
        if (alias && OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var source = Source("local");
        _settings.ModStagingDirectory = Path.Combine(source, "staging");
        if (alias)
        {
            var link = Path.Combine(_root, "source-alias");
            Directory.CreateSymbolicLink(link, source);
            source = link;
        }
        AppServices.Files = new Picker(source);
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        Assert.That(vm.ModsCollection.ArmaMods, Is.Empty);
        Assert.That(_dialogs.Errors.Single(), Does.Contain("directory loop"));
        Assert.That(_dialogs.Progress.Closed, Is.True);
        Assert.That(vm.IsBusy, Is.False);
    }

    [Test]
    public async Task DisappearingModDoesNotBlockTheRestOfTheBatch()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        var removed = Source("@removed");
        Source("@kept");
        _dialogs.OnShowProgress = () => Directory.Delete(removed, true);
        AppServices.Files = new Picker(Path.Combine(_root, "sources"));
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        await WaitForSizes(vm);
        Assert.That(vm.ModsCollection.ArmaMods.Single().Name, Is.EqualTo("kept"));
        Assert.That(_dialogs.Errors.Single(), Does.Contain("removed"));
        Assert.That(_dialogs.Progress.Value, Is.EqualTo(2));
        Assert.That(_dialogs.Progress.Closed, Is.True);
    }

    [Test]
    public async Task CancellingThePickerDoesNotOpenProgress()
    {
        AppServices.Files = new Picker(null);
        var vm = new ModsViewModel();
        await vm.AddLocalModAsync();
        Assert.That(_dialogs.ProgressWindows, Is.Zero);
        Assert.That(_dialogs.Errors, Is.Empty);
        Assert.That(vm.IsBusy, Is.False);
    }
}
