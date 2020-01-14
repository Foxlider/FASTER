using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using AutoUpdaterDotNET;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

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
            IModUpdatesOnLaunch.Click += IModUpdatesOnLaunch_Checked;
            IAppUpdatesOnLaunch.Click += IAppUpdatesOnLaunch_Checked;
            IResetDialog.KeyUp += IResetDialog_KeyUp;
            MouseDown          += IResetDialog_LostFocus;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

        }

        private void IUpdateBtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AutoUpdater.DownloadUpdate() && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
            }
            catch (Exception exception)
            {
                IMessageText.Text     = exception.Message + " " + exception.GetType();
                IMessageDialog.IsOpen = true;
            }
        }

        private void IUpdateBtnCancel_Click(object sender, RoutedEventArgs e)
        { IUpdateDialog.IsOpen = false; }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable)
                {
                    if (args.Mandatory)
                    {
                        IUpdateText.Text = $"There is a new version ({args.CurrentVersion}) available. You are using version {args.InstalledVersion}. \nThis is a required update. Press Ok to begin the update.";
                        IUpdateBtnCancel.Visibility = Visibility.Hidden;
                        IUpdateBtnOK.Content = "OK";
                    }
                    else
                    {
                        IUpdateText.Text = $"There is a new version ({args.CurrentVersion}) available. You are using version {args.InstalledVersion}. \nDo you want to update the application now?";
                        IUpdateBtnCancel.Content = "No";
                        IUpdateBtnOK.Content = "Yes";
                        IUpdateBtnCancel.Visibility = Visibility.Visible;
                    }
                    IUpdateDialog.IsOpen = true;
                }
                else
                {
                    IMessageText.Text = "There is no update available please try again later.";
                    IMessageDialog.IsOpen = true;
                }
            }
            else
            {
                IMessageText.Text     = "There is a problem reaching update server please check your internet connection and try again later.";
                IMessageDialog.IsOpen = true;
            }
        }

        private void IResetDialog_LostFocus(object sender, MouseButtonEventArgs e)
        {
            if (!IResetDialogContent.IsMouseOver)
            { IResetDialog.IsOpen = false; }
        }

        private void IResetDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            { IResetDialog.IsOpen = false; }
        }

        private void IClearSettings_Click(object sender, RoutedEventArgs e)
        { IResetDialog.IsOpen = true; }

        private void IMessageButton_Click(object sender, RoutedEventArgs e)
        { IMessageDialog.IsOpen = false; }

        private void ICancelButton_Click(object sender, RoutedEventArgs e)
        { IResetDialog.IsOpen = false; }

        private void IResetButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Options.Default.clearSettings = true;
            Properties.Options.Default.Save();
            Application.Current.Shutdown();
        }
        
        private void Settings_Initialized(object sender, EventArgs e)
        {
            IModUpdatesOnLaunch.IsChecked = Properties.Options.Default?.checkForModUpdates;
            IAppUpdatesOnLaunch.IsChecked = Properties.Options.Default?.checkForAppUpdates;
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
            Properties.Options.Default.checkForAppUpdates = IAppUpdatesOnLaunch.IsChecked ?? true;
            Properties.Options.Default.Save();
        }


        private void UpdateLocalModFolders()
        {
            try
            {
                ILocalModFolders.Items.Clear();

                if (Properties.Options.Default?.localModFolders.Count > 0)
                {
                    foreach (var folder in Properties.Options.Default?.localModFolders)
                    {
                        var cb = new CheckBox { Content = folder, IsChecked = false };
                        ILocalModFolders.Items.Add(cb);
                    }
                }
            }
            catch
            { /*ignored*/ }
        }

        private void IUpdateApp_OnClick(object sender, RoutedEventArgs e)
        { AutoUpdater.Start("https://raw.githubusercontent.com/Foxlider/Fox-s-Arma-Server-Tool-Extended-Rewrite/master/FASTER_Version.xml"); }

        
    }
}
