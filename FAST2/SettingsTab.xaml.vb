Imports MaterialDesignColors
Imports MaterialDesignThemes.Wpf

Class SettingsTab
    'Switches base theme between light and dark when control is switched 
    Private Sub IBaseThemeButton_Click(sender As Object, e As RoutedEventArgs) Handles IBaseThemeToggle.Click
        SwitchBaseTheme(IBaseThemeToggle.IsChecked)
        MainWindow.Instance.IWindowCloseButton.Background = FindResource("MaterialDesignPaper")
    End Sub

    'Switches base theme between light and dark
    Public Shared Sub SwitchBaseTheme(isDark)
        Call New PaletteHelper().SetLightDark(isDark)
        My.Settings.isDark = isDark
        My.Settings.Save()
    End Sub

    'Changes palette primary colour
    Public Shared Sub ApplyPrimary(colour As String)
        Dim materialPaletteHelper = New MaterialDesignThemes.Wpf.PaletteHelper
        materialPaletteHelper.ReplacePrimaryColor(colour)
    End Sub

    'Changes palette accent colour
    Public Shared Sub ApplyAccent(colour As String)
        Dim materialPaletteHelper = New MaterialDesignThemes.Wpf.PaletteHelper
        materialPaletteHelper.ReplaceAccentColor(colour)
    End Sub

    Private Shared Sub ChangePrimaryColour(sender As Button,  e As RoutedEventArgs) Handles  IYellowP.Click, IAmberP.Click ,IDeepOrangeP.Click, ILightBlueP.Click, ITealP.Click, ICyanP.Click, IPinkP.Click, IGreenP.Click, IDeepPurpleP.Click, IIndigoP.Click, ILightGreenP.Click, IBlueP.Click, 
                                                                                             ILimeP.Click, IRedP.Click, IOrangeP.Click, IPurpleP.Click, IBlueGreyP.Click, IGreyP.Click
        Dim colour As String = sender.Name
        colour = colour.Substring(1, colour.Length - 2)

        ApplyPrimary(colour)
        My.Settings.primaryColour = colour
        My.Settings.Save()

    End Sub



    Private Shared Sub ChangeAccentColour(sender As Button,  e As RoutedEventArgs) Handles IYellowA.Click, IAmberA.Click, IDeepOrangeA.Click, ILightBlueA.Click, ITealA.Click, ICyanA.Click, IPinkA.Click, IGreenA.Click, IDeepPurpleA.Click, IIndigoA.Click, ILightGreenP.Click, IBlueA.Click, 
                                                                                           ILimeA.Click, IRedA.Click, IOrangeA.Click, IPurpleA.Click

        Dim colour As String = sender.Name
        colour = colour.Substring(1,colour.Length-2)

        ApplyAccent(colour)
        My.Settings.accentColour = colour
        My.Settings.Save()

    End Sub

    Private Sub IClearSettings_Checked(sender As Object, e As RoutedEventArgs) Handles IClearSettings.Click
        If IClearSettings.IsChecked
            My.Settings.clearSettings = true
        Else
            My.Settings.clearSettings = false
        End If
    End Sub

End Class
