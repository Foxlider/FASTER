using FASTER.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : UserControl
    {
        public Profile()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => Refresh();

        public MainWindow MetroWindow => (MainWindow)Window.GetWindow(this);

        internal void Refresh()
        {
            ((ProfileViewModel) DataContext)?.LoadData();
        }

        private void CopyFromClient(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Client");
        }

        private void CopyFromServer(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Server");
        }

        private void CopyFromHeadless(object sender, RoutedEventArgs e)
        {
            ((ProfileViewModel) DataContext)?.ModsCopyFrom(((MenuItem)sender).Tag, "Headless");
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
