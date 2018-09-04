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
        Dim oldName = IProfileDisplayName.Content
        Dim newName = IProfileDisplayNameEdit.Text
        Dim index = My.Settings.Servers.ServerProfiles.IndexOf(My.Settings.Servers.ServerProfiles.Find(Function(profile) profile.ProfileNameBox = oldName))

        If ServerCollection.RenameServerProfile(oldName,newName)
            MainWindow.Instance.IMainContent.Items.RemoveAt(MainWindow.Instance.IMainContent.SelectedIndex)
            MainWindow.Instance.LoadServerProfiles()
            MainWindow.Instance.IMainContent.SelectedIndex = MainWindow.Instance.IMainContent.Items.Count -1
        Else 
            MsgBox("Error Try Again")
        End If
        

    End Sub

    Private Sub ShowRenameInterface(show As Boolean)
        If show
            IProfileDisplayNameEdit.Text = IProfileDisplayName.Content
            IProfileDisplayName.Visibility = Visibility.Collapsed
            IProfileNameEdit.Visibility = Visibility.Visible
            IProfileDisplayNameEdit.Focus()
            IProfileDisplayNameEdit.SelectAll()
        Else 
            IProfileDisplayNameEdit.Text = String.Empty
            IProfileDisplayName.Visibility = Visibility.Visible
            IProfileNameEdit.Visibility = Visibility.Collapsed
        End If
    End Sub

    'Manages actions for steam mods tab buttons
    Private Sub IServerActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged
        
        If ISaveProfile.IsSelected
            My.Settings.Save

        ElseIf IRenameProfile.IsSelected
            ShowRenameInterface(True)

        ElseIf IDeleteProfile.IsSelected
            MainWindow.Instance.IMainContent.Items.RemoveAt(MainWindow.Instance.IMainContent.SelectedIndex)
            ServerCollection.DeleteServerProfile(_safeName)
            If MainWindow.Instance.IMainContent.Items.Count > 5
                MainWindow.Instance.IMainContent.SelectedIndex = 6
            Else
                MainWindow.Instance.IMainContent.SelectedIndex = 0
            End If

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
