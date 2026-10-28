using System.ComponentModel;

namespace FASTER.Models
{
    public class SteamUpdaterModel : INotifyPropertyChanged
    {
        private string _output = string.Empty;
        private bool _isUpdating;
        private double _progress;


        public string InstallDirectory
        {
            get => AppSettings.Current.ServerPath;
            set
            {
                AppSettings.Current.ServerPath = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(InstallDirectory));
            }
        }

        public string Username
        {
            get => AppSettings.Current.SteamUserName;
            set
            {
                AppSettings.Current.SteamUserName = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => AppSettings.Current.SteamPassword;
            set
            {
                AppSettings.Current.SteamPassword = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(Password));
            }
        }

        public string ModStagingDirectory
        {
            get => AppSettings.Current.ModStagingDirectory;
            set
            {
                AppSettings.Current.ModStagingDirectory = value;
                AppSettings.Current.Save();
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

        public bool UsingPerfBinaries
        {
            get => AppSettings.Current.UsingPerfBinaries;
            set
            {
                AppSettings.Current.UsingPerfBinaries = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingPerfBinaries));
            }
        }

        public bool UsingContactDlc
        {
            get => AppSettings.Current.UsingContactDlc;
            set
            {
                AppSettings.Current.UsingContactDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingContactDlc));
            }
        }

        public bool UsingGMDlc
        {
            get => AppSettings.Current.UsingGMDlc;
            set
            {
                AppSettings.Current.UsingGMDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingGMDlc));
            }
        }

        public bool UsingPFDlc
        {
            get => AppSettings.Current.UsingPFDlc;
            set
            {
                AppSettings.Current.UsingPFDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingPFDlc));
            }
        }

        public bool UsingCSLADlc
        {
            get => AppSettings.Current.UsingCSLADlc;
            set
            {
                AppSettings.Current.UsingCSLADlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingCSLADlc));
            }
        }

        public bool UsingWSDlc
        {
            get => AppSettings.Current.UsingWSDlc;
            set
            {
                AppSettings.Current.UsingWSDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingWSDlc));
            }
        }

        public bool UsingSPEDlc
        {
            get => AppSettings.Current.UsingSPEDlc;
            set
            {
                AppSettings.Current.UsingSPEDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingSPEDlc));
            }
        }

        public bool UsingRFDlc
        {
            get => AppSettings.Current.UsingRFDlc;
            set
            {
                AppSettings.Current.UsingRFDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingRFDlc));
            }
        }

        public bool UsingEFDlc
        {
            get => AppSettings.Current.UsingEFDlc;
            set
            {
                AppSettings.Current.UsingEFDlc = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(UsingEFDlc));
            }
        }

        public string ApiKey
        {
            get => !string.IsNullOrEmpty(AppSettings.Current.SteamAPIKey)
                       ? AppSettings.Current.SteamAPIKey
                       : StaticData.SteamApiKey;
            set
            {
                AppSettings.Current.SteamAPIKey = value;
                AppSettings.Current.Save();
                RaisePropertyChanged(nameof(ApiKey));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
