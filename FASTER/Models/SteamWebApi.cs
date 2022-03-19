using BytexDigital.Steam.Core;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;

namespace FASTER.Models
{
    public static class SteamWebApi
    {
        private const string V = "&publishedfileids[";
        private const string V1 = "]=";
        private const string V2 = "&steamids=";
        private const string V3 = "&publishedfileids[0]=";

        // Gets mod info for multiple mods
        public static List<JObject> GetFileDetails(List<int> modIds)
        {
            try
            {
                string mods = modIds.Aggregate(string.Empty, (current, modId) => $"{current}{V}{modIds.IndexOf(modId)}{V1}{modId}");

                var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + GetApiKey() + mods);

                return response.SelectTokens("response.publishedfiledetails[*]").Cast<JObject>().ToList();
            }
            catch
            { return null; }
        }

        // Get mod info for single mod
        public static JObject GetSingleFileDetails(uint modId)
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
                return (JObject) response?.SelectToken("response.players.player[0]");
            }
            catch
            { return null; }
        }


        // Calls to Steam API Endpoint and returns the result as JSON Object
        private static JObject ApiCall(string uri)
        {
            // Create a request for the URL.
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // Get the response.
            HttpResponseMessage response = null;
            try
            { response = new HttpClient().Send(request); }
            catch (WebException e)
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
                    eventLog.WriteEntry($"Could not reach Steam API : \n[WebException] {e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error);
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
            Console.WriteLine(response?.ReasonPhrase);

            if (response == null || ((int)response.StatusCode) < 200 || ((int)response.StatusCode) >= 300) return null;

            // Get the stream containing content returned by the server.
            Stream dataStream = response.Content.ReadAsStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream ?? throw new InvalidOperationException());
            // Read the content.
            var responseFromServer = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Dispose();
            // Return the response
            return JObject.Parse(responseFromServer);
        }

        private static string GetApiKey()
        {
            return !string.IsNullOrEmpty(Properties.Settings.Default.SteamAPIKey)
                ? Properties.Settings.Default.SteamAPIKey
                : StaticData.SteamApiKey;
        }
    }

    internal class AuthCodeProvider : SteamAuthenticationCodesProvider
    {
        public override string GetEmailAuthenticationCode(SteamCredentials steamCredentials)
        {
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your email auth code: ";


            var input = MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput().Result;

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return input;
        }

        public override string GetTwoFactorAuthenticationCode(SteamCredentials steamCredentials)
        {
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your 2FA code: ";

            var input = MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput().Result;

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return input;
        }
    }


    internal class SteamApiFileDetails
    {
        public uint   result           { get; set; }
        public ulong  publishedfileid  { get; set; }
        public ulong  creator          { get; set; }
        public uint   creator_appid    { get; set; }
        public uint   consumer_appid   { get; set; }
        public string filename         { get; set; }
        public ulong  file_size        { get; set; }
        public string title            { get; set; }
        public string file_description { get; set; }
        public ulong  time_created     { get; set; }
        public ulong  time_updated     { get; set; }
    }

    internal class SteamApiPlayerInfo
    {
        public ulong  steamid     { get; set; }
        public string personaname { get; set; }
        public string profileurl  { get; set; }
    }
}
