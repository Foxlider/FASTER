Imports System.IO
Imports System.Threading

Imports FAST2.Models

Public Class LocalMods

    Private Sub SteamMods_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateModsView()
    End Sub

    'Manages actions for steam mods tab buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IModActionButtons.SelectionChanged
        
        If IRefreshList.IsSelected
            UpdateModsView()
        'ElseIf IAddLocalMod.IsSelected
        '    SteamMod.AddLocalMod()
        ElseIf IEditFolders.IsSelected
            MainWindow.Instance.IMainContent.SelectedIndex = 3
            MainWindow.Instance.ILocalModsTabSelect.IsSelected = False
            MainWindow.Instance.ISettingsTabSelect.IsSelected = True
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

    Private Sub UpdateModsView()
        ILocalModsView.Items.Clear()

        Dim localMods = LocalMod.GetLocalMods()
        Dim serverPathMods = LocalMod.GetLocalMods(True)
        Dim steamMods = SteamMod.GetSteamMods
        Dim modsToRemove = New List(Of LocalMod)

        For Each localMod As LocalMod In serverPathMods
            For Each steamMod As SteamMod in steamMods
                If localMod.Name = Functions.SafeName(steamMod.Name)
                    modsToRemove.Add(localMod)
                End If
            Next
        Next

        For Each remove In modsToRemove
            serverPathMods.RemoveAt(serverPathMods.IndexOf(serverPathMods.Find(Function(m) m.Name = remove.Name)))
        Next
       
        localMods.AddRange(serverPathMods)

        If localMods IsNot Nothing
            For Each localMod In localMods
                ILocalModsView.Items.Add(localMod)
            Next
        End If
    End Sub

    Private Sub DeleteMod(sender As Object, e As RoutedEventArgs)
        Dim localMod = CType(CType(e.Source, Button).DataContext, LocalMod)

        If Directory.Exists(localMod.Path) Then
            Directory.Delete(localMod.Path, True)
        End If

        UpdateModsView()
    End Sub

    Private Sub OpenMod(sender As Object, e As RoutedEventArgs)
        Dim localMod = CType((CType(e.Source, Button)).DataContext, LocalMod)

        If Directory.Exists(localMod.Path) Then
            Process.Start(localMod.Path)
        End If

    End Sub
End Class
