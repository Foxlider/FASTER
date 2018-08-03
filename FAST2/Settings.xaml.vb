Imports System.Windows.Forms
Imports FAST2.Models

Class Settings
    'Switches base theme between light and dark when control is switched 
    Private Sub IBaseThemeButton_Click(sender As Object, e As RoutedEventArgs) Handles IBaseThemeToggle.Click
        Theme.SwitchBase(IBaseThemeToggle.IsChecked)
        MainWindow.Instance.IWindowCloseButton.Background = FindResource("MaterialDesignPaper")
    End Sub

    Private Shared Sub PrimaryColour_Click(sender As Controls.Button,  e As RoutedEventArgs) Handles  IYellowP.Click, IAmberP.Click ,IDeepOrangeP.Click, ILightBlueP.Click, ITealP.Click, ICyanP.Click, IPinkP.Click, IGreenP.Click, IDeepPurpleP.Click, IIndigoP.Click, ILightGreenP.Click, IBlueP.Click, ILimeP.Click, IRedP.Click, IOrangeP.Click, IPurpleP.Click, IBlueGreyP.Click, IGreyP.Click
        Dim colour As String = sender.Name
        colour = colour.Substring(1, colour.Length - 2)

        Theme.ApplyPrimary(colour)
        My.Settings.primaryColour = colour
        My.Settings.Save()
    End Sub

    Private Shared Sub AccentColour_Click(sender As Controls.Button,  e As RoutedEventArgs) Handles IYellowA.Click, IAmberA.Click, IDeepOrangeA.Click, ILightBlueA.Click, ITealA.Click, ICyanA.Click, IPinkA.Click, IGreenA.Click, IDeepPurpleA.Click, IIndigoA.Click, ILightGreenA.Click, IBlueA.Click, ILimeA.Click, IRedA.Click, IOrangeA.Click, IPurpleA.Click
        Dim colour As String = sender.Name
        colour = colour.Substring(1,colour.Length-2)

        Theme.ApplyAccent(colour)
        My.Settings.accentColour = colour
        My.Settings.Save()
    End Sub

    Private Shared Sub IClearSettings_Checked(sender As Object, e As RoutedEventArgs) Handles IClearSettings.Click
        Dim result As Integer = Windows.MessageBox.Show("This will reset FAST and remove all server profiles and imported mods.", "Are you sure?", MessageBoxButtons.YesNo)
        If result = DialogResult.Yes Then
            My.Settings.clearSettings = true
            Forms.Application.Restart()
            Windows.Application.Current.Shutdown()
        End If
    End Sub

    Private Sub SettingsTab_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IBaseThemeToggle.IsChecked = My.Settings.isDark
    End Sub
End Class
