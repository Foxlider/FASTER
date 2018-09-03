Imports System.Threading
Imports FAST2.Models

Class ServerProfile
    Dim ReadOnly _safeName As String
    Dim ReadOnly _displayname As String

    Public Sub New (newSafeName As String, newDisplayName As String)
        _displayname = newDisplayName
        _safeName = newSafeName
        
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        
    End Sub



    'Turns toggle button groups into normal buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged
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

    Private Sub IProfileNameEditSave_Click(sender As Object, e As RoutedEventArgs) Handles IProfileNameEditSave.Click
        Dim currentProfiles = My.Settings.serverProfiles.ServerProfiles
        

        Dim currentProfile = currentProfiles.Find(Function(profile) profile.SafeName = IProfileDisplayName.Content)


    End Sub

    'Manages actions for steam mods tab buttons
    Private Sub IServerActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged
        
        If ISaveProfile.IsSelected
            My.Settings.Save

        ElseIf IRenameProfile.IsSelected
            IProfileDisplayNameEdit.Text = IProfileDisplayName.Content
            IProfileDisplayName.Visibility = Visibility.Collapsed
            IProfileNameEdit.Visibility = Visibility.Visible
            IProfileDisplayNameEdit.Focus()
            IProfileDisplayNameEdit.SelectAll()

        ElseIf IDeleteProfile.IsSelected
            Dim noOfTabs = MainWindow.Instance.IServerProfilesMenu.Items.Count
            If noOfTabs > 1
                MainWindow.Instance.IMainContent.SelectedIndex = noOfTabs + 5
                MainWindow.Instance.IServerProfilesMenu.SelectedIndex = 0
            Else 
                MainWindow.Instance.IMainContent.SelectedIndex = 0
                MainWindow.Instance.IMainMenuItems.SelectedIndex = 0
            End If
            ServerCollection.DeleteServerProfile(_safeName)

        ElseIf IExportProfile.IsSelected

        ElseIf ILaunchServer.IsSelected
            
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

    Private Sub ServerProfileTab_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IProfileDisplayName.Content = _displayName
    End Sub

    
End Class
