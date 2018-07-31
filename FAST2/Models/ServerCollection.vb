Imports System.Xml.Serialization

Namespace Models

    <Serializable()>
    Public Class ServerCollection
        <XmlElement(Order:=1)>
        Public Property CollectionName As String

        <XmlElement(Order:=2, ElementName:="ServerProfile")>
        Public ServerProfiles As List(Of ServerProfile) = New List(Of ServerProfile)()
    End Class

    <Serializable()>
    Public Class ServerProfile
        Private Sub New()
        End Sub

      '<localClientBox>127.0.0.1</localClientBox>
      '<headlessIPBox>127.0.0.1</headlessIPBox>
      '<pidBox>pid.log</pidBox>
      '<rankingBox>ranking.log</rankingBox>
      '<consoleLogBox>server_console.log</consoleLogBox>
      '<serverFileBox>C:\Temp\a3\arma3server.exe</serverFileBox>
      '<serverNameBox></serverNameBox>
      '<portBox>2302</portBox>
      '<passwordBox></passwordBox>
      '<maxPlayersBox>32</maxPlayersBox>
      '<adminPassBox></adminPassBox>
      '<requiredBuildBox>0</requiredBuildBox>
      '<extraParamsBox></extraParamsBox>
      '<maxSendBox>128</maxSendBox>
      '<minErrorBox>0.001</minErrorBox>
      '<minErrorNearBox>0.01</minErrorNearBox>
      '<maxBandwidthBox>2000</maxBandwidthBox>
      '<minBandwidthBox>128</minBandwidthBox>
      '<maxNonGuaranteedBox>256</maxNonGuaranteedBox>
      '<maxGuaranteedBox>512</maxGuaranteedBox>
      '<maxPacketBox>1400</maxPacketBox>
      '<maxCustFileBox>160</maxCustFileBox>
      '<profileNameBox>Test</profileNameBox>
      '<modBox />
      '<onUnsignedDataBox />
      '<regularCheckBox />
      '<onDifferentDataBox />
      '<onHackedDataBox />
      '<onUserConnectedBox />
      '<onUserDisconnectedBox />
      '<doubleIdDetectedBox />
      '<serverCommandBox />
      '<netlogCheck>0</netlogCheck>
      '<upnpCheck>0</upnpCheck>
      '<loopbackCheck>0</loopbackCheck>
      '<enableHCCheck>0</enableHCCheck>
      '<rankingCheck>0</rankingCheck>
      '<pidCheck>0</pidCheck>
      '<consoleLogCheck>0</consoleLogCheck>
      '<requiredBuildCheck>0</requiredBuildCheck>
      '<kickDupeCheck>0</kickDupeCheck>
      '<voteCheck>1</voteCheck>
      '<vonCheck>0</vonCheck>
      '<autoInitCheck>0</autoInitCheck>
      '<persistCheck>0</persistCheck>
      '<tacticalPingCheck>1</tacticalPingCheck>
      '<multipleSavesCheck>1</multipleSavesCheck>
      '<autoReportingCheck>1</autoReportingCheck>
      '<mapContentCheck>1</mapContentCheck>
      '<vonIDCheck>1</vonIDCheck>
      '<killedByCheck>1</killedByCheck>
      '<scoreTableCheck>1</scoreTableCheck>
      '<cameraShakeCheck>1</cameraShakeCheck>
      '<thirdPersonCheck>1</thirdPersonCheck>
      '<visualAidCheck>1</visualAidCheck>
      '<crosshairCheck>1</crosshairCheck>
      '<staminaBarCheck>1</staminaBarCheck>
      '<stanceIndicatorCheck>1</stanceIndicatorCheck>
      '<weaponInfoCheck>1</weaponInfoCheck>
      '<waypointsCheck>1</waypointsCheck>
      '<commandsCheck>1</commandsCheck>
      '<detectedMinesCheck>1</detectedMinesCheck>
      '<enemyNameCheck>1</enemyNameCheck>
      '<friendlyNameCheck>1</friendlyNameCheck>
      '<groupIndicatorCheck>1</groupIndicatorCheck>
      '<reducedDamageCheck>0</reducedDamageCheck>
      '<filePatchingCheck>0</filePatchingCheck>
      '<htCheck>0</htCheck>
      '<maxPingCheck>0</maxPingCheck>
      '<maxDesyncCheck>0</maxDesyncCheck>
      '<packetLossCheck>0</packetLossCheck>
      '<disconTimeCheck>0</disconTimeCheck>
      '<kickSlowCheck>0</kickSlowCheck>
      '<battleyeCheck>0</battleyeCheck>
      '<rptTimeCombo>none</rptTimeCombo>
      '<filePatchCombo>2</filePatchCombo>
      '<verifySigCombo>0</verifySigCombo>
      '<difficultyCombo>Regular</difficultyCombo>
      '<kickSlowCombo>Log</kickSlowCombo>
      '<noOfHCNumeric>1</noOfHCNumeric>
      '<modTimeNumeric>5</modTimeNumeric>
      '<voteThresholdNumeric>33</voteThresholdNumeric>
      '<voteMinPlayersNumeric>3</voteMinPlayersNumeric>
      '<codecNumeric>3</codecNumeric>
      '<aiAccuracyNumeric>0.55</aiAccuracyNumeric>
      '<aiSkillNumeric>0.75</aiSkillNumeric>
      '<aiPresetNumeric>3</aiPresetNumeric>
      '<distanceNumeric>2500</distanceNumeric>
      '<terrainNumeric>10</terrainNumeric>
      '<disconTimeNumeric>90</disconTimeNumeric>
      '<packetLossNumeric>0</packetLossNumeric>
      '<maxDesyncNumeric>0</maxDesyncNumeric>
      '<maxPingNumeric>60</maxPingNumeric>
      '<hcModsList />
      '<serverModsList />
      '<missionsList />

    End Class
End NameSpace