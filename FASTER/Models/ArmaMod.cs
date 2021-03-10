using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FASTER.Models
{
    [Serializable]
    public class ArmaModCollection : INotifyPropertyChanged
    {
        [XmlElement(Order = 1)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CollectionName { get; set; } = "Main";

        private ObservableCollection<ArmaMod> _mods = new ObservableCollection<ArmaMod>();

        [XmlElement(Order = 2, ElementName = "ArmaMod")]
        public ObservableCollection<ArmaMod> ArmaMods
        {
            get => _mods;
            set
            {
                _mods = value;
                RaisePropertyChanged("ArmaMods");
            }
        }

        private static ArmaModCollection ReloadMods()
        {
            ArmaModCollection currentMods = new ArmaModCollection();

            if (Properties.Settings.Default.steamMods != null)
                currentMods = Properties.Settings.Default.armaMods;

            return currentMods;
        }

        public static void AddSteamMod(ArmaMod newMod)
        {
            var duplicate   = false;
            var currentMods = ReloadMods();

            if (currentMods.ArmaMods.Count > 0)
            { duplicate = currentMods.ArmaMods.FirstOrDefault(mod => mod.WorkshopId == newMod.WorkshopId) != null; }

            if (!duplicate)
            {
                currentMods.ArmaMods.Add(newMod);
                Properties.Settings.Default.armaMods = currentMods;
            }
            else
            { MainWindow.Instance.DisplayMessage("Mod Already Exists"); }

            Properties.Settings.Default.Save();
        }

        public static void DeleteSteamMod(int workshopId)
        {
            var currentProfiles = ReloadMods();
            var item            = currentProfiles.ArmaMods.FirstOrDefault(x => x.WorkshopId == workshopId);
            
            if (item != null)
            { currentProfiles.ArmaMods.Remove(item); }
            
            Properties.Settings.Default.Save();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    
    [Serializable] public class ArmaMod : INotifyPropertyChanged
    {

        private uint   _workshopId;
        private string _name   = string.Empty;
        private string _author = string.Empty;
        private string _path;
        private int    _steamLastUpdated;
        private int    _localLastUpdated;
        private bool   _privateMod;
        private bool   _localMod;
        private string _status = "Not Installed";
        private long   _size   = 0;



        public uint   WorkshopId       
        { 
            get => _workshopId;
            set
            {
                _workshopId = value;
                RaisePropertyChanged("WorkshopID");
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
        public int    SteamLastUpdated
        {
            get => _steamLastUpdated;
            set
            {
                _steamLastUpdated = value;
                RaisePropertyChanged("SteamLasttUpdated");
            }
        }
        public int    LocalLastUpdated
        {
            get => _localLastUpdated;
            set
            {
                _localLastUpdated = value;
                RaisePropertyChanged("LocalLastUpdated");
            }
        }
        public bool   PrivateMod
        {
            get => _privateMod;
            set
            {
                _privateMod = value;
                RaisePropertyChanged("PrivateMod");
            }
        }
        public bool   LocalMod
        {
            get => _localMod;
            set
            {
                _localMod = value;
                RaisePropertyChanged("LocalMod");
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }
        public long   Size
        {
            get => _size;
            set
            {
                _size = value;
                RaisePropertyChanged("Size");
            }
        }

        public ArmaMod()
        {}

        internal void CheckModSize()
        {
            if (!Directory.Exists(Path)) Size = 0;
            
            var ChildProcess = Task.Factory.StartNew(() => GetDirectorySize(Path));
            Size = ChildProcess.Result;
        }

        internal void UpdateInfos()
        {

        }

        internal void UpdateMod()
        {}


        private static long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);
            return a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
