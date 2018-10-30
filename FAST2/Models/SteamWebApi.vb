Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Namespace Models
    Public Class SteamWebApi

        Private Const SteamApiKey = "1669DCBF5FD494B07B85358D12FFB85B"
        Private Const V As String = "&publishedfileids["
        Private Const V1 As String = "]="
        Private Const V2 As String = "&steamids="
        Private Const V3 As String = "&publishedfileids[0]="

        'Gets mod info for multiple mods
        Public Shared Function GetFileDetails(modIds As List(Of Integer)) As List(Of JObject)

            Dim mods As String = String.Empty
            For Each modId In modIds
                mods = mods & V & modIds.IndexOf(modId) & V1 & modId
            Next

            Dim response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" & SteamApiKey & mods)
            
            Return response.SelectTokens("response.publishedfiledetails[*]").Cast (Of JObject)().ToList()
        End Function

        'Get mod info for single mod
        Public Shared Function GetSingleFileDetails(modId As Integer) As JObject
            Dim response = ApiCall("https://api.steampowered.com/IPublishedFileService/GetDetails/v1?key=" & SteamApiKey & V3 & modId)

            Return response.SelectToken("response.publishedfiledetails[0]")
        End Function


        'Gets user info
        Public Shared Function GetPlayerSummaries(playerId As String) As JObject
            Dim response = ApiCall("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v1?key=" & SteamApiKey & V2 & playerId)

            Return response.SelectToken("response.players.player[0]")
        End Function


        'Calls to Steam API Endpoint and returns the result as JSON Object
        Private Shared Function ApiCall(uri As String) As JObject
            ' Create a request for the URL. 
            Dim request As WebRequest = WebRequest.Create(uri)
            ' Get the response.
            Dim response As WebResponse = request.GetResponse()
            ' Display the status.
            Console.WriteLine(CType(response, HttpWebResponse).StatusDescription)
            ' Get the stream containing content returned by the server.
            Dim dataStream As Stream = response.GetResponseStream()
            ' Open the stream using a StreamReader for easy access.
            Dim reader As New StreamReader(dataStream)
            ' Read the content.
            Dim responseFromServer = reader.ReadToEnd()
            ' Clean up the streams and the response.
            reader.Close()
            response.Close()
            ' Return the response
            Return JObject.Parse(responseFromServer)
        End Function
    End Class
End NameSpace