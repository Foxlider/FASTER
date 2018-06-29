Imports System.Text.RegularExpressions

Class MainWindow
    Private Sub ServerProfLabel_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles serverProfilesLabel.MouseDoubleClick
        CreateNewServerProfile(InputBox("Enter profile name:", "New Server Profile"))
    End Sub

    Public Sub CreateNewServerProfile(profileName As String)
        Dim safeProfileName As String = SafeName(profileName)

        Dim newServer As New ListBoxItem With {
            .Content = profileName,
            .Name = safeProfileName & "Select"
        }

        serverProfilesList.Items.Add(newServer)

        AddHandler newServer.Selected, AddressOf ServerProfile_Selected

        Dim newTab As New TabItem With {
            .Name = safeProfileName,
            .Content = profileName
            }

        mainContent.Items.Add(newTab)
    End Sub

    Public Sub ServerProfile_Selected(sender As ListBoxItem, e As RoutedEventArgs) Handles steamUpdaterTabSelect.Selected, serverModsTabSelect.Selected, settingsTabSelect.Selected, toolsTabSelect.Selected, aboutTabSelect.Selected
        Dim menus As New List(Of ListBox) From {
            mainMenuItems,
            serverProfilesList,
            otherMenuItems
        }

        For Each list In menus
            For Each item As ListBoxItem In list.Items
                If item.Name IsNot sender.Name Then
                    item.IsSelected = False
                End If
            Next
        Next

        For Each item As TabItem In mainContent.Items
            If item.Name = sender.Name.Replace("Select", "") Then
                mainContent.SelectedItem = item
            End If
        Next
    End Sub

    Public Function SafeName(ByVal strIn As String) As String
        Try
            Return Regex.Replace(strIn, "[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5))
        Catch __unusedRegexMatchTimeoutException1__ As RegexMatchTimeoutException
            Return String.Empty
        End Try
    End Function
End Class
