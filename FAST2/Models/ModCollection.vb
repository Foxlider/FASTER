Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ModCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String = "Main"

        <XmlElement(Order:=2, ElementName:="SteamMod")>
        Public SteamMods As List(Of SteamMod) = New List(Of SteamMod)()

        <XmlElement(Order:=3, ElementName:="LocalMod")>
        Public LocalMods As List(Of LocalMod) = New List(Of LocalMod)()
        
        Public Shared Sub AddLocalMod()
            Dim path As String = MainWindow.SelectFolder()
            Dim duplicate = False
            Dim currentMods = GetLocalMods()

            If path IsNot Nothing
                Dim modName = path.Substring(path.LastIndexOf("@", StringComparison.Ordinal) + 1)
                
                If currentMods.Count > 0 Then
                    For Each localMod In currentMods
                        If localMod.Name = modName
                            duplicate = true
                        End If
                    Next
                End If
           
                If Not duplicate Then
                    currentMods.Add(New LocalMod(modName, path))

                    My.Settings.LocalMods = currentMods
                Else
                    MainWindow.Instance.IMessageDialog.IsOpen = True
                    MainWindow.Instance.IMessageDialogText.Text = "Mod already imported."
                End If

                My.Settings.Save()
            End If
            
        End Sub
        Private Shared Function GetSteamMods() As List(Of SteamMod)
            Dim currentSteamMods As New List(Of SteamMod)

            If My.Settings.steamMods IsNot Nothing
                currentSteamMods = My.Settings.steamMods
            End If

            Return currentSteamMods
        End Function
        Private Shared Function GetLocalMods() As List(Of LocalMod)
            Dim currentLocalMods As New List(Of LocalMod)

            If My.Settings.localMods IsNot Nothing
                currentLocalMods = My.Settings.localMods
            End If

            Return currentLocalMods
        End Function

        Public Shared Sub AddSteamMod(modUrl As String)
            If modUrl  Like "http*://steamcommunity.com/*/filedetails/?id=*" Then
                Dim duplicate = False
                Dim currentMods = GetSteamMods()
                
                Dim modId = modUrl.Substring(modUrl.IndexOf("?id=", StringComparison.Ordinal))
                modId = Integer.Parse(Regex.Replace(modId, "[^\d]", ""))

                If currentMods.Count > 0 Then
                    For Each steamMod In currentMods
                        If steamMod.WorkshopId = modId
                            duplicate = true
                        End If
                    Next
                End If

                If Not duplicate Then
                    Try
                       Dim modInfo = GetModInfo(modId)
                        
                        If modInfo IsNot Nothing
                            Dim modName = modInfo.Item1
                            Dim steamUpdateTime = modInfo.Item3
                            Dim author = modInfo.Item2
                            
                            currentMods.Add(New SteamMod(modId, modName, author, steamUpdateTime, 0))

                            My.Settings.SteamMods = currentMods
                            My.Settings.Save()
                        Else
                            MainWindow.Instance.IMessageDialog.IsOpen = True
                            MainWindow.Instance.IMessageDialogText.Text = "This is a workshop Item for a different game."
                        End If
                    Catch ex As Exception
                        MsgBox("An exception occurred:" & vbCrLf & ex.Message)
                    End Try
                Else
                    MainWindow.Instance.IMessageDialog.IsOpen = True
                    MainWindow.Instance.IMessageDialogText.Text = "Mod already imported."
                End If
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "Please use format: https://steamcommunity.com/sharedfiles/filedetails/?id=*********"
            End If
        End Sub

        Private Shared Function GetModInfo(modId) As Tuple(Of String, String, String)
            Dim sourceString, appName, author, modName, steamUpdateTime As String
            Dim authorStart, authorEnd, authorLength As Integer
            Dim url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" & modId

            sourceString = New WebClient().DownloadString(url)

            appName = sourceString.Substring(sourceString.IndexOf("content=" & ControlChars.Quote & "Steam Workshop:", StringComparison.Ordinal) + 25, 6)
                        
            If appName Like "Arma 3" Then
                            
                modName = sourceString.Substring(sourceString.IndexOf("<title>Steam Workshop :: ", StringComparison.Ordinal) + 25)
                modName = StrReverse(modName)
                modName = modName.Substring(modName.IndexOf(">eltit/<", StringComparison.Ordinal) + 8)
                modName = StrReverse(modName)

                authorStart = sourceString.IndexOf("myworkshopfiles/?appid=107410"">", StringComparison.Ordinal) +31
                authorEnd = sourceString.IndexOf("'s Workshop</a>", StringComparison.Ordinal)
                authorLength = authorEnd - authorStart
                author = sourceString.Substring(authorStart, authorLength)

                Dim modInfo = MainWindow.GetModInfo(modId)

                steamUpdateTime = modInfo.Substring(modInfo.IndexOf("""time_updated"":") + 15, 10)
                
                Return New Tuple(Of String,String,String)(modName, author, steamUpdateTime)

            Else
                Return Nothing
            End If
        End Function

        Public Shared Sub UpdateInfoFromSteam()
            If My.Settings.steamMods.Count > 0
                Dim currentMods = My.Settings.steamMods

                For Each steamMod In My.Settings.steamMods
                    If Not steamMod.PrivateMod
                        Dim modInfo = GetModInfo(steamMod.WorkshopId)

                        If modInfo IsNot Nothing
                            Dim updateMod = currentMods.Find(function(c) c.WorkshopId = steamMod.WorkshopId)

                            updateMod.Name = modInfo.Item1
                            updateMod.Author = modInfo.Item2
                            updateMod.SteamLastUpdated = modInfo.Item3

                            If updateMod.SteamLastUpdated > updateMod.LocalLastUpdated
                                updateMod.Status = "Update Required"
                            Else
                                updateMod.Status = "Up to Date"
                            End If
                        End If
                    End If
                Next

                My.Settings.SteamMods = currentMods
                My.Settings.Save()

            End If
        End Sub
    End Class
    
    <Serializable()>
    Public Class SteamMod
        Private Sub New()
        End Sub

        Public Sub New(workshopId As Int32, name As String, author As String, steamLastUpdated As Int32, Optional privateMod As Boolean = False)
            Me.WorkshopId = workshopId
            Me.Name = name
            Me.Author = author
            Me.SteamLastUpdated = steamLastUpdated
            Me.PrivateMod = privateMod
        End Sub

        Public Property WorkshopId As Integer = Nothing

        Public Property Name As String = String.Empty

        Public Property Author As String = String.Empty

        Public Property SteamLastUpdated As Integer = Nothing

        Public Property LocalLastUpdated As Integer = 0

        Public Property PrivateMod As Boolean = False

        Public Property Status As String = "Not Installed"
    End Class

   
End Namespace