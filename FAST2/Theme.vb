Imports MaterialDesignColors
Imports MaterialDesignThemes.Wpf

Public Class Theme
    'Switches base theme between light and dark
    Public Shared Sub SwitchBaseTheme(isDark)
        Call New PaletteHelper().SetLightDark(isDark)
        My.Settings.isDark = isDark
    End Sub

    'Changes palette primary colour
    Private Shared Sub ApplyPrimary(swatch As Swatch)
        Call New PaletteHelper().ReplacePrimaryColor(swatch)
    End Sub

    'Changes palette accent colour
    Private Shared Sub ApplyAccent(swatch As Swatch)
        Call New PaletteHelper().ReplaceAccentColor(swatch)
    End Sub
End Class
