Imports System.Text.RegularExpressions

Class MainWindow
    'Takes any string and removes illegal characters
    Private Shared Function SafeName(input As String, Optional ignoreWhiteSpace As Boolean = False, Optional replacement As Char = "_") As String
        If ignoreWhiteSpace Then
            Return Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement)
        Else
            Return Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement)
        End If

    End Function

    'TEMPORARY Creates new server profile in menu and tab control
    Private Sub CreateNewServerProfile(profileName As String)
        Dim safeProfileName As String = SafeName(profileName)

        Dim newServer As New ListBoxItem With {
            .Content = profileName,
            .Name = safeProfileName & "Select"
        }

        ServerProfilesList.Items.Add(newServer)

        AddHandler newServer.Selected, AddressOf MenuItemm_Selected

        Dim newTab As New TabItem With {
            .Name = safeProfileName,
            .Content = profileName
        }

        MainContent.Items.Add(newTab)
    End Sub

    'Opens Folder select dialog and returns selected path
    Private Shared Function SelectFolder()
        Dim folderDialog As New Forms.FolderBrowserDialog

        If folderDialog.ShowDialog = vbOK Then
            Return folderDialog.SelectedPath
        Else
            Return Nothing
        End If
    End Function

    'Handles when any menu item is selected
    Private Sub MenuItemm_Selected(sender As ListBoxItem, e As RoutedEventArgs) Handles SteamUpdaterTabSelect.Selected, ServerModsTabSelect.Selected, SettingsTabSelect.Selected, ToolsTabSelect.Selected, AboutTabSelect.Selected
        Dim menus As New List(Of ListBox) From {
            MainMenuItems,
            ServerProfilesList,
            OtherMenuItems
        }

        For Each list In menus
            For Each item As ListBoxItem In list.Items
                If item.Name IsNot sender.Name Then
                    item.IsSelected = False
                End If
            Next
        Next

        For Each item As TabItem In MainContent.Items
            If item.Name = sender.Name.Replace("Select", "") Then
                MainContent.SelectedItem = item
            End If
        Next
    End Sub

    'Updates UI elements when window is resized/ re-rendered
    Private Sub MainWindow_LayoutUpdated(sender As Object, e As EventArgs) Handles Me.LayoutUpdated
        If Not WindowState = WindowState.Minimized Then
            'Sets max height of server profile containers to ensure no overflow to other menus
            ServerProfilesRow.MaxHeight = Height - 149
            ServerProfilesList.MaxHeight = ServerProfilesRow.ActualHeight - 50

            'Moves button to add new server profile as the menu expands
            Dim newMargin As Thickness = NewServerProfileButton.Margin
            newMargin.Left = MenuColumn.ActualWidth - 130
            NewServerProfileButton.Margin = newMargin

            'Moves folder select buttons as the menu expands
            SteamDirBox.Width = SteamGroup.ActualWidth - 70
            ServerDirBox.Width = SteamGroup.ActualWidth - 70
        End If
    End Sub

    Private Sub NewServerProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles NewServerProfileButton.Click
        CreateNewServerProfile(InputBox("Enter profile name:", "New Server Profile"))
    End Sub

    Private Sub WindowCloseButton_MouseEnter(sender As Object, e As MouseEventArgs) Handles WindowCloseButton.MouseEnter
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#D72C2C"), Brush)

        WindowCloseButton.Background = brush
    End Sub

    Private Sub WindowCloseButton_MouseLeave(sender As Object, e As MouseEventArgs) Handles WindowCloseButton.MouseLeave
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#FF303030"), Brush)

        WindowCloseButton.Background = brush
    End Sub

    Private Sub WindowCloseButton_Selected(sender As Object, e As RoutedEventArgs) Handles WindowCloseButton.Selected
        Close()
    End Sub

    Private Sub WindowMinimizeButton_Selected(sender As Object, e As RoutedEventArgs) Handles WindowMinimizeButton.Selected
        WindowMinimizeButton.IsSelected = False
        WindowState = WindowState.Minimized
    End Sub

    Private Sub WindowDragBar_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles WindowDragBar.MouseDown
        DragMove()
    End Sub

    Private Sub DirButton_Click(sender As Object, e As RoutedEventArgs) Handles SteamDirButton.Click, ServerDirButton.Click
        Dim path As String = SelectFolder()

        If path IsNot Nothing Then
            If sender Is SteamDirButton Then
                SteamDirBox.Text = path
            ElseIf sender Is ServerDirButton Then
                ServerDirBox.Text = path
            End If
        End If
    End Sub
End Class
