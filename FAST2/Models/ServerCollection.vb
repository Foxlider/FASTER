Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ServerCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String

        <XmlElement(Order:=2, ElementName:="ServerProfile")>
        Public ServerProfiles As List(Of ServerProfile) = New List(Of ServerProfile)()

        Private Shared Function GetServerProfiles() As ServerCollection
            Dim currentProfiles As New ServerCollection
            
            If My.Settings.serverProfiles IsNot Nothing
                currentProfiles = My.Settings.serverProfiles
            End If

            Return currentProfiles
        End Function
        
        Public Shared Sub AddServerProfile(name As String, safeName As String)
            Dim duplicate = False
            Dim currentProfiles = GetServerProfiles()
            
            If currentProfiles.ServerProfiles.Count > 0 Then
                For Each profile In currentProfiles.ServerProfiles
                    If profile.ProfileNameBox = name
                        duplicate = true
                    End If
                Next
            End If
           
            If Not duplicate Then
                currentProfiles.ServerProfiles.Add(New ServerProfile(name, safeName))

                My.Settings.serverProfiles = currentProfiles
            Else
                MsgBox("Profile already exists.")
            End If

            My.Settings.Save()
            MainWindow.Instance.LoadServerProfiles
        End Sub

        Public Shared Sub RenameServerProfile(safeName As String)

        End Sub

        Public Shared Sub DeleteServerProfile(safeName As String)
            Dim currentProfiles = GetServerProfiles()

            currentProfiles.ServerProfiles.RemoveAll(Function(x) x.SafeName = safeName)

            My.Settings.Save()
            MainWindow.Instance.LoadServerProfiles
        End Sub

        Public Shared Sub ExportServerProfile(safeName As String)

        End Sub
    End Class

    <Serializable()>
    Public Class ServerProfile
        Private Sub New()
        End Sub

        Public Sub New(name As String, safeName As String)
            ProfileNameBox =  name
            Me.SafeName = safeName
        End Sub

        Public Property SafeName As String = String.Empty

        Public Property LocalClientBox As String = "127.0.0.1"

        Public Property HeadlessIpBox As String = "127.0.0.1"

        Public Property PidBox As String = "pid.log" 

        Public Property RankingBox As String = "ranking.log" 

        Public Property ConsoleLogBox As String = "server_console.log" 

        Public Property ServerFileBox As String = String.Empty 

        Public Property ServerNameBox As String = String.Empty 

        Public Property PortBox As Integer = 2302 

        Public Property PasswordBox As String = String.Empty 

        Public Property MaxPlayersBox As Integer = 32 

        Public Property AdminPassBox As String = String.Empty 

        Public Property RequiredBuildBox As Integer = 0 

        Public Property ExtraParamsBox As String = String.Empty 

        Public Property MaxSendBox As Integer = 128 

        Public Property MinErrorBox As Double = 0.001 

        Public Property MinErrorNearBox As Double = 0.01 

        Public Property MaxBandwidthBox As Integer = 2000 

        Public Property MinBandwidthBox As Integer = 128 

        Public Property MaxNonGuaranteedBox As Integer = 256 

        Public Property MaxGuaranteedBox As Integer = 512 

        Public Property MaxPacketBox As Integer = 1400 

        Public Property MaxCustFileBox As Integer = 160 

        Public Property ProfileNameBox As String = "New Profile" 

        Public Property OnUnsignedDataBox As String = String.Empty 

        Public Property RegularCheckBox As String = String.Empty 

        Public Property OnDifferentDataBox As String = String.Empty 

        Public Property OnHackedDataBox As String = String.Empty 

        Public Property OnUserConnectedBox As String = String.Empty 

        Public Property OnUserDisconnectedBox As String = String.Empty 

        Public Property DoubleIdDetectedBox As String = String.Empty 

        Public Property ServerCommandBox As String = String.Empty 

        Public Property NetlogCheck As Boolean = False

        Public Property UpnpCheckWorkshopId As Boolean = False 

        Public Property LoopbackCheck As Boolean = False 

        Public Property WorenableHcCheckkshopId As Boolean = False 

        Public Property RankingCheck As Boolean = False 

        Public Property PidCheck As Boolean = False 

        Public Property ConsoleLogCheck As Boolean = False 

        Public Property RequiredBuildCheck As Boolean = False 

        Public Property KickDupeCheck As Boolean = False 

        Public Property VoteCheck As Boolean = True 

        Public Property VonCheck As Boolean = False 

        Public Property AutoInitCheck As Boolean = False 

        Public Property PersistCheck As Boolean = False 

        Public Property TacticalPingCheck As Boolean = True 

        Public Property MultipleSavesCheck As Boolean = True 

        Public Property AutoReportingCheck As Boolean = True 

        Public Property MapContentCheck As Boolean = True 

        Public Property VonIdCheck As Boolean = True 

        Public Property KilledByCheck As Boolean = True 

        Public Property ScoreTableCheck As Boolean = True 

        Public Property CameraShakeCheck As Boolean = True 

        Public Property ThirdPersonCheck As Boolean = True 

        Public Property VisualAidCheck As Boolean = True 

        Public Property CrosshairCheck As Boolean = True 

        Public Property StaminaBarCheck As Boolean = True 

        Public Property StanceIndicatorCheck As Boolean = True 

        Public Property WeaponInfoCheck As Boolean = True 

        Public Property WaypointsCheck As Boolean = True 

        Public Property CommandsCheck As Boolean = True 

        Public Property DetectedMinesCheck As Boolean = True 

        Public Property EnemyNameCheck As Boolean = True 

        Public Property FriendlyNameCheck As Boolean = True 

        Public Property GroupIndicatorCheck As Boolean = True 

        Public Property ReducedDamageCheck As Boolean = False 

        Public Property FilePatchingCheck As Boolean = False 

        Public Property HtCheck As Boolean = False 

        Public Property MaxPingCheck As Boolean = False 

        Public Property MaxDesyncCheck As Boolean = False 

        Public Property WorkshoppacketLossCheckId As Boolean = False 

        Public Property DisconTimeCheck As Boolean = False 

        Public Property KickSlowCheck As Boolean = False 

        Public Property BattleyeCheck As Boolean = False 

        Public Property RptTimeCombo As String = String.Empty 

        Public Property FilePatchCombo As Integer = 2 

        Public Property VerifySigCombo As Integer = 0 

        Public Property DifficultyCombo As String = "Regular" 

        Public Property KickSlowCombo As String = "Log" 

        Public Property NoOfHcNumeric As Integer = 1 

        Public Property ModTimeNumeric As Integer = 5 

        Public Property VoteThresholdNumeric As Integer = 33 

        Public Property WorvoteMinPlayersNumerickshopId As Integer = 3 

        Public Property CodecNumeric As Integer = 3 

        Public Property AiAccuracyNumeric As Double = 0.55 

        Public Property AiSkillNumeric As Double = 0.75 

        Public Property AiPresetNumeric As Integer = 3 

        Public Property DistanceNumeric As Integer = 2500 

        Public Property TerrainNumeric As Integer = 10 

        Public Property DisconTimeNumeric As Integer = 90 

        Public Property PacketLossNumeric As Integer = 0 

        Public Property MaxDesyncNumeric As Integer = 0 

        Public Property MaxPingNumeric As Integer = 60

        '<hcModsList />
        '<serverModsList />
        '<missionsList />
        '<modBox />

        
    End Class
End NameSpace