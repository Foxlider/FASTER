Imports MaterialDesignThemes.Wpf

Namespace Models
    Public Class Theme
        'Switches base theme between light and dark
        Public Shared Sub SwitchBase(isDark)
            Call New PaletteHelper().SetLightDark(isDark)
            My.Settings.isDark = isDark
            My.Settings.Save()
        End Sub

        'Changes palette primary colour
        Public Shared Sub ApplyPrimary(colour As String)
            Dim materialPaletteHelper = New PaletteHelper
            materialPaletteHelper.ReplacePrimaryColor(colour)
        End Sub

        'Changes palette accent colour
        Public Shared Sub ApplyAccent(colour As String)
            Dim materialPaletteHelper = New PaletteHelper
            materialPaletteHelper.ReplaceAccentColor(colour)
        End Sub

    End Class
End NameSpace