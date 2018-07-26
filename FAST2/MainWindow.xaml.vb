Imports System.Text.RegularExpressions
Imports System.Threading

Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ISteamDirBox.Text = My.Settings.steamCMDPath
        ISteamUserBox.Text = My.Settings.steamUserName
        ISteamPassBox.Text = My.Settings.steamPassword
        IServerDirBox.Text = My.Settings.serverPath
    End Sub


    'Takes any string and removes illegal characters
    Private Shared Function SafeName(input As String, Optional ignoreWhiteSpace As Boolean = False, Optional replacement As Char = "_") As String
        If ignoreWhiteSpace Then
            input = Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement)
            input = Replace(input, replacement & replacement, replacement)
            Return input
        Else
            input = Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement)
            input = Replace(input, replacement & replacement, replacement)
            Return input
        End If

    End Function

    'Creates new server profile in menu and tab control
    Private Sub CreateNewServerProfile(profileName As String)
        Dim safeProfileName As String = SafeName(profileName)

        Dim newServer As New ListBoxItem With {
            .Content = profileName,
            .Name = safeProfileName & "Select"
        }

        IServerProfilesList.Items.Add(newServer)

        AddHandler newServer.Selected, AddressOf MenuItemm_Selected

        Dim tabControls = New ServerProfileTab

        Dim newTab As New TabItem With {
            .Name = safeProfileName,
            .Content = tabControls
        }

        IMainContent.Items.Add(newTab)
        IMainContent.SelectedItem = newTab
    End Sub

    'Opens Folder select dialog and returns selected path
    Public Shared Function SelectFolder()
        Dim folderDialog As New Forms.FolderBrowserDialog

        If folderDialog.ShowDialog = vbOK Then
            Return folderDialog.SelectedPath
        Else
            Return Nothing
        End If
    End Function

    'Handles when any menu item is selected
    Private Sub MenuItemm_Selected(sender As ListBoxItem, e As RoutedEventArgs) Handles ISteamUpdaterTabSelect.Selected, ISteamModsTabSelect.Selected, ISettingsTabSelect.Selected, IToolsTabSelect.Selected, IAboutTabSelect.Selected
        Dim menus As New List(Of ListBox) From {
            IMainMenuItems,
            IServerProfilesList,
            IOtherMenuItems
        }

        For Each list In menus
            For Each item As ListBoxItem In list.Items
                If item.Name IsNot sender.Name Then
                    item.IsSelected = False
                End If
            Next
        Next

        For Each item As TabItem In IMainContent.Items
            If item.Name = sender.Name.Replace("Select", "") Then
                IMainContent.SelectedItem = item
            End If
        Next
    End Sub

    'Updates UI elements when window is resized/ re-rendered
    Private Sub MainWindow_LayoutUpdated(sender As Object, e As EventArgs) Handles Me.LayoutUpdated
        If Not WindowState = WindowState.Minimized Then
            'Sets max height of server profile containers to ensure no overflow to other menus
            IServerProfilesRow.MaxHeight = Height - 149
            IServerProfilesList.MaxHeight = IServerProfilesRow.ActualHeight - 50

            'Moves button to add new server profile as the menu expands
            Dim newMargin As Thickness = INewServerProfileButton.Margin
            newMargin.Left = IMenuColumn.ActualWidth - 130
            INewServerProfileButton.Margin = newMargin

            'Moves folder select buttons as the menu expands
            ISteamDirBox.Width = ISteamGroup.ActualWidth - 70
            IServerDirBox.Width = ISteamGroup.ActualWidth - 70
        End If
    End Sub

    'Creates a new Server Profile and adds it to the UI menu
    Private Sub NewServerProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles INewServerProfileButton.Click
        Dim profileName As String = InputBox("Enter profile name:", "New Server Profile")

        If profileName = Nothing Then
            MsgBox("Please Enter A Value")
        Else
            CreateNewServerProfile(profileName)
        End If

    End Sub

    'Makes close button red when mouse is over button
    Private Sub WindowCloseButton_MouseEnter(sender As Object, e As MouseEventArgs) Handles IWindowCloseButton.MouseEnter
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#D72C2C"), Brush)

        IWindowCloseButton.Background = brush
    End Sub

    'Changes colour of close button back to UI base when mouse leaves button
    Private Sub WindowCloseButton_MouseLeave(sender As Object, e As MouseEventArgs) Handles IWindowCloseButton.MouseLeave
        Dim brush = FindResource("MaterialDesignPaper")

        IWindowCloseButton.Background = brush
    End Sub

    'Closes app when using custom close button
    Private Sub WindowCloseButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowCloseButton.Selected
        Close()
    End Sub

    'Minimises app when using custom minimise button
    Private Sub WindowMinimizeButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowMinimizeButton.Selected
        IWindowMinimizeButton.IsSelected = False
        WindowState = WindowState.Minimized
    End Sub

    'Allows user to move the window around using the custom nav bar
    Private Sub WindowDragBar_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles IWindowDragBar.MouseLeftButtonDown, ILogoImage.MouseLeftButtonDown, IWindowTitle.MouseLeftButtonDown
        DragMove()
    End Sub

    'Opens folder select dialog when clicking certain buttons
    Private Sub DirButton_Click(sender As Object, e As RoutedEventArgs) Handles ISteamDirButton.Click, IServerDirButton.Click
        Dim path As String = SelectFolder()

        If path IsNot Nothing Then
            If sender Is ISteamDirButton Then
                ISteamDirBox.Text = path
            ElseIf sender Is IServerDirButton Then
                IServerDirBox.Text = path
            End If
        End If
    End Sub

    'Switches base theme between light and dark when control is switched 
    Private Sub IBaseThemeButton_Click(sender As Object, e As RoutedEventArgs) Handles IBaseThemeToggle.Click
        Theme.SwitchBaseTheme(IBaseThemeToggle.IsChecked)
        IWindowCloseButton.Background = FindResource("MaterialDesignPaper")
    End Sub

    'Turns toggle button groups into normal buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IActionButtons.SelectionChanged
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
End Class



