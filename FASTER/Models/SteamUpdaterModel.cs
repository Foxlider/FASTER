using System.ComponentModel;
using FASTER.Properties;
using SteamKit2.GC.TF2.Internal;

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

        public string Password
        {
            get => Settings.Default.steamPassword;
            set
            {
                Settings.Default.steamPassword = value;
                Settings.Default.Save();
                RaisePropertyChanged(nameof(Password));
            }
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
