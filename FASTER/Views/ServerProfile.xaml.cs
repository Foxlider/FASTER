using FASTER.Models;

using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using Path = System.IO.Path;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for ServerProfile.xaml
    /// </summary>
    public partial class ServerProfile : UserControl
    {
        private readonly string _safeName;
        private readonly string _profilesPath = Properties.Settings.Default.serverPath + "\\Servers\\";
        private string _replace;
        private bool headlessClientLaunched;

        private void ServerProfile_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateModsList();
            UpdateMissionsList();
        }

        private void ServerProfile_Initialized(object sender, EventArgs e)
        {
            UpdateModsList();
            UpdateMissionsList();
        }

        public ServerProfile(Models.ServerProfile profile)
        {
            // This call is required by the designer.
            InitializeComponent();

            //Get System number format provider
            NumberFormatInfo provider = CultureInfo.CurrentCulture.NumberFormat;

            // Add any initialization after the InitializeComponent() call
            _safeName = profile.SafeName;

            IDisplayName.Content             = profile.DisplayName;
            IServerName.Text                 = profile.ServerName;
            IExecutable.Text                 = profile.Executable;
            IPassword.Text                   = profile.Password;
            IAdminPassword.Text              = profile.AdminPassword;
            IMaxPlayers.Text                 = profile.MaxPlayers.ToString();
            IPort.Text                       = profile.Port.ToString();
            IHeadlessClientEnabled.IsChecked = profile.HeadlessClientEnabled;
            IHeadlessIps.Text                = profile.HeadlessIps;
            ILocalClients.Text               = profile.LocalClients;
            INoOfHeadlessClients.Value       = profile.NoOfHeadlessClients;
            ILoopback.IsChecked              = profile.Loopback;
            IUpnp.IsChecked                  = profile.Upnp;
            INetlog.IsChecked                = profile.Netlog;
            // IAutoRestartEnabled.IsChecked = profile.AutoRestartEnabled
            // IDailyRestartAEnabled.IsChecked = profile.DailyRestartAEnabled
            // IDailyRestartA.SelectedTime = profile.DailyRestartA
            // IDailyRestartBEnabled.IsChecked = profile.DailyRestartBEnabled
            // IDailyRestartB.Text = profile.DailyRestartB
            IVotingEnabled.IsChecked            = profile.VotingEnabled;
            IVotingMinPlayers.Text              = profile.VotingMinPlayers.ToString();
            IVotingThreshold.Text               = profile.VotingThreshold.ToString(provider);
            IAllowFilePatching.Text             = profile.AllowFilePatching.ToString();
            IVerifySignatures.Text              = profile.VerifySignatures.ToString();
            IRequiredBuildEnabled.IsChecked     = profile.RequiredBuildEnabled;
            IRequiredBuild.Text                 = profile.RequiredBuild.ToString();
            IKickDuplicates.IsChecked           = profile.KickDuplicates;
            IVonEnabled.IsChecked               = profile.VonEnabled;
            ICodecQuality.Value                 = profile.CodecQuality;
            IServerConsoleLogEnabled.IsChecked  = profile.ServerConsoleLogEnabled;
            IPidEnabled.IsChecked               = profile.PidEnabled;
            IRankingEnabled.IsChecked           = profile.RankingEnabled;
            IRptTimestamp.Text                  = profile.RptTimestamp;
            IMotd.Text                          = profile.Motd;
            IMotdDelay.Text                     = profile.MotdDelay.ToString();
            IManualMissions.IsChecked           = profile.ManualMissions;
            IMissionConfig.Text                 = profile.MissionsClass;
            IPersistentBattlefield.IsChecked    = profile.PersistentBattlefield;
            IAutoInit.IsChecked                 = profile.AutoInit;
            IDifficultyPreset.Text              = profile.DifficultyPreset;
            IReducedDamage.IsChecked            = profile.ReducedDamage;
            IGroupIndicators.Text               = profile.GroupIndicators;
            IFriendlyNameTags.Text              = profile.FriendlyNameTags;
            IEnemyNameTags.Text                 = profile.EnemyNameTags;
            IDetectedMines.Text                 = profile.DetectedMines;
            IMultipleSaves.IsChecked            = profile.MultipleSaves;
            IThirdPerson.IsChecked              = profile.ThirdPerson;
            IWeaponInfo.Text                    = profile.WeaponInfo;
            IStanceIndicator.Text               = profile.StanceIndicator;
            IStaminaBar.IsChecked               = profile.StaminaBar;
            ICameraShake.IsChecked              = profile.CameraShake;
            IVisualAids.IsChecked               = profile.VisualAids;
            IMapContentFriendly.IsChecked       = profile.MapContentFriendly;
            IMapContentEnemy.IsChecked          = profile.MapContentEnemy;
            IMapContentMines.IsChecked          = profile.MapContentMines;
            ICommands.Text                      = profile.Commands;
            IVonId.IsChecked                    = profile.VonId;
            IKilledBy.IsChecked                 = profile.KilledBy;
            IWaypoints.Text                     = profile.Waypoints;
            ICrosshair.IsChecked                = profile.Crosshair;
            IAutoReporting.IsChecked            = profile.AutoReporting;
            IScoreTable.IsChecked               = profile.ScoreTable;
            ITacticalPing.IsChecked             = profile.TacticalPing;
            IAiAccuracy.Text                    = profile.AiAccuracy.ToString(provider);
            IAiSkill.Text                       = profile.AiSkill.ToString(provider);
            IAiPreset.Text                      = profile.AiPreset.ToString();
            IMaxPacketLossEnabled.IsChecked     = profile.MaxPacketLossEnabled;
            IMaxPacketLoss.Text                 = profile.MaxPacketLoss.ToString();
            IDisconnectTimeOutEnabled.IsChecked = profile.DisconnectTimeoutEnabled;
            IDisconnectTimeOut.Text             = profile.DisconnectTimeout.ToString();
            IKickOnSlowNetworkEnabled.IsChecked = profile.KickOnSlowNetworkEnabled;
            IKickOnSlowNetwork.Text             = profile.KickOnSlowNetwork;
            ITerrainGrid.Text                   = profile.TerrainGrid.ToString();
            IViewDistance.Text                  = profile.ViewDistance.ToString();
            IMaxPingEnabled.IsChecked           = profile.MaxPingEnabled;
            IMaxPing.Text                       = profile.MaxPing.ToString();
            IMaxDesyncEnabled.IsChecked         = profile.MaxDesyncEnabled;
            IMaxDesync.Text                     = profile.MaxDesync.ToString();
            IMaxCustomFileSize.Text             = profile.MaxCustomFileSize.ToString();
            IMaxPacketSize.Text                 = profile.MaxPacketSize.ToString();
            IMinBandwidth.Text                  = profile.MinBandwidth.ToString(provider);
            IMaxBandwidth.Text                  = profile.MaxBandwidth.ToString(provider);
            IMaxMessagesSend.Text               = profile.MaxMessagesSend.ToString();
            IMaxSizeNonguaranteed.Text          = profile.MaxSizeNonguaranteed.ToString();
            IMaxSizeGuaranteed.Text             = profile.MaxSizeGuaranteed.ToString();
            IMinErrorToSend.Text                = profile.MinErrorToSend.ToString(provider);
            IMinErrorToSendNear.Text            = profile.MinErrorToSendNear.ToString(provider);
            ICpuCount.Text                      = profile.CpuCount;
            IMaxMem.Text                        = profile.MaxMem;
            IExtraParams.Text                   = profile.ExtraParams;
            IAdminUids.Text                     = profile.AdminUids;
            IEnableHyperThreading.IsChecked     = profile.EnableHyperThreading;
            IFilePatching.IsChecked             = profile.FilePatching;
            IServerCommandPassword.Text         = profile.ServerCommandPassword;
            IDoubleIdDetected.Text              = profile.DoubleIdDetected;
            IOnUserConnected.Text               = profile.OnUserConnected;
            IOnUserDisconnected.Text            = profile.OnUserDisconnected;
            IOnHackedData.Text                  = profile.OnHackedData;
            IOnDifferentData.Text               = profile.OnDifferentData;
            IOnUnsignedData.Text                = profile.OnUnsignedData;
            IRegularCheck.Text                  = profile.RegularCheck;
            IServerModsList.SelectedValue       = profile.ServerMods;
            IClientModsList.SelectedValue       = profile.ClientMods;
            IHeadlessModsList.SelectedValue     = profile.HeadlessMods;
            IMissionCheckList.SelectedValue     = profile.Missions;
            IBattleEye.IsChecked                = profile.BattleEye;
            IAdditionalParams.Text              = profile.additionalParams;
            IEnableAdditionalParams.IsChecked   = profile.enableAdditionalParams;

            Loaded += ServerProfile_Loaded;
            Initialized += ServerProfile_Initialized;
            
            ToggleUi_HeadleddClientEnabled(IHeadlessClientEnabled);
            ToggleUi_VonEnabled(IVonEnabled);
            ToggleUi_VotingEnabled(IVotingEnabled);
            ToggleUi_ServerConsoleLogEnabled(IServerConsoleLogEnabled);
            ToggleUi_PidEnabled(IPidEnabled);
            ToggleUi_RankingEnabled(IRankingEnabled);
            ToggleUi_ManualMisisons(IManualMissions);
        }

        public MainWindow MetroWindow
        { get { return (MainWindow)Window.GetWindow(this); } }

        private void IProfileNameEditSave_Click(object sender, RoutedEventArgs e)
        {
            var oldName = IDisplayName.Content.ToString();
            var newName = IProfileDisplayNameEdit.Text;

            if (ServerCollection.RenameServerProfile(oldName, newName))
            {
                MetroWindow.MainContent.Navigate(MetroWindow.ContentSteamUpdater);
                MainWindow.Instance.LoadServerProfiles();
                MetroWindow.MainContent.Navigate(MetroWindow.ContentProfiles.FirstOrDefault(p => p.Name == newName));
            }
            else
            { MessageBox.Show("Could not rename Server Profile. \nPlease try again."); }
        }

        private void ISaveProfile_Click(object sender, RoutedEventArgs e)
        { UpdateProfile(); }

        private void IRenameProfile_Click(object sender, RoutedEventArgs e)
        { ShowRenameInterface(true); }

        private void IDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            IConfirmDeleteDialog.IsOpen =  true;
            IConfirmDeleteText.Text     =  $"Are you sure you want to delete the profile \"{IDisplayName.Content}\" ?";
            IConfirmDeleteBtn.Click     += DeleteProfile;
        }

        private void ILaunchServer_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadyToLaunch(IDisplayName.Content.ToString())) return;

            Analytics.TrackEvent("ServerProfile - Server launched", new Dictionary<string, string> {
                { "DisplayName", IDisplayName.Content.ToString() },
                { "ServerName", IServerName.Text}
            });
            UpdateProfile();
            LaunchServer();
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            ServerCollection.DeleteServerProfile(_safeName);
            MetroWindow.MainContent.Navigate(MetroWindow.ContentSteamUpdater);

            foreach (ToggleButton m in MetroWindow.IServerProfilesMenu.Items)
            {
                if (m.Name == _safeName)
                { MetroWindow.IServerProfilesMenu.Items.Remove(m); }
            }

            foreach (ServerProfile t in MetroWindow.ContentProfiles)
            {
                if (t.Name == _safeName)
                { MetroWindow.ContentProfiles.Remove(t); }
            }
            IConfirmDeleteBtn.Click -= DeleteProfile;
        }

        private void ToggleUi_ManualMisisons(object uiElement, RoutedEventArgs e = null)
        {
            if (IManualMissions.IsChecked ?? false)
            {
                IMissionAutoGrid.Visibility   = Visibility.Collapsed;
                IMissionManualGrid.Visibility = Visibility.Visible;
            }
            else
            {
                IMissionAutoGrid.Visibility   = Visibility.Visible;
                IMissionManualGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleUi_MaxDesyncEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IMaxDesyncEnabled.IsChecked ?? false) 
            { IMaxDesync.IsEnabled = true; }
            else 
            { IMaxDesync.IsEnabled = false; }
        }

        private void ToggleUi_MaxPingEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IMaxPingEnabled.IsChecked ?? false) 
            { IMaxPing.IsEnabled = true; }
            else 
            { IMaxPing.IsEnabled = false; }
        }

        private void ToggleUi_KickOnSlowNetworkEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IKickOnSlowNetworkEnabled.IsChecked ?? false) 
            { IKickOnSlowNetwork.IsEnabled = true; }
            else 
            { IKickOnSlowNetwork.IsEnabled = false; }
        }

        private void ToggleUi_DisconnectTimeOutEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IDisconnectTimeOutEnabled.IsChecked ?? false) 
            { IDisconnectTimeOut.IsEnabled = true; }
            else 
            { IDisconnectTimeOut.IsEnabled = false; }
        }

        private void ToggleUi_MaxPacketLoss(object uiElement, RoutedEventArgs e = null)
        {
            if (IMaxPacketLossEnabled.IsChecked ?? false) 
            { IMaxPacketLoss.IsEnabled = true; }
            else 
            { IMaxPacketLoss.IsEnabled = false; }
        }

        private void ToggleUi_PersistantBattlefield(object uiElement, RoutedEventArgs e = null)
        {
            if (IPersistentBattlefield.IsChecked ?? false) 
            { IAutoInit.IsEnabled = true; }
            else 
            { IAutoInit.IsEnabled = false; }
        }

        private void ToggleUi_HeadleddClientEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IHeadlessClientEnabled.IsChecked ?? false)
            {
                ILaunchHCs.Visibility          = Visibility.Visible;
                IHcIpGroup.IsEnabled           = true;
                IHcSliderGroup.IsEnabled       = true;
                IHeadlessClientEnabled.ToolTip = "Disable HC";
            }
            else
            {
                ILaunchHCs.Visibility          = Visibility.Collapsed;
                IHcIpGroup.IsEnabled           = false;
                IHcSliderGroup.IsEnabled       = false;
                IHeadlessClientEnabled.ToolTip = "Enable HC";
            }
        }

        private void ToggleUi_VonEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IVonEnabled.IsChecked ?? false)
            {
                IVonGroup.IsEnabled = true;
                IVonEnabled.ToolTip = "Disable VON";
            }
            else
            {
                IVonGroup.IsEnabled = false;
                IVonEnabled.ToolTip = "Enable VON";
            }
        }

        private void ToggleUi_VotingEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IVotingEnabled.IsChecked ?? false)
            {
                IVotingMinPlayers.IsEnabled = true;
                IVotingThreshold.IsEnabled  = true;
                IVotingEnabled.ToolTip      = "Disable Voting";
            }
            else
            {
                IVotingMinPlayers.IsEnabled = false;
                IVotingThreshold.IsEnabled  = false;
                IVotingEnabled.ToolTip      = "Enable Voting";
            }
        }

        private void ToggleUi_ServerConsoleLogEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IServerConsoleLogEnabled.IsChecked ?? false)
            {
                IServerConsoleLog.IsEnabled = true;
                IConsoleLogButton.IsEnabled = true;
            }
            else
            {
                IServerConsoleLog.IsEnabled = false;
                IConsoleLogButton.IsEnabled = false;
            }
        }

        private void ToggleUi_PidEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IPidEnabled.IsChecked ?? false)
            {
                IPidLog.IsEnabled    = true;
                IPidButton.IsEnabled = true;
            }
            else
            {
                IPidLog.IsEnabled    = false;
                IPidButton.IsEnabled = false;
            }
        }

        private void ToggleUi_RankingEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IRankingEnabled.IsChecked ?? false)
            {
                IRankingLog.IsEnabled    = true;
                IRankingButton.IsEnabled = true;
            }
            else
            {
                IRankingLog.IsEnabled    = false;
                IRankingButton.IsEnabled = false;
            }
        }

        private void ToggleUi_RequiredBuildEnabled(object uiElement, RoutedEventArgs e = null)
        {
            if (IRequiredBuildEnabled.IsChecked ?? false)
            { IRequiredBuild.IsEnabled = true; }
            else
            { IRequiredBuild.IsEnabled = false; }
        }

        private void ILaunchHCs_Click(object sender, RoutedEventArgs e)
        {
            string profileName = Functions.SafeName(IDisplayName.Content.ToString());
            string profilePath = _profilesPath + profileName + "\\";

            if (!(IHeadlessClientEnabled.IsChecked ?? false)) return;

            headlessClientLaunched = true;
            LaunchHCs(profilePath);
        }

        private void LaunchHCs(string profilePath)
        {
            Analytics.TrackEvent("ServerProfile - HC Clients launched", new Dictionary<string, string> {
                { "HC Number", INoOfHeadlessClients.Value.ToString()}
            });
            for (int hc = 1; hc <= INoOfHeadlessClients.Value; hc++)
            {
                string hcCommandLine = "-client -connect=127.0.0.1 -password=" + IPassword.Text + " -profiles=" + profilePath + " -nosound -port=" + IPort.Text;
                string hcMods = IHeadlessModsList.Items
                                                 .Cast<CheckBox>()
                                                 .Where(addon => addon.IsChecked ?? false)
                                                 .Aggregate<CheckBox, string>(null, (current, addon) => current + (addon.Content + ";"));

                hcCommandLine = hcCommandLine + " \"-mod=" + hcMods + "\"";
                Clipboard.SetText(hcCommandLine);
                ProcessStartInfo hcStartInfo = new ProcessStartInfo(IExecutable.Text, hcCommandLine);
                Process          hcProcess   = new Process { StartInfo = hcStartInfo };
                hcProcess.Start();
            }
        }

        private void IServerFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title                     = "Select the arma server executable",
                IsFolderPicker            = false,
                AddToMostRecentlyUsedList = false,
                InitialDirectory          = Properties.Settings.Default.serverPath,
                DefaultDirectory          = Properties.Settings.Default.serverPath,
                AllowNonFileSystemItems   = false,
                EnsureFileExists          = true,
                EnsurePathExists          = true,
                EnsureReadOnly            = false,
                EnsureValidNames          = true,
                Multiselect               = false,
                ShowPlacesList            = true
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Arma 3 Server Executable", ".exe"));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            if (dialog.FileName != null)
            { IExecutable.Text = dialog.FileName; }
            else
            { MessageBox.Show("Please enter a valid arma3server executable location"); }
        }

        private void IResetPerf_Click(object sender, RoutedEventArgs e)
        {
            IMaxCustomFileSize.Text    = "160000";
            IMaxPacketSize.Text        = "1400";
            IMinBandwidth.Text         = "131072";
            IMaxBandwidth.Text         = "10000000000";
            IMaxMessagesSend.Text      = "128";
            IMaxSizeGuaranteed.Text    = "512";
            IMaxSizeNonguaranteed.Text = "256";
            IMinErrorToSend.Text       = "0.001";
            IMinErrorToSendNear.Text   = "0.01";
        }

        private void IMissionsRefresh_Click(object sender, RoutedEventArgs e)
        { UpdateMissionsList(); }

        private void IModsRefresh_Click(object sender, RoutedEventArgs e)
        { UpdateModsList(); }

        private void ModsAll_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Control)?.Name)
            {
                case "IServerModsAll":
                    foreach (CheckBox cb in IServerModsList.Items)
                    { cb.IsChecked = true; }
                    break;
                case "IClientModsAll":
                    foreach (CheckBox cb in IClientModsList.Items)
                    { cb.IsChecked = true; }
                    break;
                case "IHeadlessModsAll":
                    foreach (CheckBox cb in IHeadlessModsList.Items)
                    { cb.IsChecked = true; }
                    break;
                case "IAllMissionsButton":
                    foreach (CheckBox cb in IMissionCheckList.Items)
                    { cb.IsChecked = true; }
                    break;
            }
        }

        private void ModsNone_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Control)?.Name)
            {
                case "IServerModsNone":
                    foreach (CheckBox cb in IServerModsList.Items)
                    { cb.IsChecked = false; }
                    break;
                case "IClientModsNone":
                    foreach (CheckBox cb in IClientModsList.Items)
                    { cb.IsChecked = false; }
                    break;
                case "IHeadlessModsNone":
                    foreach (CheckBox cb in IHeadlessModsList.Items)
                    { cb.IsChecked = false; }
                    break;
                case "INoMissionsButton":
                    foreach (CheckBox cb in IMissionCheckList.Items)
                    { cb.IsChecked = false; }
                    break;
            }
        }

        private void IOpenRpt_Click(object sender, RoutedEventArgs e)
        { OpenLastFile(Path.Combine(_profilesPath, Functions.SafeName(IDisplayName.Content.ToString())), "*.rpt"); }

        private void IOpenNetLog_Click(object sender, RoutedEventArgs e)
        { OpenLastFile(Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%"), "Arma 3"), "netlog-*.log"); }

        private void IOpenRanking_Click(object sender, RoutedEventArgs e)
        { OpenLastFile(Path.Combine(_profilesPath, Functions.SafeName(IDisplayName.Content.ToString())), "ranking.log"); }

        private void IOpenConsoleLog_Click(object sender, RoutedEventArgs e)
        { OpenLastFile(Path.Combine(_profilesPath, Functions.SafeName(IDisplayName.Content.ToString())), "server_console*.log"); }

        private void IDeleteRpt_Click(object sender, RoutedEventArgs e)
        { DeleteAllFiles(Path.Combine(_profilesPath, Functions.SafeName(IDisplayName.Content.ToString())), "*.rpt"); }

        private void IDeleteNetLog_Click(object sender, RoutedEventArgs e)
        { DeleteAllFiles(Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%"), "Arma 3"), "netlog-*.log"); }

        private void ICopyFromPlayerMods_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in IClientModsList.Items)
            {
                var c = ((CheckBox)IHeadlessModsList.Items.GetItemAt(IClientModsList.Items.IndexOf(cb)));
                c.IsChecked = cb.IsChecked;
            }
            IHeadlessModsCount.Content = IHeadlessModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
        }
        
        private async void ICopyModsKeys_Click(object sender, RoutedEventArgs e)
        {
            ICopyModsKeys.IsEnabled = false;
            await CopyModsKeysToKeyFolder();
            ICopyModsKeys.IsEnabled = true;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Control)?.Name)
            {
                case "IServerCheckBox":
                    IServerModsCount.Content = IServerModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
                    break;
                case "IClientCheckBox":
                    IClientModsCount.Content = IClientModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
                    break;
                case "IHeadlessCheckBox":
                    IHeadlessModsCount.Content = IHeadlessModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
                    break;
            }
        }

        private void UpdateProfile()
        {
            CreateProfileFiles();

            NumberFormatInfo provider = CultureInfo.CurrentCulture.NumberFormat;

            var profile = Properties.Settings.Default.Servers.ServerProfiles.Find(p => p.SafeName == _safeName);
            profile.DisplayName              = IDisplayName.Content.ToString();
            profile.ServerName               = IServerName.Text;
            profile.Executable               = IExecutable.Text;
            profile.Password                 = IPassword.Text;
            profile.AdminPassword            = IAdminPassword.Text;
            profile.MaxPlayers               = Convert.ToInt32(Convert.ToDouble(IMaxPlayers.Text), provider);
            profile.Port                     = Convert.ToInt32(Convert.ToDouble(IPort.Text),       provider);
            profile.HeadlessClientEnabled    = IHeadlessClientEnabled.IsChecked ?? false;
            profile.HeadlessIps              = IHeadlessIps.Text;
            profile.LocalClients             = ILocalClients.Text;
            profile.NoOfHeadlessClients      = (int) INoOfHeadlessClients.Value;
            profile.Loopback                 = ILoopback.IsChecked      ?? false;
            profile.Upnp                     = IUpnp.IsChecked          ?? false;
            profile.Netlog                   = INetlog.IsChecked        ?? false;
            profile.VotingEnabled            = IVotingEnabled.IsChecked ?? false;
            profile.VotingMinPlayers         = Convert.ToInt32(Convert.ToDouble(IVotingMinPlayers.Text), provider);
            profile.VotingThreshold          = decimal.Parse(IVotingThreshold.Text, provider);
            profile.AllowFilePatching        = Convert.ToInt32(Convert.ToDouble(IAllowFilePatching.Text), provider);
            profile.VerifySignatures         = Convert.ToInt32(Convert.ToDouble(IVerifySignatures.Text),  provider);
            profile.RequiredBuildEnabled     = IRequiredBuildEnabled.IsChecked ?? false;
            profile.KickDuplicates           = IKickDuplicates.IsChecked       ?? false;
            profile.VonEnabled               = IVonEnabled.IsChecked           ?? false;
            profile.CodecQuality             = Convert.ToInt32(Convert.ToDouble(ICodecQuality.Value.ToString(provider), provider), provider);
            profile.ServerConsoleLogEnabled  = IServerConsoleLogEnabled.IsChecked ?? false;
            profile.PidEnabled               = IPidEnabled.IsChecked              ?? false;
            profile.RankingEnabled           = IRankingEnabled.IsChecked          ?? false;
            profile.RptTimestamp             = IRptTimestamp.Text;
            profile.Motd                     = IMotd.Text;
            profile.MotdDelay                = Convert.ToInt32(Convert.ToDouble(IMotdDelay.Text), provider);
            profile.ManualMissions           = IManualMissions.IsChecked ?? false;
            profile.MissionsClass            = IMissionConfig.Text;
            profile.PersistentBattlefield    = IPersistentBattlefield.IsChecked ?? false;
            profile.AutoInit                 = IAutoInit.IsChecked              ?? false;
            profile.DifficultyPreset         = IDifficultyPreset.Text;
            profile.ReducedDamage            = IReducedDamage.IsChecked ?? false;
            profile.GroupIndicators          = IGroupIndicators.Text;
            profile.FriendlyNameTags         = IFriendlyNameTags.Text;
            profile.EnemyNameTags            = IEnemyNameTags.Text;
            profile.DetectedMines            = IDetectedMines.Text;
            profile.MultipleSaves            = IMultipleSaves.IsChecked ?? false;
            profile.ThirdPerson              = IThirdPerson.IsChecked   ?? false;
            profile.WeaponInfo               = IWeaponInfo.Text;
            profile.StanceIndicator          = IStanceIndicator.Text;
            profile.StaminaBar               = IStaminaBar.IsChecked         ?? false;
            profile.CameraShake              = ICameraShake.IsChecked        ?? false;
            profile.VisualAids               = IVisualAids.IsChecked         ?? false;
            profile.MapContentFriendly       = IMapContentFriendly.IsChecked ?? false;
            profile.MapContentEnemy          = IMapContentEnemy.IsChecked    ?? false;
            profile.MapContentMines          = IMapContentMines.IsChecked    ?? false;
            profile.Commands                 = ICommands.Text;
            profile.VonId                    = IVonId.IsChecked    ?? false;
            profile.KilledBy                 = IKilledBy.IsChecked ?? false;
            profile.Waypoints                = IWaypoints.Text;
            profile.Crosshair                = ICrosshair.IsChecked     ?? false;
            profile.AutoReporting            = IAutoReporting.IsChecked ?? false;
            profile.ScoreTable               = IScoreTable.IsChecked    ?? false;
            profile.TacticalPing             = ITacticalPing.IsChecked  ?? false;
            profile.AiAccuracy               = double.Parse(IAiAccuracy.Text, provider);
            profile.AiSkill                  = double.Parse(IAiSkill.Text,    provider);
            profile.AiPreset                 = Convert.ToInt32(Convert.ToDouble(IAiPreset.Text), provider);
            profile.MaxPacketLossEnabled     = IMaxPacketLossEnabled.IsChecked ?? false;
            profile.MaxPacketLoss            = Convert.ToInt32(Convert.ToDouble(IMaxPacketLoss.Text, provider), provider);
            profile.DisconnectTimeoutEnabled = IDisconnectTimeOutEnabled.IsChecked ?? false;
            profile.DisconnectTimeout        = Convert.ToInt32(Convert.ToDouble(IDisconnectTimeOut.Text, provider), provider);
            profile.KickOnSlowNetworkEnabled = IKickOnSlowNetworkEnabled.IsChecked ?? false;
            profile.KickOnSlowNetwork        = IKickOnSlowNetwork.Text;
            profile.TerrainGrid              = Convert.ToInt32(Convert.ToDouble(ITerrainGrid.Text,  provider), provider);
            profile.ViewDistance             = Convert.ToInt32(Convert.ToDouble(IViewDistance.Text, provider), provider);
            profile.MaxPingEnabled           = IMaxPingEnabled.IsChecked ?? false;
            profile.MaxPing                  = Convert.ToInt32(Convert.ToDouble(IMaxPing.Text, provider), provider);
            profile.MaxDesyncEnabled         = IMaxDesyncEnabled.IsChecked ?? false;
            profile.MaxDesync                = Convert.ToInt32(Convert.ToDouble(IMaxDesync.Text,         provider), provider);
            profile.MaxCustomFileSize        = Convert.ToInt32(Convert.ToDouble(IMaxCustomFileSize.Text, provider), provider);
            profile.MaxPacketSize            = Convert.ToInt32(Convert.ToDouble(IMaxPacketSize.Text,     provider), provider);
            profile.MinBandwidth             = double.Parse(IMinBandwidth.Text, provider);
            profile.MaxBandwidth             = double.Parse(IMaxBandwidth.Text, provider);
            profile.MaxMessagesSend          = Convert.ToInt32(Convert.ToDouble(IMaxMessagesSend.Text,      provider), provider);
            profile.MaxSizeNonguaranteed     = Convert.ToInt32(Convert.ToDouble(IMaxSizeNonguaranteed.Text, provider), provider);
            profile.MaxSizeGuaranteed        = Convert.ToInt32(Convert.ToDouble(IMaxSizeGuaranteed.Text,    provider), provider);
            profile.MinErrorToSend           = double.Parse(IMinErrorToSend.Text,     provider);
            profile.MinErrorToSendNear       = double.Parse(IMinErrorToSendNear.Text, provider);
            profile.CpuCount                 = ICpuCount.Text;
            profile.MaxMem                   = IMaxMem.Text;
            profile.ExtraParams              = IExtraParams.Text;
            profile.AdminUids                = IAdminUids.Text;
            profile.EnableHyperThreading     = IEnableHyperThreading.IsChecked ?? false;
            profile.FilePatching             = IFilePatching.IsChecked         ?? false;
            profile.ServerCommandPassword    = IServerCommandPassword.Text;
            profile.DoubleIdDetected         = IDoubleIdDetected.Text;
            profile.OnUserConnected          = IOnUserConnected.Text;
            profile.OnUserDisconnected       = IOnUserDisconnected.Text;
            profile.OnHackedData             = IOnHackedData.Text;
            profile.OnDifferentData          = IOnDifferentData.Text;
            profile.OnUnsignedData           = IOnUnsignedData.Text;
            profile.RegularCheck             = IRegularCheck.Text;
            profile.ServerMods               = "";
            foreach (CheckBox addon in IServerModsList.Items)
            {
                if (!profile.ServerMods.Contains((string) addon.Content) && (addon.IsChecked ?? false))
                { profile.ServerMods += $"{addon.Content};"; }
            }
            profile.ClientMods = "";
            foreach (CheckBox addon in IClientModsList.Items)
            {
                if (!profile.ClientMods.Contains((string)addon.Content) && (addon.IsChecked ?? false))
                { profile.ClientMods += $"{addon.Content};"; }
            }
            profile.HeadlessMods = "";
            foreach (CheckBox addon in IHeadlessModsList.Items)
            {
                if (!profile.HeadlessMods.Contains((string)addon.Content) && (addon.IsChecked ?? false))
                { profile.HeadlessMods += $"{addon.Content};"; }
            }
            profile.Missions = "";
            foreach (CheckBox addon in IMissionCheckList.Items)
            {
                if (!profile.Missions.Contains((string)addon.Content) && (addon.IsChecked ?? false))
                { profile.Missions += $"{addon.Content};"; }
            }
            profile.BattleEye = IBattleEye.IsChecked ?? false;
            profile.additionalParams = IAdditionalParams.Text;
            profile.enableAdditionalParams = IEnableAdditionalParams.IsChecked ?? false;

            Properties.Settings.Default.Save();
        }

        private void CreateProfileFiles()
        {
            try
            {
                object profileName = Functions.SafeName(IDisplayName.Content.ToString());
                string path        = _profilesPath;
                string profilePath = path
                                   + (profileName + "\\");
                if (!Directory.Exists(path)) 
                { Directory.CreateDirectory(path); }

                if (!Directory.Exists(profilePath)) 
                { Directory.CreateDirectory(profilePath); }

                if (!File.Exists(profilePath + (profileName + "_config.cfg")))
                {
                    var cfg = File.Create(profilePath + (profileName + "_config.cfg"));
                    cfg.Close();
                }

                if (!File.Exists(profilePath + (profileName + "_basic.cfg")))
                {
                    var cfg = File.Create(profilePath + (profileName + "_basic.cfg"));
                    cfg.Close();
                }
            }
            catch (Exception e) { Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } }); }
        }

        private void WriteConfigFiles(string profile)
        {
            string profilePath = Properties.Settings.Default.serverPath;
            profile = Functions.SafeName(profile);

            string config        = Path.Combine(profilePath, "Servers", profile, $"{profile}_config.cfg");
            string basic         = Path.Combine(profilePath, "Servers", profile, $"{profile}_basic.cfg");
            string serverProfile = Path.Combine(profilePath, "Servers", profile, "users", profile, $"{profile}.Arma3Profile");

            Directory.CreateDirectory(Path.Combine(profilePath, "Servers", profile, "users", profile));

            #region CONFIG FILE CREATION
            ConfFileCreation(config);
            #endregion

            #region BASIC FILE CREATION
            List<string> basicLines = new List<string>
            {
                "adapter = -1;",
                "3D_Performance=1;",
                "Resolution_W = 0;",
                "Resolution_H = 0;",
                "Resolution_Bpp = 32;",
                $"terrainGrid = {ITerrainGrid.Text};",
                $"viewDistance = {IViewDistance.Text};",
                "Windowed = 0;",
                $"MaxMsgSend = {IMaxMessagesSend.Text};",
                $"MaxSizeGuaranteed = {IMaxSizeGuaranteed.Text};",
                $"MaxSizeNonguaranteed = {IMaxSizeNonguaranteed.Text};",
                $"MinBandwidth = {IMinBandwidth.Text};",
                $"MaxBandwidth = {IMaxBandwidth.Text};",
                $"MinErrorToSend = {IMinErrorToSend.Text.Replace(',', '.')};",
                $"MinErrorToSendNear = {IMinErrorToSendNear.Text.Replace(',', '.')};",
                $"MaxCustomFileSize = {IMaxCustomFileSize.Text};",
                "class sockets{maxPacketSize = " + IMaxPacketSize.Text + ";};"
            };
            File.WriteAllLines(basic, basicLines);
            #endregion

            #region PROFILE FILE CREATION
            List<string> profileLines = new List<string>
            {
                $"difficulty = \"{IDifficultyPreset.Text}\";",
                "class DifficultyPresets {",
                "\tclass CustomDifficulty {",
                "\t\tclass Options {",
                $"\t\t\treduceDamage={IReducedDamage.IsChecked};",
                $"\t\t\tgroupIndicators={IGroupIndicators.Text};",
                $"\t\t\tfriendlyTags={IFriendlyNameTags.Text};",
                $"\t\t\tenemyTags={IEnemyNameTags.Text};",
                $"\t\t\tdetectedMines={IDetectedMines.Text};",
                $"\t\t\tcommands={ICommands.Text};",
                $"\t\t\twaypoints={IWaypoints.Text};",
                $"\t\t\ttacticalPing={ITacticalPing.IsChecked};",
                $"\t\t\tweaponInfo={IWeaponInfo.Text};",
                $"\t\t\tstanceIndicator={IStanceIndicator.Text};",
                $"\t\t\tstaminaBar={IStaminaBar.IsChecked};",
                $"\t\t\tweaponCrosshair={ICrosshair.IsChecked};",
                $"\t\t\tvisionAid={IVisualAids.IsChecked};",
                $"\t\t\tthirdPersonView={IThirdPerson.IsChecked};",
                $"\t\t\tcameraShake={ICameraShake.IsChecked};",
                $"\t\t\tscoreTable={IScoreTable.IsChecked};",
                $"\t\t\tdeathMessages={IKilledBy.IsChecked};",
                $"\t\t\tvonID={IVonId.IsChecked};",
                $"\t\t\tmapContentFriendly={IMapContentFriendly.IsChecked};",
                $"\t\t\tmapContentEnemy={IMapContentEnemy.IsChecked};",
                $"\t\t\tmapContentMines={IMapContentMines.IsChecked};",
                $"\t\t\tautoReport={IAutoReporting.IsChecked};",
                $"\t\t\tmultipleSaves={IMultipleSaves.IsChecked};",
                "\t\t};",
                "",
                $"\t\taiLevelPreset={IAiPreset.Text};",
                "",
                "\t\tclass CustomAILevel {",
                $"\t\t\tskillAI={IAiSkill.Text};",
                $"\t\t\tprecisionAI={IAiAccuracy.Text};",
                "\t\t};",
                "\t};",
                "};"
            };
            var profileNever   = profileLines.Select(s => s.Replace("Never", "0")).ToList();
            var profileLimited = profileNever.Select(s => s.Replace("Limited Distance", "1")).ToList();
            var profileFade    = profileLimited.Select(s => s.Replace("Fade Out", "1")).ToList();
            var profileAlways  = profileFade.Select(s => s.Replace("Always", "2")).ToList();
            var profileTrue    = profileAlways.Select(s => s.Replace("True", "1")).ToList();
            var profileFalse   = profileTrue.Select(s => s.Replace("False", "0")).ToList();
            File.WriteAllLines(serverProfile, profileFalse);
            #endregion  
        }

        private void ConfFileCreation(string config)
        {
            var von = !IVonEnabled.IsChecked ?? true;

            //Creating first part of the config lines
            var configLines = ConfigLinesPartOne(von, out var lines);

            configLines.Add("};");
            configLines.Add($"motdInterval = {IMotdDelay.Text};");
            var headless = IHeadlessIps.Text.Replace(",", "\",\"");
            var local    = ILocalClients.Text.Replace(",", "\",\"");

            //Adding a lot of checkbox values
            ConfigFileChecks(configLines, headless, local);

            configLines.Add($"doubleIdDetected = \"{IDoubleIdDetected.Text}\";");
            configLines.Add($"onUserConnected = \"{IOnUserConnected.Text}\";");
            configLines.Add($"onUserDisconnected = \"{IOnUserDisconnected.Text}\";");
            configLines.Add($"onHackedData = \"{IOnHackedData.Text}\";");
            configLines.Add($"onDifferentData = \"{IOnDifferentData.Text}\";");
            configLines.Add($"onUnsignedData = \"{IOnUnsignedData.Text}\";");
            configLines.Add($"regularCheck = \"{IRegularCheck.Text}\";");
            configLines.Add("admins[]= {");
            var admins = IAdminUids.Text.Split('\n');
            foreach (var line in admins)
            {
                _replace = line;
                _replace = _replace.Replace("\r", "").Replace("\n", "");
                configLines.Add(lines.IndexOf(_replace) == lines.Count - 1 ? $"\t\"{_replace}\"" : $"\t\"{_replace}\",");
            }

            configLines.Add("};");
            configLines.Add($"timeStampFormat = \"{IRptTimestamp.Text}\";");
            if (IManualMissions.IsChecked ?? false)
            {
                var missionLines = Functions.GetLinesCollectionFromTextBox(IMissionConfig);
                foreach (var line in missionLines) { configLines.Add(line); }
            }
            else
            {
                configLines.Add("class Missions {");
                var difficulty = IDifficultyPreset.Text;
                if (string.IsNullOrEmpty(difficulty)) { difficulty = "Regular"; }

                foreach (CheckBox mission in IMissionCheckList.Items)
                {
                    if (!(mission.IsChecked ?? false)) continue;

                    configLines.Add($"\tclass Mission_{IMissionCheckList.Items.IndexOf(mission) + 1} " + "{");
                    configLines.Add($"\t\ttemplate = \"{mission.Content}\";");
                    configLines.Add($"\t\tdifficulty = \"{difficulty}\";");
                    configLines.Add("\t};");
                }
                configLines.Add("};");
            }

            if (IEnableAdditionalParams.IsChecked ?? false)
            {
                var moreParams = Functions.GetLinesCollectionFromTextBox(IAdditionalParams);
                foreach (var line in moreParams) { configLines.Add(line); }
            }

            File.WriteAllLines(config, configLines);
        }

        private List<string> ConfigLinesPartOne(bool von, out StringCollection lines)
        {
            List<string> configLines = new List<string>
            {
                $"passwordAdmin = \"{IAdminPassword.Text}\";",
                $"password = \"{IPassword.Text}\";",
                $"serverCommandPassword = \"{IServerCommandPassword.Text}\";",
                $"hostname = \"{IServerName.Text}\";",
                $"maxPlayers = {IMaxPlayers.Text};",
                $"kickduplicate = {(IKickDuplicates.IsChecked != null && (bool) IKickDuplicates.IsChecked ? "1" : "0")};",
                $"upnp = {(IUpnp.IsChecked                    != null && (bool) IUpnp.IsChecked ? "1" : "0")};",
                $"allowedFilePatching = {IAllowFilePatching.Text};",
                $"verifySignatures = {IVerifySignatures.Text};",
                $"disableVoN = {(von ? "1" : "0")};",
                $"vonCodecQuality = {(int) ICodecQuality.Value};",
                "vonCodec = 1;",
                $"BattlEye = {(IBattleEye.IsChecked               != null && (bool) IBattleEye.IsChecked ? "1" : "0")};",
                $"persistent = {(IPersistentBattlefield.IsChecked != null && (bool) IPersistentBattlefield.IsChecked ? "1" : "0")};",
                "motd[]= {"
            };

            lines = Functions.GetLinesCollectionFromTextBox(IMotd);
            foreach (var line in lines)
            {
                _replace = line;
                _replace = _replace.Replace("\r", "").Replace("\n", "");

                configLines.Add(lines.IndexOf(_replace) == lines.Count - 1 ? $"\t\"{_replace}\"" : $"\t\"{_replace}\",");
            }
            return configLines;
        }

        private void ConfigFileChecks(List<string> configLines, string headless, string local)
        {
            if (IHeadlessClientEnabled.IsChecked ?? false)
            {
                configLines.Add("headlessClients[] = {\"" + headless + "\"};");
                configLines.Add("localClient[] = {\"" + local        + "\"};");
            }

            if (IVotingEnabled.IsChecked ?? false)
            {
                configLines.Add($"voteMissionPlayers = {IVotingMinPlayers.Text};");
                configLines.Add($"voteThreshold = {double.Parse(IVotingThreshold.Text) / 100.0};");
            }
            else
            {
                configLines.Add("allowedVoteCmds[] = {};");
                configLines.Add("allowedVotedAdminCmds[] = {};");
                configLines.Add("voteMissionPlayers = 1;");
                configLines.Add("voteThreshold = 0;");
            }

            if (ILoopback.IsChecked ?? false) 
            { configLines.Add("loopback = True;"); }

            if (IDisconnectTimeOutEnabled.IsChecked ?? false) 
            { configLines.Add($"disconnectTimeout = {IDisconnectTimeOut.Text};"); }

            if (IMaxDesyncEnabled.IsChecked ?? false) 
            { configLines.Add($"maxdesync = {IMaxDesync.Text};"); }

            if (IMaxPingEnabled.IsChecked ?? false) 
            { configLines.Add($"maxping = {IMaxPing.Text};"); }

            if (IMaxPacketLossEnabled.IsChecked ?? false) 
            { configLines.Add($"maxpacketloss = {IMaxPacketLoss.Text};"); }

            if (IKickOnSlowNetworkEnabled.IsChecked ?? false)
            {
                switch (IKickOnSlowNetwork.Text)
                {
                    case "Log":
                        configLines.Add("kickClientsOnSlowNetwork[] = { 0, 0, 0, 0 };");
                        break;
                    case "Log & Kick":
                        configLines.Add("kickClientsOnSlowNetwork[] = { 1, 1, 1, 1 };");
                        break;
                }
            }

            if (IServerConsoleLogEnabled.IsChecked ?? false) 
            { configLines.Add("logFile = \"server_console.log\";"); }

            if (IRequiredBuildEnabled.IsChecked ?? false) 
            { configLines.Add($"requiredBuild = {IRequiredBuild.Text};"); }
        }

        private void UpdateMissionsList()
        {
            var          currentMissions = IMissionCheckList.Items;
            List<string> newMissions     = new List<string>();
            var          checkedMissions = new List<CheckBox>();
            var          profile         = Properties.Settings.Default.Servers.ServerProfiles.Find(p => p.SafeName == _safeName);

            if (profile != null)
            {
                foreach (var mission in profile.Missions.Split(';'))
                {
                    if (checkedMissions.FirstOrDefault(c => (string) c.Content == mission.Replace(";", "")) == null && !string.IsNullOrWhiteSpace(mission))
                        checkedMissions.Add(new CheckBox { Content = mission.Replace(";", ""), IsChecked = true });
                }
            }

            IMissionCheckList.Items.Clear();

            if (!Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"))) return;

            newMissions.AddRange(Directory.GetFiles(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions"), "*.pbo")
                                          .Select(mission => mission.Replace(Path.Combine(Properties.Settings.Default.serverPath, "mpmissions") + "\\", "")));

            foreach (var mission in newMissions.ToList()
                                               .Where(mission => currentMissions.Contains(mission))) 
            { newMissions.Remove(mission); }

            foreach (var mission in newMissions.ToList())
            {
                var checkedMission = checkedMissions.FirstOrDefault(m => (string)m.Content == mission.Replace(".pbo", ""))?.IsChecked ?? false;
                IMissionCheckList.Items.Add(new CheckBox { Content = mission.Replace(".pbo", "") , IsChecked = checkedMission});
            }
            
            IMissionCheckList.SelectedValue = checkedMissions;
        }

        private void UpdateModsList()
        {
            var          currentMods = IServerModsList.Items;
            List<string> newMods     = new List<string>();

            var profile           = Properties.Settings.Default.Servers.ServerProfiles.Find(p => p.SafeName == _safeName);
            var checkedServerMods = new List<CheckBox>();
            var checkedHcMods = new List<CheckBox>();
            var checkedClientMods = new List<CheckBox>();
            
            if (profile != null) 
            { SetUpModlists(profile, checkedServerMods, checkedHcMods, checkedClientMods); }

            IServerModsList.Items.Clear();
            IClientModsList.Items.Clear();
            IHeadlessModsList.Items.Clear();

            if (Directory.Exists(Properties.Settings.Default.serverPath))
            {
                newMods.AddRange(Directory.GetDirectories(Properties.Settings.Default.serverPath, "@*")
                                          .Select(addon => addon.Replace(Properties.Settings.Default.serverPath + @"\", "")));
                List<string> targetForDeletion = new List<string>();
                foreach (var folder in Properties.Settings.Default.localModFolders)
                {
                    if (Directory.Exists(folder))
                        newMods.AddRange(Directory.GetDirectories(folder, "@*"));
                    else
                    {
                        if (!IFlyout.IsOpen || IFlyoutMessage.Content as string != "A folder could not be found and have been deleted")
                        {
                            IFlyoutMessage.Content = "A folder could not be found and have been deleted";
                            IFlyout.IsOpen         = true;
                        }
                        targetForDeletion.Add(folder);
                    }
                }
                //Cleanup localmodfolders
                foreach (var folder in targetForDeletion)
                { Properties.Settings.Default.localModFolders.Remove(folder); }
                
                foreach (var addon in newMods.ToList()
                                             .Where(addon => currentMods.Contains(addon))) 
                { newMods.Remove(addon); }

                foreach (var addon in newMods.ToList())
                {
                    var serverCheck = checkedServerMods.FirstOrDefault(m => (string) m.Content == addon)?.IsChecked ?? false;
                    var clientCheck = checkedClientMods.FirstOrDefault(m => (string)m.Content == addon)?.IsChecked ?? false;
                    var hcCheck = checkedHcMods.FirstOrDefault(m => (string)m.Content == addon)?.IsChecked ?? false;
                    IServerModsList.Items.Add(new CheckBox { Content = addon, IsChecked = serverCheck });
                    IClientModsList.Items.Add(new CheckBox { Content = addon, IsChecked = clientCheck  });
                    IHeadlessModsList.Items.Add(new CheckBox { Content = addon, IsChecked = hcCheck });
                }

                IServerModsList.SelectedValue   = checkedServerMods;
                IClientModsList.SelectedValue   = checkedClientMods;
                IHeadlessModsList.SelectedValue = checkedHcMods;
            }
            else
            { MessageBox.Show("Please install game before continuing."); }
        }

        private void SetUpModlists(Models.ServerProfile profile, List<CheckBox> checkedServerMods, List<CheckBox> checkedHcMods, List<CheckBox> checkedClientMods)
        {
            foreach (var mod in profile.ServerMods.Split(';'))
            {
                if (checkedServerMods.FirstOrDefault(c => (string) c.Content == mod.Replace(";", "")) == null && !string.IsNullOrWhiteSpace(mod)) 
                    checkedServerMods.Add(new CheckBox { Content = mod.Replace(";", ""), IsChecked = true });
            }
            IServerModsCount.Content = IServerModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
            foreach (var mod in profile.HeadlessMods.Split(';'))
            {
                if (checkedHcMods.FirstOrDefault(c => (string) c.Content == mod.Replace(";", "")) == null && !string.IsNullOrWhiteSpace(mod)) 
                    checkedHcMods.Add(new CheckBox { Content = mod.Replace(";", ""), IsChecked = true });
            }
            IClientModsCount.Content = IClientModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
            foreach (var mod in profile.ClientMods.Split(';'))
            {
                if (checkedClientMods.FirstOrDefault(c => (string) c.Content == mod.Replace(";", "")) == null && !string.IsNullOrWhiteSpace(mod)) 
                    checkedClientMods.Add(new CheckBox { Content = mod.Replace(";", ""), IsChecked = true });
            }
            IHeadlessModsCount.Content = IHeadlessModsList.Items.Cast<object>().Count(i => ((CheckBox) i).IsChecked == true);
        }

        private void OpenLastFile(string path, string filter)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                var file = dir.EnumerateFiles(filter).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (file != null)
                {
                    try
                    { Process.Start(file.FullName); }
                    catch (Exception e)
                    { Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } }); }
                }
                else
                {
                    MetroWindow.DisplayMessage("Cannot Open - File Not Found \n\nIf Opening PID file make sure Server is running.");
                }
            }
            catch (Exception)
            {
                MetroWindow.DisplayMessage("Cannot Open - File Not Found \n\nIf Opening PID file make sure Server is running.");
            }
        }

        private void DeleteAllFiles(string path, string filter)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.EnumerateFiles(filter);
            var i = 0;
            foreach (var file in files)
            {
                try { file.Delete(); }
                catch (Exception e) { Crashes.TrackError(e, new Dictionary<string, string> {{ "Name", Properties.Settings.Default.steamUserName }}); }
                i += 1;
            }
            MetroWindow.DisplayMessage($"Deleted {i} files.");
        }

        private void LaunchServer()
        {
            string profileName = Functions.SafeName(IDisplayName.Content.ToString());
            string profilePath = Path.Combine(_profilesPath, profileName);
            string configs = Path.Combine(profilePath, profileName);
            bool start = true;
            string serverMods = IServerModsList.Items.Cast<CheckBox>()
                                               .Where(addon => addon.IsChecked ?? false)
                                               .Aggregate<CheckBox, string>(null, (current, addon) => current + (addon.Content + ";"));

            string playerMods = IClientModsList.Items.Cast<CheckBox>()
                                               .Where(addon => addon.IsChecked ?? false)
                                               .Aggregate<CheckBox, string>(null, (current, addon) => current + (addon.Content + ";"));

            try
            { WriteConfigFiles(profileName); }
            catch (Exception)
            {
                MetroWindow.DisplayMessage("Config files in use elsewhere - make sure server is not running.");
                start = false;
            }

            if (!start) return;

            var commandLine = SetCommandLine(configs, profilePath, profileName, playerMods, serverMods);
            try { Clipboard.SetText(commandLine); }
            catch (COMException e)
            {
                try
                {
                    Crashes.TrackError(e, new Dictionary<string, string>
                                           {{ "Name", Properties.Settings.Default.steamUserName }});
                    Clipboard.SetDataObject(commandLine);
                }
                catch (COMException ex)
                {
                    Crashes.TrackError(ex, new Dictionary<string, string>
                                           {{ "Name", Properties.Settings.Default.steamUserName }});
                }
            }
                
            ProcessStartInfo sStartInfo = new ProcessStartInfo(IExecutable.Text, commandLine);
            Process          sProcess   = new Process { StartInfo = sStartInfo };
            sProcess.Start();

            if (!headlessClientLaunched && (IHeadlessClientEnabled.IsChecked ?? false))
            { LaunchHCs(profilePath); }

            headlessClientLaunched = false;
        }

        private string SetCommandLine(string configs, string profilePath, string profileName, string playerMods, string serverMods)
        {
            var commandLine = "-port=" + IPort.Text;
            commandLine = commandLine + " \"-config=" + configs       + "_config.cfg\"";
            commandLine = commandLine + " \"-cfg=" + configs          + "_basic.cfg\"";
            commandLine = commandLine + " \"-profiles=" + profilePath + "\"";
            commandLine = commandLine + " -name="                     + profileName;
            commandLine = commandLine + " \"-mod=" + playerMods       + "\"";
            commandLine = commandLine + " \"-serverMod=" + serverMods + "\"";

            if (IEnableHyperThreading.IsChecked ?? false) 
            { commandLine += " -enableHT"; }

            if (IFilePatching.IsChecked ?? false) 
            { commandLine += " -filePatching"; }

            if (INetlog.IsChecked ?? false) 
            { commandLine += " -netlog"; }

            if (IRankingEnabled.IsChecked ?? false) 
            { commandLine = commandLine + " -ranking=Servers\\" + Functions.SafeName(IDisplayName.Content.ToString()) + "\\" + "ranking.log"; }

            if (IPidEnabled.IsChecked ?? false) 
            { commandLine = commandLine + " -pid=Servers\\" + Functions.SafeName(IDisplayName.Content.ToString()) + "\\" + "pid.log"; }

            if (IAutoInit.IsChecked ?? false) 
            { commandLine += " -autoInit"; }

            if (!string.IsNullOrEmpty(IMaxMem.Text)) 
            { commandLine = commandLine + " \"-maxMem=" + IMaxMem.Text + "\""; }

            if (!string.IsNullOrEmpty(ICpuCount.Text)) 
            { commandLine = commandLine + " \"-cpuCount=" + ICpuCount.Text + "\""; }

            if (!string.IsNullOrEmpty(IExtraParams.Text)) 
            { commandLine = commandLine + " " + IExtraParams.Text; }

            return commandLine;
        }

        private bool ReadyToLaunch(string profile)
        {
            profile = Functions.SafeName(profile);
            if (!ProfileFilesExist(profile))
            {
                MetroWindow.DisplayMessage("The profile does not exist in the game files.");
                return false;
            }

            if (!IExecutable.Text.Contains("arma3server") && !IExecutable.Text.EndsWith(".exe"))
            {
                MetroWindow.DisplayMessage("Please select a valid Arma 3 Sever Executable.");
                return false;
            }

            if (File.Exists(IExecutable.Text)) return true;

            MetroWindow.DisplayMessage("Arma 3 Server Executable does not exist. Please reselect correct file.");
            return false;
        }

        private static bool ProfileFilesExist(string profile)
        {
            string path = Properties.Settings.Default.serverPath;

            if (!Directory.Exists(Path.Combine(path, "Servers", profile)))
            { return false; }

            return File.Exists(Path.Combine(path, "Servers", profile, $"{profile}_config.cfg")) 
                && File.Exists(Path.Combine(path, "Servers", profile, $"{profile}_basic.cfg"));
        }

        private void ShowRenameInterface(bool show)
        {
            if (show)
            {
                IProfileDisplayNameEdit.Text = IDisplayName.Content.ToString();
                IDisplayName.Visibility = Visibility.Collapsed;
                IBattleEye.Visibility = Visibility.Collapsed;
                IProfileNameEdit.Visibility = Visibility.Visible;
                IProfileDisplayNameEdit.Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 255));
                IProfileDisplayNameEdit.SelectAll();
                return;
            }

            IProfileDisplayNameEdit.Text = string.Empty;
            IDisplayName.Visibility = Visibility.Visible;
            IBattleEye.Visibility = Visibility.Visible;
            IProfileNameEdit.Visibility = Visibility.Collapsed;
        }

        private static async Task CopyModsKeysToKeyFolder()
        {
            var mods      = new List<string>();
            var steamMods = Directory.GetDirectories(Path.Combine(Properties.Settings.Default.steamCMDPath, "steamapps", "workshop", "content", "107410"));

            foreach (var line in steamMods)
            {
                try 
                { mods.AddRange(Directory.GetFiles(Path.Combine(line, "keys"))); }
                catch (DirectoryNotFoundException)
                { /*there was no directory*/ }
            }

            foreach (var folder in Properties.Settings.Default.localModFolders)
            {
                try 
                { mods.AddRange(Directory.GetFiles(Path.Combine(folder, "keys"))); }
                catch (DirectoryNotFoundException)
                { /*there was no directory*/ }
            }

            if (!Directory.Exists(Path.Combine(Properties.Settings.Default.serverPath, "keys"))) { Directory.CreateDirectory(Path.Combine(Properties.Settings.Default.serverPath, "keys")); }

            foreach (var link in mods)
            {
                try { File.Copy(link, Path.Combine(Properties.Settings.Default.serverPath, "keys", Path.GetFileName(link)), true); }
                catch (IOException)
                {
                    MainWindow.Instance.IFlyout.IsOpen = true;
                    MainWindow.Instance.IFlyoutMessage.Content = $"Some keys could not be copied : {Path.GetFileName(link)}";
                }
            }
            await Task.Delay(1000);
        }
    }
}
