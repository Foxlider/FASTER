using System;
using FASTER.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
            IModActionButtons.SelectionChanged += IActionButtons_SelectionChanged;
        }


        private void LocalMods_Loaded(object sender, RoutedEventArgs e)
        { UpdateModsView(); }

        private void IActionButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IRefreshList.IsSelected)
            { UpdateModsView(); }
            else if (IEditFolders.IsSelected)
            {
                MainWindow.Instance.IMainContent.SelectedIndex     = 3;
                MainWindow.Instance.ILocalModsTabSelect.IsSelected = false;
                MainWindow.Instance.ISettingsTabSelect.IsSelected  = true;
            }

            Thread thread = new Thread(() =>
            {
                Thread.Sleep(600);
                Dispatcher?.Invoke(() =>
                {
                    ((ListBox)sender).SelectedItem = null;
                });
            });
            thread.Start();
        }


        private void UpdateModsView()
        {
            ILocalModsView.Items.Clear();

            var localMods      = LocalMod.GetLocalMods();
            var serverPathMods = LocalMod.GetLocalMods(true);
            var steamMods      = SteamMod.GetSteamMods();
            var modsToRemove   = new List<LocalMod>();

            foreach (LocalMod localMod in serverPathMods)
            {
                foreach (SteamMod steamMod in steamMods)
                {
                    if (localMod.Name == Functions.SafeName(steamMod.Name))
                        modsToRemove.Add(localMod);
                }
            }

            foreach (var remove in modsToRemove)
                serverPathMods.RemoveAt(serverPathMods.IndexOf(serverPathMods.Find(m => m.Name == remove.Name)));

            localMods.AddRange(serverPathMods);

            if (localMods.Count > 0)
            {
                foreach (var localMod in localMods)
                    ILocalModsView.Items.Add(localMod);
            }
        }


        private void DeleteMod(object sender, RoutedEventArgs e)
        {
            var localMod = (LocalMod)((Button)e.Source).DataContext;

            if (Directory.Exists(localMod.Path))
                Directory.Delete(localMod.Path, true);

            UpdateModsView();
        }


        private void OpenMod(object sender, RoutedEventArgs e)
        {
            var localMod = (LocalMod)((Button)e.Source).DataContext;

            if (Directory.Exists(localMod.Path))
            {
                try
                { Process.Start(localMod.Path); }
                catch (Exception ex)
                { MessageBox.Show("Impossible to open the mod : " + ex.Message); }
            }
        }

    }
}
