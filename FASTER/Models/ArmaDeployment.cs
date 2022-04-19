using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace FASTER.Models
{
    [Serializable]
    public class ArmaDeployment : INotifyPropertyChanged
    {
        private ObservableCollection<DeploymentMod> _mods = new ObservableCollection<DeploymentMod>();
        private string _deployModsFilter = "";
        private bool _deployModsFilterIsCaseSensitive = false;
        private bool _deployModsFilterIsWholeWord = false;
        private bool _deployModsFilterIsRegex = false;
        private bool _deployModsFilterIsInvalid = false;
        private string _name = "Main";
        
        [XmlElement(Order = 1)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string InstallPath 
        { 
            get => _name; 
            set 
            {
                _name = value;
                RaisePropertyChanged(nameof(InstallPath));
            } 
        }

        

        [XmlElement(Order = 2, ElementName = "DeployMods")]
        public ObservableCollection<DeploymentMod> DeployMods
        {
            get => _mods;
            set
            {
                _mods = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged("DeployMods");
                RaisePropertyChanged("FilteredDeployMods");
            }
        }

        public ObservableCollection<DeploymentMod> FilteredDeployMods
        {
            get
            {
                if (string.IsNullOrEmpty(DeployModsFilter))
                {
                    if (DeployModsFilterIsInvalid)
                    {
                        DeployModsFilterIsInvalid = false;
                    }
                    return new ObservableCollection<DeploymentMod>(_mods);
                }

                var pattern = DeployModsFilter;
                if (!DeployModsFilterIsRegex)
                {
                    pattern = Regex.Replace(pattern, @"[\\\{\}\*\+\?\|\^\$\.\[\]\(\)]", "\\$&");
                }

                if (DeployModsFilterIsWholeWord)
                {
                    if (!Regex.IsMatch(pattern[0].ToString(), @"\B"))
                    {
                        pattern = $"\\b{pattern}";
                    }
                    if (!Regex.IsMatch(pattern[pattern.Length - 1].ToString(), @"\B"))
                    {
                        pattern = $"{pattern}\\b";
                    }
                }

                var options = DeployModsFilterIsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                try
                {
                    var filteredDeployMods = new ObservableCollection<DeploymentMod>(_mods.Where(m => Regex.IsMatch(m.Name, pattern, options)));
                    if (DeployModsFilterIsInvalid)
                    {
                        DeployModsFilterIsInvalid = false;
                    }
                    return filteredDeployMods;
                }
                catch (ArgumentException)
                {
                    if (!DeployModsFilterIsInvalid)
                    {
                        DeployModsFilterIsInvalid = true;
                    }
                    return new ObservableCollection<DeploymentMod>();
                }
            }
        }

        public string DeployModsFilter
        {
            get => _deployModsFilter;
            set
            {
                _deployModsFilter = value;
                RaisePropertyChanged("DeployModsFilter");
                RaisePropertyChanged("FilteredDeployMods");
            }
        }

        public bool DeployModsFilterIsCaseSensitive
        {
            get => _deployModsFilterIsCaseSensitive;
            set
            {
                _deployModsFilterIsCaseSensitive = value;
                RaisePropertyChanged("DeployModsFilterIsCaseSensitive");
                RaisePropertyChanged("FilteredDeployMods");
            }
        }

        public bool DeployModsFilterIsWholeWord
        {
            get => _deployModsFilterIsWholeWord;
            set
            {
                _deployModsFilterIsWholeWord = value;
                RaisePropertyChanged("DeployModsFilterIsWholeWord");
                RaisePropertyChanged("FilteredDeployMods");
            }
        }

        public bool DeployModsFilterIsRegex
        {
            get => _deployModsFilterIsRegex;
            set
            {
                _deployModsFilterIsRegex = value;
                RaisePropertyChanged("DeployModsFilterIsRegex");
                RaisePropertyChanged("FilteredDeployMods");
            }
        }

        public bool DeployModsFilterIsInvalid
        {
            get => _deployModsFilterIsInvalid;
            set
            {
                _deployModsFilterIsInvalid = value;
                RaisePropertyChanged("DeployModsFilterIsInvalid");
            }
        }


        public ArmaDeployment()
        {
            InstallPath = Properties.Settings.Default.serverPath;
            DeployMods = new ObservableCollection<DeploymentMod>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    [Serializable]
    public class DeploymentMod : INotifyPropertyChanged
    {
        private bool marked;
        private string _path;
        private string _name;
        private string _author;
        private uint _workshopId;
        private string _status;
        private long _size;
        private bool _isLocal;


        public DeploymentMod()
        {
            Marked = false;
            Author = "Default Author";
            Name = "Default Name";
            WorkshopId = 0;
            Path = "Default Path";
            Status = "Default Status";
            IsLocal = false;
            Size = 0;
        }

        public DeploymentMod(ArmaMod m)
        {
            Marked = false;
            Author = m.Author;
            Name = m.Name;
            WorkshopId = m.WorkshopId;
            Path = m.Path;
            Status = m.Status;
            IsLocal = m.IsLocal;
            Size = m.Size;
        }

        public void UpdateInfos()
        {
            var originalMod = Properties.Settings.Default.armaMods.ArmaMods.FirstOrDefault(m => m.WorkshopId == WorkshopId);
            if (originalMod == null)
                return;

            Author = originalMod.Author;
            Name = originalMod.Name;
            Path = originalMod.Path;
            Status = originalMod.Status;
            Size = originalMod.Size;
        }

        public uint WorkshopId
        {
            get => _workshopId;
            set
            {
                _workshopId = value;
                RaisePropertyChanged("WorkshopID");
            }
        }

        public bool IsLocal
        {
            get => _isLocal;
            set
            {
                _isLocal = value;
                RaisePropertyChanged(nameof(IsLocal));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                RaisePropertyChanged("Author");
            }
        }
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                RaisePropertyChanged(nameof(Size));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }
        public bool Marked
        {
            get => marked;
            set
            {
                marked = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(Marked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
