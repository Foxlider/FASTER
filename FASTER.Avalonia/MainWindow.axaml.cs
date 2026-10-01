using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Avalonia.ViewModels;
using FASTER.Services;
using FASTER.ViewModel;

namespace FASTER.Avalonia;

public partial class MainWindow : Window
{
    private MainViewModel Main => (MainViewModel)DataContext!;

    public MainWindow() => InitializeComponent();

    private void NavUpdater_Click(object? sender, RoutedEventArgs e) => Navigate(NavUpdater, Main.ShowUpdater);

    private void MessageCollapse_Click(object? sender, RoutedEventArgs e)
        => Main.ToggleMessageExpanded();

    private void NavMods_Click(object? sender, RoutedEventArgs e) => Navigate(NavMods, Main.ShowMods);
    private void NavDeployment_Click(object? sender, RoutedEventArgs e) => Navigate(NavDeployment, Main.ShowDeployment);
    private void NavServerStatus_Click(object? sender, RoutedEventArgs e) => Navigate(NavServerStatus, Main.ShowServerStatus);
    private void NavSettings_Click(object? sender, RoutedEventArgs e) => Navigate(NavSettings, Main.ShowSettings);
    private void NavAbout_Click(object? sender, RoutedEventArgs e) => Navigate(NavAbout, Main.ShowAbout);

    private void Navigate(Button active, Action show)
    {
        foreach (var button in new[] { NavUpdater, NavMods, NavDeployment, NavServerStatus, NavSettings, NavAbout })
            button.Classes.Set("selected", button == active);
        ProfilesList.SelectedItem = null;
        show();
    }

    private void ProfilesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ProfilesList.SelectedItem is ProfileViewModel profile)
            Main.ShowProfile(profile);
    }

    private async void AddProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = await AppServices.Dialogs.ShowInputAsync(Main, "New profile", "Profile name:");
        if (string.IsNullOrWhiteSpace(name))
            return;
        Main.AddProfile(name.Trim());
    }

    private void MoveProfileUp_Click(object? sender, RoutedEventArgs e) => MoveProfile(-1);
    private void MoveProfileDown_Click(object? sender, RoutedEventArgs e) => MoveProfile(1);
    private void MoveProfile(int offset)
    {
        if (ProfilesList.SelectedItem is not ProfileViewModel profile) return;
        var settings = FASTER.Models.AppSettings.Current;
        if (settings.Profiles?.MoveProfile(profile.Profile.Id, offset) != true) return;
        settings.Save();
        Main.Profiles.Move(Main.Profiles.IndexOf(profile), Main.Profiles.IndexOf(profile) + offset);
        ProfilesList.SelectedItem = profile;
    }
    private void OpenServer_Click(object? sender, RoutedEventArgs e) => OpenFolder(FASTER.Models.AppSettings.Current.ServerPath);
    private void OpenStaging_Click(object? sender, RoutedEventArgs e) => OpenFolder(FASTER.Models.AppSettings.Current.ModStagingDirectory);
    private void OpenData_Click(object? sender, RoutedEventArgs e) => OpenFolder(System.IO.Path.GetDirectoryName(FASTER.Models.AppSettings.SettingsPath)!);
    private void OpenFolder(string path)
    {
        try { Platform.Current.OpenFolder(path); }
        catch (Exception ex) { Main.ShowStatus("Could not open directory: " + ex.Message); }
    }

    private void CloneProfile_Click(object? sender, RoutedEventArgs e)
    {
        if (ProfilesList.SelectedItem is ProfileViewModel profile)
            Main.CloneProfile(profile);
    }

    private void DeleteProfile_Click(object? sender, RoutedEventArgs e)
    {
        if (ProfilesList.SelectedItem is ProfileViewModel profile)
            Main.DeleteProfile(profile);
    }
}
