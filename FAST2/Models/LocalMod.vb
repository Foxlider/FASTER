Imports System.IO

Namespace Models
    <Serializable()>
    Public Class LocalMod
        Private Sub New()
        End Sub

        Public Sub New(name As String, path As String, Optional author As String = "Unknown", Optional website As String = Nothing)
            Me.Name = name
            Me.Path = path
            Me.Author = author
            Me.Website = website
        End Sub
        
        Public Property Name As String = String.Empty
        Public Property Path As String = String.Empty
        Public Property Author As String = String.Empty
        Public Property Website As String = String.Empty

        Public Shared Function GetLocalMods(Optional serverPathOnly As Boolean = False) As List(Of LocalMod)
            Dim localMods = New List(Of LocalMod)

            Dim foldersToSearch As New List(Of String)

            If serverPathOnly Then
                If Not My.Settings.excludeServerFolder And My.Settings.serverPath IsNot String.Empty
                    foldersToSearch.Add(My.Settings.serverPath)
                End If
            End If
            
            If Not serverPathOnly Then
                For Each folder In My.Settings.localModFolders
                    If folder IsNot Nothing
                        foldersToSearch.Add(folder)
                    End If
                Next
            End If
        
            If foldersToSearch.Count > 0 
              For Each localModFolder In foldersToSearch
                Dim modFolders = Directory.GetDirectories(localModFolder, "@*")

                localMods.AddRange(From modFolder In modFolders Let name = modFolder.Substring(modFolder.LastIndexOf("@", StringComparison.Ordinal) + 1) Let author = "Unknown" Let website = "Unknown" Select New LocalMod(name, modFolder, author, website))
                Next
            End If

            Return localMods
        End Function

    End Class
End NameSpace