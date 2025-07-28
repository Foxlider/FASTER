using System;
using System.ComponentModel;

namespace FASTER.Models
{
    [Serializable]
    public class AdvancedOptions : INotifyPropertyChanged
    {
        private bool   logObjectNotFound       = true;       // logging enabled
        private bool   skipDescriptionParsing  = false;      // Parse description.ext
        private bool   ignoreMissionLoadErrors = false;      // Do not ignore errors
        private int    queueSizeLogG           = 0;          // If a specific players message queue is larger than <Value> and '#monitor' is running, dump the messages to a logfile for analysis
        private string advancedOptionsContent;

        public bool LogObjectNotFound
        {
            get => logObjectNotFound;
            set
            {
                logObjectNotFound = value;
                RaisePropertyChanged(nameof(LogObjectNotFound));
            }
        }

        public bool SkipDescriptionParsing
        {
            get => skipDescriptionParsing;
            set
            {
                skipDescriptionParsing = value;
                RaisePropertyChanged(nameof(SkipDescriptionParsing));
            }
        }

        public bool IgnoreMissionLoadErrors
        {
            get => ignoreMissionLoadErrors;
            set
            {
                ignoreMissionLoadErrors = value;
                RaisePropertyChanged(nameof(IgnoreMissionLoadErrors));
            }
        }

        public int QueueSizeLogG
        {
            get => queueSizeLogG;
            set
            {
                queueSizeLogG = value;
                RaisePropertyChanged(nameof(QueueSizeLogG));
            }
        }

        public string AdvancedOptionsContent
        {
            get => advancedOptionsContent;
            set
            {
                advancedOptionsContent = value;
                RaisePropertyChanged(nameof(AdvancedOptionsContent));
            }
        }

        public AdvancedOptions()
        {
            if(string.IsNullOrWhiteSpace(AdvancedOptionsContent))
            { 
                AdvancedOptionsContent = ProcessFile();
            }
        }

        public string ProcessFile()
        {
            string output = "//\r\n"
                          + "// AdvancedOptions\r\n"
                          + "//\r\n"
                          + "// comments are written with \"//\" in front of them.\r\n"
                          + "\r\n"
                          + "\r\n"
                          + $"queueSizeLogG = {QueueSizeLogG};\t\t\t// If a specific players message queue is larger than Value number and #monitor is running, dump his messages to a logfile for analysis\r\n"
                          + $"LogObjectNotFound = {LogObjectNotFound};\t\t// When false to skip logging 'Server: Object not found messages'.\r\n"
                          + $"SkipDescriptionParsing = {SkipDescriptionParsing};\t\t// When true to skip parsing of description.ext/mission.sqm. Will show pbo filename instead of configured missionName. OverviewText and such won't work, but loading the mission list is a lot faster when there are many missions\r\n"
                          + $"ignoreMissionLoadErrors = {IgnoreMissionLoadErrors};\t\t// When set to true, the mission will load no matter the amount of loading errors. If set to false, the server will abort mission's loading and return to mission selection.\r\n"
                          + "\r\n"
                          + "\r\n";
            return output;
		}
        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (property != nameof(AdvancedOptionsContent))
            {
                AdvancedOptionsContent = ProcessFile();
            }
        }
    }
}