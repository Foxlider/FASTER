using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FASTER.Avalonia.Views;
using FASTER.Avalonia.Services;
using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;
using NUnit.Framework;

[assembly: AvaloniaTestApplication(typeof(FASTER.Avalonia.Tests.TestAppBuilder))]

namespace FASTER.Avalonia.Tests;

public static class TestAppBuilder
{
    public static string Root { get; } = Path.Combine(Path.GetTempPath(), $"faster-view-tests-{Guid.NewGuid():N}");

    public static AppBuilder BuildAvaloniaApp()
    {
        Directory.CreateDirectory(Root);
        AppSettings.PathOverrideForTests = Path.Combine(Root, "faster.json");
        return AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}

[TestFixture, NonParallelizable]
public class ModViewsTests
{
    private Window? _window;
    private string _root = null!;
    private ArmaMod _mod = null!;
    private IFilePickerService _previousFiles = null!;
    private IDialogService _previousDialogs = null!;

    private sealed class LocalModDialogs(Window owner) : IDialogService
    {
        public ProgressDialog? Progress { get; private set; }
        public List<string> Errors { get; } = new();
        public Task<IProgressDialog> ShowProgressAsync(object context, string title, string message)
        {
            Progress = new ProgressDialog { Title = title };
            Progress.SetMessage(message);
            Progress.Show(owner);
            return Task.FromResult<IProgressDialog>(Progress);
        }
        public Task ShowMessageAsync(object context, string title, string message) { Errors.Add(message); return Task.CompletedTask; }
        public Task<string?> ShowInputAsync(object context, string title, string message) => throw new NotSupportedException();
        public Task<bool> ShowConfirmationAsync(object context, string title, string message) => throw new NotSupportedException();
    }

    private sealed class Picker(string? path, bool preset = false) : IFilePickerService
    {
        public int Calls { get; private set; }
        public Task<string?> PickFolderAsync(string current) { Calls++; return Task.FromResult(path); }
        public Task<string?> PickServerExecutableAsync() => throw new NotSupportedException();
        public Task<string?> PickModPresetFileAsync() => preset ? Task.FromResult(path) : throw new NotSupportedException();
    }

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(TestAppBuilder.Root, Guid.NewGuid().ToString("N"));
        var folder = Path.Combine(_root, "mod");
        Directory.CreateDirectory(folder);
        AppSettings.PathOverrideForTests = Path.Combine(TestAppBuilder.Root, "faster.json");
        File.WriteAllBytes(Path.Combine(folder, "mod.pbo"), new byte[2048]);
        _mod = new ArmaMod { WorkshopId = 42, Path = folder, Name = "Test Mod", Status = ArmaModStatus.UpToDate };
        var settings = AppSettings.Current;
        settings.ServerPath = string.Empty;
        settings.ArmaMods = new ArmaModCollection();
        settings.ArmaMods.ArmaMods.Add(_mod);
        settings.Deployments = new ArmaDeployment { InstallPath = string.Empty };
        settings.CheckForModUpdates = false;
        _previousFiles = AppServices.Files;
        _previousDialogs = AppServices.Dialogs;
    }

    [TearDown]
    public void TearDown()
    {
        AppServices.Files = _previousFiles;
        AppServices.Dialogs = _previousDialogs;
        Directory.Delete(_root, true);
    }

    [OneTimeTearDown]
    public void RemoveSettings()
    {
        AppSettings.PathOverrideForTests = null;
        Directory.Delete(TestAppBuilder.Root, true);
    }

    [AvaloniaTest]
    public async Task LocalModImportClosesTheRealProgressWindow()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Requires Unix symlink permissions.");
        AppSettings.Current.ArmaMods!.ArmaMods.Clear();
        AppSettings.Current.ModStagingDirectory = Path.Combine(_root, "staging");
        Directory.CreateDirectory(AppSettings.Current.ModStagingDirectory);
        AppServices.Files = new Picker(_mod.Path);
        var vm = new ModsViewModel();
        _window = new Window { Width = 1100, Height = 700, Content = new ModsView { DataContext = vm } };
        _window.Show();
        var dialogs = new LocalModDialogs(_window);
        AppServices.Dialogs = dialogs;
        try
        {
            await Until(() => _window.GetVisualDescendants().OfType<Button>().Any(b => Equals(b.GetValue(ToolTip.TipProperty), "Add Local Mod")));
            Click(_window.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.GetValue(ToolTip.TipProperty), "Add Local Mod")));
            await Until(() => dialogs.Progress is { IsVisible: false } && vm.ModsCollection.ArmaMods.Count == 1 &&
                vm.ModsCollection.ArmaMods.Single().Size == 2048);
            Assert.That(dialogs.Errors, Is.Empty);
            Assert.That(vm.IsBusy, Is.False);
            Assert.That(vm.ModsCollection.ArmaMods.Single().Name, Is.EqualTo("mod"));
        }
        finally
        {
            dialogs.Progress?.Close();
            _window.Close();
        }
    }

    [AvaloniaTest]
    public async Task ProfileLoadsRegisteredModsAndImportsTheMmmPresetFromTheButton()
    {
        var preset = Path.Combine(AppContext.BaseDirectory, "TestData", "MMM_CSO_20260922.html");
        var mods = ModUtilities.ParseModsFromArmaProfileFile(preset);
        Assert.That(mods.Count, Is.EqualTo(16));
        AppSettings.Current.ArmaMods!.ArmaMods.Clear();
        foreach (var mod in mods)
            AppSettings.Current.ArmaMods.ArmaMods.Add(mod);
        AppServices.Files = new Picker(preset, preset: true);
        var profile = new ServerProfile("CSO Main", createFolder: false);
        _window = new Window { Width = 1180, Height = 760,
            Content = new ProfileView { DataContext = new ProfileViewModel(profile) } };
        _window.Show();
        try
        {
            await Until(() => profile.ProfileMods.Count == 16 &&
                _window.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "CBA_A3"));
            var import = _window.GetVisualDescendants().OfType<Button>().Single(b =>
                b.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "Load from File"));
            Click(import);
            await Until(() => profile.ClientModsChecked == 16);
            Assert.That(profile.ProfileMods.OrderBy(m => m.LoadPriority).Select(m => m.Id),
                Is.EqualTo(mods.Select(m => m.WorkshopId)));
            Assert.That(profile.CommandLine, Does.Contain("@CBA_A3").And.Contain("@ace"));
        }
        finally { _window.Close(); }
    }

    [AvaloniaTest]
    public async Task BothPagesDisplaySizesWithoutWritingZeroBackToTheModel()
    {
        _window = new Window { Width = 1100, Height = 700,
            Content = new ModsView { DataContext = new ModsViewModel() } };
        _window.Show();
        try
        {
            await Until(() => HasSizeCell() && _mod.Size == 2048);
            // Changing the underlying size must not feed the formatted string back into Size.
            _mod.Size = 4096;
            var formattedSize = 4d.ToString("F", System.Globalization.CultureInfo.CurrentCulture);
            await Until(() => _window.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text?.Contains(formattedSize) == true));
            Assert.That(_mod.Size, Is.EqualTo(4096));

            var deployment = new DeploymentViewModel();
            _window.Content = new DeploymentView { DataContext = deployment };
            await Until(() => HasSizeCell() && deployment.Deployment.DeployMods.Count == 1);
            Assert.That(deployment.Deployment.DeployMods.Single().Size, Is.EqualTo(2048));
            Assert.That(_mod.Size, Is.EqualTo(2048));
        }
        finally { _window.Close(); }
    }

    [AvaloniaTest]
    public async Task CheckboxAndDeployAllPickADirectoryAndUpdateTheActualLinks()
    {
        if (OperatingSystem.IsWindows()) Assert.Ignore("Creating symlinks on Windows requires Developer Mode or elevation.");
        var server = Path.Combine(_root, "server");
        Directory.CreateDirectory(server);
        var deployment = new DeploymentViewModel();
        _window = new Window { Width = 1100, Height = 700,
            Content = new DeploymentView { DataContext = deployment } };
        _window.Show();
        try
        {
            await Until(() => _window.GetVisualDescendants().OfType<CheckBox>().Any(c => c.DataContext is DeploymentMod));
            var checkbox = _window.GetVisualDescendants().OfType<CheckBox>().Single(c => c.DataContext is DeploymentMod);
            var picker = new Picker(server);
            AppServices.Files = picker;
            var mod = deployment.Deployment.DeployMods.Single();
            var link = Path.Combine(server, "@Test_Mod");

            Assert.That(checkbox.IsEnabled, Is.True);
            Click(checkbox);
            await Until(() => mod.Marked && checkbox.IsChecked == true);
            Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(_mod.Path));
            Assert.That(picker.Calls, Is.EqualTo(1));

            Click(checkbox);
            await Until(() => !mod.Marked && checkbox.IsChecked == false);
            Assert.That(Directory.Exists(link), Is.False);
            Assert.That(Directory.Exists(_mod.Path), Is.True);

            deployment.Deployment.InstallPath = string.Empty;
            var cancel = new Picker(null);
            AppServices.Files = cancel;
            Click(checkbox);
            await Until(() => cancel.Calls == 1 && checkbox.IsChecked == false);
            Assert.That(mod.Marked, Is.False);

            AppServices.Files = new Picker(server);
            Click(_window.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.Content, "Deploy All")));
            await Until(() => mod.Marked && checkbox.IsChecked == true);
            Assert.That(Directory.ResolveLinkTarget(link, true)?.FullName, Is.EqualTo(_mod.Path));
        }
        finally { _window.Close(); }
    }

    private bool HasSizeCell() => _window!.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text?.Contains("KB") == true);

    private void Click(Control control)
    {
        var window = _window ?? throw new InvalidOperationException("No test window is open.");
        var point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
    }

    private static async Task Until(Func<bool> condition)
    {
        for (var i = 0; i < 100; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            if (condition()) return;
            await Task.Delay(20);
        }
        Assert.Fail("Timed out waiting for the rendered UI state.");
    }
}
