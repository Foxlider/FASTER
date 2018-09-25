Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ServerCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String = "Main"

        <XmlElement(Order:=2, ElementName:="ServerProfile")>
        Public ServerProfiles As List(Of ServerProfile) = New List(Of ServerProfile)()

        Private Shared Function GetServerProfiles() As ServerCollection
            Dim currentProfiles As New ServerCollection
            
            If My.Settings.Servers IsNot Nothing
                currentProfiles = My.Settings.Servers
            End If

            Return currentProfiles
        End Function
        
        Public Shared Sub AddServerProfile(name As String, safeName As String)
            Dim duplicate = False
            Dim currentProfiles = GetServerProfiles()
            
            If currentProfiles.ServerProfiles.Count > 0 Then
                For Each profile In currentProfiles.ServerProfiles
                    If profile.DisplayName = name
                        duplicate = true
                    End If
                Next
            End If
           
            If Not duplicate Then
                currentProfiles.ServerProfiles.Add(New ServerProfile(name, safeName))
                My.Settings.Servers = currentProfiles
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "Profile Already Exists"
            End If

            My.Settings.Save()
            MainWindow.Instance.LoadServerProfiles
        End Sub

        Public Shared Function RenameServerProfile(oldName As String, newName As String) As Boolean
            Try
                Dim currentProfiles = My.Settings.Servers.ServerProfiles
        
                Dim currentProfile As ServerProfile = currentProfiles.Find(Function(profile) profile.DisplayName = oldName)

                currentProfile.DisplayName = newName
                currentProfile.SafeName = MainWindow.SafeName(newName)
                Return True
            Catch ex As Exception
                MsgBox(ex.Message)
                Return False
            End Try
        End Function

        Public Shared Sub DeleteServerProfile(safeName As String)
            Dim currentProfiles = GetServerProfiles()

            currentProfiles.ServerProfiles.RemoveAll(Function(x) x.SafeName = safeName)

            My.Settings.Save()
            MainWindow.Instance.LoadServerProfiles
        End Sub
    End Class

    <Serializable()>
    Public Class ServerProfile
        Private Sub New()
        End Sub

        Public Sub New(name As String, safeName As String)
            DisplayName =  name
            Me.SafeName = safeName
        End Sub

        Public Property SafeName As String = String.Empty
        Public Property DisplayName As String = String.Empty
        Public Property ServerName As String = String.Empty
        Public Property Executable As String = String.Empty
        Public Property Password As String = String.Empty
        Public Property AdminPassword As String = String.Empty
        Public Property MaxPlayers As Integer = 32
        Public Property Port As Integer = 2302
        Public Property HeadlessClientEnabled As Boolean = False
        Public Property HeadlessIps As String = String.Empty
        Public Property LocalClients As String = String.Empty
        Public Property NoOfHeadlessClients As Integer = 0
        Public Property Loopback As Boolean = False
        Public Property Upnp As Boolean = False
        Public Property Netlog As Boolean = False
        Public Property AutoRestartEnabled As Boolean = False
        Public Property DailyRestartAEnabled As Boolean = False
        Public Property DailyRestartA As Date
        Public Property DailyRestartBEnabled As Boolean = False
        Public Property DailyRestartB As Date
        Public Property VotingEnabled As Boolean = True
        Public Property VotingMinPlayers As Integer = 3
        Public Property VotingThreshold As Decimal = 0.33
        Public Property AllowFilePatching As Integer = 0
        Public Property VerifySignatures As Integer = 0
        Public Property RequiredBuildEnabled As Boolean = False
        Public Property RequiredBuild As Boolean = False
        Public Property KickDuplicates As Boolean = False
        Public Property VonEnabled As Boolean = True
        Public Property CodecQuality As Integer = 3
        Public Property ServerConsoleLogEnabled As Boolean = False
        Public Property ServerConsoleLog As String = "server_console.log"
        Public Property PidEnabled As Boolean = False
        Public Property PidLog As String = "pid.log"
        Public Property RankingEnabled As Boolean = False
        Public Property RankingLog As String = "ranking.log"
        Public Property RptTimestamp As String = "none"
        Public Property Motd As String = String.Empty
        Public Property MotdDelay As Integer = 5
        Public Property ServerMods As String = String.Empty
        Public Property ClientMods As String = String.Empty
        Public Property HeadlessMods As String = String.Empty
        Public Property Missions As String = String.Empty
        Public Property PersistentBattlefield As Boolean = False
        Public Property AutoInit As Boolean = False
        Public Property DifficultyPreset As String = String.Empty
        Public Property ReducedDamage As Boolean = False
        Public Property GroupIndicators As Boolean = False
        Public Property FriendlyNameTags As Boolean = False
        Public Property EnemyNameTags As Boolean = False
        Public Property DetectedMines As Boolean = False
        Public Property MultipleSaves As Boolean = False
        Public Property ThirdPerson As Boolean = False
        Public Property WeaponInfo As Boolean = False
        Public Property StanceIndicator As Boolean = False
        Public Property StaminaBar As Boolean = False
        Public Property CameraShake As Boolean = False
        Public Property VisualAids As Boolean = False
        Public Property ExtendedMapContent As Boolean = False
        Public Property Commands As Boolean = False
        Public Property VonId As Boolean = False
        Public Property KilledBy As Boolean = False
        Public Property Waypoints As Boolean = False
        Public Property Crosshair As Boolean = False
        Public Property AutoReporting As Boolean = False
        Public Property ScoreTable As Boolean = False
        Public Property TacticalPing As Boolean = False
        Public Property AiAccuracy As Decimal = 0.95
        Public Property AiSkill As Decimal = 0.55
        Public Property AiPreset As Integer = 3
        Public Property MaxPacketLossEnabled As Boolean = False
        Public Property MaxPacketLoss As Integer = 0
        Public Property DisconnectTimeoutEnabled As Boolean = False
        Public Property DisconnectTimeout As Integer = 90
        Public Property KickOnSlowNetworkEnabled As Boolean = False
        Public Property KickOnSlowNetwork As String = String.Empty
        Public Property TerrainGrid As Integer = 10
        Public Property ViewDistance As Integer = 2500
        Public Property MaxPingEnabled As Boolean = False
        Public Property MaxPing As Integer = 60
        Public Property MaxDesyncEnabled As Boolean = False
        Public Property MaxDesync As Integer = 0
        Public Property MaxCustomFileSize As Integer = 160
        Public Property MaxPacketSize As Integer = 1400
        Public Property MinBandwidth As Integer = 128
        Public Property MaxBandwidth As Integer = 2000
        Public Property MaxMessagesSend As Integer = 128
        Public Property MaxSizeNonguaranteed As Integer = 256
        Public Property MaxSizeGuaranteed As Integer = 512
        Public Property MinErrorToSend As Decimal = 0.001
        Public Property MinErrorToSendNear As Decimal = 0.01
        Public Property ExtraParams As String = String.Empty
        Public Property EnableHyperThreading As Boolean = False
        Public Property FilePatching As Boolean = False
        Public Property ServerCommandPassword As String = String.Empty
        Public Property DoubleIdDetected As String = String.Empty
        Public Property OnUserConnected As String = String.Empty
        Public Property OnUserDisconnected As String = String.Empty
        Public Property OnHackedData As String = String.Empty
        Public Property OnDifferentData As String = String.Empty
        Public Property OnUnsignedData As String = String.Empty
        Public Property RegularCheck As String = String.Empty
        Public Property BattleEye As Boolean = True
    End Class
End NameSpace