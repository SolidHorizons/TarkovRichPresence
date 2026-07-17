using DiscordRPC;
using System.Text.RegularExpressions;

namespace TarkovRichPresence;

enum GameEdition
{
    Unknown,
    Standard,
    LeftBehind,
    PrepareForEscape,
    EdgeOfDarkness,
    Unheard,
}

enum Faction
{
    Unknown,
    Bear,
    USEC,
    Scav,
}

enum Gamemode
{
    PvE,
    PvP,
    Offline,
    Unknown,
}

class PlayerData
{
    public int Level { get; set; } = 0;
    public GameEdition Edition { get; set; } = GameEdition.Unknown;

    public Gamemode Mode { get; set; } = Gamemode.PvE;
    public Faction PlayerFaction { get; set; } = Faction.Unknown;
}


class RPCManager
{
    public bool disablePlayerStatistics { get; set; } = false;
    private static readonly string ClientId = "1527351336046301254";
    private readonly DiscordRpcClient _client = new(ClientId);
    private static RPCManager? _instance;
    private static PlayerData _playerData = new();
    private Location? _currentLocation;

    private RPCManager()
    {
        _client.OnReady += (sender, e) =>
        {
            Console.WriteLine("Connected to Discord as " + e.User.Username);
        };

        _client.Initialize();
    }

    public static void Initialize()
    {
        if (_instance == null)
        {
            _instance = new RPCManager();
        }
    }

    // Basically does the same as Initialize, but returns the instance. We run initialize in Program.cs to ensure the client is initialized before we set the presence, but this allows us to get the instance without having to worry about whether it's initialized or not.
    public static RPCManager getInstance => _instance ??= new RPCManager();


    public void setDiscordRpcStatus(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            Console.WriteLine("Location is null or empty. Cannot set Discord RPC status.");
            return;
        }

        if (!_client.IsInitialized)
        {
            Console.WriteLine("Discord RPC client is not initialized.");
            return;
        }

        Location? loc = TarkovRPStates.GetLocation(location);

        if (loc == null)
        {
            Console.WriteLine($"Location '{location}' not found in TarkovRPStates.");
            return;
        }

        _currentLocation = loc;
        updateDiscordRpcStatus(loc);
    }

    // Made as a separate method to avoid code duplication and to make it easier to update the Discord RPC status in the future.
    private void updateDiscordRpcStatus(Location loc)
    {
        if (!_client.IsInitialized)
        {
            Console.WriteLine("Discord RPC client is not initialized.");
            return;
        }

        _client.SetPresence(new RichPresence()
        {
            Details = loc.Name + " - " + loc.State ?? "Unknown Location",
            State = disablePlayerStatistics ? null : $"{_playerData.Mode}: LVL {_playerData.Level} • {Regex.Replace(_playerData.Edition.ToString(), "(?<!^)([A-Z])", " $1")}",

            Timestamps = loc.MaxRaidTimeInSeconds > 0 ? new Timestamps(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(loc.MaxRaidTimeInSeconds)) : null,
            Assets = new Assets()
            {
                LargeImageKey = loc.LocationImage,
                LargeImageText = loc.Name + " - " + loc.State,
                SmallImageKey = disablePlayerStatistics ? null : $"{_playerData.PlayerFaction.ToString().ToLower()}_logotype",
                SmallImageText = disablePlayerStatistics ? null : _playerData.PlayerFaction != Faction.Unknown ? _playerData.PlayerFaction.ToString() : null,
            }
        });
    }

    // <summary>
    // Sets the player data for the Discord Rich Presence. Does not update Rich Presence
    // </summary>
    public void setPlayerData(PlayerData data)
    {
        _playerData = data;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}