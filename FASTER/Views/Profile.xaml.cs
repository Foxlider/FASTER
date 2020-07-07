using FASTER.ViewModel;
using System;
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
            if (DataContext == null) return;
            ((ProfileViewModel)DataContext).LoadData();
        }
    }
}
