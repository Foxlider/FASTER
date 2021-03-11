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
    /// Interaction logic for Updater.xaml
    /// </summary>
    public partial class Updater : UserControl
    {
        public Updater()
        {
            InitializeComponent();
        }
        private void UpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void ServerDir_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void SteamCmdDir_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((SteamUpdaterViewModel) DataContext)?.PasswordChanged(((PasswordBox) sender).Password);
        }
    }
}
