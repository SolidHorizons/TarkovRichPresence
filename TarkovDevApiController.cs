using System.Text.Json;

namespace TarkovRichPresence;

class TarkovDevApiController
{
    private static readonly string TarkovDevApiRoute = "https://players.tarkov.dev";
    private static readonly HttpClient _client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TarkovRichPresence/1.0");
        client.Timeout = TimeSpan.FromSeconds(10);
        return client;
    }

    public static async Task<bool> isTarkovDevApiAvailable()
    {
        try
        {
            var response = await _client.GetAsync(TarkovDevApiRoute);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[TarkovDevApiController] Error checking API availability: {ex}");
            return false;
        }
    }

    public static async Task<TarkovPlayerProfile?> GetPlayerProfile(string accountId, Gamemode gamemode)
    {
        string api_gamemode_path;

        switch (gamemode)
        {
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
            return JsonSerializer.Deserialize<TarkovPlayerProfile>(response);
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[TarkovDevApiController] Error fetching player profile for {accountId}: {ex}; Full api URL: {TarkovDevApiRoute}/{api_gamemode_path}/{accountId}.json");
            return null;
        }
    }
}