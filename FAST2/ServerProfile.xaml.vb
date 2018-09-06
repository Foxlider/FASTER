Imports System.Threading
Imports System.Windows.Controls.Primitives
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

        Dim servers = My.Settings.Servers.ServerProfiles

        For Each server in servers
            IServerModsList.Items.Add(server.SafeName)
        Next
        
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

    Private Sub UiCheckBoxes(sender As CheckBox, e As RoutedEventArgs) Handles IConsoleLogCheck.Click, IPidCheck.Click, IRankingCheck.Click, IRequiredBuildCheck.Click
        Select Case sender.Name
            Case "IConsoleLogCheck"
                If IConsoleLogCheck.IsChecked
                    IConsoleLogFile.IsEnabled = True
                    IConsoleLogButton.IsEnabled = True
                Else 
                    IConsoleLogFile.IsEnabled = False
                    IConsoleLogButton.IsEnabled = False
                End If
            Case "IPidCheck"
                If IPidCheck.IsChecked
                    IPidFile.IsEnabled = True
                    IPidButton.IsEnabled = True
                Else 
                    IPidFile.IsEnabled = False
                    IPidButton.IsEnabled = False
                End If
            Case "IRankingCheck"
                If IRankingCheck.IsChecked
                    IRakingFile.IsEnabled = True
                    IRankingButton.IsEnabled = True
                Else 
                    IRakingFile.IsEnabled = False
                    IRankingButton.IsEnabled = False
                End If
            Case "IRequiredBuildCheck"
                If IRequiredBuildCheck.IsChecked
                    IRequiredBuild.IsEnabled = True
                Else 
                    IRequiredBuild.IsEnabled = False
                End If
        End Select
    End Sub

    Private Sub UiToggles(sender As ToggleButton, e As RoutedEventArgs) Handles  IHcCheck.Click, IAutoRestartCheck.Click, IVonCheck.Click, IVotingCheck.Click
        Select Case sender.Name
            Case "IHcCheck"
                If IHcCheck.IsChecked
                    IHcIpGroup.IsEnabled = True
                    IHcSliderGroup.IsEnabled = True
                    IHcCheck.ToolTip = "Disable HC"
                Else 
                    IHcIpGroup.IsEnabled = False
                    IHcSliderGroup.IsEnabled = False
                    IHcCheck.ToolTip = "Enable HC"
                End If
            Case "IVonCheck"
                If IVonCheck.IsChecked
                    IVonGroup.IsEnabled = True
                    IVonCheck.ToolTip = "Disable VON"
                Else
                    IVonGroup.IsEnabled = False
                    IVonCheck.ToolTip = "Enable VON"
                End If
            Case "IVotingCheck"
                If IVotingCheck.IsChecked
                    IMinVotePlayers.IsEnabled = True
                    IMinVoteThreshold.IsEnabled = True
                    IVotingCheck.ToolTip = "Disable Voting"
                Else
                    IMinVotePlayers.IsEnabled = False
                    IMinVoteThreshold.IsEnabled = False
                    IVotingCheck.ToolTip = "Enable Voting"
                End If
            Case "IAutoRestartCheck"
                If IAutoRestartCheck.IsChecked
                    IDailyRestartPickerA.IsEnabled = True
                    IDailyRestartPickerB.IsEnabled = True
                    IDailyRestartCheckA.IsEnabled = True
                    IDailyRestartCheckB.IsEnabled = True
                    IAutoRestartCheck.ToolTip = "Disable Auto Restart"
                Else
                    
                    IDailyRestartPickerA.IsEnabled = False
                    IDailyRestartPickerB.IsEnabled = False
                    IDailyRestartCheckA.IsEnabled = False
                    IDailyRestartCheckB.IsEnabled = False
                    IVotingCheck.ToolTip = "Enable Auto Restart"
                End If
                
        End Select

    End Sub
End Class
