Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ModCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String

        <XmlElement(Order:=2, ElementName:="SteamMod")>
        Public SteamMods As List(Of SteamMod) = New List(Of SteamMod)()

        <XmlElement(Order:=3, ElementName:="LocalMod")>
        Public LocalMods As List(Of LocalMod) = New List(Of LocalMod)()
        
        Public Shared Sub AddLocalMod()
            Dim path As String = MainWindow.SelectFolder()
            Dim duplicate = False
            Dim currentMods As New ModCollection

            If path IsNot Nothing
                Dim modName = path.Substring(path.LastIndexOf("@", StringComparison.Ordinal) + 1)


                If My.Settings.Mods IsNot Nothing
                    currentMods = My.Settings.Mods
                End If

                If currentMods.LocalMods.Count > 0 Then
                    For Each localMod In currentMods.LocalMods
                        If localMod.Name = modName
                            duplicate = true
                        End If
                    Next
                End If
           
                If Not duplicate Then
                    currentMods.LocalMods.Add(New LocalMod(modName, path))

                    My.Settings.Mods = currentMods
                Else
                    MsgBox("Mod already imported.")
                End If

                My.Settings.Save()
            End If
            
        End Sub

        Public Shared Sub AddSteamMod(modUrl As String)
            If modUrl  Like "http*://steamcommunity.com/*/filedetails/?id=*" Then
                Dim duplicate = False
                Dim currentMods As New ModCollection

                If My.Settings.Mods IsNot Nothing
                     currentMods = My.Settings.Mods
                End If

                Dim modId = modUrl.Substring(modUrl.IndexOf("?id=", StringComparison.Ordinal))
                modId = Integer.Parse(Regex.Replace(modId, "[^\d]", ""))

                If currentMods.SteamMods.Count > 0 Then
                    For Each steamMod In currentMods.SteamMods
                        If steamMod.WorkshopId = modId
                            duplicate = true
                        End If
                    Next
                End If

                If Not duplicate Then
                    Try
                        Dim modName As String
                        Dim appName As String
                        Dim sourceString As String

                        sourceString = New WebClient().DownloadString(modUrl)

                        appName = sourceString.Substring(sourceString.IndexOf("content=" & ControlChars.Quote & "Steam Workshop:", StringComparison.Ordinal) + 25, 6)
                        
                        If appName Like "Arma 3" Then
                            
                            modName = sourceString.Substring(sourceString.IndexOf("<title>Steam Workshop :: ", StringComparison.Ordinal) + 25)
                            modName = StrReverse(modName)
                            modName = modName.Substring(modName.IndexOf(">eltit/<", StringComparison.Ordinal) + 8)
                            modName = StrReverse(modName)
                            modName = MainWindow.SafeName(modName)

                            Dim modInfo = MainWindow.GetModInfo(modId)

                            Dim steamUpdateTime = modInfo.Substring(modInfo.IndexOf("""time_updated"":") + 15, 10)

                            currentMods.SteamMods.Add(New SteamMod(modId, modName, "Unknown", steamUpdateTime, 0))

                            My.Settings.Mods = currentMods
                        Else
                            MessageBox.Show("This is a workshop Item for a different game.")
                        End If
                    Catch ex As Exception
                        MsgBox("An exception occurred:" & vbCrLf & ex.Message)
                    End Try
                Else
                    MsgBox("Mod already imported.")
                End If

            Else
                MessageBox.Show("Please use format: https://steamcommunity.com/sharedfiles/filedetails/?id=*********")
            End If

            My.Settings.Save()
        End Sub
    End Class
    
    <Serializable()>
    Public Class SteamMod
        Private Sub New()
        End Sub

        Public Sub New(workshopId As Int32, name As String, author As String, steamLastUpdated As Int32, localLastUpdated As Int32, Optional privateMod As Boolean = False)
            Me.WorkshopId = workshopId
            Me.Name = name
            Me.Author = author
            Me.SteamLastUpdated = steamLastUpdated
            Me.LocalLastUpdated = localLastUpdated
            Me.PrivateMod = privateMod
        End Sub

        <XmlElement(Order:=1)>
        Public Property WorkshopId As Integer = Nothing

        <XmlElement(Order:=2)>
        Public Property Name As String = String.Empty

        <XmlElement(Order:=3)>
        Public Property Author As String = String.Empty

        <XmlElement(Order:=4)>
        Public Property SteamLastUpdated As Integer = Nothing

        <XmlElement(Order:=5)>
        Public Property LocalLastUpdated As Integer = Nothing

        <XmlElement(Order:=6)>
        Public Property PrivateMod As Boolean = False
    End Class

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
        
        <XmlElement(Order:=1)>
        Public Property Name As String = String.Empty

        <XmlElement(Order:=2)>
        Public Property Path As String = String.Empty

        <XmlElement(Order:=3)>
        Public Property Author As String = String.Empty

        <XmlElement(Order:=4)>
        Public Property Website As String = String.Empty
    End Class
End Namespace