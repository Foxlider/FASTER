Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports FAST2.Models

Class ServerProfile
    Shared _safeName As String
    Private ReadOnly _profilesPath = My.Settings.serverPath & "\Servers\"

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
        IPidEnabled.IsChecked = profile.PidEnabled
        IRankingEnabled.IsChecked = profile.RankingEnabled
        IRptTimestamp.Text = profile.RptTimestamp
        IMotd.Text = profile.Motd
        IMotdDelay.Text = profile.MotdDelay
        IPersistentBattlefield.IsChecked = profile.PersistentBattlefield
        IAutoInit.IsChecked = profile.AutoInit
        IDifficultyPreset.Text = profile.DifficultyPreset
        IReducedDamage.IsChecked = profile.ReducedDamage
        IGroupIndicators.Text = profile.GroupIndicators
        IFriendlyNameTags.Text = profile.FriendlyNameTags
        IEnemyNameTags.Text = profile.EnemyNameTags
        IDetectedMines.Text = profile.DetectedMines
        IMultipleSaves.IsChecked = profile.MultipleSaves
        IThirdPerson.IsChecked = profile.ThirdPerson
        IWeaponInfo.Text = profile.WeaponInfo
        IStanceIndicator.Text = profile.StanceIndicator
        IStaminaBar.IsChecked = profile.StaminaBar
        ICameraShake.IsChecked = profile.CameraShake
        IVisualAids.IsChecked = profile.VisualAids
        IExtendedMapContent.IsChecked = profile.ExtendedMapContent
        ICommands.Text = profile.Commands
        IVonId.IsChecked = profile.VonId
        IKilledBy.IsChecked = profile.KilledBy
        IWaypoints.Text = profile.Waypoints
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
        ITerrainGrid.Text = profile.TerrainGrid
        IViewDistance.Text = profile.ViewDistance
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
        ICpuCount.Text = profile.CpuCount
        IMaxMem.Text = profile.MaxMem
        IExtraParams.Text = profile.ExtraParams
        IAdminUids.Text = profile.AdminUids
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
        IServerModsList.SelectedValue = profile.ServerMods
        IClientModsList.SelectedValue = profile.ClientMods
        IHeadlessModsList.SelectedValue = profile.HeadlessMods
        IMissionCheckList.SelectedValue = profile.Missions
        IBattleEye.IsChecked = profile.BattleEye


        ToggleUi(IHeadlessClientEnabled)
        ToggleUi(IAutoRestartEnabled)
        ToggleUi(IVonEnabled)
        ToggleUi(IVotingEnabled)
        ToggleUi(IServerConsoleLogEnabled)
        ToggleUi(IPidEnabled)
        ToggleUi(IRankingEnabled)

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
            If ReadyToLaunch(IDisplayName.Content) Then
                UpdateProfile()
                LaunchServer()
            End If
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
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Filter = "Arma 3 Server Files|arma*.exe"
        }

        If dialog.ShowDialog() <> DialogResult.OK Then
            IExecutable.Text = dialog.FileName
        End If
    End Sub

    Private Sub IResetPerf_Click(sender As Object, e As RoutedEventArgs) Handles IResetPerf.Click
        IMaxCustomFileSize.Text = "160"
        IMaxPacketSize.Text = "1400"
        IMinBandwidth.Text = "131072"
        IMaxBandwidth.Text = "10000000000"
        IMaxMessagesSend.Text = "128"
        IMaxSizeGuaranteed.Text = "256"
        IMaxSizeNonguaranteed.Text = "512"
        IMinErrorToSend.Text = "0.001"
        IMinErrorToSendNear.Text = "0.01"
    End Sub

    Private Sub ServerProfile_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        UpdateModsList()
        UpdateMissionsList()
    End Sub

    Private Sub IMissionsRefresh_Click(sender As Object, e As RoutedEventArgs) Handles IRefreshMissionsButton.Click
        UpdateMissionsList()
    End Sub

    Private Sub IModsRefresh_Click(sender As Object, e As RoutedEventArgs) Handles IClientModsRefresh.Click, IHeadlessModsRefresh.Click, IServerModsRefresh.Click
        UpdateModsList()
    End Sub

    'Private Property ModsToCopy As String

    'Private Sub ModsCopy_Click(sender As Controls.Button, e As RoutedEventArgs) Handles IClientModsCopy.Click, IServerModsCopy.Click, IHeadlessModsCopy.Click
    '    Select Case sender.Name
    '        Case "IClientModsCopy"
    '            ModsToCopy = IClientModsList.SelectedValue
    '        Case "IServerModsCopy"
    '            ModsToCopy = IServerModsList.SelectedValue
    '        Case "IHeadlessModsCopy"
    '            ModsToCopy = IHeadlessModsList.SelectedValue
    '    End Select
    'End Sub

    'Private Sub ModsPaste_Click(sender As Controls.Button, e As RoutedEventArgs) Handles IClientModsPaste.Click, IServerModsPaste.Click, IHeadlessModsPaste.Click
    '    If ModsToCopy IsNot String.Empty
    '        Select Case sender.Name
    '            Case "IServerModsPaste"
    '                IServerModsList.SelectedValue = ModsToCopy
    '            Case "IClientModsPaste"
    '                IClientModsList.SelectedValue = ModsToCopy
    '            Case "IHeadlessModsPaste"
    '                IHeadlessModsList.SelectedValue = ModsToCopy
    '        End Select
    '    End If
    'End Sub

    Private Sub ModsAll_Click(sender As Controls.Button, e As RoutedEventArgs) Handles IServerModsAll.Click, IClientModsAll.Click, IHeadlessModsAll.Click, IAllMissionsButton.Click
        Select Case sender.Name
            Case "IServerModsAll"
                IServerModsList.SelectedItemsOverride = IServerModsList.Items
            Case "IClientModsAll"
                IClientModsList.SelectedItemsOverride = IClientModsList.Items
            Case "IHeadlessModsAll"
                IHeadlessModsList.SelectedItemsOverride = IHeadlessModsList.Items
            Case "IAllMissionsButton"
                IMissionCheckList.SelectedItemsOverride = IMissionCheckList.Items
        End Select
    End Sub

    Private Sub ModsNone_Click(sender As Controls.Button, e As RoutedEventArgs) Handles IServerModsNone.Click, IClientModsNone.Click, IHeadlessModsNone.Click, INoMissionsButton.Click
        Select Case sender.Name
            Case "IServerModsNone"
                IServerModsList.SelectedItemsOverride = Nothing
            Case "IClientModsNone"
                IClientModsList.SelectedItemsOverride = Nothing
            Case "IHeadlessModsNone"
                IHeadlessModsList.SelectedItemsOverride = Nothing
            Case "INoMissionsButton"
                IMissionCheckList.SelectedItemsOverride = Nothing
        End Select
    End Sub

    Private Sub IOpenRpt_Click(sender As Object, e As RoutedEventArgs) Handles IOpenRpt.Click
        OpenLastFile(_profilesPath & Functions.SafeName(IDisplayName.Content), "*.rpt")
    End Sub

    Private Sub IOpenNetlog_Click(sender As Object, e As RoutedEventArgs) Handles IOpenNetlog.Click
        OpenLastFile(Environment.ExpandEnvironmentVariables("%LocalAppData%") & "\Arma 3", "netlog-*.log")
    End Sub

    Private Sub IRankingButton_Click(sender As Object, e As RoutedEventArgs) Handles IRankingButton.Click
        OpenLastFile(_profilesPath & Functions.SafeName(IDisplayName.Content), "ranking.log")
    End Sub

    Private Sub IPidButton_Click(sender As Object, e As RoutedEventArgs) Handles IPidButton.Click
        OpenLastFile(_profilesPath & Functions.SafeName(IDisplayName.Content), "*pid*.log")
    End Sub

    Private Sub IConsoleLogButton_Click(sender As Object, e As RoutedEventArgs) Handles IConsoleLogButton.Click
        OpenLastFile(_profilesPath & Functions.SafeName(IDisplayName.Content), "server_console*.log")
    End Sub

    Private Sub IDeleteRpt_Click(sender As Object, e As RoutedEventArgs) Handles IDeleteRpt.Click
        DeleteAllFiles(_profilesPath & Functions.SafeName(IDisplayName.Content), "*.rpt")
    End Sub

    Private Shared Sub IDeleteNetlog_Click(sender As Object, e As RoutedEventArgs) Handles IDeleteNetlog.Click
        DeleteAllFiles(Environment.ExpandEnvironmentVariables("%LocalAppData%") & "\Arma 3", "netlog-*.log")
    End Sub

    Private Sub ShowRenameInterface(show As Boolean)
        If show Then
            IProfileDisplayNameEdit.Text = IDisplayName.Content
            IDisplayName.Visibility = Visibility.Collapsed
            IBattleEye.Visibility = Visibility.Collapsed
            IProfileNameEdit.Visibility = Visibility.Visible
            IProfileDisplayNameEdit.Focus()
            IProfileDisplayNameEdit.SelectAll()
        Else
            IProfileDisplayNameEdit.Text = String.Empty
            IDisplayName.Visibility = Visibility.Visible
            IBattleEye.Visibility = Visibility.Visible
            IProfileNameEdit.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Function ReadyToLaunch(profile As String)
        profile = Functions.SafeName(profile)

        If Not ProfileFilesExist(profile) Then
            Return False
        End If

        If Not (IExecutable.Text Like "*arma3server*.exe") Then
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Please select a valid Arma 3 Sever Executable."
            Return False
        End If

        If Not File.Exists(IExecutable.Text) Then
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Arma 3 Server Executable does not exist. Please reselect correct file."
            Return False
        End If
        Return True
    End Function

    Private Shared Function ProfileFilesExist(profile As String)
        Dim path = My.Settings.serverPath

        If Not Directory.Exists(path & "\Servers\" & profile) Then
            Return False
        End If

        If Not File.Exists(path & "\Servers\" & profile & "\" & profile & "_config.cfg") Then
            Return False
        End If

        If Not File.Exists(path & "\Servers\" & profile & "\" & profile & "_basic.cfg") Then
            Return False
        End If

        Return True
    End Function

    Private Sub LaunchServer()
        Dim profileName As String = Functions.SafeName(IDisplayName.Content)
        Dim profilePath As String = _profilesPath & profileName & "\"
        Dim configs As String = profilePath & profileName
        Dim start = True
        Dim playerMods As String = Nothing
        Dim serverMods As String = Nothing

        For Each addon In IServerModsList.SelectedItems
            serverMods = serverMods & addon & ";"
        Next

        For Each addon In IClientModsList.SelectedItems
            playerMods = playerMods & addon & ";"
        Next

        Try
            WriteConfigFiles(profileName)
        Catch ex As Exception
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Config files in use elsewhere - make sure server is not running."
            start = False
        End Try

        If start Then
            Dim commandLine As String
            commandLine = "-port=" & IPort.Text
            commandLine = commandLine & " ""-config=" & configs & "_config.cfg"""
            commandLine = commandLine & " ""-cfg=" & configs & "_basic.cfg"""
            commandLine = commandLine & " ""-profiles=" & profilePath & """"
            commandLine = commandLine & " -name=" & profileName
            commandLine = commandLine & " ""-mod=" & playerMods & """"
            commandLine = commandLine & " ""-serverMod=" & serverMods & """"


            If IHeadlessClientEnabled.IsChecked Then
                commandLine = commandLine & " -enableHT"
            End If

            If IFilePatching.IsChecked Then
                commandLine = commandLine & " -filePatching"
            End If

            If INetlog.IsChecked Then
                commandLine = commandLine & " -netlog"
            End If

            If IRankingEnabled.IsChecked Then
                commandLine = commandLine & " -ranking=Servers\" & Functions.SafeName(IDisplayName.Content) & "\" & "ranking.log"
            End If

            If IPidEnabled.IsChecked Then
                commandLine = commandLine & " -pid=Servers\" & Functions.SafeName(IDisplayName.Content) & "\" & "pid.log"
            End If

            If IAutoInit.IsChecked Then
                commandLine = commandLine & " -autoInit"
            End If

            If IMaxMem.Text IsNot String.Empty Then
                commandLine = commandLine & " ""-maxMem=" & IMaxMem.Text & """"
            End If

            If ICpuCount.Text IsNot String.Empty Then
                commandLine = commandLine & " ""-cpuCount=" & ICpuCount.Text & """"
            End If

            If IExtraParams.Text IsNot Nothing Then
                commandLine = commandLine & " " & IExtraParams.Text
            End If

            Clipboard.SetText(commandLine)



            Dim sStartInfo As New ProcessStartInfo(IExecutable.Text, commandLine)
            Dim sProcess As New Process With {
                    .StartInfo = sStartInfo
                }
            sProcess.Start()

            If IHeadlessClientEnabled.IsChecked Then
                For hc = 1 To INoOfHeadlessClients.Value
                    Dim hcCommandLine As String = "-client -connect=127.0.0.1 -password=" & IPassword.Text & " -profiles=" & profilePath & " -nosound"
                    Dim hcMods As String = Nothing

                    For Each addon In IHeadlessModsList.SelectedItems
                        hcMods = hcMods & addon & ";"
                    Next

                    hcCommandLine = hcCommandLine & " ""-mod=" & hcMods & """"

                    Clipboard.SetText(hcCommandLine)

                    Dim hcStartInfo As New ProcessStartInfo(IExecutable.Text, hcCommandLine)
                    Dim hcProcess As New Process With {
                            .StartInfo = hcStartInfo
                        }
                    hcProcess.Start()
                Next
            End If
        End If
    End Sub

    Private Sub WriteConfigFiles(profile As String)
        Dim profilePath = My.Settings.serverPath
        profile = Functions.SafeName(profile)

        Dim config As String = profilePath & "\Servers\" & profile & "\" & profile & "_config.cfg"
        Dim basic As String = profilePath & "\Servers\" & profile & "\" & profile & "_basic.cfg"
        Dim serverProfile As String = profilePath & "\Servers\" & profile & "\users\" & profile & "\" & profile & ".Arma3Profile"

        Directory.CreateDirectory(profilePath & "\Servers\" & profile & "\users\" & profile)

        Dim von = Not (IVonEnabled.IsChecked)

        Dim configLines As New List(Of String) From {
            "passwordAdmin = """ & IAdminPassword.Text & """;",
            "password = """ & IPassword.Text & """;",
            "serverCommandPassword = """ & IServerCommandPassword.Text & """;",
            "hostname = """ & IServerName.Text & """;",
            "maxPlayers = " & IMaxPlayers.Text & ";",
            "kickduplicate = " & IKickDuplicates.IsChecked & ";",
            "upnp = " & IUpnp.IsChecked & ";",
            "allowedFilePatching = " & IAllowFilePatching.Text & ";",
            "verifySignatures = " & IVerifySignatures.Text & ";",
            "disableVoN = " & von & ";",
            "vonCodecQuality = " & ICodecQuality.Value & ";",
            "vonCodec = 1;",
            "BattlEye = " & IBattleEye.IsChecked & ";",
            "persistent = " & IPersistentBattlefield.IsChecked & ";"
        }

        configLines.Add("motd[]= {")
        Dim lines = Functions.GetLinesCollectionFromTextBox(IMotd)

        For Each line In lines
            line = line.Replace(vbCr, "").Replace(vbLf, "")
            If Not line = "" Then
                If lines.IndexOf(line) = lines.Count - 1 Then
                    configLines.Add(vbTab & """" & line & """")
                Else
                    configLines.Add(vbTab & """" & line & """,")
                End If
            End If
        Next
        configLines.Add("};")

        configLines.Add("motdInterval = " & IMotdDelay.Text & ";")

        Dim headless = IHeadlessIps.Text.Replace(",", """,""")
        Dim local = ILocalClients.Text.Replace(",", """,""")


        If IHeadlessClientEnabled.IsChecked Then
            configLines.Add("headlessClients[] = {""" & headless & """};")
            configLines.Add("localClient[] = {""" & local & """};")
        End If

        If IVotingEnabled.IsChecked Then
            configLines.Add("allowedVoteCmds[] = {};")
            configLines.Add("allowedVotedAdminCmds[] = {};")
            configLines.Add("voteMissionPlayers = " & IVotingMinPlayers.Text & ";")
            configLines.Add("voteThreshold = " & IVotingThreshold.Text / 100 & ";")
        Else
            configLines.Add("voteMissionPlayers = " & IVotingMinPlayers.Text & ";")
            configLines.Add("voteThreshold = " & IVotingThreshold.Text / 100 & ";")
        End If

        If ILoopback.IsChecked Then
            configLines.Add("loopback = True;")
        End If

        If IDisconnectTimeOutEnabled.IsChecked Then
            configLines.Add("disconnectTimeout = " & IDisconnectTimeOut.Text & ";")
        End If

        If IMaxDesyncEnabled.IsChecked Then
            configLines.Add("maxdesync = " & IMaxDesync.Text & ";")
        End If

        If IMaxPingEnabled.IsChecked Then
            configLines.Add("maxping = " & IMaxPing.Text & ";")
        End If

        If IMaxPacketLossEnabled.IsChecked Then
            configLines.Add("maxpacketloss = " & IMaxPacketLoss.Text & ";")
        End If

        If IKickOnSlowNetworkEnabled.IsChecked Then
            If IKickOnSlowNetwork.Text = "Log" Then
                configLines.Add("kickClientsOnSlowNetwork[] = { 0, 0, 0, 0 };")
            ElseIf IKickOnSlowNetwork.Text = "Log & Kick" Then
                configLines.Add("kickClientsOnSlowNetwork[] = { 1, 1, 1, 1 };")
            End If
        End If

        If IServerConsoleLogEnabled.IsChecked Then
            configLines.Add("logFile = ""server_console.log"";")
        End If

        If IRequiredBuildEnabled.IsChecked Then
            configLines.Add("requiredBuild = " & IRequiredBuild.Text & ";")
        End If

        configLines.Add("doubleIdDetected = """ & IDoubleIdDetected.Text & """;")
        configLines.Add("onUserConnected = """ & IOnUserConnected.Text & """;")
        configLines.Add("onUserDisconnected = """ & IOnUserDisconnected.Text & """;")
        configLines.Add("onHackedData = """ & IOnHackedData.Text & """;")
        configLines.Add("onDifferentData = """ & IOnDifferentData.Text & """;")
        configLines.Add("onUnsignedData = """ & IOnUnsignedData.Text & """;")
        configLines.Add("regularCheck = """ & IRegularCheck.Text & """;")

        configLines.Add("admins[]= {")
        lines = Functions.GetLinesCollectionFromTextBox(IAdminUids)

        For Each line In lines
            line = line.Replace(vbCr, "").Replace(vbLf, "")
            If Not line = "" Then
                If lines.IndexOf(line) = lines.Count - 1 Then
                    configLines.Add(vbTab & """" & line & """")
                Else
                    configLines.Add(vbTab & """" & line & """,")
                End If
            End If
        Next
        configLines.Add("};")

        configLines.Add("timeStampFormat = """ & IRptTimestamp.Text & """;")

        configLines.Add("class Missions {")
        Dim difficulty = IDifficultyPreset.Text
        If difficulty = String.Empty Then
            difficulty = "Regular"
        End If
        For Each mission In IMissionCheckList.SelectedItems
            configLines.Add(vbTab & "class Mission_" & IMissionCheckList.SelectedItems.IndexOf(mission) + 1 & " {")
            configLines.Add(vbTab & vbTab & "template = """ & mission & """;")
            configLines.Add(vbTab & vbTab & "difficulty = """ & difficulty & """;")
            configLines.Add(vbTab & "};")
        Next
        configLines.Add("};")


        '"drawingInMap = 0;"
        '"forceRotorLibSimulation = 0;"
        '"forcedDifficulty = ""regular"";"
        '"missionWhitelist[] = {""intro.altis""};"


        Dim configMid = configLines.[Select](Function(s) s.Replace("False", "0")).ToList()
        Dim configFinal = configMid.[Select](Function(s) s.Replace("True", "1")).ToList()

        File.WriteAllLines(config, configFinal)

        Dim basicLines As New List(Of String) From {
            "adapter = -1;",
            "3D_Performance=1;",
            "Resolution_W = 0;",
            "Resolution_H = 0;",
            "Resolution_Bpp = 32;",
            "terrainGrid = " & ITerrainGrid.Text & ";",
            "viewDistance = " & IViewDistance.Text & ";",
            "Windowed = 0;",
            "MaxMsgSend =" & IMaxMessagesSend.Text & ";",
            "MaxSizeGuaranteed =" & IMaxSizeGuaranteed.Text & ";",
            "MaxSizeNonguaranteed =" & IMaxSizeNonguaranteed.Text & ";",
            "MinBandwidth =" & IMinBandwidth.Text & ";",
            "MaxBandwidth =" & IMaxBandwidth.Text & ";",
            "MinErrorToSend =" & IMinErrorToSend.Text & ";",
            "MinErrorToSendNear =" & IMinErrorToSendNear.Text & ";",
            "MaxCustomFileSize =" & IMaxCustomFileSize.Text & ";",
            "class sockets{maxPacketSize = " & IMaxPacketSize.Text & ";};"
        }

        File.WriteAllLines(basic, basicLines)

        Dim profileLines As New List(Of String) From {
            "difficulty = """ & IDifficultyPreset.Text & """;",
            "class DifficultyPresets {",
            vbTab & "class CustomDifficulty {",
            vbTab & vbTab & "class Options {",
            vbTab & vbTab & vbTab & "reduceDamage = " & IReducedDamage.IsChecked & ";",
            vbTab & vbTab & vbTab & "groupIndicators = " & IGroupIndicators.Text & ";",
            vbTab & vbTab & vbTab & "friendlyTags = " & IFriendlyNameTags.Text & ";",
            vbTab & vbTab & vbTab & "enemyTags = " & IEnemyNameTags.Text & ";",
            vbTab & vbTab & vbTab & "detectedMines = " & IDetectedMines.Text & ";",
            vbTab & vbTab & vbTab & "commands = " & ICommands.Text & ";",
            vbTab & vbTab & vbTab & "waypoints = " & IWaypoints.Text & ";",
            vbTab & vbTab & vbTab & "tacticalPing = " & ITacticalPing.IsChecked & ";",
            vbTab & vbTab & vbTab & "weaponInfo = " & IWeaponInfo.Text & ";",
            vbTab & vbTab & vbTab & "stanceIndicator = " & IStanceIndicator.Text & ";",
            vbTab & vbTab & vbTab & "staminaBar = " & IStaminaBar.IsChecked & ";",
            vbTab & vbTab & vbTab & "weaponCrosshair = " & ICrosshair.IsChecked & ";",
            vbTab & vbTab & vbTab & "visionAid = " & IVisualAids.IsChecked & ";",
            vbTab & vbTab & vbTab & "thirdPersonView = " & IThirdPerson.IsChecked & ";",
            vbTab & vbTab & vbTab & "cameraShake = " & ICameraShake.IsChecked & ";",
            vbTab & vbTab & vbTab & "scoreTable = " & IScoreTable.IsChecked & ";",
            vbTab & vbTab & vbTab & "deathMessages = " & IKilledBy.IsChecked & ";",
            vbTab & vbTab & vbTab & "vonID = " & IVonId.IsChecked & ";",
            vbTab & vbTab & vbTab & "mapContent = " & IExtendedMapContent.IsChecked & ";",
            vbTab & vbTab & vbTab & "autoReport = " & IAutoReporting.IsChecked & ";",
            vbTab & vbTab & vbTab & "multipleSaves = " & IMultipleSaves.IsChecked & ";",
            vbTab & vbTab & "};",
            "",
            vbTab & vbTab & "aiLevelPreset = " & IAiPreset.Text & ";",
            "",
            vbTab & vbTab & "class CustomAILevel {",
            vbTab & vbTab & vbTab & "skillAI = " & IAiSkill.Text & ";",
            vbTab & vbTab & vbTab & "precisionAI = " & IAiAccuracy.Text & ";",
            vbTab & vbTab & "};",
            vbTab & "};",
            "};"
        }

        Dim profileNever = profileLines.[Select](Function(s) s.Replace("Never", "0")).ToList()
        Dim profileLimited = profileNever.[Select](Function(s) s.Replace("Limited Distance", "1")).ToList()
        Dim profileFade = profileLimited.[Select](Function(s) s.Replace("Fade Out", "1")).ToList()
        Dim profileAlways = profileFade.[Select](Function(s) s.Replace("Always", "2")).ToList()
        Dim profileTrue = profileAlways.[Select](Function(s) s.Replace("True", "1")).ToList()
        Dim profileFalse = profileTrue.[Select](Function(s) s.Replace("False", "0")).ToList()

        File.WriteAllLines(serverProfile, profileFalse)

    End Sub

    Private Sub UpdateProfile()
        Try
            Dim profileName = Functions.SafeName(IDisplayName.Content)
            Dim path As String = _profilesPath
            Dim profilePath As String = path & profileName & "\"

            If Not Directory.Exists(path) Then
                Directory.CreateDirectory(path)
            End If

            If Not Directory.Exists(profilePath) Then
                Directory.CreateDirectory(profilePath)
            End If

            If Not File.Exists(profilePath & profileName & "_config.cfg") Then
                Dim cfg = File.Create(profilePath & profileName & "_config.cfg")
                cfg.Close()
            End If

            If Not File.Exists(profilePath & profileName & "_basic.cfg") Then
                Dim cfg = File.Create(profilePath & profileName & "_basic.cfg")
                cfg.Close()
            End If
        Catch ex As Exception

        End Try

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
        If IDailyRestartA.SelectedTime IsNot Nothing Then
            profile.DailyRestartA = IDailyRestartA.SelectedTime
        End If
        profile.DailyRestartBEnabled = IDailyRestartBEnabled.IsChecked
        If IDailyRestartB.SelectedTime IsNot Nothing Then
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
        profile.PidEnabled = IPidEnabled.IsChecked
        profile.RankingEnabled = IRankingEnabled.IsChecked
        profile.RptTimestamp = IRptTimestamp.Text
        profile.Motd = IMotd.Text
        profile.MotdDelay = IMotdDelay.Text
        profile.PersistentBattlefield = IPersistentBattlefield.IsChecked
        profile.AutoInit = IAutoInit.IsChecked
        profile.DifficultyPreset = IDifficultyPreset.Text
        profile.ReducedDamage = IReducedDamage.IsChecked
        profile.GroupIndicators = IGroupIndicators.Text
        profile.FriendlyNameTags = IFriendlyNameTags.Text
        profile.EnemyNameTags = IEnemyNameTags.Text
        profile.DetectedMines = IDetectedMines.Text
        profile.MultipleSaves = IMultipleSaves.IsChecked
        profile.ThirdPerson = IThirdPerson.IsChecked
        profile.WeaponInfo = IWeaponInfo.Text
        profile.StanceIndicator = IStanceIndicator.Text
        profile.StaminaBar = IStaminaBar.IsChecked
        profile.CameraShake = ICameraShake.IsChecked
        profile.VisualAids = IVisualAids.IsChecked
        profile.ExtendedMapContent = IExtendedMapContent.IsChecked
        profile.Commands = ICommands.Text
        profile.VonId = IVonId.IsChecked
        profile.KilledBy = IKilledBy.IsChecked
        profile.Waypoints = IWaypoints.Text
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
        profile.TerrainGrid = ITerrainGrid.Text
        profile.ViewDistance = IViewDistance.Text
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
        profile.CpuCount = ICpuCount.Text
        profile.MaxMem = IMaxMem.Text
        profile.ExtraParams = IExtraParams.Text
        profile.AdminUids = IAdminUids.Text
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
        profile.ServerMods = IServerModsList.SelectedValue
        profile.ClientMods = IClientModsList.SelectedValue
        profile.HeadlessMods = IHeadlessModsList.SelectedValue
        profile.Missions = IMissionCheckList.SelectedValue
        profile.BattleEye = IBattleEye.IsChecked

        My.Settings.Save()
    End Sub

    Private Sub UpdateMissionsList()
        Dim currentMissions = IMissionCheckList.Items
        Dim newMissions As New List(Of String)
        Dim checkedMissions As String = IMissionCheckList.SelectedValue

        IMissionCheckList.Items.Clear()

        If Directory.Exists(My.Settings.serverPath & "\mpmissions") Then
            For Each mission In Directory.GetFiles(My.Settings.serverPath & "\mpmissions", "*.pbo")
                newMissions.Add(Replace(mission, My.Settings.serverPath & "\mpmissions\", ""))
            Next

            For Each mission In newMissions.ToList
                For Each nMission In currentMissions
                    If nMission - mission Then
                        newMissions.Remove(nMission)
                    End If
                Next
            Next

            For Each mission In newMissions.ToList
                IMissionCheckList.Items.Add(mission.Replace(".pbo", ""))
            Next

            IMissionCheckList.SelectedValue = checkedMissions
        End If
    End Sub

    Private Sub UpdateModsList()
        Dim currentMods = IServerModsList.Items
        Dim newMods As New List(Of String)
        Dim checkedServerMods, checkedHcMods, checkedClientMods As String

        checkedServerMods = IServerModsList.SelectedValue
        checkedHcMods = IHeadlessModsList.SelectedValue
        checkedClientMods = IClientModsList.SelectedValue

        IServerModsList.Items.Clear()
        IClientModsList.Items.Clear()
        IHeadlessModsList.Items.Clear()

        If Directory.Exists(My.Settings.serverPath) Then
            For Each addon In Directory.GetDirectories(My.Settings.serverPath, "@*")
                newMods.Add(Replace(addon, My.Settings.serverPath & "\", ""))
            Next

            For Each addon In newMods.ToList
                For Each nAddon In currentMods
                    If nAddon = addon Then
                        newMods.Remove(nAddon)
                    End If
                Next
            Next

            For Each addon In newMods.ToList
                IServerModsList.Items.Add(addon)
                IClientModsList.Items.Add(addon)
                IHeadlessModsList.Items.Add(addon)
            Next

            IServerModsList.SelectedValue = checkedServerMods
            IClientModsList.SelectedValue = checkedClientMods
            IHeadlessModsList.SelectedValue = checkedHcMods
        Else
            MsgBox("Please install game before continuing.")
        End If
    End Sub

    Private Shared Sub OpenLastFile(path As String, filter As String)
        Try
            Dim dir = New DirectoryInfo(path)
            Dim file = dir.EnumerateFiles(filter).OrderByDescending(Function(f) f.LastWriteTime).FirstOrDefault()
            If file IsNot Nothing Then
                Process.Start(file.FullName)
            Else
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "Cannot Open - File Not Found" & Environment.NewLine & Environment.NewLine & "If Opening PID file make sure Server is running."
            End If
        Catch ex As Exception
            MainWindow.Instance.IMessageDialog.IsOpen = True
            MainWindow.Instance.IMessageDialogText.Text = "Cannot Open - File Not Found" & Environment.NewLine & Environment.NewLine & "If Opening PID file make sure Server is running."
        End Try
    End Sub

    Private Shared Sub DeleteAllFiles(path As String, filter As String)
        Dim dir = New DirectoryInfo(path)
        Dim files = dir.EnumerateFiles(filter)
        Dim i = 0
        For Each file In files
            file.Delete()
            i += 1
        Next
        MainWindow.Instance.IMessageDialog.IsOpen = True
        MainWindow.Instance.IMessageDialogText.Text = "Deleted " & i & " files."
    End Sub
End Class
