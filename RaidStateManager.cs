namespace TarkovRichPresence;

enum RaidPhase
{
    MainMenu,        // Not in a raid, browsing menus/flea/traders
    LoadingIntoRaid, // Map is known but the raid hasn't started yet
    InRaid,          // Actively in the raid
    LeavingRaid,     // Raid is ending (extracted/died), on the way back to menu
}

// Tracks which phase of the main menu -> raid -> raid end cycle we're currently in.
// Tarkov's logs only tell us the map name while it's still loading (before the raid actually
// starts), so this class remembers it via SetPendingLocation and uses it once EnterRaid fires.
// Each phase transition also pushes the matching Discord presence, so callers only need to react
// to their own regex signals and tell us which phase we're entering.
class RaidStateManager
{
    private static RaidStateManager? _instance;
    public static RaidStateManager getInstance => _instance ??= new RaidStateManager();

    private RaidStateManager() { }

    public RaidPhase CurrentPhase { get; private set; } = RaidPhase.MainMenu;
    public Location? CurrentLocation { get; private set; }

    public event Action<RaidPhase>? PhaseChanged;

    public void EnterMainMenu()
    {
        CurrentLocation = null;
        SetPhase(RaidPhase.MainMenu);
        RPCManager.getInstance.setDiscordRpcStatus("mainmenu", 
        menuScreen =>
        {
             MenuScreen? screen = TarkovRPStates.GetMenuScreen(menuScreen);

                        if (screen == null)
                        {
                            Console.WriteLine($"Location '{menuScreen}' not found in TarkovRPStates.");
                            return null;
                        }

                        return RPCManager.getInstance.CreateMenuScreenPresence(screen);
        });
    }

    public void EnterLoading()
    {
        // Do not clear CurrentLocation here: the map's "scene preset path" line is logged
        // before TRACE-NetworkGameMatching, so by the time we get here it may already hold
        // the location for the raid that's about to start. It gets cleared in EnterMainMenu.
        SetPhase(RaidPhase.LoadingIntoRaid);

        RPCManager.getInstance.setDiscordRpcStatus("loading",
        locationKey =>
        {
            Location? loc = TarkovRPStates.GetLocation(locationKey);

            if (loc == null)
            {
                Console.WriteLine($"Location '{locationKey}' not found in TarkovRPStates.");
                return null;
            }

            return RPCManager.getInstance.CreateLocationPresence(loc);
        });
    }

    // Called once the map name shows up in the logs, while we're still loading into the raid.
    // Does not change phase or presence, just remembers the map for when EnterRaid fires.
    public void SetPendingLocation(Location location)
    {
        CurrentLocation = location;
        FileLogger.Log($"[RaidStateManager] Pending raid location set: {location.Name}");
    }

    // updatePresence should only be false for early/imprecise raid-start signals (e.g. the
    // [Transit] log line), which fire before the raid timer should actually start. They still
    // flip CurrentPhase so state stays accurate, but only the precise signal (GameStarted) is
    // allowed to push Discord presence, otherwise the countdown timer starts too early.
    public void EnterRaid(bool updatePresence = true)
    {
        SetPhase(RaidPhase.InRaid);

        if (!updatePresence)
            return;

        if (CurrentLocation == null)
        {
            FileLogger.Log("[RaidStateManager] EnterRaid called without a known location.");
            return;
        }

        RPCManager.getInstance.setDiscordRpcStatus(CurrentLocation,
            loc => RPCManager.getInstance.CreateLocationPresence(loc));
    }

    public void EnterRaidEnd()
    {
        SetPhase(RaidPhase.LeavingRaid);

        // No dedicated art/state for "leaving raid" exists yet, so drop straight back to the
        // main menu presence rather than leaving Discord stuck showing the finished raid.
        EnterMainMenu();
    }

    private void SetPhase(RaidPhase newPhase)
    {
        if (newPhase == CurrentPhase)
            return;

        FileLogger.Log($"[RaidStateManager] {CurrentPhase} -> {newPhase}");
        CurrentPhase = newPhase;
        PhaseChanged?.Invoke(newPhase);
    }
}
