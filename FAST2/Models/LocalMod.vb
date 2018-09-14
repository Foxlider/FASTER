Namespace Models
    <Serializable()>
    Public Class LocalMod
        Private Sub New()
        End Sub

        Public Sub New(name As String, path As String, Optional author As String = "Unknown", Optional website As String = Nothing)
            Me.Name = name
            Me.Path = path
            Me.Author = author
            Me.Website = website
        End Sub
        
        Public Property Name As String = String.Empty

        Public Property Path As String = String.Empty

        Public Property Author As String = String.Empty

        Public Property Website As String = String.Empty
    End Class
End NameSpace