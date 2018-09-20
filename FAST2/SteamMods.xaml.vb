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
        IUpdateProgress.IsIndeterminate = True
        If My.Settings.steamMods.Count > 0
            Dim thread As New Thread(
                Sub()
                    ModCollection.UpdateInfoFromSteam()
                End Sub
                )
            thread.Start()
        Else 
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "No Mods To Check"
        End If
        IUpdateProgress.IsIndeterminate = True
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

    Private Sub IImportSteamModDialog_KeyUp(sender As Object, e As Input.KeyEventArgs) Handles IImportSteamModDialog.KeyUp
        If e.Key = Key.Escape
            IImportSteamModDialog.IsOpen = False
            IPrivateModCheck.IsChecked = False
            ISteamItemBox.Text = String.Empty
        End If
    End Sub

    Private Sub IContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles IImportModButton.Click
        Mouse.OverrideCursor = Input.Cursors.Wait
        IImportSteamModDialog.IsOpen = False
        ModCollection.AddSteamMod(ISteamItemBox.Text)
        IPrivateModCheck.IsChecked = False
        ISteamItemBox.Text = String.Empty
        Mouse.OverrideCursor = Input.Cursors.Arrow
        UpdateModsView()
    End Sub



End Class
