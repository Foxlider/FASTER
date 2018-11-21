Public Class About
    Private Sub IDiscordButton_Click(sender As Object, e As RoutedEventArgs) Handles IDiscordButton.Click
        Process.Start("https://links.kestrelstudios.co.uk/fast2-discord")
    End Sub

    Private Sub IDonateButton_Click(sender As Object, e As RoutedEventArgs) Handles IDonateButton.Click
        Process.Start("https://links.kestrelstudios.co.uk/fast2-donate")
    End Sub

    Private Sub IGitHubButton_Click(sender As Object, e As RoutedEventArgs) Handles IGitHubButton.Click
        Process.Start("https://links.kestrelstudios.co.uk/fast2-github")
    End Sub

    Private Sub IForumsButton_Click(sender As Object, e As RoutedEventArgs) Handles IForumsButton.Click
        Process.Start("https://links.kestrelstudios.co.uk/fast2-forums")
    End Sub

    Private Sub About_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IVersionLabel.Text = "Version: " & [GetType].Assembly.GetName.Version.ToString
    End Sub
End Class
