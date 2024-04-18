using FASTER.Models;
using FASTER.ViewModel;

using System;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Mods.xaml
    /// </summary>
    public partial class Mods
    {
        public Mods()
        {
            InitializeComponent();
            MainWindow.Instance.ModsViewModel.DialogCoordinator =  DialogCoordinator.Instance;

            this.Dispatcher.ShutdownStarted += DispatcherOnShutdownStarted;
        }

        private void DispatcherOnShutdownStarted(object sender, EventArgs e)
        {
            ((ModsViewModel) DataContext)?.UnloadData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Empty to avoid double loading for now
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.UnloadData();
        }

        private async void UpdateSelectedMods(object sender, RoutedEventArgs e)
        {
            await ((ModsViewModel) DataContext)?.UpdateSelectedMods();
        }

        private void DeleteSelectedMods(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.DeleteSelectedMods();
        }

        private void OpenModPage(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement) sender).DataContext as ArmaMod;
            ((ModsViewModel) DataContext)?.OpenModPage(mod);
        }

        private void OpenModFolder(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement) sender).DataContext as ArmaMod;
            ((ModsViewModel) DataContext)?.OpenModFolder(mod);
        }

        private async void AddSteamMod_Click(object sender, RoutedEventArgs e)
        {
            await ((ModsViewModel) DataContext)?.AddSteamMod();
        }

        private void AddLocalMod_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.AddLocalModAsync();
        }

        private async void ImportLauncherFile_Click(object sender, RoutedEventArgs e)
        {
            await ((ModsViewModel)DataContext)?.OpenLauncherFile();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.CheckForUpdates();
        }

        private void UpdateAll_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.UpdateAll();
        }
		
		private async void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            await ((ModsViewModel) DataContext)?.DeleteAllMods();
        }
    }
}
