using FASTER.Services;

using Newtonsoft.Json.Linq;

using System.Net.Http;

namespace FASTER.Models;

public static class SteamWebApi
{
    private const string V2 = "&steamids=";
    private const string V3 = "&publishedfileids[0]=";
    private const string FileDetailsEndpoint = "https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=";
    private const string PlayerSummariesEndpoint = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v1?key=";

    public static JObject? GetSingleFileDetails(uint modId)
    {
        try
        {
            var response = ApiCall(FileDetailsEndpoint + GetApiKey() + V3 + modId);
            return (JObject?)response?.SelectToken("response.publishedfiledetails[0]");
        }
        catch
        { return null; }
    }

    public static JObject? GetPlayerSummaries(string playerId)
    {
        try
        {
            var response = ApiCall(PlayerSummariesEndpoint + GetApiKey() + V2 + playerId);
            return (JObject?)response?.SelectToken("response.players.player[0]");
        }
        catch
        { return null; }
    }

    private static JObject? ApiCall(string uri)
    {
        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };
        HttpResponseMessage? response = null;

        try
        { response = client.GetAsync(uri).Result; }
        catch (Exception e)
        {
            Ui.Current.DisplayMessage("Cannot reach Steam API.\nCheck https://steamstat.us/ for status.");
            Console.WriteLine($"Could not reach Steam API: {e.Message}");
        }

        Console.WriteLine(response?.StatusCode);

        return response == null
            ? null
            : JObject.Parse(response.Content.ReadAsStringAsync().Result);
    }

    private static string GetApiKey()
    {
        return !string.IsNullOrEmpty(AppSettings.Current.SteamAPIKey)
            ? AppSettings.Current.SteamAPIKey
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
    public string? filename { get; set; }
    public ulong file_size { get; set; }
    public string? title { get; set; }
    public string? file_description { get; set; }
    public ulong time_created { get; set; }
    public ulong time_updated { get; set; }
}

internal class SteamApiPlayerInfo
{
    public ulong steamid { get; set; }
    public string? personaname { get; set; }
    public string? profileurl { get; set; }
}
