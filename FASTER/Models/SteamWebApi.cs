using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FASTER.Models
{
    public static class SteamWebApi
    {
        private const string V2 = "&steamids=";
        private const string V3 = "&publishedfileids[0]=";


        // Get mod info for single mod
        public static JObject GetSingleFileDetails(uint modId)
        {
            try
            {
                var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + GetApiKey() + V3 + modId);
                return (JObject)response?.SelectToken("response.publishedfiledetails[0]");
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
                return (JObject)response?.SelectToken("response.players.player[0]");
            }
            catch
            { return null; }
        }


                private static readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };

        // Calls to Steam API Endpoint and returns the result as JSON Object
        private static JObject ApiCall(string uri)
        {
            // Get the response.
            HttpResponseMessage response = null;

            try
            { response = client.GetAsync(uri).GetAwaiter().GetResult(); }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or SocketException)
            {
                try
                {
                    MainWindow.Instance.Dispatcher?.InvokeAsync(() =>
                    {
                        MainWindow.Instance.DisplayMessage("Cannot reach Steam API \n\nCheck https://steamstat.us/ \n\nPlease check the Windows Event Logs for more informations");
                    });

                    // Create an EventLog instance and assign its source.
                    using EventLog eventLog = new EventLog("Application")
                    { Source = "FASTER" };
                    eventLog.WriteEntry($"Could not reach Steam API : \n[{e.GetType().Name}] {e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error);
                }
                catch (Exception) //In case it was called before Initialized in SteamMods_Initialized() and could not connect to SteamAPI
                {
                    if (Environment.UserInteractive)
                        MessageBox.Show("Cannot reach Steam API \n\nCheck https://steamstat.us/", "Steam API Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        Console.WriteLine("Cannot reach Steam API \n\nCheck https://steamstat.us/");
                }
            }
            // Display the status.
            Console.WriteLine((response)?.StatusCode);

            if (response == null)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Steam API returned HTTP {(int)response.StatusCode} {response.StatusCode}. Please check your Steam API Key in Settings.");

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Return the response
        }

        private static string GetApiKey()
        {
            return !string.IsNullOrEmpty(Properties.Settings.Default.SteamAPIKey)
                ? Properties.Settings.Default.SteamAPIKey
                : StaticData.SteamApiKey;
        }
    }

    internal class SteamApiFileDetails
    {
        public uint result { get; set; }
        public ulong publishedfileid { get; set; }
        public ulong creator { get; set; }
        public uint creator_appid { get; set; }
        public uint consumer_appid { get; set; }
        public string filename { get; set; }
        public ulong file_size { get; set; }
        public string title { get; set; }
        public string file_description { get; set; }
        public ulong time_created { get; set; }
        public ulong time_updated { get; set; }
    }

    internal class SteamApiPlayerInfo
    {
        public ulong steamid { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
    }
}
