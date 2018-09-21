Imports System.IO
Imports System.Threading

Imports FAST2.Models

Public Class LocalMods

    'Manages actions for steam mods tab buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IModActionButtons.SelectionChanged
        
        If IRefreshList.IsSelected
            UpdateModsView()
        ElseIf IAddLocalMod.IsSelected
            ModCollection.AddLocalMod()
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
      UpdateModsView()
    End Sub

    Private Sub UpdateModsView()
        ILocalModsView.Items.Clear()

        Dim localMods = GetLocalMods
       
        If localMods IsNot Nothing
            For Each localMod In localMods
                ILocalModsView.Items.Add(localMod)
            Next
        End If
    End Sub

    Private Shared Function GetLocalMods() As List(Of LocalMod)
        Dim localMods = New List(Of LocalMod)

        Dim foldersToSearch As New List(Of String)

        For Each folder In My.Settings.localModFolders
            foldersToSearch.Add(folder)
        Next

        foldersToSearch.Add(My.Settings.serverPath)
        
        If foldersToSearch.Count > 0
            For Each localModFolder In foldersToSearch
                Dim modFolders = Directory.GetDirectories(localModFolder, "@*")

                For Each modFolder In modFolders
                    Dim name As String = modFolder.Substring(modFolder.LastIndexOf("@", StringComparison.Ordinal) + 1)
                    Dim author = "Unknown"
                    Dim website = "Unknown"

                    Dim localMod = New LocalMod(name,modFolder,author,website)

                    localMods.Add(localMod)
                Next
            Next
        End If

        Return localMods
    End Function
End Class
