using FASTER.Models;
using FASTER.ViewModel;
using System.Windows;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Deployment.xaml
    /// </summary>
    public partial class Deployment
    {
        public Deployment()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((DeploymentViewModel)DataContext)?.LoadData();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ((DeploymentViewModel)DataContext)?.UnloadData();
        }

        private void InstallFolder_Click(object sender, RoutedEventArgs e)
        {
            ((DeploymentViewModel)DataContext)?.InstallFolderClick();
        }

        private void DeployMod(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement)sender).DataContext as DeploymentMod;
            ((DeploymentViewModel)DataContext)?.DeployMod(mod);
        }

        private void OpenModPage(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement)sender).DataContext as DeploymentMod;
            ((DeploymentViewModel)DataContext)?.OpenModPage(mod);
        }

        private void OpenModFolder(object sender, RoutedEventArgs e)
        {
            var mod = ((FrameworkElement)sender).DataContext as DeploymentMod;
            ((DeploymentViewModel)DataContext)?.OpenModFolder(mod);
        }

        private void DeployAll_Click(object sender, RoutedEventArgs e)
        {
            ((DeploymentViewModel)DataContext)?.DeployAll();
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            ((DeploymentViewModel)DataContext)?.ClearAll();
        }
    }
}
