using System;
using FASTER.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile
    {
        public Profile()
        {
            InitializeComponent();
            this.Dispatcher.ShutdownStarted += DispatcherOnShutdownStarted;
        }

        private void DispatcherOnShutdownStarted(object sender, EventArgs e)
        {
            ((ProfileViewModel) DataContext)?.UnloadData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Empty to avoid double loading for now
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel)DataContext)?.UnloadData();
        }

        public MainWindow MetroWindow => (MainWindow)Window.GetWindow(this);

        internal void Refresh()
        {
            ((ProfileViewModel) DataContext)?.LoadData();
        }

        private void ClearModOrder(object sender, RoutedEventArgs e)
        {
            ( (ProfileViewModel) DataContext )?.ClearModOrder();
        }

        private void CopyFromClient(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Client + Server");
        }

        private void CopyFromServer(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Server Only");
        }

        private void CopyFromHeadless(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "HC");
        }
		
        private void CopyFromOpt(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Opt");
        }

        private void ModsSelectAll(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsSelectAll(((MenuItem)sender).Tag, true);
        }

        private void ModsSelectNone(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsSelectAll(((MenuItem)sender).Tag, false);
        }

        private void MissionSelectAll(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.MissionSelectAll(true);
        }

        private void MissionSelectNone(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.MissionSelectAll(false);
        }

        private void MissionRefresh(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.LoadMissions();
        }

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.LoadModsFromFile();
        }

        private void CopyModKeys_Click(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.CopyModKeys();
        }

        private void ClearModKeys_Click(object sender, RoutedEventArgs e)
        {
            ( (ProfileViewModel) DataContext )?.ClearModKeys();
        }

        private void SelectServerFile(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.SelectServerFile();
        }

        private void OpenProfileLocation(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.OpenProfileLocation();
        }

        private void SaveProfile(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.SaveProfile();
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.DeleteProfile();
        }

        private void LaunchServer(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.LaunchServer();
        }

        private void LaunchHCs(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.LaunchHCs();
        }
    }
}
