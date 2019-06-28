using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {

            Initialized += Settings_Initialized;
            InitializeComponent();
            IModUpdatesOnLaunch.Checked += IModUpdatesOnLaunch_Checked;
            IAppUpdatesOnLaunch.Checked += IAppUpdatesOnLaunch_Checked;
        }


        private void IBaseThemeButton_Click(object sender, RoutedEventArgs e)
        { MainWindow.Instance.IWindowCloseButton.Background = (Brush)FindResource("MaterialDesignPaper"); }


        //private void PrimaryColor_Click(object sender, RoutedEventArgs e)
        //{
        //    /*TODO : WORK ON THIS*/
        //}


        //private void AccentColor_Click(object sender, RoutedEventArgs e)
        //{
        //    /*TODO : AND THIS*/
        //}


        private void IClearSettings_Click(object sender, RoutedEventArgs e)
        { IResetDialog.IsOpen = true; }


        private void IResetButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Options.Default.clearSettings = true;
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }


        private void Settings_Initialized(object sender, EventArgs e)
        {
            //TODO : WHEN THEME HANDLING DONE GET VALUE FROM SETTINGS FILE
            IBaseThemeToggle.IsChecked = true;
            IModUpdatesOnLaunch.IsChecked = Properties.Options.Default.checkForModUpdates;
            IAppUpdatesOnLaunch.IsChecked = Properties.Options.Default.checkForAppUpdates;
            UpdateLocalModFolders();
        }


        private void INewLocalFolder_Click(object sender, RoutedEventArgs e)
        {
            string newModFolder = MainWindow.Instance.SelectFolder();

            if (!string.IsNullOrEmpty(newModFolder))
            {
                if (Properties.Options.Default.localModFolders == null)
                { Properties.Options.Default.localModFolders = new List<string>(); }
                Properties.Options.Default.localModFolders.Add(newModFolder);
                Properties.Options.Default.Save();
            }
            UpdateLocalModFolders();
        }

        

        private void IRemoveLocalFolders_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ILocalModFolders.Items)
            {
                var cb = item as CheckBox;
                if (cb?.IsChecked ?? false)
                {
                    Properties.Options.Default.localModFolders.Remove(cb.Content.ToString());
                    Properties.Options.Default.Save();
                }
            }
            UpdateLocalModFolders();
        }


        private void IModUpdatesOnLaunch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Options.Default.checkForModUpdates = IModUpdatesOnLaunch.IsChecked ?? true;
            Properties.Options.Default.Save();
        }


        private void IAppUpdatesOnLaunch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Options.Default.checkForAppUpdates = IModUpdatesOnLaunch.IsChecked ?? true;
            Properties.Options.Default.Save();
        }



        private void UpdateLocalModFolders()
        {
            ILocalModFolders.Items.Clear();

            if (Properties.Options.Default.localModFolders.Count > 0)
            {
                foreach (var folder in Properties.Options.Default.localModFolders)
                {
                    var cb = new CheckBox { Content = folder, IsChecked = false };
                    ILocalModFolders.Items.Add(cb);
                }
            }
        }
    }
}
