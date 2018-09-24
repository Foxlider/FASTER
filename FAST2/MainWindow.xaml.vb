Imports System.ComponentModel
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.IO.Compression
Imports System.Text
'Imports System.Windows.Forms
Imports System.Xml
Imports System.Xml.Serialization
Imports FAST2.Models
Imports WPFFolderBrowser


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
    Private _oProcess As New Process()
    Private _steamCodeValid As Boolean = False


    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        LoadServerProfiles()
        LoadSteamUpdaterSettings()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As EventArgs) Handles Me.Loaded
        If InstallSteamCmd Then
            InstallSteam()
        End If
    End Sub

    'Opens folder select dialog when clicking certain buttons
    Private Sub DirButton_Click(sender As Object, e As RoutedEventArgs) Handles ISteamDirButton.Click, IServerDirButton.Click
        Dim path As String = SelectFolder()

        If path IsNot Nothing Then
            If sender Is ISteamDirButton Then
                ISteamDirBox.Text = path
                ISteamDirBox.Focus()
            ElseIf sender Is IServerDirButton Then
                IServerDirBox.Text = path
                IServerDirBox.Focus()
            End If
        End If
    End Sub

    'Checks if the program is ready to run a command
    Public Function ReadyToUpdate() As Boolean
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

        RunSteamCommand(steamCmd, steamCommand, "server")
    End Sub

    'Installs the SteamCMD tool
    Private Sub InstallSteam()
        If ISteamDirBox.Text Is String.Empty
            Instance.IMessageDialog.IsOpen = True
            Instance.IMessageDialogText.Text = "Please make sure you have set a valid path for SteamCMD."
        Else
            If Not File.Exists(My.Settings.steamCMDPath & "\steamcmd.exe")
                IMessageDialog.IsOpen = True
                IMessageDialogText.Text = "Steam CMD will now download and start the install process. If prompted please enter your Steam Guard Code." & Environment.NewLine & Environment.NewLine & "You will receive this by email from steam. When this is all complete type 'quit' to finish."
        
                ISteamOutputBox.Document.Blocks.Clear()
                ISteamOutputBox.AppendText("Installing SteamCMD")
                ISteamOutputBox.AppendText(Environment.NewLine & "File Downloading...")

                Const url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip"
                Dim fileName As String = My.Settings.steamCMDPath & "\steamcmd.zip"

                Dim client As New WebClient()

                AddHandler client.DownloadFileCompleted, New AsyncCompletedEventHandler(AddressOf SteamDownloadCompleted)
                client.DownloadFileAsync(New Uri(url), fileName)
            Else
                Instance.IMessageDialog.IsOpen = True
                Instance.IMessageDialogText.Text = "SteamCMD already appears to be installed." & Environment.NewLine & Environment.NewLine & "Please delete all files in the selected folder to reinstall."
            End If
        End If
    End Sub

    'Continues SteamCMD install when file download is complete
    Private Sub SteamDownloadCompleted(sender As Object, e As AsyncCompletedEventArgs)
        ISteamOutputBox.AppendText(Environment.NewLine & "Download Finished")

        Dim steamPath = My.Settings.steamCMDPath
        Dim zip As String = steamPath & "\steamcmd.zip"
        
        ISteamOutputBox.AppendText(Environment.NewLine & "Unzipping...")
        ZipFile.ExtractToDirectory(zip, steamPath)
        ISteamOutputBox.AppendText(Environment.NewLine & "Installing...")
        RunSteamCommand(steamPath & "\steamcmd.exe", "+login anonymous +quit", "install")
           
        File.Delete(zip)
    End Sub

    Private Sub UpdateTextBox(text As String)
        If _oProcess IsNot Nothing
            Dim oStreamWriter = _oProcess.StandardInput

            Dispatcher.Invoke(
                Sub()
                    ISteamOutputBox.AppendText(text & Environment.NewLine)
                    ISteamOutputBox.ScrollToEnd()
                End Sub
            )

            If text Like "*at the console." Then
                Dispatcher.Invoke(
                    Sub()
                        ISteamGuardDialog.IsOpen = True
                    End Sub
                )
                Do 

                Loop Until _steamCodeValid

                _steamCodeValid = False
                Dispatcher.Invoke(
                    Sub()
                        oStreamWriter.Write(ISteamGuardCode.Text & Environment.NewLine)
                    End Sub
                    )
            ElseIf text Like "*Mobile Authenticator*" Then
                Dispatcher.Invoke(
                    Sub()
                        ISteamGuardDialog.IsOpen = True
                    End Sub
                    )

                Do 

                Loop Until _steamCodeValid

                _steamCodeValid = False
                Dispatcher.Invoke(
                    Sub()
                        oStreamWriter.Write(ISteamGuardCode.Text & Environment.NewLine)
                    End Sub
                    )
            End If

            If text Like "*Update state*" Then
                Dim counter As Integer = text.IndexOf(":", StringComparison.Ordinal)
                Dim progress As String = text.Substring(counter + 2, 2)
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

            If text Like "*Success*" Then
                Dispatcher.Invoke(
                    Sub()
                        ISteamProgressBar.Value = 100
                    End Sub
                    )
            End If

            If text Like "*Timeout*" Then
                Dispatcher.Invoke(
                    Sub()
                        Instance.IMessageDialog.IsOpen = True
                        Instance.IMessageDialogText.Text = "A Steam Download timed out. You may have to download again when task is complete."
                    End Sub
                    )
            End If
        End If
    End Sub

    Private Sub Proc_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        UpdateTextBox(e.Data)
    End Sub

    'Runs Steam command via SteamCMD and redirects input and output to FAST
    Public Async Sub RunSteamCommand(steamCmd As String, steamCommand As String, type As String)
        If ReadyToUpdate() Then
            Dim clear = True
            _oProcess = New Process()
            ISteamProgressBar.Value = 0
            ISteamCancelButton.IsEnabled = True
            ISteamUpdateButton.IsEnabled = False
            IMainContent.SelectedItem = ISteamUpdaterTab
            Dim tasks As New List(Of Task)

            ISteamProgressBar.IsIndeterminate = True

            If type Is "addon" Then
                ISteamOutputBox.AppendText("Starting SteamCMD to update Addon" & Environment.NewLine & Environment.NewLine)
            ElseIf type Is "server" Then
                ISteamOutputBox.AppendText("Starting SteamCMD to update Server" & Environment.NewLine)
            ElseIf type Is "install" Then 
               clear = False
            End If

            If clear 
                ISteamOutputBox.Document.Blocks.Clear()
            End If

            tasks.Add(Task.Run(
                Sub()
                    _oProcess.StartInfo.FileName = steamCmd
                    _oProcess.StartInfo.Arguments = steamCommand
                    _oProcess.StartInfo.UseShellExecute = False
                    _oProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                    _oProcess.StartInfo.CreateNoWindow = True
                    _oProcess.StartInfo.RedirectStandardOutput = True
                    _oProcess.StartInfo.RedirectStandardError = True
                    _oProcess.StartInfo.RedirectStandardInput = True
                    _oProcess.EnableRaisingEvents = True

                    AddHandler _oProcess.ErrorDataReceived, AddressOf Proc_OutputDataReceived
                    AddHandler _oProcess.OutputDataReceived, AddressOf Proc_OutputDataReceived
                    _oProcess.start()

                    _oProcess.BeginErrorReadLine()
                    _oProcess.BeginOutputReadLine()
                    Do
                        
                    Loop Until _oProcess.HasExited = True
                    
                End Sub
            ))

            Await Task.WhenAll(tasks)

            If (_cancelled = True) Then
                _cancelled = False

                ISteamProgressBar.IsIndeterminate = False
                ISteamProgressBar.Value = 0

                ISteamOutputBox.Document.Blocks.Clear()
                ISteamOutputBox.AppendText("Process Canceled")

                _oProcess.Close()
                _oProcess = Nothing
            Else
                ISteamOutputBox.AppendText("Task Completed" & Environment.NewLine)
                ISteamOutputBox.ScrollToEnd()
                ISteamProgressBar.IsIndeterminate = False
                ISteamProgressBar.Value = 100

                If type Is "addon" Then

                ElseIf type Is "server" Then
                    Instance.IMessageDialog.IsOpen = True
                    Instance.IMessageDialogText.Text = "Server Installed/ Updated."
                ElseIf type Is "install" Then
                    Instance.IMessageDialog.IsOpen = True
                    Instance.IMessageDialogText.Text = "SteamCMD Installed."
                End If
            End If

            ISteamCancelButton.IsEnabled = False
            ISteamUpdateButton.IsEnabled = True
        Else
            IMessageDialog.IsOpen = True
            IMessageDialogText.Text = "Please check that SteamCMD is installed and that all fields are correct:" & Environment.NewLine & Environment.NewLine & Environment.NewLine & "   -  Steam Dir" & Environment.NewLine & Environment.NewLine & "   -  User Name & Pass" & Environment.NewLine & Environment.NewLine & "   -  Server Dir"
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
        Dim folderDialog As New WPFFolderBrowserDialog

        If folderDialog.ShowDialog = True Then
            Return folderDialog.FileName
        Else
            Return Nothing
        End If
    End Function

    'Handles when any menu item is selected
    Private Sub MenuItemm_Selected(sender As ListBoxItem, e As RoutedEventArgs) Handles ISteamUpdaterTabSelect.Selected, ISteamModsTabSelect.Selected, ISettingsTabSelect.Selected, IAboutTabSelect.Selected, ILocalModsTabSelect.Selected
        Dim menus As New List(Of ListBox) From {
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

    'Makes close button red when mouse is over button
    Private Sub WindowCloseButton_MouseEnter(sender As Object, e As MouseEventArgs) Handles IWindowCloseButton.MouseEnter
        Dim converter = New BrushConverter()
        Dim brush = CType(converter.ConvertFromString("#D72C2C"), Brush)

        IWindowCloseButton.Background = brush
    End Sub

    'Changes colour of close button back to UI base when mouse leaves button
    Private Sub WindowCloseButton_MouseLeave(sender As Object, e As MouseEventArgs) Handles IWindowCloseButton.MouseLeave
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

    'Opens Tools Dialogs
    Private Sub ToolsButton_Selected(sender As Object, e As RoutedEventArgs) Handles IToolsButton.Selected
        IToolsButton.IsSelected = False
        IToolsDialog.IsOpen = True
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
                Instance.IMessageDialog.IsOpen = True
                Instance.IMessageDialogText.Text = "There may be an issue with Steam please try again shortly."
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

    Private Sub NewServerProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles INewServerProfileButton.Click
        INewServerProfileDialog.IsOpen = True
    End Sub

    Private Sub INewServerProfileDialog_KeyUp(sender As Object, e As KeyEventArgs) Handles INewServerProfileDialog.KeyUp
        If e.Key = Key.Escape Then
            INewServerProfileDialog.IsOpen = False
            INewProfileName.Text = String.Empty
        End If
    End Sub

    Private Sub ICreateProfileButton_Click(sender As Object, e As RoutedEventArgs) Handles ICreateProfileButton.Click
        INewProfileName.Text = INewProfileName.Text.Trim()

        If INewProfileName.Text = String.Empty Then
            IMessageDialog.IsOpen = True
            IMessageDialogText.Text = "Please use a suitable profile name."
        Else
            Mouse.OverrideCursor = Cursors.Wait
            Dim profileName = INewProfileName.Text
            INewServerProfileDialog.IsOpen = False
            ServerCollection.AddServerProfile(profileName, SafeName(profileName))
            INewProfileName.Text = String.Empty
            Mouse.OverrideCursor = Cursors.Arrow
        End If
    End Sub

    Private Sub InstallSteamCmd_Click(sender As Object, e As RoutedEventArgs) Handles InstallSteamCmdButton.Click
        IToolsDialog.IsOpen = False
        InstallSteam()
    End Sub

    Private Sub OpenArmaServerLocation_Click(sender As Object, e As RoutedEventArgs) Handles OpenArmaServerLocation.Click
        IToolsDialog.IsOpen = False
        Process.Start(IServerDirBox.Text)
    End Sub

    Private Sub OpenSteamCmdLocation_Click(sender As Object, e As RoutedEventArgs) Handles OpenSteamCmdLocation.Click
        IToolsDialog.IsOpen = False
        Process.Start(ISteamDirBox.Text)
    End Sub

    Private Sub IToolsDialog_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles IToolsDialog.MouseLeftButtonUp
        IToolsDialog.IsOpen = False
    End Sub

    Private Sub IServerDirBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles IServerDirBox.TextChanged
        My.Settings.serverPath = IServerDirBox.Text
    End Sub

    Private Sub ISubmitCode_Click(sender As Object, e As RoutedEventArgs) Handles ISubmitCode.Click
        _steamCodeValid = True
        ISteamGuardDialog.IsOpen = False
    End Sub

    Private Sub ISteamSettings_Changed(sender As Object, e As RoutedEventArgs) Handles ISteamUserBox.LostFocus, ISteamPassBox.LostFocus, IServerDirBox.LostFocus, ISteamDirBox.LostFocus, IServerBranch.LostFocus
        UpdateSteamUpdaterSettings()
    End Sub
    
End Class