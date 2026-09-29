using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

using FASTER.Models;

using Velopack;
using Velopack.Sources;

namespace FASTER.Avalonia.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadCurrent();
    }

    private void LoadCurrent()
    {
        var settings = AppSettings.Current;
        ThemeBox.SelectedIndex = settings.Theme.StartsWith("Dark", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        ModUpdatesBox.IsChecked = settings.CheckForModUpdates;
        AppUpdatesBox.IsChecked = settings.CheckForAppUpdates;
        ApiKeyBox.Text = settings.SteamAPIKey;
        WorkersSlider.Value = settings.CliWorkers;
    }

    private void ThemeBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Application.Current == null)
            return;
        bool dark = ThemeBox.SelectedIndex == 0;
        Application.Current.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        AppSettings.Current.Theme = dark ? "Dark.Blue" : "Light.Blue";
        AppSettings.Current.Save();
    }

    private void ModUpdatesBox_Changed(object? sender, RoutedEventArgs e)
    {
        AppSettings.Current.CheckForModUpdates = ModUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
    }

    private void AppUpdatesBox_Changed(object? sender, RoutedEventArgs e)
    {
        AppSettings.Current.CheckForAppUpdates = AppUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
    }

    private void WorkersSlider_Changed(object? sender, RangeBaseValueChangedEventArgs e)
    {
        AppSettings.Current.CliWorkers = Convert.ToUInt16(e.NewValue);
        AppSettings.Current.Save();
    }

    private void ApiKeyButton_Click(object? sender, RoutedEventArgs e)
        => Functions.OpenBrowser("https://steamcommunity.com/dev/apikey");

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ApiKeyBox.Text))
            AppSettings.Current.SteamAPIKey = ApiKeyBox.Text;
        AppSettings.Current.CheckForAppUpdates = AppUpdatesBox.IsChecked ?? true;
        AppSettings.Current.CheckForModUpdates = ModUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
        UpdateMessage.Text = "Settings saved.";
    }

    private async void CheckUpdate_Click(object? sender, RoutedEventArgs e)
    {
        UpdateMessage.Text = "Checking for updates...";
        try
        {
            var source = new GithubSource("https://github.com/milutinke/FASTER", null, false);
            var manager = new UpdateManager(source);
            var update = await manager.CheckForUpdatesAsync();
            if (update == null)
            {
                UpdateMessage.Text = "No update available.";
                return;
            }
            UpdateMessage.Text = $"Downloading {update.TargetFullRelease.Version}...";
            await manager.DownloadUpdatesAsync(update);
            UpdateMessage.Text = "Update downloaded, restarting...";
            manager.ApplyUpdatesAndRestart(update);
        }
        catch (Exception ex)
        {
            UpdateMessage.Text = "Update check failed: " + ex.Message;
        }
    }

    private void Reset_Click(object? sender, RoutedEventArgs e)
    {
        AppSettings.Current.ClearSettings = true;
        AppSettings.Current.Save();
        UpdateMessage.Text = "Settings will reset on next launch.";
    }
}
