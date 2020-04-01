using FASTER.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for SteamUpdater.xaml
    /// </summary>
    public partial class SteamUpdater
    {
        private bool _cancelled;
        private Process _oProcess = new Process();

        public SteamUpdater()
        {
            InitializeComponent();
        }
        
        public MainWindow MetroWindow
        { 
            get => (MainWindow)Window.GetWindow(this);
            set => MetroWindow = value;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            LoadSteamUpdaterSettings();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSteamUpdaterSettings();
        }

        private void ISteamDirButton_Click(object sender, RoutedEventArgs e)
        {
            string path = MetroWindow.SelectFolder(Properties.Settings.Default.steamCMDPath);

            if (path == null) return;

            ISteamDirBox.Text = path;
            ISteamDirBox.Focus();
        }

        private void IServerDirButton_Click(object sender, RoutedEventArgs e)
        {
            string path = MetroWindow.SelectFolder(Properties.Settings.Default.serverPath);

            if (path == null) return;

            IServerDirBox.Text = path;
            IServerDirBox.Focus();
        }

        private void ISteamSettings_LostFocus(object sender, RoutedEventArgs e)
        { UpdateSteamUpdaterSettings(); }

        private void IServerDirBox_TextChanged(object sender, TextChangedEventArgs e)
        { Properties.Settings.Default.serverPath = IServerDirBox.Text; }

        private void ISteamUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string steamCmd = ISteamDirBox.Text + @"\steamcmd.exe";
            string branch = "";
            switch (IServerBranch.Text)
            {
                case "Stable":
                    branch = "233780"; //Arma3 Server main branch
                    break;
                case "Contact": //Arma 3 server Contact DLC
                    branch = "233780 -beta contact";
                    break;
                case "Creator DLC": //Arma 3 server Creator DLC
                    branch = "233780 -beta creatordlc";
                    break;
                case "LegacyPorts": //Arma 3 server Legacy Ports branch for linux
                    branch = "233780 -beta legacyPorts Arma3LegacyPorts";
                    break;
                case "Developpment": //Arma 3 Developpment branch, only for developpment clients
                    branch = "107410 -beta development";
                    break;
                default:
                    Console.WriteLine("Nothing to see here");
                    break;
            }

            var steamCommand = "+login " + ISteamUserBox.Text + " " + Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword) + " +force_install_dir \"" + IServerDirBox.Text + "\" +app_update " + branch + " validate +quit";

            _ = RunSteamCommand(steamCmd, steamCommand, "server", null, true);
        }

        private void ISteamCancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _oProcess?.Kill();
                _cancelled = true;
            }
            catch (Exception ex)
            { MessageBox.Show($"CancelUpdateButton - An exception occurred:\n{ex.Message}", "Error"); }
        }


        private void ISubmitCode_Click(object sender, RoutedEventArgs e)
        {
            var oStreamWriter = _oProcess.StandardInput;
            Dispatcher?.Invoke(() => { oStreamWriter.Write(ISteamGuardCode.Text + "\n"); });
            ISteamGuardDialog.IsOpen = false;
        }

        public bool ReadyToUpdate()
        {
            return !string.IsNullOrEmpty(ISteamDirBox.Text)
                && !string.IsNullOrEmpty(ISteamUserBox.Text)
                && !string.IsNullOrEmpty(Properties.Settings.Default.steamPassword)
                && !string.IsNullOrEmpty(IServerDirBox.Text)
                && File.Exists(Properties.Settings.Default.steamCMDPath + "\\steamcmd.exe");
        }

        private void UpdateSteamUpdaterSettings()
        {
            Properties.Settings.Default.steamCMDPath = ISteamDirBox.Text;
            Properties.Settings.Default.steamUserName = ISteamUserBox.Text;
            Properties.Settings.Default.steamPassword = Encryption.Instance.EncryptData(ISteamPassBox.Password);
            Properties.Settings.Default.serverPath = IServerDirBox.Text;
            Properties.Settings.Default.serverBranch = IServerBranch.Text;
        }

        private void LoadSteamUpdaterSettings()
        {
            ISteamDirBox.Text = Properties.Settings.Default.steamCMDPath;
            ISteamUserBox.Text = Properties.Settings.Default.steamUserName;
            ISteamPassBox.Password = Encryption.Instance.DecryptData(Properties.Settings.Default.steamPassword);
            IServerDirBox.Text = Properties.Settings.Default.serverPath;
            IServerBranch.Text = Properties.Settings.Default.serverBranch;
        }


        public async Task RunSteamCommand(string steamCmd, string steamCommand, string type, List<string> modIds = null, bool localLaunch = false)
        {

            if (ReadyToUpdate())
            {
                _oProcess = new Process();
                ISteamProgressBar.Value = 0;
                ISteamCancelButton.IsEnabled = true;
                ISteamUpdateButton.IsEnabled = false;

                if(!localLaunch)
                {
                    MainWindow.Instance.NavEnabled = false;
                    MainWindow.Instance?.NavigateToConsole();
                }
                
                var tasks = new List<Task>();

                ISteamProgressBar.IsIndeterminate = true;

                switch (type)
                {
                    case "addon":
                        ISteamOutputBox.Document.Blocks.Clear();
                        ISteamOutputBox.AppendText("Starting SteamCMD to update Addon" + Environment.NewLine + Environment.NewLine);
                        break;
                    case "server:":
                        ISteamOutputBox.Document.Blocks.Clear();
                        ISteamOutputBox.AppendText("Starting SteamCMD to update Server" + Environment.NewLine);
                        break;
                    case "install":
                        ISteamOutputBox.AppendText("Proceeding with install" + Environment.NewLine);
                        break;
                }

                tasks.Add(Task.Run(() =>
                {
                    _oProcess.StartInfo.FileName = steamCmd;
                    _oProcess.StartInfo.Arguments = steamCommand;
                    _oProcess.StartInfo.UseShellExecute = false;
                    _oProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    _oProcess.StartInfo.CreateNoWindow = true;
                    _oProcess.StartInfo.RedirectStandardOutput = true;
                    _oProcess.StartInfo.RedirectStandardError = true;
                    _oProcess.StartInfo.RedirectStandardInput = true;
                    _oProcess.EnableRaisingEvents = true;

                    _oProcess.Start();

                    //NOTES
                    // SteamCMD's behaviour is quite odd. It seems like it does not keep on writing new lines but is almost constantly editing lines
                    // Editing lines does not trigger the OutputDataReceived event and nor does the Input header
                    // (When asking for user input, the programmer can add a header specifying what to enter and it is not picked by the event until the user entered its text)

                    ProcessOutputCharacters(_oProcess.StandardError);
                    ProcessOutputCharacters(_oProcess.StandardOutput);

                    _oProcess.WaitForExit();
                }));

                await Task.WhenAll(tasks);

                if (_cancelled)
                {
                    _cancelled = false;
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value = 0;

                    ISteamOutputBox.Document.Blocks.Clear();
                    ISteamOutputBox.AppendText("Process Canceled");

                    _oProcess.Close();
                    _oProcess = null;
                    CheckModUpdatesComplete(modIds);
                }
                else
                {
                    ISteamOutputBox.AppendText("SteamCMD Exited" + Environment.NewLine);
                    ISteamOutputBox.ScrollToEnd();
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value = 100;
                    switch (type)
                    {
                        case "addon":
                            CheckModUpdatesComplete(modIds);
                            break;
                        case "server:":
                            MetroWindow.DisplayMessage("Server Installed/ Updated.");
                            break;
                        case "install":
                            MetroWindow.DisplayMessage("SteamCMD Installed.");
                            break;
                    }
                }
                ISteamCancelButton.IsEnabled = false;
                ISteamUpdateButton.IsEnabled = true;

                if(!localLaunch)
                    MainWindow.Instance.NavEnabled = true;
            }
            else
            {
                MainWindow.Instance?.DisplayMessage("Please check that SteamCMD is installed and that all fields are correct: \n\n\n"
                                        + "   -  Steam Dir\n\n"
                                        + "   -  User Name & Pass\n\n"
                                        + "   -  Server Dir");
            }
        }

        private void ProcessOutputCharacters(StreamReader output)
        {
            while (!output.EndOfStream)
            {
                string line = output.ReadLine();
                if (line != null && !line.Contains("\\src\\common\\contentmanifest.cpp (650) : Assertion Failed: !m_bIsFinalized*"))
                { UpdateTextBox(line); }
            }
        }

        private bool _runLog;
        private readonly object _runLogLock = new object();
        private int threadSlept;
        private void UpdateTextBox(string text)
        {
            if (_oProcess == null) return;

            Dispatcher?.Invoke(() =>
            {
                ISteamOutputBox.AppendText(text + "\n");
                ISteamOutputBox.ScrollToEnd();
            });

            if (text.StartsWith("Logging in user") && text.Contains("to Steam"))
            {
                lock (_runLogLock)
                { _runLog = true; }
                Thread t = new Thread(() =>
                {
                    threadSlept = 0;
                    bool _localRunThread;
                    do
                    {
                        Thread.Sleep(500);
                        threadSlept += 500;
                        lock (_runLogLock)
                        { _localRunThread = _runLog; }
                    }
                    while (_localRunThread && threadSlept < 10000);
                    if (_localRunThread)
                    { 
                        Dispatcher?.Invoke(() => 
                        {
                            ISteamGuardDialog.Visibility = Visibility.Visible;
                            ISteamGuardDialog.IsOpen = true; 
                        }); 
                    }
                });
                t.Start();
            }

            if (text.Contains("Logged in OK"))
            {
                lock (_runLogLock)
                { _runLog = false; }
            }

            if (text.StartsWith("Retrying..."))
            { threadSlept = 0; }

            if (text.EndsWith("..."))
            { Dispatcher?.Invoke(() => { ISteamOutputBox.AppendText(Environment.NewLine); }); }

            if (text.Contains("Update state"))
            {
                int counter = text.IndexOf(":", StringComparison.Ordinal);
                string progress = text.Substring(counter + 2, 2);
                int progressValue;
                if (progress.Contains("."))
                { int.TryParse(progress.Substring(0, 1), out progressValue); }
                else
                { int.TryParse(progress, out progressValue); }

                Dispatcher?.Invoke(() =>
                {
                    ISteamProgressBar.IsIndeterminate = false;
                    ISteamProgressBar.Value = progressValue;
                });
            }

            if (text.Contains("Success"))
            { Dispatcher?.Invoke(() => { ISteamProgressBar.Value = 100; }); }

            if (text.Contains("Timeout"))
            {
                Dispatcher?.Invoke(() =>
                {
                    MetroWindow.DisplayMessage("A Steam Download timed out. You may have to download again when task is complete.");
                });
            }
        }

        private void CheckModUpdatesComplete(IReadOnlyCollection<string> modIds)
        {
            if (modIds == null) return;

            foreach (var modID in modIds)
            {
                var modToUpdate = Properties.Settings.Default.steamMods.SteamMods.Find(m => m.WorkshopId.ToString() == modID);
                var steamCmdOutputText = Functions.StringFromRichTextBox(ISteamOutputBox);

                if (steamCmdOutputText.Contains("ERROR! Timeout downloading"))
                { modToUpdate.Status = "Download Not Complete"; }
                else
                {
                    string modTempPath = Properties.Settings.Default.steamCMDPath + @"\steamapps\workshop\downloads\107410\" + modID;
                    string modPath = Properties.Settings.Default.steamCMDPath + @"\steamapps\workshop\content\107410\" + modID;

                    if (Directory.Exists(modTempPath))
                        modToUpdate.Status = "Download Not Complete";
                    else if (Directory.GetFiles(modPath).Length != 0)
                    {
                        modToUpdate.Status = "Up to Date";
                        var nx = new DateTime(1970, 1, 1);
                        var ts = DateTime.UtcNow - nx;

                        modToUpdate.LocalLastUpdated = (int)ts.TotalSeconds;
                    }
                }
            }
            Properties.Settings.Default.Save();
        }

    }
}
