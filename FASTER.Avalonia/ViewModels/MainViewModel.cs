using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Avalonia.Controls;
using Avalonia.Threading;

using FASTER.Avalonia.Services;
using FASTER.Avalonia.Views;
using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;

namespace FASTER.Avalonia.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private object? _currentView;
    private string _statusMessage = string.Empty;
    private bool _isMessageExpanded;

    private bool _startupChecksRan;
    public IApplicationUpdates Updates { get; } = new ApplicationUpdates(new VelopackUpdates(),
        () => SteamUpdaterViewModel.Instance.IsDownloading || App.Main.Mods.IsBusy);

    public ModsViewModel Mods { get; } = new();
    public DeploymentViewModel Deployment { get; } = new();
    public SteamUpdaterViewModel Updater => SteamUpdaterViewModel.Instance;
    public ObservableCollection<ProfileViewModel> Profiles { get; } = new();

    public UpdaterView UpdaterView { get; } = new();
    public ModsView ModsView { get; } = new();
    public DeploymentView DeploymentView { get; } = new();
    public ServerStatusView ServerStatusView { get; } = new();
    public SettingsView SettingsView { get; } = new();
    public AboutView AboutView { get; } = new();
    public SetupView SetupView { get; } = new();

    public object? CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsMessageExpanded
    {
        get => _isMessageExpanded;
        set { _isMessageExpanded = value; OnPropertyChanged(); }
    }

    public void ToggleMessageExpanded() => IsMessageExpanded = !IsMessageExpanded;

    public MainViewModel()
    {
        UpdaterView.DataContext = Updater;
        ModsView.DataContext = Mods;
        DeploymentView.DataContext = Deployment;
        ServerStatusView.DataContext = this;
        SettingsView.DataContext = this;
        AboutView.DataContext = this;
        SetupView.DataContext = this;
        CurrentView = SetupView;
    }

    public void Attach(Window window)
    {
        var bridge = new AvaUiBridge(this);
        Ui.Current = bridge;
        AppServices.Dialogs = new AvaDialogService(window);
        AppServices.Clipboard = new AvaClipboardService(window);
        AppServices.Files = new AvaFilePickerService(window);
        LoadServerProfiles();
        bridge.MarkAttached();

        if (!AppSettings.Current.FirstRun)
            ShowUpdater();
    }

    public async System.Threading.Tasks.Task RunStartupChecksAsync()
    {
        if (_startupChecksRan || AppSettings.Current.FirstRun) return;
        _startupChecksRan = true;
        await Telemetry.SetEnabledAsync(AppSettings.Current.EnableAnalytics);
        try
        {
            if (AppSettings.Current.CheckForModUpdates) await Mods.CheckForUpdates();
        }
        catch (Exception ex) { ShowStatus("Mod update check failed: " + ex.Message); }
        if (AppSettings.Current.CheckForAppUpdates) await CheckApplicationUpdatesAsync();
    }

    public async System.Threading.Tasks.Task CheckApplicationUpdatesAsync()
    {
        var result = await Updates.CheckAsync();
        ShowStatus(Updates.Status);
        if (result == UpdateResult.Unpackaged && await AppServices.Dialogs.ShowConfirmationAsync(this,
            "Application updates", "This is an unpackaged installation. Open the releases page?"))
        {
            try { Platform.Current.OpenUrl(VelopackUpdates.ReleasePage); }
            catch (Exception ex) { ShowStatus("Could not open releases: " + ex.Message); }
        }
        if (result == UpdateResult.Ready && await AppServices.Dialogs.ShowConfirmationAsync(this,
            "Update ready", "Restart FASTER to apply the downloaded update? Running servers will remain running."))
        {
            Updates.TryRestart();
            ShowStatus(Updates.Status);
        }
    }

    public void ShowUpdater() => CurrentView = UpdaterView;
    public void ShowMods() => CurrentView = ModsView;
    public void ShowDeployment() => CurrentView = DeploymentView;
    public void ShowServerStatus() => CurrentView = ServerStatusView;
    public void ShowSettings() => CurrentView = SettingsView;
    public void ShowAbout() => CurrentView = AboutView;

    public void ShowStatus(string message)
        => Dispatcher.UIThread.Post(() =>
        {
            StatusMessage = message;
        });

    public void AppendUpdaterOutput(string text)
        => Dispatcher.UIThread.Post(() => Updater.Parameters.Output += text);

    public void LoadServerProfiles()
    {
        if (AppSettings.Current.Profiles == null)
        {
            AppSettings.Current.Profiles = new ServerProfileCollection();
            AppSettings.Current.Save();
        }

        Dispatcher.UIThread.Post(() =>
        {
            Profiles.Clear();
            foreach (var profile in AppSettings.Current.Profiles ?? Enumerable.Empty<ServerProfile>())
            {
                if (Profiles.Any(p => p.Profile.Id == profile.Id))
                    continue;
                Profiles.Add(new ProfileViewModel(profile));
            }
        });
    }

    public void AddProfile(string name)
    {
        ServerProfileCollection.AddServerProfile(name);
        LoadServerProfiles();
    }

    public void CloneProfile(ProfileViewModel source)
    {
        var clone = source.Profile.Clone();
        ServerProfileCollection.AddServerProfile(clone);
        LoadServerProfiles();
    }

    public void DeleteProfile(ProfileViewModel profile)
    {
        profile.DeleteProfile();
        RemoveProfile(profile.Profile.Id);
    }

    public void RemoveProfile(string profileId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var existing = Profiles.FirstOrDefault(p => p.Profile.Id == profileId);
            if (existing != null)
                Profiles.Remove(existing);
        });
    }

    public void ShowProfile(ProfileViewModel profile)
    {
        var view = new ProfileView { DataContext = profile };
        CurrentView = view;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? property = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
}
