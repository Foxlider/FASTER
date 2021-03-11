using FASTER.Models;
using FASTER.ViewModel;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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

        private async void UpdateMod(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement) sender).DataContext as ArmaMod;
            if (mod == null)
                return;
            
            await Task.Run(() => mod.UpdateMod());
        }

        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement) sender).DataContext as ArmaMod;
            ((ModsViewModel) DataContext)?.DeleteMod(mod);
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

        private void AddSteamMod_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.AddSteamMod();
        }

        private void AddLocalMod_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.AddLocalMod();
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

        private void ScanSteam_Click(object sender, RoutedEventArgs e)
        {
            ((ModsViewModel) DataContext)?.ImportFromSteam();
        }
    }
}
