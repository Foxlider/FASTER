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
        Loaded += (_, _) => ViewModel.LoadData();
        Unloaded += (_, _) => ViewModel.UnloadData();
    }

    private async void InstallFolder_Click(object? sender, RoutedEventArgs e)
        => await ViewModel.InstallFolderClick();

    private void DeployAll_Click(object? sender, RoutedEventArgs e)
        => ViewModel.DeployAll();

    private void ClearAll_Click(object? sender, RoutedEventArgs e)
        => ViewModel.ClearAll();

    private void DeployMod(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is DeploymentMod mod)
            ViewModel.DeployMod(mod);
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
