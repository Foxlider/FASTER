using FASTER.Models;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for LocalMods.xaml
    /// </summary>
    public partial class LocalMods : UserControl
    {
        public LocalMods()
        {
            InitializeComponent();
            Loaded += LocalMods_Loaded;
        }

        private void LocalMods_Loaded(object sender, RoutedEventArgs e)
        { UpdateModsView(); }
        
        private void IRefreshList_Click(object sender, RoutedEventArgs e)
        { UpdateModsView(); }


        private void IEditFolders_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.IMainContent.SelectedIndex     = 3;
            MainWindow.Instance.ILocalModsTabSelect.IsSelected = false;
            MainWindow.Instance.ISettingsTabSelect.IsSelected  = true;
        }

        private void UpdateModsView()
        {
            ILocalModsView.Items.Clear();

            var localMods      = LocalMod.GetLocalMods();
            var serverPathMods = LocalMod.GetLocalMods(true);
            var steamMods      = SteamMod.GetSteamMods();

            var modsToRemove   = (from localMod in serverPathMods 
                                  from steamMod in steamMods 
                                  where localMod.Name == Functions.SafeName(steamMod.Name) 
                                  select localMod).ToList();

            foreach (var remove in modsToRemove)
                serverPathMods.RemoveAt(serverPathMods.IndexOf(serverPathMods.Find(m => m.Name == remove.Name)));

            localMods.AddRange(serverPathMods);

            if (localMods.Count <= 0) return;

            foreach (var localMod in localMods)
                ILocalModsView.Items.Add(localMod);
        }

        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            var localMod = (LocalMod)((Button)e.Source).DataContext;

            if (Directory.Exists(localMod.Path))
            {
                try 
                { Directory.Delete(localMod.Path, true); }
                catch
                {
                    MainWindow.Instance.IFlyoutMessage.Content = $"Could not delete mod \"{localMod.Name}\"";
                    MainWindow.Instance.IFlyout.IsOpen         = true;
                }
            }
            UpdateModsView();
        }

        private void OpenMod(object sender, RoutedEventArgs e)
        {
            var localMod = (LocalMod)((Button)e.Source).DataContext;

            if (!Directory.Exists(localMod.Path)) return;

            try
            { Process.Start(localMod.Path); }
            catch (Exception ex)
            { MessageBox.Show("Impossible to open the mod : " + ex.Message); }
        }
    }
}
