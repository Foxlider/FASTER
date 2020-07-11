
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;

namespace FASTER.Models
{
    class ServerProfileNew : INotifyPropertyChanged
    {
        //PRIVATE VARS DECLARATION
        private string _id;
        private string _name;

        private List<ProfileMod> _profileMods = new List<ProfileMod>();
        private ServerCfg _serverCfg;


        //PUBLIC VAR DECLARATIONS
        public string Id => _id;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        //Current logit to count the checked mods
        public int ServerModsChecked { get => ProfileMods.Where(m => m.ServerSideChecked == true).Count(); }
        public int ClientModsChecked { get => ProfileMods.Where(m => m.ClientSideChecked == true).Count(); }
        public int HeadlessModsChecked { get => ProfileMods.Where(m => m.HeadlessChecked == true).Count(); }

        public List<ProfileMod> ProfileMods
        {
            get => _profileMods;
            set
            {
                //Removing previous triggers
                _profileMods.ForEach(m => m.PropertyChanged -= Item_PropertyChanged);

                _profileMods = value;

                //Adding the trigger to count checked mods
                _profileMods.ForEach(m => m.PropertyChanged += Item_PropertyChanged);

                RaisePropertyChanged("ProfileMods");
            }
        }

        public ServerCfg ServerCfg { 
            get => _serverCfg;
            set
            {
                _serverCfg = value;
                RaisePropertyChanged("ServerCfg");
            }
        }

        //CTORS
        public ServerProfileNew(string name)
        {
            _id = $"_{Guid.NewGuid():N}";
            Name = name;
            ServerCfg = new ServerCfg(){ Hostname = name};
        }

        public ServerProfileNew(string name, Guid guid)
        {
            _id = $"_{guid:N}";
            Name = name;
            ServerCfg = new ServerCfg() { Hostname = name };
        }

        //This is used to trigger PropertyChanged to count each checked mod
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("ServerModsChecked");
            RaisePropertyChanged("ClientModsChecked");
            RaisePropertyChanged("HeadlessModsChecked");
        }


        //INOTIFYPROPERTYCHANGED
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
    }

    internal class ProfileMod : INotifyPropertyChanged
    {
        private bool serverSideChecked;
        private bool clientSideChecked;
        private bool headlessChecked;
        private int _id;
        private string name;

        public bool ServerSideChecked
        {
            get => serverSideChecked;
            set
            {
                serverSideChecked = value;
                RaisePropertyChanged("ServerSideChecked");
                RaisePropertyChanged("HeadlessModsChecked");
            }
        }
        public bool ClientSideChecked
        {
            get => clientSideChecked;
            set
            {
                clientSideChecked = value;
                RaisePropertyChanged("ClientSideChecked");
                RaisePropertyChanged("ClientModsChecked");
            }
        }
        public bool HeadlessChecked
        {
            get => headlessChecked;
            set
            {
                headlessChecked = value;
                RaisePropertyChanged("HeadlessChecked");
                RaisePropertyChanged("HeadlessModsChecked");
            }
        }

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged("Name");
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
