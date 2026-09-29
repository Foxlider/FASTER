using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.ViewModel;

namespace FASTER.Avalonia.Views;

public partial class UpdaterView : UserControl
{
    private SteamUpdaterViewModel ViewModel => (SteamUpdaterViewModel)DataContext!;

    public UpdaterView()
    {
        InitializeComponent();
        Loaded += (_, _) => PasswordBox.Text = ViewModel.GetPw() ?? string.Empty;
    }

    private void UpdateCancel_Click(object? sender, RoutedEventArgs e) => ViewModel.UpdateCancelClick();

    private async void Update_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateClick();

    private void ServerDir_Click(object? sender, RoutedEventArgs e) => ViewModel.ServerDirClick();

    private void ModStagingDir_Click(object? sender, RoutedEventArgs e) => ViewModel.ModStagingDirClick();

    private void PasswordBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { IsFocused: true } box)
            ViewModel.PasswordChanged(box.Text ?? string.Empty);
    }

    private void ClientReset_OnClick(object? sender, RoutedEventArgs e) => ViewModel.SteamReset();

    private void ClientConnect_OnClick(object? sender, RoutedEventArgs e) => _ = ViewModel.SteamLogin();
}
