using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FASTER.Models;

namespace FASTER.ViewModel
{
    public class ModsViewModel
    {
        public ModsViewModel()
        { ModsCollection = Properties.Settings.Default.armaMods ?? new ArmaModCollection(); }

        public ArmaModCollection ModsCollection { get; set; }


        internal void DisplayMessage(string msg)
        {
            MainWindow.Instance.IFlyout.IsOpen         = true;
            MainWindow.Instance.IFlyoutMessage.Content = msg;
        }

        public void UnloadData()
        {
            Properties.Settings.Default.armaMods = ModsCollection;
        }

        public void AddSteamMod()
        {
            Random r = new Random();
            ModsCollection.ArmaMods.Add(new ArmaMod
            {
             Name       = "Yeet",
             Path       = "oui",
             Author     = "Honhon",
             WorkshopId = Convert.ToUInt32(r.Next(0, int.MaxValue)),
             LocalMod   = false,
             Size       = Convert.ToInt64(r.Next(0, int.MaxValue))
            });
        }

        public void AddLocalMod()
        {
            Random r = new Random();
            ModsCollection.ArmaMods.Add(new ArmaMod
            {
                Name       = "Yeet",
                Path       = "oui",
                Author     = "Honhon",
                WorkshopId = Convert.ToUInt32(r.Next(0, int.MaxValue)),
                LocalMod   = true,
                Size       = Convert.ToInt64(r.Next(0, int.MaxValue))
            });
        }
    }
}
