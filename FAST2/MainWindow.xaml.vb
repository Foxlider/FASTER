Imports System.ComponentModel
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Windows.Forms
Imports System.Xml
Imports System.Xml.Serialization
Imports FAST2.Models


Public Class MainWindow

    Private Shared _instance As MainWindow
    
    ''' <summary>
    ''' Gets the one and only instance.
    ''' </summary>
    Public Shared ReadOnly Property Instance As MainWindow
        Get
            'If there is no instance or it has been destroyed...
            If _instance Is Nothing Then
                '...create a new one.
                _instance = New MainWindow
            End If

            Return _instance
        End Get
    End Property

    'The only constructor is private so an instance cannot be created externally.
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    
    Public InstallSteamCmd As Boolean = False
    Private _cancelled As Boolean
    Private ReadOnly _oProcess As New Process()

    'Opens folder select dialog when clicking certain buttons
    Private Sub DirButton_Click(sender As Object, e As RoutedEventArgs) Handles ISteamDirButton.Click, IServerDirButton.Click
        Dim path As String = SelectFolder()

        If path IsNot Nothing Then
            If sender Is ISteamDirButton Then
                ISteamDirBox.Text = path
            ElseIf sender Is IServerDirButton Then
                IServerDirBox.Text = path
            End If
        End If
    End Sub

    'Checks if the program is ready to run a command
    Private Function ReadyToUpdate() As Boolean
        If ISteamDirBox.Text = String.Empty Then
            Return False
        ElseIf ISteamUserBox.Text = String.Empty Then
            Return False
        ElseIf ISteamPassBox.Password = String.Empty Then
            Return False
        ElseIf IServerDirBox.Text = String.Empty Then
            Return False
        ElseIf (Not File.Exists(My.Settings.steamCMDPath & "\steamcmd.exe")) Then
            Return False
        Else
            Return True
        End If
    End Function

    'Handles when a user presses the cancel button
    Private Sub ISteamUpdateButton_Click(sender As Object, e As RoutedEventArgs) Handles ISteamUpdateButton.Click
        Dim branch, steamCommand As String
        Dim steamCmd As String = ISteamDirBox.Text + "\steamcmd.exe"

        If IServerBranch.Text = "Stable" Then
            branch = "233780"
        Else
            branch = "107410 -beta development"
        End If

        steamCommand = "+login " & ISteamUserBox.Text & " " & ISteamPassBox.Password & " +force_install_dir " & IServerDirBox.Text & " +app_update " & branch & " validate +quit"

        RunSteamCommand(steamCMD, steamCommand, "server")
    End Sub

    'Installs the SteamCMD tool
    Private Sub InstallSteam()
        IMessageDialog.IsOpen = True
        IMessageDialogText.Text = "Steam CMD will now download and start the install process. If prompted please enter your Steam Guard Code." & Environment.NewLine & Environment.NewLine & "You will recieve this by email from steam. When this is all complete type 'quit' to finish."

        Const url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip"
        Dim fileName As String = My.Settings.steamCMDPath & "\steamcmd.zip"

        Dim client As New WebClient()

        AddHandler client.DownloadFileCompleted, New AsyncCompletedEventHandler(AddressOf SteamDownloadCompleted)
        client.DownloadFileAsync(New Uri(url), fileName)

        ISteamOutputBox.AppendText("Installing SteamCMD")
        ISteamOutputBox.AppendText(Environment.NewLine & "File Downloading...")
    End Sub

    'Continues SteamCMD install when file download is complete
    Private Sub SteamDownloadCompleted(sender As Object, e As AsyncCompletedEventArgs)
        ISteamOutputBox.AppendText(Environment.NewLine & "Download Finished")

        Dim steamPath = My.Settings.steamCMDPath
        Dim zip As String = steamPath & "\steamcmd.zip"

        ISteamOutputBox.AppendText(Environment.NewLine & "Unzipping...")
        ZipFile.ExtractToDirectory(zip, steamPath)


        ISteamOutputBox.AppendText(Environment.NewLine & "Installing...")
        File.Delete(zip)

        RunSteamCommand(steamPath & "\steamcmd.exe", "+login anonymous +quit", "install")
    End Sub

    'Runs Steam command via SteamCMD and redirects input and output to FAST
    Private Async Sub RunSteamCommand(steamCmd As String, steamCommand As String, type As String, Optional modIDs As IReadOnlyCollection(Of String) = Nothing)
        If ReadyToUpdate() Then
            
            ISteamProgressBar.Value = 0
            ISteamCancelButton.IsEnabled = True
            IMainContent.SelectedItem = ISteamUpdaterTab
            Dim tasks As New List(Of Task)

            ISteamProgressBar.IsIndeterminate = True

            If type Is "addon" Then
                ISteamOutputBox.AppendText("Starting SteamCMD to update Addon" & Environment.NewLine & Environment.NewLine)
            ElseIf type Is "server" Then
                ISteamOutputBox.AppendText("Starting SteamCMD to update Server" & Environment.NewLine)
            End If

            tasks.Add(Task.Run(
                    Sub()
                        Dim oStartInfo As New ProcessStartInfo(steamCmd, steamCommand) With {
                            .CreateNoWindow = True,
                            .WindowStyle = ProcessWindowStyle.Hidden,
                            .UseShellExecute = False,
                            .RedirectStandardOutput = True,
                            .RedirectStandardInput = True,
                            .RedirectStandardError = True
                        }
                        _oProcess.StartInfo = oStartInfo
                        _oProcess.Start()

                        Dim sOutput As String

                        Dim oStreamReader As StreamReader = _oProcess.StandardOutput
                        Dim oStreamWriter As StreamWriter = _oProcess.StandardInput
                        Do
                            sOutput = oStreamReader.ReadLine

                            Dispatcher.Invoke(
                                    Sub()
' ReSharper disable once AccessToModifiedClosure
                                        ISteamOutputBox.AppendText(Environment.NewLine & sOutput)
                                    End Sub
                                )

                            If sOutput Like "*at the console." Then
                                Dim steamCode As String

                                steamCode = InputBox("Enter Steam Guard code from email or mobile app.", "Steam Guard Code", "")
                                oStreamWriter.Write(steamCode & Environment.NewLine)
                            ElseIf sOutput Like "*Mobile Authenticator*" Then
                                Dim steamCode As String

                                steamCode = InputBox("Enter Steam Guard code from email or mobile app.", "Steam Guard Code", "")
                                oStreamWriter.Write(steamCode & Environment.NewLine)
                            End If

                            If sOutput Like "*Update state*" Then
                                Dim counter As Integer = sOutput.IndexOf(":", StringComparison.Ordinal)
                                Dim progress As String = sOutput.Substring(counter + 2, 2)
                                Dim progressValue As Integer

                                If progress.Contains(".") Then
                                    progressValue = progress.Substring(0, 1)
                                Else
                                    progressValue = progress
                                End If
                                Dispatcher.Invoke(
                                            Sub()
                                                ISteamProgressBar.IsIndeterminate = False
                                                ISteamProgressBar.Value = progressValue
                                            End Sub
                                    )
                            End If

                            If sOutput Like "*Success*" Then
                                Dispatcher.Invoke(
                                            Sub()
                                                ISteamProgressBar.Value = 100
                                            End Sub
                                    )
                            End If

                            If sOutput Like "*Timeout*" Then
                                MainWindow.Instance.IMessageDialog.IsOpen = True
                                MainWindow.Instance.IMessageDialogText.Text = "Steam download timed out, please update mod again."
                            End If

                            Dispatcher.Invoke(
                                Sub()
' ReSharper disable once AccessToModifiedClosure
                                    If sOutput = Nothing Then

                                    Else
' ReSharper disable once AccessToModifiedClosure
                                        ISteamOutputBox.AppendText(sOutput & Environment.NewLine)
                                    End If

                                    'IsteamOutputBox.SelectionStart = steamOutputBox.Text.Length
                                    ISteamOutputBox.ScrollToEnd()
                                End Sub
                                )

                        Loop While _oProcess.HasExited = False


                    End Sub
                ))

            Await Task.WhenAll(tasks)

            If (_cancelled = True) Then
                _cancelled = Nothing

                ISteamProgressBar.IsIndeterminate = False
                ISteamProgressBar.Value = 0

                ISteamOutputBox.Document.Blocks.Clear()
                ISteamOutputBox.AppendText("Process Canceled")
            Else
                ISteamOutputBox.AppendText(Environment.NewLine & "Task Completed" & Environment.NewLine)
                ISteamOutputBox.ScrollToEnd()
                ISteamProgressBar.IsIndeterminate = False
                ISteamProgressBar.Value = 100

                If type Is "addon" Then
                    'UpdateModGrid()

                    For Each item In modIDs
                        CopyKeys()
                    Next

                ElseIf type Is "server" Then
                    MainWindow.Instance.IMessageDialog.IsOpen = True
                    MainWindow.Instance.IMessageDialogText.Text = "Server Installed/ Updated."
                ElseIf type Is "install" Then
                    MainWindow.Instance.IMessageDialog.IsOpen = True
                    MainWindow.Instance.IMessageDialogText.Text = "SteamCMD Installed."
                End If
            End If

            ISteamCancelButton.IsEnabled = False
            'modsTab.Enabled = True
            'updateServerButton.Enabled = True
            'modsDataGrid.PerformLayout()

        Else
            IMessageDialog.IsOpen = True
            IMessageDialogText.Text = "Please check that SteamCMD is installed and that all fields are correct:" & Environment.NewLine & Environment.NewLine & Environment.NewLine & "   -  Steam Dir" & Environment.NewLine & Environment.NewLine & "   -  User Name & Pass" & Environment.NewLine & Environment.NewLine & "   -  Server Dir"
        End If
    End Sub

    Private Shared Sub CopyKeys()
        Throw New NotImplementedException
    End Sub

    'Executes some events when the window is loaded
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim newSteamModsTab As TabItem = IMainContent.Items.Item(1)
        Dim newSettingsTab As TabItem = IMainContent.Items.Item(2)
        Dim newAboutTab As TabItem = IMainContent.Items.Item(4)
        
        newSteamModsTab.Content = New SteamMods
        newSettingsTab.Content = New Settings
        newAboutTab.Content = New About

        LoadServerProfiles()
        LoadSteamUpdaterSettings()

        If InstallSteamCmd Then
            InstallSteam()
        End If
    End Sub

    'Takes any string and removes illegal characters
    Public Shared Function SafeName(input As String, Optional ignoreWhiteSpace As Boolean = False, Optional replacement As Char = "_") As String
        If ignoreWhiteSpace Then
            input = Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement)
            input = Replace(input, replacement & replacement, replacement)
            Return input
        Else
            input = Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement)
            input = Replace(input, replacement & replacement, replacement)
            Return input
        End If
    End Function

    'Loads all server profiles and displays them in UI
    Public Sub LoadServerProfiles()
        If My.Settings.Servers IsNot Nothing Then
            Dim currentProfiles = My.Settings.Servers

            IServerProfilesMenu.Items.Clear()

            For i As Integer = IMainContent.Items.Count - 4 To 0
                IMainContent.Items.RemoveAt(i)
            Next

            For Each profile In currentProfiles.ServerProfiles
                Dim newItem As New ListBoxItem With {
                    .Name = profile.SafeName,
                    .Content = profile.DisplayName
                }

                IServerProfilesMenu.Items.Add(newItem)

                AddHandler newItem.Selected, AddressOf MenuItemm_Selected

                Dim duplicate = False

                For Each tab As TabItem In IMainContent.Items
                    If profile.SafeName = tab.Name Then
                        duplicate = True
                    End If
                Next

                If Not duplicate Then
                    Dim tabControls = New ServerProfile(profile)

                    Dim newTab As New TabItem With {
                            .Name = profile.SafeName,
                            .Content = tabControls,
                            .Header = profile.SafeName
                            }

                    IMainContent.Items.Add(newTab)
                End If
            Next
        End If
    End Sub

    'Opens Folder select dialog and returns selected path
    Public Shared Function SelectFolder()
        Dim folderDialog As New FolderBrowserDialog

        If folderDialog.ShowDialog = vbOK Then
            Return folderDialog.SelectedPath
        Else
            Return Nothing
        End If
    End Function

    'Handles when any menu item is selected
    Private Sub MenuItemm_Selected(sender As ListBoxItem, e As RoutedEventArgs) Handles ISteamUpdaterTabSelect.Selected, ISteamModsTabSelect.Selected, ISettingsTabSelect.Selected, IToolsTabSelect.Selected, IAboutTabSelect.Selected
        Dim menus As New List(Of Controls.ListBox) From {
            IMainMenuItems,
            IServerProfilesMenu,
            IOtherMenuItems
        }

        For Each list In menus
            For Each item As ListBoxItem In list.Items
                If item.Name IsNot sender.Name Then
                    item.IsSelected = False
                End If
            Next
        Next

        For Each item As TabItem In IMainContent.Items
            If item.Name = sender.Name.Replace("Select", "") Then
                IMainContent.SelectedItem = item
            End If
        Next
    End Sub

    'Creates a new Server Profile and adds it to the UI menu
    Private Shared Sub NewServerProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles INewServerProfileButton.Click
        Dim newProfileDialog As New NewServerProfile
        newProfileDialog.Show()
    End Sub

    'Makes close button red when mouse is over button
    Private Sub WindowCloseButton_MouseEnter(sender As Object, e As Input.MouseEventArgs) Handles IWindowCloseButton.MouseEnter
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#D72C2C"), Brush)

        IWindowCloseButton.Background = brush
    End Sub

    'Changes colour of close button back to UI base when mouse leaves button
    Private Sub WindowCloseButton_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles IWindowCloseButton.MouseLeave
        Dim brush = FindResource("MaterialDesignPaper")

        IWindowCloseButton.Background = brush
    End Sub

    'Closes app when using custom close button
    Private Sub WindowCloseButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowCloseButton.Selected
        Close()
    End Sub

    'Minimises app when using custom minimise button
    Private Sub WindowMinimizeButton_Selected(sender As Object, e As RoutedEventArgs) Handles IWindowMinimizeButton.Selected
        IWindowMinimizeButton.IsSelected = False
        WindowState = WindowState.Minimized
    End Sub

    'Allows user to move the window around using the custom nav bar
    Private Sub WindowDragBar_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles IWindowDragBar.MouseLeftButtonDown, ILogoImage.MouseLeftButtonDown, IWindowTitle.MouseLeftButtonDown
        DragMove()
    End Sub

    'Retrieves mods from an XML file
    Public Shared Function GetModsFromXml(filename As String) As ModCollection
        Dim xml = File.ReadAllText(filename)
        Return Deserialize(Of ModCollection)(xml)
    End Function

    Public Shared Sub ExportModsToXml(filename As String, mods As ModCollection)
        File.WriteAllText(filename, Serialize(mods))
    End Sub

    'Serialise a class
    Private Shared Function Serialize(Of T)(value As T) As String
        If value Is Nothing Then
            Return Nothing
        End If

        Dim serializer = New XmlSerializer(GetType(T))
        Dim settings = New XmlWriterSettings() With {
                .Encoding = New UnicodeEncoding(False, False),
                .Indent = True,
                .OmitXmlDeclaration = False
                }

        Using textWriter = New StringWriter()

            Using xmlWriter As XmlWriter = XmlWriter.Create(textWriter, settings)
                serializer.Serialize(xmlWriter, value)
            End Using

            Return textWriter.ToString()
        End Using
    End Function

    'Deserialse a class
    Private Shared Function Deserialize(Of T)(xml As String) As T
        If String.IsNullOrEmpty(xml) Then
            Return Nothing
        End If

        Dim serializer = New XmlSerializer(GetType(T))
        Dim settings = New XmlReaderSettings()

        Using textReader = New StringReader(xml)

            Using xmlReader As XmlReader = XmlReader.Create(textReader, settings)
                Return serializer.Deserialize(xmlReader)
            End Using
        End Using
    End Function

    'Gets mod info from Steam using Steam Web API
    Public Shared Function GetModInfo(modId As String)
        Try
            ' Create a request using a URL that can receive a post.   
            Dim request As WebRequest = WebRequest.Create("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/")
            ' Set the Method property of the request to POST.  
            request.Method = "POST"
            ' Create POST data and convert it to a byte array.  
            Dim postData As String = "itemcount=1&publishedfileids[0]=" & modId
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            ' Set the ContentType property of the WebRequest.  
            request.ContentType = "application/x-www-form-urlencoded"
            ' Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length
            ' Get the request stream.  
            Dim dataStream As Stream = request.GetRequestStream()
            ' Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length)
            ' Close the Stream object.  
            dataStream.Close()
            ' Get the response.
            Dim response As WebResponse = Nothing
            Try
                response = request.GetResponse()
            Catch ex As Exception
                MainWindow.Instance.IMessageDialog.IsOpen = True
                MainWindow.Instance.IMessageDialogText.Text = "There may be an issue with Steam please try again shortly."
            End Try
            ' Get the stream containing content returned by the server.  
            dataStream = response.GetResponseStream()
            ' Open the stream using a StreamReader for easy access.  
            Dim reader As New StreamReader(dataStream)
            ' Read the content.  
            Dim responseFromServer As String = reader.ReadToEnd()
            ' Clean up the streams.  
            reader.Close()
            dataStream.Close()
            response.Close()
            ' Return the content.  
            Return responseFromServer

        Catch ex As Exception
            MsgBox("GetModInfo - An exception occurred:" & vbCrLf & ex.Message)
            Return Nothing
        End Try
    End Function

    'Executes some code when the window is closing
    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        UpdateSteamUpdaterSettings()
        My.Settings.Save()

        If My.Settings.clearSettings Then
            My.Settings.Reset()
        End If
    End Sub

    Private Sub UpdateSteamUpdaterSettings()
        My.Settings.steamCMDPath = ISteamDirBox.Text
        My.Settings.steamUserName = ISteamUserBox.Text
        My.Settings.steamPassword = Encryption.Instance.EncryptData(ISteamPassBox.Password)
        My.Settings.serverPath = IServerDirBox.Text
        My.Settings.serverBranch = IServerBranch.Text
    End Sub

    Private Sub LoadSteamUpdaterSettings()
        ISteamDirBox.Text = My.Settings.steamCMDPath
        ISteamUserBox.Text = My.Settings.steamUserName
        ISteamPassBox.Password = Encryption.Instance.DecryptData(My.Settings.steamPassword)
        IServerDirBox.Text = My.Settings.serverPath
        IServerBranch.Text = My.Settings.serverBranch
    End Sub

    Private Sub ISteamCancelButton_Click(sender As Object, e As RoutedEventArgs) Handles ISteamCancelButton.Click
        Try
            _oProcess.Kill()
            _cancelled = True
        Catch ex As Exception
            MsgBox("CancelUpdateButton - An exception occurred:" & vbCrLf & ex.Message)
        End Try
    End Sub
End Class