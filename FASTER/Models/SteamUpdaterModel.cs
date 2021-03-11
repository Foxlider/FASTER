﻿using System.ComponentModel;
using FASTER.Properties;

namespace FASTER.Models
{
    public class SteamUpdaterModel : INotifyPropertyChanged
    {
        public SteamUpdaterModel()
        { }

        private string _output;
        public string InstallDirectory
        {
            get => Settings.Default.steamCMDPath;
            set
            {
                Settings.Default.steamCMDPath = value;
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
            get;
            set;
            //get => Settings.Default.modStagingDirectory;
            //set
            //{
            //    Settings.Default.modStagingDirectory = value;
            //    Settings.Default.Save();
            //    RaisePropertyChanged(nameof(ModStagingDirectory));
            //}
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
