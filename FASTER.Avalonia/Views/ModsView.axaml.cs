using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Models;
using FASTER.ViewModel;

namespace FASTER.Avalonia.Views;

public partial class ModsView : UserControl
{
    private ModsViewModel ViewModel => (ModsViewModel)DataContext!;

    public ModsView()
    {
        InitializeComponent();
        Unloaded += (_, _) => ViewModel.UnloadData();
    }

    private void ModsGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (var mod in ViewModel.ModsCollection.ArmaMods)
            mod.IsSelected = ModsGrid.SelectedItems.Contains(mod);
    }

    private async void UpdateSelectedMods(object? sender, RoutedEventArgs e)
        => await ViewModel.UpdateSelectedMods();

    private void DeleteSelectedMods(object? sender, RoutedEventArgs e)
        => ViewModel.DeleteSelectedMods();

    private void OpenModPage(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is ArmaMod mod)
            ViewModel.OpenModPage(mod);
    }

    private void OpenModFolder(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is ArmaMod mod)
            ViewModel.OpenModFolder(mod);
    }

    private async void AddSteamMod_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.AddSteamMod();

    private void AddLocalMod_Click(object? sender, RoutedEventArgs e)
        => _ = ViewModel.AddLocalModAsync();

    private async void ImportLauncherFile_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.OpenLauncherFile();

    private void CheckForUpdates_Click(object? sender, RoutedEventArgs e)
        => ViewModel.CheckForUpdates();

    private void UpdateAll_Click(object? sender, RoutedEventArgs e)
        => _ = ViewModel.UpdateAll();

    private async void DeleteAll_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.DeleteAllMods();
}
