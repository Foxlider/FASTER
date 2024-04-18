using AutoUpdaterDotNET;

using BytexDigital.Steam.ContentDelivery;

using ControlzEx.Theming;

using FASTER.Models;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        UpdateInfoEventArgs _args;

        private MainWindow MetroWindow { get; set; }


        public Settings(MainWindow main)
        {
            InitializeComponent();
            MetroWindow = main;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            colorPicker.ItemsSource = ThemeManager.Current.Themes/*.Where(t => t.BaseColorScheme == "Dark")*/;
            colorPicker.DisplayMemberPath = "DisplayName";
            colorPicker.SelectedItem = ThemeManager.Current.Themes.FirstOrDefault(t => t.Name == Properties.Settings.Default.theme);

            fontPicker.ItemsSource = Fonts.SystemFontFamilies;
            fontPicker.DisplayMemberPath = "Source";
            fontPicker.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(t => t.Source == Properties.Settings.Default.font);

            Slider.Value = Properties.Settings.Default.CliWorkers;
        }

        private void IUpdateBtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AutoUpdater.DownloadUpdate(_args) && Application.Current.MainWindow != null)
                { Application.Current.MainWindow.Close(); }
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
                _args = args;
                if (args.IsUpdateAvailable)
                {
                    if (args.Mandatory.Value)
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
            Properties.Settings.Default.clearSettings = true;
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }
        
        private void Settings_Initialized(object sender, EventArgs e)
        {
            IModUpdatesOnLaunch.IsChecked = Properties.Settings.Default?.checkForModUpdates;
            IAppUpdatesOnLaunch.IsChecked = Properties.Settings.Default?.checkForAppUpdates;
            IAPIKeyBox.Text = Properties.Settings.Default?.SteamAPIKey ?? string.Empty;
            Slider.Value = Properties.Settings.Default.CliWorkers;
            NumericUpDown.Value = Slider.Value;
        }

        private void IModUpdatesOnLaunch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkForModUpdates = IModUpdatesOnLaunch.IsChecked ?? true;
            Properties.Settings.Default.Save();
        }

        private void IAppUpdatesOnLaunch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkForAppUpdates = IAppUpdatesOnLaunch.IsChecked ?? true;
            Properties.Settings.Default.Save();
        }

        private void IUpdateApp_OnClick(object sender, RoutedEventArgs e)
        { AutoUpdater.Start("https://raw.githubusercontent.com/Foxlider/FASTER/master/FASTER_Version.xml"); }

        private void APIKeyButton_Click(object sender, RoutedEventArgs e)
        { Functions.OpenBrowser("https://steamcommunity.com/dev/apikey"); }

        private void ISaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IAPIKeyBox.Text)) 
                Properties.Settings.Default.SteamAPIKey = IAPIKeyBox.Text;
            Properties.Settings.Default.checkForAppUpdates = IAppUpdatesOnLaunch.IsChecked ?? true;
            Properties.Settings.Default.checkForModUpdates = IModUpdatesOnLaunch.IsChecked ?? true;
            Properties.Settings.Default.Save();
        }

        private void ResetTheme_Click(object sender, RoutedEventArgs e)
        {
            colorPicker.SelectedItem = ThemeManager.Current.Themes.FirstOrDefault(t => t.Name == "Dark.Blue");

            Properties.Settings.Default.theme = "Dark.Blue";
            Properties.Settings.Default.Save();
        }

        private void ResetFont_Click(object sender, RoutedEventArgs e)
        {
            fontPicker.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == "Segoe UI");

            Properties.Settings.Default.font = "Segoe UI";
            Properties.Settings.Default.Save();
        }

        private void Colors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(e.AddedItems[0] is not Theme theme)
                return;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.ChangeTheme(Application.Current, theme);

            Properties.Settings.Default.theme = theme.Name;
            Properties.Settings.Default.Save();
        }

        private void Fonts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is not FontFamily font)
                return;

            MetroWindow.FontFamily = font;

            Properties.Settings.Default.font = font.Source;
            Properties.Settings.Default.Save();
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if ((sender is Slider slider) && !slider.IsLoaded)
                return;

            Properties.Settings.Default.CliWorkers = Convert.ToUInt16(e.NewValue);
            NumericUpDown.Value = e.NewValue;
            if (MainWindow.Instance.SteamUpdaterViewModel.SteamContentClient != null)
                MainWindow.Instance.SteamUpdaterViewModel.SteamContentClient = new SteamContentClient(MainWindow.Instance.SteamUpdaterViewModel.SteamClient, Properties.Settings.Default.CliWorkers);
        }
    }
}
