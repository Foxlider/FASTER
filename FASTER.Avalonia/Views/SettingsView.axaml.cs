using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

using FASTER.Models;


namespace FASTER.Avalonia.Views;

public partial class SettingsView : UserControl
{
    private bool _loading = true;
    private const string SteamApiKeyUrl = "https://steamcommunity.com/dev/apikey"; // NOSONAR - stable public service endpoint, intentionally compiled in
    public SettingsView()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadCurrent();
    }

    private void LoadCurrent()
    {
        _loading = true;
        var settings = AppSettings.Current;
        AccentBox.ItemsSource = Services.Appearance.Accents.Keys;
        FontBox.ItemsSource = Services.Appearance.GetFonts();
        FontBox.SelectedItem = Services.Appearance.AppliedFont;
        AccentBox.SelectedItem = settings.Theme.Split('.').Length > 1 ? settings.Theme.Split('.')[1] : "Blue";
        DebugBox.IsChecked = settings.EnableDebugLog;
        TelemetryStatus.Text = FASTER.Services.Telemetry.Current.Status;
        ThemeBox.SelectedIndex = settings.Theme.StartsWith("Dark", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        ModUpdatesBox.IsChecked = settings.CheckForModUpdates;
        AppUpdatesBox.IsChecked = settings.CheckForAppUpdates;
        AnalyticsBox.IsChecked = settings.EnableAnalytics;
        ApiKeyBox.Text = settings.SteamAPIKey;
        WorkersSlider.Value = settings.CliWorkers;
        _loading = false;
    }

    private void ThemeBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SaveAppearance();
    }

    private void Appearance_Changed(object? sender, SelectionChangedEventArgs e) => SaveAppearance();
    private void SaveAppearance()
    {
        if (_loading) return;
        AppSettings.Current.Theme = (ThemeBox.SelectedIndex == 0 ? "Dark." : "Light.") + (AccentBox.SelectedItem as string ?? "Blue");
        if (FontBox.SelectedItem is string font) AppSettings.Current.Font = font;
        AppSettings.Current.Save();
        Services.Appearance.Apply();
    }
    private void ResetAccent_Click(object? sender, RoutedEventArgs e) { ThemeBox.SelectedIndex = 0; AccentBox.SelectedItem = "Blue"; SaveAppearance(); }
    private void ResetFont_Click(object? sender, RoutedEventArgs e) { AppSettings.Current.Font = "Segoe UI"; AppSettings.Current.Save(); Services.Appearance.Apply(); LoadCurrent(); }
    private void DebugBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.Current.EnableDebugLog = DebugBox.IsChecked.GetValueOrDefault();
        AppSettings.Current.Save();
    }
    private void OpenLog_Click(object? sender, RoutedEventArgs e)
    {
        try { Logger.LogCritical("Log opened from Settings."); FASTER.Services.Platform.Current.OpenFile(Logger.LogFilePath); }
        catch (Exception ex) { UpdateMessage.Text = "Could not open log: " + ex.Message; }
    }

    private void ModUpdatesBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.Current.CheckForModUpdates = ModUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
    }

    private void AppUpdatesBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.Current.CheckForAppUpdates = AppUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
    }

    private async void AnalyticsBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.Current.EnableAnalytics = AnalyticsBox.IsChecked ?? true;
        AppSettings.Current.Save();
        await FASTER.Services.Telemetry.SetEnabledAsync(AppSettings.Current.EnableAnalytics);
        TelemetryStatus.Text = FASTER.Services.Telemetry.Current.Status;
    }

    private void WorkersSlider_Changed(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_loading) return;
        AppSettings.Current.CliWorkers = Convert.ToUInt16(e.NewValue);
        AppSettings.Current.Save();
    }

    private void ApiKeyButton_Click(object? sender, RoutedEventArgs e)
        => Functions.OpenBrowser(SteamApiKeyUrl);

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        AppSettings.Current.SteamAPIKey = ApiKeyBox.Text ?? string.Empty;
        AppSettings.Current.CheckForAppUpdates = AppUpdatesBox.IsChecked ?? true;
        AppSettings.Current.CheckForModUpdates = ModUpdatesBox.IsChecked ?? true;
        AppSettings.Current.Save();
        UpdateMessage.Text = "Settings saved.";
    }

    private async void CheckUpdate_Click(object? sender, RoutedEventArgs e)
    {
        await App.Main.CheckApplicationUpdatesAsync();
        UpdateMessage.Text = App.Main.Updates.Status;
    }

    private async void Reset_Click(object? sender, RoutedEventArgs e)
    {
        if (!await FASTER.Services.AppServices.Dialogs.ShowConfirmationAsync(this, "Reset settings",
            "Reset configuration on the next launch? Downloaded mods, server installations and generated files will be kept."))
            return;
        AppSettings.Current.ClearSettings = true;
        AppSettings.Current.Save();
        UpdateMessage.Text = "Settings will reset on next launch.";
    }
}
