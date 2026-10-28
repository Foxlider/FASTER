using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Models;
using FASTER.ViewModel;

namespace FASTER.Avalonia.Views;

public partial class DeploymentView : UserControl
{
    private DeploymentViewModel ViewModel => (DeploymentViewModel)DataContext!;

    public DeploymentView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadDataAsync();
        Unloaded += (_, _) => ViewModel.UnloadData();
    }

    private async void InstallFolder_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.InstallFolderClick();

    private async void DeployAll_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.DeployAllAsync();

    private void ClearAll_Click(object? sender, RoutedEventArgs e)
        => ViewModel.ClearAll();

    private async void DeployMod(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is DeploymentMod mod)
        {
            await ViewModel.DeployModAsync(mod);
            if (sender is CheckBox checkBox)
                checkBox.SetCurrentValue(CheckBox.IsCheckedProperty, mod.Marked);
        }
    }

    private void OpenModPage(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is DeploymentMod mod)
            ViewModel.OpenModPage(mod);
    }

    private void OpenModFolder(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is DeploymentMod mod)
            ViewModel.OpenModFolder(mod);
    }
}
