Imports System.IO
Imports System.Threading
Imports FAST2.Models

Public Class SteamMods

    'Manages actions for steam mods tab buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IModActionButtons.SelectionChanged

        If ICheckForUpdates.IsSelected Then
            CheckForUpdates()
        ElseIf IUpdateAll.IsSelected Then
            UpdateAllMods()
        ElseIf IImportLauncherFile.IsSelected Then
            ImportLauncherFile()
        ElseIf IAddSteamMod.IsSelected Then
            IImportSteamModDialog.IsOpen = True
        End If

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

    Private Sub SteamMods_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If My.Settings.steamMods.Count > 0 Then
            UpdateModsView()
        End If
    End Sub

    Private Sub IImportSteamModDialog_KeyUp(sender As Object, e As KeyEventArgs) Handles IImportSteamModDialog.KeyUp
        If e.Key = Key.Escape Then
            IImportSteamModDialog.IsOpen = False
            IPrivateModCheck.IsChecked = False
            ISteamItemBox.Text = String.Empty
        End If
    End Sub

    Private Sub IImportModButton_Click(sender As Object, e As RoutedEventArgs) Handles IImportModButton.Click
        Mouse.OverrideCursor = Cursors.Wait
        IImportSteamModDialog.IsOpen = False
        SteamMod.AddSteamMod(ISteamItemBox.Text)
        IPrivateModCheck.IsChecked = False
        ISteamItemBox.Text = String.Empty
        Mouse.OverrideCursor = Cursors.Arrow
        UpdateModsView()
    End Sub

    Private Async Sub SteamMods_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        If My.Settings.steamMods.Count > 0 And My.Settings.checkForModUpdates Then
            IUpdateProgress.IsIndeterminate = True
            IModView.IsEnabled = False
            IProgressInfo.Visibility = Visibility.Visible
            IProgressInfo.Content = "Checking for updates..."

            Dim tasks As New List(Of Task) From {
                    Task.Run(
                        Sub()
                            SteamMod.UpdateInfoFromSteam()
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

    Private Async Sub ImportLauncherFile()
        IUpdateProgress.IsIndeterminate = True
        IModView.IsEnabled = False
        IProgressInfo.Visibility = Visibility.Visible
        IProgressInfo.Content = "Importing Mods..."

        Dim tasks As New List(Of Task) From {
                Task.Run(
                    Sub()
                        OpenLauncherFile()
                    End Sub
                    )
                }

        Await Task.WhenAll(tasks)

        IModView.IsEnabled = True
        IProgressInfo.Visibility = Visibility.Collapsed
        IUpdateProgress.IsIndeterminate = False
        UpdateModsView()
    End Sub

    Private Shared Sub OpenLauncherFile()
        Dim modsFile = Functions.SelectFile("Arma 3 Launcher File|*.html")

        If modsFile IsNot String.Empty Then
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
                    SteamMod.AddSteamMod(link, True)
                End If
            Loop
            dataReader.Close()
        End If
    End Sub

    Private Shared Sub UpdateAllMods()
        If MainWindow.Instance.ReadyToUpdate Then
            Dim modsToUpdate = New List(Of String)

            For Each steamMod In My.Settings.steamMods
                If steamMod.SteamLastUpdated > steamMod.LocalLastUpdated Then
                    modsToUpdate.Add(steamMod.WorkshopId)

                    UpdateMod(steamMod.WorkshopId, steamMod.Name, False)
                End If
            Next

            If modsToUpdate.Count > 0 Then
                Dim steamCommand As String = "+login " & MainWindow.Instance.ISteamUserBox.Text & " " & MainWindow.Instance.ISteamPassBox.Password

                For Each steamMod In modsToUpdate
                    steamCommand = steamCommand & " +workshop_download_item 107410 " & steamMod
                Next

                Dim steamCmd As String = MainWindow.Instance.ISteamDirBox.Text + "\steamcmd.exe"
                steamCommand = steamCommand & " validate +quit"
                MainWindow.Instance.RunSteamCommand(steamCmd, steamCommand, "addon", modsToUpdate)
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "No Mods to Update"
            End If
        Else
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Check all fields are correctly filled out on Steam Updater"
        End If
    End Sub

    Private Shared Sub UpdateMod(modId As Int32, modName As String, Optional singleMod As Boolean = True)
        If MainWindow.Instance.ReadyToUpdate Then
            Dim modPath As String = My.Settings.steamCMDPath & "\steamapps\workshop\content\107410\" & modId

            If Not Directory.Exists(modPath) Then
                Directory.CreateDirectory(modPath)
            End If

            Try
                Dim linkCommand As String
                Dim linkPath As String

                Directory.CreateDirectory(modPath)

                linkPath = My.Settings.serverPath & "\@" & Functions.SafeName(modName)
                linkCommand = "/c mklink /D " & linkPath & " " & modPath

                Process.Start("cmd", linkCommand)

            Catch ex As Exception
                MsgBox("An exception occurred:" & vbCrLf & ex.Message)
            End Try

            If singleMod Then
                Dim steamCmd As String = MainWindow.Instance.ISteamDirBox.Text + "\steamcmd.exe"
                Dim steamCommand As String = "+login " & MainWindow.Instance.ISteamUserBox.Text & " " & MainWindow.Instance.ISteamPassBox.Password & " +workshop_download_item 107410 " & modId & " validate +quit"

                Dim modIDs As New List(Of String) From {
                        modId
                        }

                MainWindow.Instance.RunSteamCommand(steamCmd, steamCommand, "addon", modIDs)
            End If
        Else
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Check all fields are correctly filled out on Steam Updater"
        End If
    End Sub

    Private Async Sub CheckForUpdates()
        If My.Settings.steamMods.Count > 0 Then
            IUpdateProgress.IsIndeterminate = True
            IModView.IsEnabled = False
            IProgressInfo.Visibility = Visibility.Visible
            IProgressInfo.Content = "Checking for updates..."

            Dim tasks As New List(Of Task) From {
                    Task.Run(
                        Sub()
                            SteamMod.UpdateInfoFromSteam()
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


    Private Sub UpdateModsView()
        IModView.Items.Clear()

        If My.Settings.steamMods IsNot Nothing Then

            For Each steamMod In My.Settings.steamMods
                IModView.Items.Add(steamMod)
            Next
        End If
    End Sub

    Private Sub DeleteMod(sender As Object, e As RoutedEventArgs)
        Dim steamMod = CType((CType(e.Source, Button)).DataContext, SteamMod)

        If Directory.Exists(My.Settings.steamCMDPath & "\steamapps\workshop\content\107410\" & steamMod.WorkshopId) Then
            Directory.Delete(My.Settings.steamCMDPath & "\steamapps\workshop\content\107410\" & steamMod.WorkshopId, True)
        End If

        If Directory.Exists(My.Settings.serverPath & "\@" & Functions.SafeName(steamMod.Name)) Then
            Directory.Delete(My.Settings.serverPath & "\@" & Functions.SafeName(steamMod.Name), True)
        End If

        SteamMod.DeleteSteamMod(steamMod.WorkshopId)
        UpdateModsView()
    End Sub

    Private Sub UpdateMod(sender As Object, e As RoutedEventArgs)
        Dim steamMod = CType((CType(e.Source, Button)).DataContext, SteamMod)

        UpdateMod(steamMod.WorkshopId, steamMod.Name)
    End Sub
End Class
