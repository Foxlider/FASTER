Imports System.Text.RegularExpressions

Class MainWindow
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

    ''' <summary>
    ''' Function to cleanup strings to remove all characters except (aA-zZ 0-9) (-) and (_).
    ''' Optional parameters to allow ignoring of whitespace and definition of replacement char.
    ''' </summary>
    ''' <param name="input">The string being cleaned up.</param>
    ''' <param name="ignoreWhiteSpace">If true white space is left in the string.</param>
    ''' <param name="replacement">Char to replace illegal characters with.</param>
    ''' <returns>Clean string</returns>
    Public Function SafeName(input As String, Optional ignoreWhiteSpace As Boolean = False, Optional replacement As Char = "") As String
        If ignoreWhiteSpace Then
            Return Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement)
        Else
            Return Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement)
        End If

    End Function

    Private Sub MainWindow_LayoutUpdated(sender As Object, e As EventArgs) Handles Me.LayoutUpdated
        'Sets max height of server profile containers to ensure no overflow to other menus
        serverProfilesRow.MaxHeight = Me.Height - 149
        serverProfilesList.MaxHeight = serverProfilesRow.ActualHeight - 50

        'Moves button to add new server profile as the menu expands
        Dim newMargin As Thickness = newServerProfileButton.Margin
        newMargin.Left = menuColumn.ActualWidth - 130
        newServerProfileButton.Margin = newMargin

        steamProgressBar.Height = steamCancelButton.ActualHeight

        steamDirBox.Width = steamGroup.ActualWidth - 70
        serverDirBox.Width = steamGroup.ActualWidth - 70
    End Sub

    Private Sub NewServerProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles newServerProfileButton.Click
        CreateNewServerProfile(InputBox("Enter profile name:", "New Server Profile"))
    End Sub

    Private Sub DirButton_Click(sender As Object, e As RoutedEventArgs) Handles steamDirButton.Click, serverDirButton.Click
        Dim path As String = SelectFolder()

        If path IsNot Nothing Then
            If sender Is steamDirButton Then
                steamDirBox.Text = path
            ElseIf sender Is serverDirButton Then
                serverDirBox.Text = path
            End If
        End If
    End Sub


    Public Function SelectFolder()
        Dim folderDialog As New Forms.FolderBrowserDialog

        If folderDialog.ShowDialog = vbOK Then
            Return folderDialog.SelectedPath
        Else
            Return Nothing
        End If
    End Function

End Class
