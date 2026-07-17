using System.Text.Json.Serialization;

namespace TarkovRichPresence;

class TarkovPlayerProfile
{
    [JsonPropertyName("info")]
    public PlayerInfo? Info { get; set; }
}

class PlayerInfo
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("experience")]
    public long Experience { get; set; }
}
