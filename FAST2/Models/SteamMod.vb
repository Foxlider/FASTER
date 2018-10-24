Imports System.Text.RegularExpressions

Namespace Models

    <Serializable()>
    Public Class SteamMod
        Private Sub New()
        End Sub

        Private Sub New(workshopId As Int32, name As String, author As String, steamLastUpdated As Int32, Optional privateMod As Boolean = False)
            Me.WorkshopId = workshopId
            Me.Name = name
            Me.Author = author
            Me.SteamLastUpdated = steamLastUpdated
            Me.PrivateMod = privateMod
        End Sub

        Public Property WorkshopId As Integer = Nothing
        Public Property Name As String = String.Empty
        Private Property Author As String = String.Empty
        Public Property SteamLastUpdated As Integer = Nothing
        Public Property LocalLastUpdated As Integer = 0
        Private Property PrivateMod As Boolean = False
        Public Property Status As String = "Not Installed"

        Public Shared Sub DeleteSteamMod(workshopId As Int32)
            Dim currentMods = GetSteamMods()

            currentMods.RemoveAll(Function(x) x.WorkshopId = workshopId)

            My.Settings.Save()
        End Sub

        Public Shared Function GetSteamMods() As List(Of SteamMod)
            Dim currentSteamMods As New List(Of SteamMod)

            If My.Settings.steamMods IsNot Nothing
                currentSteamMods = My.Settings.steamMods
            End If

            Return currentSteamMods
        End Function

        Public Shared Sub AddSteamMod(modUrl As String, Optional multiple As Boolean = False)
            If modUrl Like "http*://steamcommunity.com/*/filedetails/?id=*" Then
                Dim duplicate = False
                Dim currentMods = GetSteamMods()

                Dim modId = modUrl.Substring(modUrl.IndexOf("?id=", StringComparison.Ordinal))
                If modId.Contains("&") Then
                    modId = modId.Substring(0, InStr(modId, "&") - 1)
                End If
                modId = Integer.Parse(Regex.Replace(modId, "[^\d]", ""))

                If currentMods.Count > 0 Then
                    For Each steamMod In currentMods
                        If steamMod.WorkshopId = modId
                            duplicate = True
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

                            My.Settings.steamMods = currentMods
                            My.Settings.Save()
                        Else

                            MainWindow.Instance.IMessageDialog.IsOpen = True
                            MainWindow.Instance.IMessageDialogText.Text = "This is a workshop Item for a different game."
                        End If
                    Catch ex As Exception
                        MsgBox("An exception occurred:" & vbCrLf & ex.Message)
                    End Try
                Else
                    If Not multiple
                        MainWindow.Instance.IMessageDialog.IsOpen = True
                        MainWindow.Instance.IMessageDialogText.Text = "Mod already imported."
                    End If
                End If
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "Please use format: https://steamcommunity.com/sharedfiles/filedetails/?id=*********"
            End If
        End Sub

        Private Shared Function GetModInfo(modId) As Tuple(Of String, String, String)
            Dim modInfo = SteamWebApi.GetSingleFileDetails(modId)
            Dim author = SteamWebApi.GetPlayerSummaries(modInfo.SelectToken("creator")).SelectToken("personaname")
            Dim modName = modInfo.SelectToken("title")
            Dim steamUpdateTime =modInfo.SelectToken("time_updated")
            
            If modInfo.SelectToken("creator_appid") = "107410" Then
                Return New Tuple(Of String, String, String)(modName, author, steamUpdateTime)
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
                            Dim updateMod = currentMods.Find(Function(c) c.WorkshopId = steamMod.WorkshopId)

                            updateMod.Name = modInfo.Item1
                            updateMod.Author = modInfo.Item2
                            updateMod.SteamLastUpdated = modInfo.Item3

                            If updateMod.SteamLastUpdated > updateMod.LocalLastUpdated And updateMod.Status IsNot "Download Not Complete" Then
                                updateMod.Status = "Update Required"
                            Else
                                updateMod.Status = "Up to Date"
                            End If
                        End If
                    End If
                Next

                My.Settings.steamMods = currentMods
                My.Settings.Save()
            End If
        End Sub
    End Class
End Namespace