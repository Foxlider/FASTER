Imports System.Threading
Imports FAST2.Models

Class ServerProfileTab
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

    'Manages actions for steam mods tab buttons
    Private Sub IServerActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged
        
        If ISaveProfile.IsSelected
            My.Settings.Save
        ElseIf IRenameProfile.IsSelected

        ElseIf IDeleteProfile.IsSelected

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
 End Class
