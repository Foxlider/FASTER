using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;

namespace FASTER.Models
{
    class ServerProfileNew : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        
        private ServerCfg _serverCfg;

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

        public ServerCfg ServerCfg { 
            get => _serverCfg;
            set
            {
                _serverCfg = value;
                RaisePropertyChanged("ServerCfg");
            }
        }

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




        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)); }
    }
}
