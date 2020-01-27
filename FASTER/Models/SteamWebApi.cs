using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace FASTER.Models
{
    internal static class SteamWebApi
    {
        private const string SteamApiKey = "89B74BCDEF2493AB2774D8A02D9CED0D";
        private const string V = "&publishedfileids[";
        private const string V1 = "]=";
        private const string V2 = "&steamids=";
        private const string V3 = "&publishedfileids[0]=";

        // Gets mod info for multiple mods
        public static List<JObject> GetFileDetails(List<int> modIds)
        {
            try {
                string mods = string.Empty;
                foreach (var modId in modIds)
                    mods = $"{mods}{V}{modIds.IndexOf(modId)}{V1}{modId}";

                var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + GetApiKey() + mods);

                return response.SelectTokens("response.publishedfiledetails[*]").Cast<JObject>().ToList();
            }
            catch
            { return null; }
        }

        // Get mod info for single mod
        public static JObject GetSingleFileDetails(int modId)
        {
            try
            {
                var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + GetApiKey() + V3 + modId);
                return (JObject) response?.SelectToken("response.publishedfiledetails[0]");
            }
            catch
            { return null; }
        }


        // Gets user info
        public static JObject GetPlayerSummaries(string playerId)
        {
            try
            {
                var response = ApiCall("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v1?key=" + GetApiKey() + V2 + playerId);
                return (JObject) response.SelectToken("response.players.player[0]");
            }
            catch
            { return null; }
        }


        // Calls to Steam API Endpoint and returns the result as JSON Object
        private static JObject ApiCall(string uri)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(uri);
            // Get the response.
            WebResponse response = null;
            try
            { response = request.GetResponse(); }
            catch (WebException e)
            {
                try
                {
                    MainWindow.Instance.Dispatcher?.InvokeAsync(() =>
                    {
                        MainWindow.Instance.IMessageDialogText.Text = "Cannot reach Steam API \n\nCheck https://steamstat.us/ \n\nPlease check the Windows Event Logs for more informations";
                        MainWindow.Instance.IMessageDialog.IsOpen = true;
                    });

                    // Create an EventLog instance and assign its source.
                    using EventLog eventLog = new EventLog("Application")
                    { Source = "FASTER" };
                    eventLog.WriteEntry($"Could not reach Steam API : \n[WebException] {e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error);
                }
                catch (Exception) //In case it was called before Initialized in SteamMods_Initialized() and could not connect to SteamAPI
                { MessageBox.Show("Cannot reach Steam API \n\nCheck https://steamstat.us/", "Steam API Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response)?.StatusDescription);
            // Get the stream containing content returned by the server.
            if (response != null) {
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream ?? throw new InvalidOperationException());
                // Read the content.
                var responseFromServer = reader.ReadToEnd();
                // Clean up the streams and the response.
                reader.Close();
                response.Close();
                // Return the response
                return JObject.Parse(responseFromServer);
            }
            return null;
        }

        private static string GetApiKey()
        {
            return !string.IsNullOrEmpty(Properties.Settings.Default.SteamAPIKey) 
                ? Properties.Settings.Default.SteamAPIKey 
                : SteamApiKey;
        }
    }
}
