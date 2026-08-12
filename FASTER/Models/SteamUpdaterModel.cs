using FASTER.Properties;
using System;
using System.ComponentModel;
using System.IO;

namespace FASTER.Models
{
    public class SteamUpdaterModel : INotifyPropertyChanged
    {
        private string _output;
        private bool   _isUpdating;
        private double _progress;


        public string InstallDirectory
        {
            get => Settings.Default.serverPath;
            set
            {
                Settings.Default.serverPath = value;
                Settings.Default.Save();
                RaisePropertyChanged(nameof(InstallDirectory));
            }
        }

        public string Username
        {
            get => Settings.Default.steamUserName;
            set
            {
                Settings.Default.steamUserName = value;
                Settings.Default.Save();
                RaisePropertyChanged(nameof(Username));
            }
        }

        public string SteamCmdDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.steamCMDPath))
                {
                    Settings.Default.steamCMDPath = GetDefaultSteamCmdDirectory();
                    Settings.Default.Save();
                }

                return Settings.Default.steamCMDPath;
            }
            set
            {
                Settings.Default.steamCMDPath = string.IsNullOrWhiteSpace(value)
                    ? GetDefaultSteamCmdDirectory()
                    : value.Trim();
                Settings.Default.Save();
                RaisePropertyChanged(nameof(SteamCmdDirectory));
            }
        }

        private static string GetDefaultSteamCmdDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FASTER",
                "SteamCMD");
        }

        public string ModStagingDirectory
        {
            get => Settings.Default.modStagingDirectory;
            set
            {
                Settings.Default.modStagingDirectory = value;
                Settings.Default.Save();
                RaisePropertyChanged(nameof(ModStagingDirectory));
            }
        }

        public string Output
        {
            get => _output;
            set
            {
                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                RaisePropertyChanged(nameof(IsUpdating));
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                RaisePropertyChanged(nameof(Progress));
            }
        }

        public string ServerBranch
        {
            get
            {
                string? branch = Settings.Default.serverBranch?.Trim().ToLowerInvariant();
                if (branch is not ("public" or "contact" or "creatordlc" or "profiling"))
                {
                    // Migrate the old independent depot toggles to SteamCMD's
                    // mutually-exclusive whole-app branches. Profiling takes
                    // precedence, followed by Contact and Creator DLC.
                    branch = Settings.Default.usingPerfBinaries
                        ? "profiling"
                        : Settings.Default.usingContactDlc
                            ? "contact"
                            : Settings.Default.usingGMDlc || Settings.Default.usingPFDlc ||
                              Settings.Default.usingCSLADlc || Settings.Default.usingWSDlc ||
                              Settings.Default.usingSPEDlc || Settings.Default.usingRFDlc ||
                              Settings.Default.usingEFDlc
                                ? "creatordlc"
                                : "public";
                    Settings.Default.serverBranch = branch;
                    Settings.Default.Save();
                }

                return branch;
            }
            set
            {
                string? branch = value?.Trim().ToLowerInvariant();
                Settings.Default.serverBranch = branch is "public" or "contact" or "creatordlc" or "profiling"
                    ? branch
                    : "public";
                Settings.Default.Save();
                RaisePropertyChanged(nameof(ServerBranch));
            }
        }

        public string ApiKey
        {
            get => !string.IsNullOrEmpty(Settings.Default.SteamAPIKey)
                       ? Settings.Default.SteamAPIKey
                       : StaticData.SteamApiKey;
            set
            {
                Settings.Default.SteamAPIKey = value;
                Settings.Default.Save();
                RaisePropertyChanged(nameof(ApiKey));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
