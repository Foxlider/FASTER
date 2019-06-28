using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FASTER.Models
{
    class SteamWebApi
    {
        private const string SteamApiKey = "1669DCBF5FD494B07B85358D12FFB85B";
        private const string V = "&publishedfileids[";
        private const string V1 = "]=";
        private const string V2 = "&steamids=";
        private const string V3 = "&publishedfileids[0]=";

        // Gets mod info for multiple mods
        public static List<JObject> GetFileDetails(List<int> modIds)
        {
            string mods = string.Empty;
            foreach (var modId in modIds)
                mods = mods + V + modIds.IndexOf(modId) + V1 + modId;

            var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + SteamApiKey + mods);

            return response.SelectTokens("response.publishedfiledetails[*]").Cast<JObject>().ToList();
        }

        // Get mod info for single mod
        public static JObject GetSingleFileDetails(int modId)
        {
            var response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" + SteamApiKey + V3 + modId);

            return (JObject) response.SelectToken("response.publishedfiledetails[0]");
        }


        // Gets user info
        public static JObject GetPlayerSummaries(string playerId)
        {
            var response = ApiCall("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v1?key=" + SteamApiKey + V2 + playerId);

            return (JObject) response.SelectToken("response.players.player[0]");
        }


        // Calls to Steam API Endpoint and returns the result as JSON Object
        private static JObject ApiCall(string uri)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(uri);
            // Get the response.
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                MainWindow.Instance.IMessageDialog.IsOpen = true;
                MainWindow.Instance.IMessageDialogText.Text = "Cannot reach Steam API \n\n"
                                                            + "Check https://steamstat.us/";
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
    }
}
