using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
    }

    
    [Serializable] 
    public class ArmaMod : INotifyPropertyChanged
    {

        private uint   _workshopId;
        private string _name   = string.Empty;
        private string _author = string.Empty;
        private string _path;
        private ulong  _steamLastUpdated;
        private ulong  _localLastUpdated;
        private bool   _privateMod;
        private bool   _isLocal;
        private string _status = "Not Installed";
        private long   _size   = 0;
        private bool   _isLoading;



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
        public ulong SteamLastUpdated
        {
            get => _steamLastUpdated;
            set
            {
                _steamLastUpdated = value;
                RaisePropertyChanged("SteamLasttUpdated");
            }
        }
        public ulong  LocalLastUpdated
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
        public bool   IsLocal
        {
            get => _isLocal;
            set
            {
                _isLocal = value;
                RaisePropertyChanged("IsLocal");
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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }




        internal void CheckModSize()
        {
            IsLoading = true;

            if (!Directory.Exists(Path))
            {
                Size      = 0;
                IsLoading = false;
                return;
            }
            
            var ChildProcess = Task.Factory.StartNew(() => GetDirectorySize(Path));
            Size      = ChildProcess.Result;
            IsLoading = false;
        }


        internal void UpdateMod()
        {
            if (IsLocal)
            {
                Status = ArmaModStatus.Local;
                CheckModSize();
                IsLoading = false;
                return;
            }

            UpdateInfos(false);
            
            IsLoading = true;

            //update code here
            System.Threading.Thread.Sleep(5000);

            CheckModSize();

            IsLoading = false;
        }


        internal void UpdateInfos(bool checkFileSize = true)
        {
            IsLoading = true;

            if (IsLocal)
            {
                Status = ArmaModStatus.Local;
                if(checkFileSize)
                    CheckModSize();
                IsLoading = false;
                return;
            }

            
            int  failNum = 0;
            bool success = false;
            do
            {
                var modInfo = SteamWebApi.GetSingleFileDetails(WorkshopId);
                
                if (modInfo == null)
                {
                    failNum++;
                    continue;
                }

                var modDetails = modInfo.ToObject<SteamApiFileDetails>();

                if (modDetails == null || modDetails.result != 1)
                {
                    failNum++;
                    continue;
                }

                try
                {
                    var creatorDetails = SteamWebApi.GetPlayerSummaries(modDetails.creator.ToString()).ToObject<SteamApiPlayerInfo>();
                    Author = creatorDetails == null ? "Unknown" : creatorDetails.personaname;
                }
                catch
                { Author = "Unknown"; }

                SteamLastUpdated = modDetails.time_updated;
                Name             = modDetails.title;

                if (SteamLastUpdated > LocalLastUpdated && Status != ArmaModStatus.NotComplete)
                    Status = ArmaModStatus.UpdateRequired;
                else if (Status != ArmaModStatus.NotComplete)
                    Status = ArmaModStatus.UpToDate;
                success = true;
            } while (failNum < 3 && !success);
            
            if(checkFileSize)
                CheckModSize();

            IsLoading        = false;
        }

        internal bool IsOnWorkshop()
        {
            try
            {
                var infos = SteamWebApi.GetSingleFileDetails(WorkshopId).ToObject<SteamApiFileDetails>();
                return infos?.result == 1;
            }
            catch
            { return false; }
        }


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

    public class ArmaModStatus
    {
        public static string NotComplete    => "Download Not Complete";
        public static string UpToDate       => "Up To Date";
        public static string UpdateRequired => "Update Required";
        public static string Local          => "Local Mod";
    }
}
