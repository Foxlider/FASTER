Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ModCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String

        <XmlElement(Order:=2, ElementName:="SteamMod")>
        Public SteamMods As List(Of SteamMod) = New List(Of SteamMod)()

        <XmlElement(Order:=3, ElementName:="LocalMod")>
        Public LocalMods As List(Of LocalMod) = New List(Of LocalMod)()
    End Class


    <Serializable()>
    Public Class SteamMod
        Private Sub New()
        End Sub

        Public Sub New(workshopId As Int32, name As String, author As String, steamLastUpdated As Int32, localLastUpdated As Int32, Optional privateMod As Boolean = False)
            Me.WorkshopId = workshopId
            Me.Name = name
            Me.Author = author
            Me.SteamLastUpdated = steamLastUpdated
            Me.LocalLastUpdated = localLastUpdated
            Me.PrivateMod = privateMod
        End Sub

        <XmlElement(Order:=1)>
        Public Property WorkshopId As Integer = Nothing

        <XmlElement(Order:=2)>
        Public Property Name As String = String.Empty

        <XmlElement(Order:=3)>
        Public Property Author As String = String.Empty

        <XmlElement(Order:=4)>
        Public Property SteamLastUpdated As Integer = Nothing

        <XmlElement(Order:=5)>
        Public Property LocalLastUpdated As Integer = Nothing

        <XmlElement(Order:=6)>
        Public Property PrivateMod As Boolean = False
    End Class

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

        
        <XmlElement(Order:=1)>
        Public Property Name As String = String.Empty

        <XmlElement(Order:=2)>
        Public Property Path As String = String.Empty

        <XmlElement(Order:=3)>
        Public Property Author As String = String.Empty

        <XmlElement(Order:=4)>
        Public Property Website As String = String.Empty
    End Class
End Namespace