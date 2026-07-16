using System.Text.Json.Nodes;

namespace TarkovRichPresence;

class TarkovDevApiController
{
    private static readonly string TarkovDevApiRoute = "https://players.tarkov.dev";
    private static readonly HttpClient _client = new();

    public static async Task<JsonNode?> GetPlayerProfile(string accountId, Gamemode gamemode)
    {
        string api_gamemode_path;

        switch(gamemode){
            case Gamemode.PvE:
                api_gamemode_path = "pve";
                break;
            case Gamemode.PvP:
                api_gamemode_path = "profile";
                break;
            default:
                throw new Exception();
        }

        try
        {
            string url = $"{TarkovDevApiRoute}/{api_gamemode_path}/{accountId}.json";
            string response = await _client.GetStringAsync(url);
            return JsonNode.Parse(response);
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[TarkovDevApiController] Error fetching player profile for {accountId}: {ex}");
            return null;
        }
    }
}