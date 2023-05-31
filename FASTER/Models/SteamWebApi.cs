using BytexDigital.Steam.Core;

using MahApps.Metro.Controls.Dialogs;

using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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


        // Calls to Steam API Endpoint and returns the result as JSON Object
        private static JObject ApiCall(string uri)
        {
            // Create a request for the URL. 
            HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(5);

            // Get the response.
            HttpResponseMessage response = null;

            try
            { response = client.GetAsync(uri).Result; }
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
            Console.WriteLine((response)?.StatusCode);

            return response == null
                       ? null
                       : JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Return the response
        }

        private static string GetApiKey()
        {
            return !string.IsNullOrEmpty(Properties.Settings.Default.SteamAPIKey)
                ? Properties.Settings.Default.SteamAPIKey
                : StaticData.SteamApiKey;
        }
    }

    internal class AuthCodeProvider : SteamAuthenticator
    {

        private readonly string _persistenceDirectory;
        private readonly string _uniqueStorageName;
        public string AccessToken { get; protected set; }
        public string GuardData { get; protected set; }


        public AuthCodeProvider(string uniqueStorageName, string persistenceDirectory)
        {
            _uniqueStorageName = uniqueStorageName;
            _persistenceDirectory = persistenceDirectory;
        }

        public override Task<string> GetEmailAuthenticationCodeAsync(string account_email, bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
        {
            if (previousCodeWasIncorrect)
                MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPreviously entered email code was incorrect!";


            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your 2FA code: ";

            var input = MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput().Result;

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return Task.FromResult(input);
        }

        public override Task<string> GetTwoFactorAuthenticationCodeAsync(bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
        {
            if (previousCodeWasIncorrect)
                MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPreviously entered 2FA code was incorrect!";


            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your 2FA code: ";

            var input = MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput().Result;

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return Task.FromResult(input);
        }

        public override Task<bool> NotifyMobileNotificationAsync(CancellationToken cancellationToken = default)
        {
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nMobile notification sent. Answer \"y\" once you've authorized this login. If no notification was received or you'd like to enter a traditional 2FA code, enter \"n\": ";

            MessageDialogResult response;

            do
            {
                response = MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInputPhone().Result;
            } while (response != MessageDialogResult.Affirmative);

            return Task.FromResult(true);
        }

        public override Task PersistAccessTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            AccessToken = token;

            if (string.IsNullOrEmpty(_persistenceDirectory)) return Task.CompletedTask;

            Directory.CreateDirectory(_persistenceDirectory);
            File.WriteAllText(Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_accesstoken"), AccessToken);

            return Task.CompletedTask;
        }


        public override Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_persistenceDirectory))
            {
                return Task.FromResult(AccessToken);
            }

            var path = Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_accesstoken");

            return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : AccessToken);
        }

        public override Task PersistGuardDataAsync(string data, CancellationToken cancellationToken = default)
        {
            GuardData = data;

            if (string.IsNullOrEmpty(_persistenceDirectory)) return Task.CompletedTask;

            Directory.CreateDirectory(_persistenceDirectory);
            File.WriteAllText(Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_guarddata"), GuardData);

            return Task.CompletedTask;
        }

        public override Task<string> GetGuardDataAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_persistenceDirectory))
            {
                return Task.FromResult(GuardData);
            }

            var path = Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_guarddata");

            return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : GuardData);
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
