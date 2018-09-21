Imports System.Threading

Imports FAST2.Models

Public Class SteamMods

    'Manages actions for steam mods tab buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IModActionButtons.SelectionChanged
        
        Dim thread As New Thread(
            Sub()
                Thread.Sleep(600)
                Dispatcher.Invoke(
                    Sub()
                        sender.SelectedItem = Nothing
                    End Sub
                    )
            End Sub
            )
        thread.Start()
    End Sub

    Private Sub ICheckForUpdates_Selected(sender As Object, e As RoutedEventArgs) Handles ICheckForUpdates.Selected
        ICheckForUpdates.IsSelected = False
        CheckForUpdates()
    End Sub

    Private Async Sub CheckForUpdates()
        If My.Settings.steamMods.Count > 0
            IUpdateProgress.IsIndeterminate = True
            IModView.IsEnabled = False
            IProgressInfo.Visibility = Visibility.Visible
            IProgressInfo.Content = "Checking for updates..."

            Dim tasks As New List(Of Task) From {
                    Task.Run(
                        Sub()
                            ModCollection.UpdateInfoFromSteam()
                        End Sub
                        )
                    }

            Await Task.WhenAll(tasks)

            IModView.IsEnabled = True
            IProgressInfo.Visibility = Visibility.Collapsed
            IUpdateProgress.IsIndeterminate = False   
            UpdateModsView()
        Else 
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "No Mods To Check"
        End If
    End Sub
    
    Private Sub IAddSteamMod_Selected(sender As Object, e As RoutedEventArgs) Handles IAddSteamMod.Selected
        IImportSteamModDialog.IsOpen = True
        IAddSteamMod.IsSelected = False
    End Sub

    Private Sub SteamMods_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
      UpdateModsView()
    End Sub

    Private Sub UpdateModsView()
        IModView.Items.Clear()

        If My.Settings.SteamMods IsNot Nothing
            For Each steamMod In My.Settings.SteamMods
                IModView.Items.Add(steamMod)
            Next
        End If
    End Sub

    Private Sub IImportSteamModDialog_KeyUp(sender As Object, e As KeyEventArgs) Handles IImportSteamModDialog.KeyUp
        If e.Key = Key.Escape
            IImportSteamModDialog.IsOpen = False
            IPrivateModCheck.IsChecked = False
            ISteamItemBox.Text = String.Empty
        End If
    End Sub

    Private Sub IImportModButton_Click(sender As Object, e As RoutedEventArgs) Handles IImportModButton.Click
        Mouse.OverrideCursor = Cursors.Wait
        IImportSteamModDialog.IsOpen = False
        ModCollection.AddSteamMod(ISteamItemBox.Text)
        IPrivateModCheck.IsChecked = False
        ISteamItemBox.Text = String.Empty
        Mouse.OverrideCursor = Cursors.Arrow
        UpdateModsView()
    End Sub

    Private Sub SteamMods_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        CheckForUpdates()
    End Sub
End Class
