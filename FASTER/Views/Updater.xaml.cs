using FASTER.ViewModel;

using System.Windows;
using System.Windows.Controls;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Updater.xaml
    /// </summary>
    public partial class Updater
    {
        public Updater()
        {
            InitializeComponent();
            DataContext = MainWindow.Instance.SteamUpdaterViewModel;
        }

        private void UpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel)DataContext)?.UpdateCancelClick();
        }
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await ((SteamUpdaterViewModel)DataContext)?.UpdateClick()!;
        }
        private async void ServerDir_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SteamUpdaterViewModel vm)
                await vm.ServerDirClick();
        }

        private async void ModStagingDir_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SteamUpdaterViewModel vm)
                await vm.ModStagingDirClick();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox { IsFocused: true } box)
                ((SteamUpdaterViewModel)DataContext)?.PasswordChanged(box.Password);
        }

        private void Updater_OnLoaded(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = ((SteamUpdaterViewModel)DataContext)?.GetPw() ?? string.Empty;
        }

        private void ClientReset_OnClick(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel)DataContext)?.SteamReset();
        }

        private void ClientConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel)DataContext)?.SteamLogin();
        }
    }
}
