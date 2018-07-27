' This class implements a team mod type that implements the IPropertyChange interface.
Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Namespace Models

    Public Class SteamMod
        Implements INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ' This method is called by the Set accessor of each property.
        ' The CallerMemberName attribute that is applied to the optional propertyName
        ' parameter causes the property name of the caller to be substituted as an argument.
        Private Sub NotifyPropertyChanged(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub


        Private _workshopId As Int32 = Nothing
        Private _name As String = String.Empty
        Private _author As String = String.Empty
        Private _steamLastUpdated As Int32 = Nothing
        Private _localLastUpdated As Int32 = Nothing

        Private Enum Type
            PrivateMod
            PublicMod
        End Enum

        Public Property WorkshopID() As String
            Get
                Return Me._workshopId
            End Get

            Set
                If Not (Value = _workshopId) Then
                    Me._workshopId = Value
                    NotifyPropertyChanged()
                End If
            End Set
        End Property

    End Class
End Namespace