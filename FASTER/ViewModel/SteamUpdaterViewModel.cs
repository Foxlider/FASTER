using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using FASTER.Models;

namespace FASTER.ViewModel
{
    public class SteamUpdaterViewModel
    {
        public SteamUpdaterViewModel()
        {
            Parameters = new SteamUpdaterModel();
        }
        public SteamUpdaterModel Parameters { get; set; }
        public void PasswordChanged(string password)
        { Parameters.Password = password; }
    }
}
