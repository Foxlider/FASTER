Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports FAST2.Models

Class ServerProfile
    Shared _safeName As String

    Public Sub New(profile As Models.ServerProfile)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call
        _safeName = profile.SafeName

        IDisplayName.Content = profile.DisplayName
        IServerName.Text = profile.ServerName
        IExecutable.Text = profile.Executable
        IPassword.Text = profile.Password
        IAdminPassword.Text = profile.AdminPassword
        IMaxPlayers.Text = profile.MaxPlayers
        IPort.Text = profile.Port
        IHeadlessClientEnabled.IsChecked = profile.HeadlessClientEnabled
        IHeadlessIps.Text = profile.HeadlessIps
        ILocalClients.Text = profile.LocalClients
        INoOfHeadlessClients.Value = profile.NoOfHeadlessClients
        ILoopback.IsChecked = profile.Loopback
        IUpnp.IsChecked = profile.Upnp
        INetlog.IsChecked = profile.Netlog
        IAutoRestartEnabled.IsChecked = profile.AutoRestartEnabled
        IDailyRestartAEnabled.IsChecked = profile.DailyRestartAEnabled
        IDailyRestartA.SelectedTime = profile.DailyRestartA
        IDailyRestartBEnabled.IsChecked = profile.DailyRestartBEnabled
        IDailyRestartB.Text = profile.DailyRestartB
        IVotingEnabled.IsChecked = profile.VotingEnabled
        IVotingMinPlayers.Text = profile.VotingMinPlayers
        IVotingThreshold.Text = profile.VotingThreshold
        IAllowFilePatching.Text = profile.AllowFilePatching
        IVerifySignatures.Text = profile.VerifySignatures
        IRequiredBuildEnabled.IsChecked = profile.RequiredBuildEnabled
        IRequiredBuild.Text = profile.RequiredBuild
        IKickDuplicates.IsChecked = profile.KickDuplicates
        IVonEnabled.IsChecked = profile.VonEnabled
        ICodecQuality.Value = profile.CodecQuality
        IServerConsoleLogEnabled.IsChecked = profile.ServerConsoleLogEnabled
        IServerConsoleLog.Text = profile.ServerConsoleLog
        IPidEnabled.IsChecked = profile.PidEnabled
        IPidLog.Text = profile.PidLog
        IRankingEnabled.IsChecked = profile.RankingEnabled
        IRankingLog.Text = profile.RankingLog
        IRptTimestamp.Text = profile.RptTimestamp
        IMotd.Text = profile.Motd      
        IMotdDelay.Text = profile.MotdDelay
        IPersistentBattlefield.IsChecked = profile.PersistentBattlefield
        IAutoInit.IsChecked = profile.AutoInit
        IDifficultyPreset.Text = profile.DifficultyPreset
        IReducedDamage.IsChecked = profile.ReducedDamage
        IGroupIndicators.IsChecked = profile.GroupIndicators
        IFriendlyNameTags.IsChecked = profile.FriendlyNameTags
        IEnemyNameTags.IsChecked = profile.EnemyNameTags
        IDetectedMines.IsChecked = profile.DetectedMines
        IMultipleSaves.IsChecked = profile.MultipleSaves
        IThirdPerson.IsChecked = profile.ThirdPerson
        IWeaponInfo.IsChecked = profile.WeaponInfo
        IStanceIndicator.IsChecked = profile.StanceIndicator
        IStaminaBar.IsChecked = profile.StaminaBar
        ICameraShake.IsChecked = profile.CameraShake
        IVisualAids.IsChecked = profile.VisualAids
        IExtendedMapContent.IsChecked = profile.ExtendedMapContent
        ICommands.IsChecked = profile.Commands
        IVonId.IsChecked = profile.VonId
        IKilledBy.IsChecked = profile.KilledBy
        IWaypoints.IsChecked = profile.Waypoints
        ICrosshair.IsChecked = profile.Crosshair
        IAutoReporting.IsChecked = profile.AutoReporting
        IScoreTable.IsChecked = profile.ScoreTable
        ITacticalPing.IsChecked = profile.TacticalPing
        IAiAccuracy.Text = profile.AiAccuracy
        IAiSkill.Text = profile.AiSkill
        IAiPreset.Text = profile.AiPreset
        IMaxPacketLossEnabled.IsChecked = profile.MaxPacketLossEnabled
        IMaxPacketLoss.Text = profile.MaxPacketLoss
        IDisconnectTimeOutEnabled.IsChecked = profile.DisconnectTimeoutEnabled
        IDisconnectTimeOut.Text = profile.DisconnectTimeout
        IKickOnSlowNetworkEnabled.IsChecked = profile.KickOnSlowNetworkEnabled
        IKickOnSlowNetwork.Text = profile.KickOnSlowNetwork
        IMaxPingEnabled.IsChecked = profile.MaxPingEnabled
        IMaxPing.Text = profile.MaxPing
        IMaxDesyncEnabled.IsChecked = profile.MaxDesyncEnabled
        IMaxDesync.Text = profile.MaxDesync
        IMaxCustomFileSize.Text = profile.MaxCustomFileSize
        IMaxPacketSize.Text = profile.MaxPacketSize
        IMinBandwidth.Text = profile.MinBandwidth
        IMaxBandwidth.Text = profile.MaxBandwidth
        IMaxMessagesSend.Text = profile.MaxMessagesSend
        IMaxSizeNonguaranteed.Text = profile.MaxSizeNonguaranteed
        IMaxSizeGuaranteed.Text = profile.MaxSizeGuaranteed
        IMinErrorToSend.Text = profile.MinErrorToSend
        IMinErrorToSendNear.Text = profile.MinErrorToSendNear
        IExtraParams.Text = profile.ExtraParams
        IEnableHyperThreading.IsChecked = profile.EnableHyperThreading
        IFilePatching.IsChecked = profile.FilePatching
        IServerCommandPassword.Text = profile.ServerCommandPassword
        IDoubleIdDetected.Text = profile.DoubleIdDetected
        IOnUserConnected.Text = profile.OnUserConnected
        IOnUserDisconnected.Text = profile.OnUserDisconnected
        IOnHackedData.Text = profile.OnHackedData
        IOnDifferentData.Text = profile.OnDifferentData
        IOnUnsignedData.Text = profile.OnUnsignedData
        IRegularCheck.Text = profile.RegularCheck

        ToggleUi(IHeadlessClientEnabled)
        ToggleUi(IAutoRestartEnabled)
        ToggleUi(IVonEnabled)
        ToggleUi(IVotingEnabled)

    End Sub

    'Turns toggle button groups into normal buttons
    Private Sub IActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged
        Dim thread As New Thread(
            Sub()
                Thread.Sleep(600)
                Dispatcher.Invoke(
                    Sub()
                        sender.SelectedItem = Nothing
                    End Sub
                    )
            End Sub
            )
        thread.Start()
    End Sub

    Private Sub IProfileNameEditSave_Click(sender As Object, e As RoutedEventArgs) Handles IProfileNameEditSave.Click
        Dim oldName = IDisplayName.Content
        Dim newName = IProfileDisplayNameEdit.Text

        If ServerCollection.RenameServerProfile(oldName, newName) Then
            MainWindow.Instance.IMainContent.Items.RemoveAt(MainWindow.Instance.IMainContent.SelectedIndex)
            MainWindow.Instance.LoadServerProfiles()
            MainWindow.Instance.IMainContent.SelectedIndex = MainWindow.Instance.IMainContent.Items.Count - 1
        Else
            MsgBox("Error Try Again")
        End If
    End Sub

    Private Sub ShowRenameInterface(show As Boolean)
        If show Then
            IProfileDisplayNameEdit.Text = IDisplayName.Content
            IDisplayName.Visibility = Visibility.Collapsed
            IProfileNameEdit.Visibility = Visibility.Visible
            IProfileDisplayNameEdit.Focus()
            IProfileDisplayNameEdit.SelectAll()
        Else
            IProfileDisplayNameEdit.Text = String.Empty
            IDisplayName.Visibility = Visibility.Visible
            IProfileNameEdit.Visibility = Visibility.Collapsed
        End If
    End Sub

    'Manages actions for steam mods tab buttons
    Private Sub IServerActionButtons_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles IServerActionButtons.SelectionChanged

        If ISaveProfile.IsSelected Then
            UpdateProfile()

        ElseIf IRenameProfile.IsSelected Then
            ShowRenameInterface(True)

        ElseIf IDeleteProfile.IsSelected Then
            MainWindow.Instance.IMainContent.Items.RemoveAt(MainWindow.Instance.IMainContent.SelectedIndex)
            ServerCollection.DeleteServerProfile(_safeName)
            If MainWindow.Instance.IMainContent.Items.Count > 5 Then
                MainWindow.Instance.IMainContent.SelectedIndex = 6
            Else
                MainWindow.Instance.IMainContent.SelectedIndex = 0
            End If

        ElseIf IExportProfile.IsSelected Then

        ElseIf ILaunchServer.IsSelected Then

        End If

        Dim thread As New Thread(
            Sub()
                Thread.Sleep(600)
                Dispatcher.Invoke(
                    Sub()
                        sender.SelectedItem = Nothing
                    End Sub
                    )
            End Sub
            )
        thread.Start()
    End Sub

    Private Sub UpdateProfile()
        Dim profile = My.Settings.Servers.ServerProfiles.Find(Function(p) p.SafeName = _safeName)

        profile.DisplayName = IDisplayName.Content
        profile.ServerName = IServerName.Text
        profile.Executable = IExecutable.Text
        profile.Password = IPassword.Text
        profile.AdminPassword = IAdminPassword.Text
        profile.MaxPlayers = IMaxPlayers.Text
        profile.Port = IPort.Text
        profile.HeadlessClientEnabled = IHeadlessClientEnabled.IsChecked
        profile.HeadlessIps = IHeadlessIps.Text
        profile.LocalClients = ILocalClients.Text
        profile.NoOfHeadlessClients = INoOfHeadlessClients.Value
        profile.Loopback = ILoopback.IsChecked
        profile.Upnp = IUpnp.IsChecked
        profile.Netlog = INetlog.IsChecked
        profile.AutoRestartEnabled = IAutoRestartEnabled.IsChecked
        profile.DailyRestartAEnabled = IDailyRestartAEnabled.IsChecked
        If IDailyRestartA.SelectedTime IsNot Nothing
            profile.DailyRestartA = IDailyRestartA.SelectedTime
        End If
        profile.DailyRestartBEnabled = IDailyRestartBEnabled.IsChecked
        If IDailyRestartB.SelectedTime IsNot Nothing
            profile.DailyRestartB = IDailyRestartB.SelectedTime
        End If
        profile.VotingEnabled = IVotingEnabled.IsChecked
        profile.VotingMinPlayers = IVotingMinPlayers.Text
        profile.VotingThreshold = IVotingThreshold.Text
        profile.AllowFilePatching = IAllowFilePatching.Text
        profile.VerifySignatures = IVerifySignatures.Text
        profile.RequiredBuildEnabled = IRequiredBuildEnabled.IsChecked
        profile.RequiredBuild = IRequiredBuild.Text
        profile.KickDuplicates = IKickDuplicates.IsChecked
        profile.VonEnabled = IVonEnabled.IsChecked
        profile.CodecQuality = ICodecQuality.Value
        profile.ServerConsoleLogEnabled = IServerConsoleLogEnabled.IsChecked
        profile.ServerConsoleLog = IServerConsoleLog.Text
        profile.PidEnabled = IPidEnabled.IsChecked
        profile.PidLog = IPidLog.Text
        profile.RankingEnabled = IRankingEnabled.IsChecked
        profile.RankingLog = IRankingLog.Text
        profile.RptTimestamp = IRptTimestamp.Text
        profile.Motd = IMotd.Text
        profile.MotdDelay = IMotdDelay.Text
        profile.PersistentBattlefield = IPersistentBattlefield.IsChecked
        profile.AutoInit = IAutoInit.IsChecked
        profile.DifficultyPreset = IDifficultyPreset.Text
        profile.ReducedDamage = IReducedDamage.IsChecked
        profile.GroupIndicators = IGroupIndicators.IsChecked
        profile.FriendlyNameTags = IFriendlyNameTags.IsChecked
        profile.EnemyNameTags = IEnemyNameTags.IsChecked
        profile.DetectedMines = IDetectedMines.IsChecked
        profile.MultipleSaves = IMultipleSaves.IsChecked
        profile.ThirdPerson = IThirdPerson.IsChecked
        profile.WeaponInfo = IWeaponInfo.IsChecked
        profile.StanceIndicator = IStanceIndicator.IsChecked
        profile.StaminaBar = IStaminaBar.IsChecked
        profile.CameraShake = ICameraShake.IsChecked
        profile.VisualAids = IVisualAids.IsChecked
        profile.ExtendedMapContent = IExtendedMapContent.IsChecked
        profile.Commands = ICommands.IsChecked
        profile.VonId = IVonId.IsChecked
        profile.KilledBy = IKilledBy.IsChecked
        profile.Waypoints = IWaypoints.IsChecked
        profile.Crosshair = ICrosshair.IsChecked
        profile.AutoReporting = IAutoReporting.IsChecked
        profile.ScoreTable = IScoreTable.IsChecked
        profile.TacticalPing = ITacticalPing.IsChecked
        profile.AiAccuracy = IAiAccuracy.Text
        profile.AiSkill = IAiSkill.Text
        profile.AiPreset = IAiPreset.Text
        profile.MaxPacketLossEnabled = IMaxPacketLossEnabled.IsChecked
        profile.MaxPacketLoss = IMaxPacketLoss.Text
        profile.DisconnectTimeoutEnabled = IDisconnectTimeOutEnabled.IsChecked
        profile.DisconnectTimeout = IDisconnectTimeOut.Text
        profile.KickOnSlowNetworkEnabled = IKickOnSlowNetworkEnabled.IsChecked
        profile.KickOnSlowNetwork = IKickOnSlowNetwork.Text
        profile.MaxPingEnabled = IMaxPingEnabled.IsChecked
        profile.MaxPing = IMaxPing.Text
        profile.MaxDesyncEnabled = IMaxDesyncEnabled.IsChecked
        profile.MaxDesync = IMaxDesync.Text
        profile.MaxCustomFileSize = IMaxCustomFileSize.Text
        profile.MaxPacketSize = IMaxPacketSize.Text
        profile.MinBandwidth = IMinBandwidth.Text
        profile.MaxBandwidth = IMaxBandwidth.Text
        profile.MaxMessagesSend = IMaxMessagesSend.Text
        profile.MaxSizeNonguaranteed = IMaxSizeNonguaranteed.Text
        profile.MaxSizeGuaranteed = IMaxSizeGuaranteed.Text
        profile.MinErrorToSend = IMinErrorToSend.Text
        profile.MinErrorToSendNear = IMinErrorToSendNear.Text
        profile.ExtraParams = IExtraParams.Text
        profile.EnableHyperThreading = IEnableHyperThreading.IsChecked
        profile.FilePatching = IFilePatching.IsChecked
        profile.ServerCommandPassword = IServerCommandPassword.Text
        profile.DoubleIdDetected = IDoubleIdDetected.Text
        profile.OnUserConnected = IOnUserConnected.Text
        profile.OnUserDisconnected = IOnUserDisconnected.Text
        profile.OnHackedData = IOnHackedData.Text
        profile.OnDifferentData = IOnDifferentData.Text
        profile.OnUnsignedData = IOnUnsignedData.Text
        profile.RegularCheck = IRegularCheck.Text

        My.Settings.Save()
    End Sub


    Private Sub ToggleUi(uiElement As Object, Optional e As RoutedEventArgs = Nothing) Handles IHeadlessClientEnabled.Click, IAutoRestartEnabled.Click, IVonEnabled.Click, IVotingEnabled.Click, IServerConsoleLogEnabled.Click,
                                                                                               IPidEnabled.Click, IRankingEnabled.Click, IRequiredBuildEnabled.Click, IDailyRestartAEnabled.Click, IDailyRestartBEnabled.Click, 
                                                                                               IPersistentBattlefield.Click, IMaxPacketLossEnabled.Click, IDisconnectTimeOutEnabled.Click, IKickOnSlowNetworkEnabled.Click, IMaxPingEnabled.Click, 
                                                                                               IMaxDesyncEnabled.Click
        Select Case uiElement.Name
            Case "IMaxDesyncEnabled"
                If IMaxDesyncEnabled.IsChecked Then
                    IMaxDesync.IsEnabled = True
                Else
                    IMaxDesync.IsEnabled = False
                End If
            Case "IMaxPingEnabled"
                If IMaxPingEnabled.IsChecked Then
                    IMaxPing.IsEnabled = True
                Else
                    IMaxPing.IsEnabled = False
                End If
            Case "IKickOnSlowNetworkEnabled"
                If IKickOnSlowNetworkEnabled.IsChecked Then
                    IKickOnSlowNetwork.IsEnabled = True
                Else
                    IKickOnSlowNetwork.IsEnabled = False
                End If
            Case "IDisconnectTimeOutEnabled"
                If IDisconnectTimeOutEnabled.IsChecked Then
                    IDisconnectTimeOut.IsEnabled = True
                Else
                    IDisconnectTimeOut.IsEnabled = False
                End If
            Case "IMaxPacketLossEnabled"
                If IMaxPacketLossEnabled.IsChecked Then
                    IMaxPacketLoss.IsEnabled = True
                Else
                    IMaxPacketLoss.IsEnabled = False
                End If
            Case "IPersistentBattlefield"
                If IPersistentBattlefield.IsChecked Then
                    IAutoInit.IsEnabled = True
                Else
                    IAutoInit.IsEnabled = False
                End If
            Case "IHeadlessClientEnabled"
                If IHeadlessClientEnabled.IsChecked Then
                    IHcIpGroup.IsEnabled = True
                    IHcSliderGroup.IsEnabled = True
                    IHeadlessClientEnabled.ToolTip = "Disable HC"
                Else
                    IHcIpGroup.IsEnabled = False
                    IHcSliderGroup.IsEnabled = False
                    IHeadlessClientEnabled.ToolTip = "Enable HC"
                End If
            Case "IVonEnabled"
                If IVonEnabled.IsChecked Then
                    IVonGroup.IsEnabled = True
                    IVonEnabled.ToolTip = "Disable VON"
                Else
                    IVonGroup.IsEnabled = False
                    IVonEnabled.ToolTip = "Enable VON"
                End If
            Case "IVotingEnabled"
                If IVotingEnabled.IsChecked Then
                    IVotingMinPlayers.IsEnabled = True
                    IVotingThreshold.IsEnabled = True
                    IVotingEnabled.ToolTip = "Disable Voting"
                Else
                    IVotingMinPlayers.IsEnabled = False
                    IVotingThreshold.IsEnabled = False
                    IVotingEnabled.ToolTip = "Enable Voting"
                End If
            Case "IAutoRestartEnabled"
                If IAutoRestartEnabled.IsChecked Then
                    IDailyRestartAEnabled.IsEnabled = True
                    IDailyRestartBEnabled.IsEnabled = True
                    IAutoRestartEnabled.ToolTip = "Disable Auto Restart"
                Else

                    IDailyRestartAEnabled.IsEnabled = False
                    IDailyRestartBEnabled.IsEnabled = False
                    IAutoRestartEnabled.ToolTip = "Enable Auto Restart"
                End If
            Case "IServerConsoleLogEnabled"
                If IServerConsoleLogEnabled.IsChecked Then
                    IServerConsoleLog.IsEnabled = True
                    IConsoleLogButton.IsEnabled = True
                Else
                    IServerConsoleLog.IsEnabled = False
                    IConsoleLogButton.IsEnabled = False
                End If
            Case "IPidEnabled"
                If IPidEnabled.IsChecked Then
                    IPidLog.IsEnabled = True
                    IPidButton.IsEnabled = True
                Else
                    IPidLog.IsEnabled = False
                    IPidButton.IsEnabled = False
                End If
            Case "IRankingEnabled"
                If IRankingEnabled.IsChecked Then
                    IRankingLog.IsEnabled = True
                    IRankingButton.IsEnabled = True
                Else
                    IRankingLog.IsEnabled = False
                    IRankingButton.IsEnabled = False
                End If
            Case "IRequiredBuildEnabled"
                If IRequiredBuildEnabled.IsChecked Then
                    IRequiredBuild.IsEnabled = True
                Else
                    IRequiredBuild.IsEnabled = False
                End If
            Case "IDailyRestartAEnabled"
                If IDailyRestartAEnabled.IsChecked Then
                    IDailyRestartA.IsEnabled = True
                Else
                    IDailyRestartA.IsEnabled = False
                End If
            Case "IDailyRestartBEnabled"
                If IDailyRestartBEnabled.IsChecked Then
                    IDailyRestartB.IsEnabled = True
                Else
                    IDailyRestartB.IsEnabled = False
                End If
        End Select
    End Sub

    Private Sub IServerFileButton_Click(sender As Object, e As RoutedEventArgs) Handles IServerFileButton.Click
        Dim dialog As New Microsoft.Win32.OpenFileDialog
        dialog.Filter = "Arma 3 Server Files|arma*.exe"

        If dialog.ShowDialog() <> DialogResult.OK Then
            IExecutable.Text = dialog.FileName
        End If
        
    End Sub

    Private Sub IResetPerf_Click(sender As Object, e As RoutedEventArgs) Handles IResetPerf.Click
        IMaxCustomFileSize.Text = "160"
        IMaxPacketSize.Text = "1400"
        IMinBandwidth.Text = "128"
        IMaxBandwidth.Text = "2000"
        IMaxMessagesSend.Text = "128"
        IMaxSizeGuaranteed.Text = "256"
        IMaxSizeNonguaranteed.Text = "512"
        IMinErrorToSend.Text = "0.001"
        IMinErrorToSendNear.Text = "0.01"
    End Sub
End Class
