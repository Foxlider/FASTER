using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FASTER.ViewModel;

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

        public MainWindow MetroWindow => (MainWindow) Window.GetWindow(this);

        private void UpdateMod(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OpenModPage(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddSteamMod_Click(object sender, RoutedEventArgs e)
        {
            ( (ModsViewModel) DataContext )?.AddSteamMod();
        }

        private void AddLocalMod_Click(object sender, RoutedEventArgs e)
        {
            ( (ModsViewModel) DataContext )?.AddLocalMod();
        }

        private void ImportLauncherFile_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateAll_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScanSteam_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
