Class MainWindow
    Private Sub SteamUpdaterTabSelect_Selected(sender As Object, e As RoutedEventArgs) Handles steamUpdaterTabSelect.Selected
        mainContent.SelectedIndex = 0
    End Sub

    Private Sub ServerModsTabSelect_Selected(sender As Object, e As RoutedEventArgs) Handles serverModsTabSelect.Selected
        mainContent.SelectedIndex = 1
    End Sub

    Private Sub SettingsTabSelect_Selected(sender As Object, e As RoutedEventArgs) Handles settingsTabSelect.Selected
        mainContent.SelectedIndex = 2
    End Sub

    Private Sub ToolsTabSelect_Selected(sender As Object, e As RoutedEventArgs) Handles toolsTabSelect.Selected
        mainContent.SelectedIndex = 3
    End Sub

    Private Sub AboutTabSelect_Selected(sender As Object, e As RoutedEventArgs) Handles aboutTabSelect.Selected
        mainContent.SelectedIndex = 4
    End Sub
End Class
