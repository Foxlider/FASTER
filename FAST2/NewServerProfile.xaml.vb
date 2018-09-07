Imports System.Windows.Forms
Imports FAST2.Models

Public Class NewServerProfile

    Private Sub IContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles IContinueButton.Click
        Dim profileName = IServerName.Text

        ServerCollection.AddServerProfile(profileName, MainWindow.SafeName(profileName))

        Forms.Cursor.Show()
        Forms.Cursor.Current = Cursors.WaitCursor
        Close()
    End Sub



    'Makes close button red when mouse is over button
    Private Sub IWindowCloseButton_MouseEnter(sender As Object, e As Input.MouseEventArgs) Handles IWindowCloseButton.MouseEnter
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#D72C2C"), Brush)

        IWindowCloseButton.Background = brush
    End Sub

    'Changes colour of close button back to UI base when mouse leaves button
    Private Sub IWindowCloseButton_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles IWindowCloseButton.MouseLeave
        Dim brush = FindResource("MaterialDesignPaper")

        IWindowCloseButton.Background = brush
    End Sub

    'Closes app when using custom close button
    Private Sub IWindowCloseButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowCloseButton.Selected
        Close()
    End Sub

    'Minimises app when using custom minimise button
    Private Sub IWindowMinimizeButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowMinimizeButton.Selected
        IWindowMinimizeButton.IsSelected = False
        WindowState = WindowState.Minimized
    End Sub

    'Allows user to move the window around using the custom nav bar
    Private Sub IWindowDragBar_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles IWindowDragBar.MouseLeftButtonDown, ILogoImage.MouseLeftButtonDown, IWindowTitle.MouseLeftButtonDown
        DragMove()
    End Sub


End Class
