using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.AppCenter.Analytics;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FASTER.Models
{
    [Serializable]
    public class SteamModCollection
    {
        [XmlElement(Order = 1)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CollectionName { get; set; } = "Main";

        [XmlElement(Order = 2, ElementName = "SteamMod")]
        public List<SteamMod> SteamMods = new List<SteamMod>();

        private static SteamModCollection GetSteamMods()
        {
            SteamModCollection currentMods = new SteamModCollection();

            if (Properties.Settings.Default.steamMods != null)
                currentMods = Properties.Settings.Default.steamMods;

            return currentMods;
        }

        public static void AddSteamMod(SteamMod newMod)
        {
            var duplicate       = false;
            var currentMods = GetSteamMods();

            if (currentMods.SteamMods.Count > 0)
            {
                foreach (var mod in currentMods.SteamMods)
                {
                    if (mod.WorkshopId == newMod.WorkshopId)
                        duplicate = true;
                }
            }

            if (!duplicate)
            {
                currentMods.SteamMods.Add(newMod);
                Properties.Settings.Default.steamMods = currentMods;
            }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "Mod Already Exists";
            }
            Properties.Settings.Default.Save();
        }

        public static void DeleteSteamMod(int workshopId)
        {
            var currentProfiles = GetSteamMods();

            currentProfiles.SteamMods.RemoveAll(x => x.WorkshopId == workshopId);

            Properties.Settings.Default.Save();
        }
    }

    [Serializable]
    public class SteamMod
    {
        private SteamMod()
        {
            
        }
        public SteamMod(int workshopId, string name, string author, int steamLastUpdated, bool privateMod = false)
        {
            WorkshopId = workshopId;
            Name = name;
            Author = author;
            SteamLastUpdated = steamLastUpdated;
            PrivateMod = privateMod;
        }

        public int WorkshopId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int SteamLastUpdated { get; set; }
        public int LocalLastUpdated { get; set; }
        public bool PrivateMod { get; set; }
        public string Status { get; set; } = "Not Installed";

        public static void DeleteSteamMod(int workshopId)
        {
            var currentMods = GetSteamMods();

            currentMods.RemoveAll(x => x.WorkshopId == workshopId);

            Properties.Settings.Default.Save();
        }

        public static List<SteamMod> GetSteamMods()
        {
            List<SteamMod> currentSteamMods = new List<SteamMod>();

            if (Properties.Settings.Default.steamMods != null)
            {
                Properties.Settings.Default.Reload();
                currentSteamMods = Properties.Settings.Default.steamMods?.SteamMods;
            }

            return currentSteamMods;
        }

        private static int SteamIdFromUrl(string modUrl)
        {
            var modId = modUrl.Substring(modUrl.IndexOf("?id=", StringComparison.Ordinal));
            if (modId.Contains("&"))
                // ReSharper disable once StringIndexOfIsCultureSpecific.1
                modId = modId.Substring(0, modId.IndexOf("&"));
            modId = int.Parse(Regex.Replace(modId, @"[^\d]", "")).ToString();

            return int.Parse(modId);
        }

        public static void AddSteamMod(string modUrl, bool multiple = false)
        {
            Analytics.TrackEvent("SteamMod - Adding Mod", new Dictionary<string, string> {
                { "Name", Properties.Settings.Default.steamUserName },
                { "ModUrl", modUrl },
            });
            int modId = 0;
            var invalid = false;

            if (int.TryParse(modUrl, out _))
            { modId = int.Parse(modUrl);}
            else if (modUrl.Contains("://steamcommunity.com/") && modUrl.Contains("/filedetails/?id="))
            { modId = SteamIdFromUrl(modUrl); }
            else { invalid = true; }

            if (!invalid) { AddMod(multiple, modId); }
            else
            {
                MainWindow.Instance.IMessageDialog.IsOpen = true;
                MainWindow.Instance.IMessageDialogText.Text = "Please use format: https://steamcommunity.com/sharedfiles/filedetails/?id=*********";
            }
        }

        private static void AddMod(bool multiple, int modId)
        {
            var duplicate   = false;
            var currentMods = GetSteamMods();

            if (currentMods.Count > 0)
            {
                foreach (var steamMod in currentMods.Where(steamMod => steamMod.WorkshopId == modId)) 
                { duplicate = true; }
            }

            if (!duplicate)
            {
                try
                {
                    var modInfo = GetModInfo(modId);

                    if (modInfo != null)
                    {
                        var modName         = modInfo.Item1;
                        var steamUpdateTime = modInfo.Item3;
                        var author          = modInfo.Item2;

                        currentMods.Add(new SteamMod(modId, modName, author, steamUpdateTime));

                        var modCollection = new SteamModCollection { CollectionName = "Steam", SteamMods = currentMods };

                        Properties.Settings.Default.steamMods = modCollection;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        MainWindow.Instance.IMessageDialog.IsOpen   = true;
                        MainWindow.Instance.IMessageDialogText.Text = "This is a workshop Item for a different game.";
                    }
                }
                catch (Exception)
                {
                    /*ignored*/
                }
            }
            else if (!multiple)
            {
                MainWindow.Instance.IMessageDialog.IsOpen   = true;
                MainWindow.Instance.IMessageDialogText.Text = "Mod already imported.";
            }
        }

        public static Tuple<string, string, int> GetModInfo(int modId)
        {
            var modInfo = SteamWebApi.GetSingleFileDetails(modId);
            string author = null;
            int steamUpdateTime = 0;
            try
            {
                if (modInfo != null) 
                { author = SteamWebApi.GetPlayerSummaries(modInfo.SelectToken("creator").ToString())?.SelectToken("personaname")?.ToString(); }
            }
            catch { author = "Unknown"; }

            try
            {
                if (modInfo?.SelectToken("time_updated") != null) 
                { steamUpdateTime = int.Parse(modInfo.SelectToken("time_updated").ToString()); }
            }
            catch { steamUpdateTime = 0; }

            var modName = modInfo?.SelectToken("title").ToString();
            
            return modInfo?.SelectToken("creator_appid").ToString() == "107410" ? new Tuple<string, string, int>(modName, author, steamUpdateTime) : null;
        }

        public static void UpdateInfoFromSteam()
        {
            if (Properties.Settings.Default.steamMods.SteamMods.Count <= 0) return;
            var currentMods = Properties.Settings.Default.steamMods.SteamMods;
            var failnum     = 0;
            foreach (var steamMod in currentMods.ToList())
            {
                if (steamMod.PrivateMod) continue;
                var modInfo = GetModInfo(steamMod.WorkshopId);

                if (modInfo != null)
                {
                    var updateMod = currentMods.Find(c => c.WorkshopId == steamMod.WorkshopId);

                    updateMod.Name             = modInfo.Item1;
                    updateMod.Author           = modInfo.Item2;
                    updateMod.SteamLastUpdated = modInfo.Item3;

                    if (updateMod.SteamLastUpdated > updateMod.LocalLastUpdated && updateMod.Status != "Download Not Complete")
                    { updateMod.Status = "Update Required";}
                    else if (updateMod.Status != "Download Not Complete") 
                    { updateMod.Status = "Up to Date"; }
                }
                else
                    failnum++;
                if (failnum < 3) continue;
                MainWindow.Instance.Dispatcher?.InvokeAsync(() =>
                {
                    MainWindow.Instance.IMessageDialogText.Text = "Could not reach stream after 3 attempts.\n\nPlease check the event logs.";
                    MainWindow.Instance.IMessageDialog.IsOpen   = true;
                });
                break;
            }

            Properties.Settings.Default.steamMods.SteamMods = currentMods;
            Properties.Settings.Default.Save();
        }
    }
}
