Imports FAST2.Models

Class Settings
    'Switches base theme between light and dark when control is switched 
    Private Sub IBaseThemeButton_Click(sender As Object, e As RoutedEventArgs) Handles IBaseThemeToggle.Click
        Theme.SwitchBase(IBaseThemeToggle.IsChecked)
        MainWindow.Instance.IWindowCloseButton.Background = FindResource("MaterialDesignPaper")
    End Sub

    Private Shared Sub PrimaryColour_Click(sender As Button,  e As RoutedEventArgs) Handles  IYellowP.Click, IAmberP.Click ,IDeepOrangeP.Click, ILightBlueP.Click, ITealP.Click, ICyanP.Click, IPinkP.Click, IGreenP.Click, IDeepPurpleP.Click, IIndigoP.Click, ILightGreenP.Click, IBlueP.Click, ILimeP.Click, IRedP.Click, IOrangeP.Click, IPurpleP.Click, IBlueGreyP.Click, IGreyP.Click
        Dim colour As String = sender.Name
        colour = colour.Substring(1, colour.Length - 2)

        Theme.ApplyPrimary(colour)
        My.Settings.primaryColour = colour
        My.Settings.Save()
    End Sub

    Private Shared Sub AccentColour_Click(sender As Button,  e As RoutedEventArgs) Handles IYellowA.Click, IAmberA.Click, IDeepOrangeA.Click, ILightBlueA.Click, ITealA.Click, ICyanA.Click, IPinkA.Click, IGreenA.Click, IDeepPurpleA.Click, IIndigoA.Click, ILightGreenA.Click, IBlueA.Click, ILimeA.Click, IRedA.Click, IOrangeA.Click, IPurpleA.Click
        Dim colour As String = sender.Name
        colour = colour.Substring(1,colour.Length-2)

        Theme.ApplyAccent(colour)
        My.Settings.accentColour = colour
        My.Settings.Save()
    End Sub

    Private Sub IClearSettings_Click(sender As Object, e As RoutedEventArgs) Handles IClearSettings.Click
        IResetDialog.IsOpen = True
    End Sub
    
    Private Sub IResetButton_Click(sender As Object, e As RoutedEventArgs) Handles IResetButton.Click
        My.Settings.clearSettings = true
        Forms.Application.Restart()
        Windows.Application.Current.Shutdown()
    End Sub

    Private Sub Settings_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        IExcludeServerFolder.IsChecked = My.Settings.excludeServerFolder
        IBaseThemeToggle.IsChecked = My.Settings.isDark
        IUpdatesOnLaunch.IsChecked = My.Settings.checkForModUpdates
        UpdateLocalModFolders()
    End Sub
    
    Private Sub UpdateLocalModFolders()
        ILocalModFolders.Items.Clear()

        If My.Settings.localModFolders.Count > 0
            For Each folder in My.Settings.localModFolders
                ILocalModFolders.Items.Add(folder)
            Next
        End If
    End Sub

    Private Sub INewLocalFolder_Click(sender As Object, e As RoutedEventArgs) Handles INewLocalFolder.Click
        My.Settings.localModFolders.Add(MainWindow.SelectFolder())
        UpdateLocalModFolders()
    End Sub


    Private Sub IRemoveLocalFolders_Click(sender As Object, e As RoutedEventArgs) Handles IRemoveLocalFolders.Click
        For Each folder In ILocalModFolders.SelectedItems
           My.Settings.localModFolders.Remove(folder)
        Next
        UpdateLocalModFolders()
    End Sub
    
    Private Sub IExcludeServerFolder_Click(sender As Object, e As RoutedEventArgs) Handles IExcludeServerFolder.Click
        If IExcludeServerFolder.IsChecked
            My.Settings.excludeServerFolder = True
        Else
            My.Settings.excludeServerFolder = False
        End If
        My.Settings.Save()
    End Sub

    Private Sub IUpdatesOnLaunch_Click(sender As Object, e As RoutedEventArgs) Handles IUpdatesOnLaunch.Click
        If IUpdatesOnLaunch.IsChecked
            My.Settings.checkForModUpdates = True
        Else
            My.Settings.checkForModUpdates = False
        End If
        My.Settings.Save()
    End Sub
End Class
