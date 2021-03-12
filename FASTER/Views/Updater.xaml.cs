using FASTER.ViewModel;

using MahApps.Metro.Controls.Dialogs;

using System.Windows;
using System.Windows.Controls;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Updater.xaml
    /// </summary>
    public partial class Updater : UserControl
    {
        public Updater()
        {
            InitializeComponent();
            MainWindow.Instance.SteamUpdaterViewModel.dialogCoordinator = DialogCoordinator.Instance;
            DataContext = MainWindow.Instance.SteamUpdaterViewModel;
        }

        private void UpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.UpdateCancelClick();
        }
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await ((SteamUpdaterViewModel)DataContext)?.UpdateClick();
        }
        private void ServerDir_Click(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.ServerDirClick(sender);
        }

        private void SteamCmdDir_Click(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.SteamCmdDirClick();
        }
        private void ModStagingDir_Click(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.ModStagingDirClick();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.PasswordChanged(((PasswordBox) sender).Password);
        }
        
    }
}
