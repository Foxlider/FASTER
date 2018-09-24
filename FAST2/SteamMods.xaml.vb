Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
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
        CheckForUpdates()
    End Sub

    Private Shared Sub IUpdateAll_Selected(sender As Object, e As RoutedEventArgs) Handles IUpdateAll.Selected
        UpdateAllMods()
    End Sub


    Private Async Sub IImportLauncherFile_Selected(sender As Object, e As RoutedEventArgs) Handles IImportLauncherFile.Selected
        IUpdateProgress.IsIndeterminate = True
        IModView.IsEnabled = False
        IProgressInfo.Visibility = Visibility.Visible
        IProgressInfo.Content = "Importing Mods..."

        Dim tasks As New List(Of Task) From {
                Task.Run(
                    Sub()
                        ImportLauncherFile()
                    End Sub
                    )
                }

        Await Task.WhenAll(tasks)

        IModView.IsEnabled = True
        IProgressInfo.Visibility = Visibility.Collapsed
        IUpdateProgress.IsIndeterminate = False
        UpdateModsView()
    End Sub

    Private Shared Function SelectFile(filter As String)
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
                .Filter = filter
                }

        If dialog.ShowDialog() <> DialogResult.OK Then
            Return dialog.FileName
        Else 
            Return Nothing
        End If
    End Function

    Private Shared Sub ImportLauncherFile
        Dim modsFile = SelectFile("Arma 3 Launcher File|*.html")
        
        If modsFile IsNot String.Empty
            Dim dataReader As StreamReader
            dataReader = My.Computer.FileSystem.OpenTextFileReader(modsFile)

            Dim modLine As String

            Do
                modLine = dataReader.ReadLine
                If modLine Is Nothing Then Exit Do
                If modLine.Contains("data-type=""Link"">") Then
                    Dim link As String
                    link = modLine.Substring(modLine.IndexOf("http://steam", StringComparison.Ordinal))
                    link = StrReverse(link)
                    link = link.Substring(link.IndexOf("epyt-atad", StringComparison.Ordinal) + 11)
                    link = StrReverse(link)
                    ModCollection.AddSteamMod(link, True)
                End If
            Loop
            dataReader.Close()
        End If
    End Sub

    Private Shared Sub UpdateAllMods()
        If MainWindow.Instance.ReadyToUpdate Then 
            Dim modsToUpdate = New List(Of String)

            For Each steamMod In My.Settings.steamMods
                If steamMod.SteamLastUpdated > steamMod.LocalLastUpdated
                    modsToUpdate.Add(steamMod.WorkshopId)
                End If
            Next

            If modsToUpdate.Count > 0
                Dim steamCommand As String = "+login " & MainWindow.Instance.ISteamUserBox.Text & " " & MainWindow.Instance.ISteamPassBox.Password
                
                For Each steamMod In modsToUpdate
                    steamCommand = steamCommand & " +workshop_download_item 107410 " & steamMod
                Next

                Dim steamCmd As String = MainWindow.Instance.ISteamDirBox.Text + "\steamcmd.exe"
                steamCommand = steamCommand & " validate +quit"
                MainWindow.Instance.RunSteamCommand(steamCmd, steamCommand, "addon")
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "No Mods to Update"
            End If
        End If
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
    End Sub

    Private Sub SteamMods_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If My.Settings.steamMods.Count > 0
           UpdateModsView()
        End If
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

    Private Sub IImportModButton_Click(sender As Object, e As RoutedEventArgs) Handles IImportModButton.Click
        Mouse.OverrideCursor = Input.Cursors.Wait
        IImportSteamModDialog.IsOpen = False
        ModCollection.AddSteamMod(ISteamItemBox.Text)
        IPrivateModCheck.IsChecked = False
        ISteamItemBox.Text = String.Empty
        Mouse.OverrideCursor = Input.Cursors.Arrow
        UpdateModsView()
    End Sub

    Private Async Sub SteamMods_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        If My.Settings.steamMods.Count > 0 And My.Settings.checkForModUpdates
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
        End If
    End Sub
End Class
