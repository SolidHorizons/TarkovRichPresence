namespace TarkovRichPresence;

static class AppGlobals
{
    public static TrayApplicationContext? TAppContext { get; set; }
}

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    /// 

    [STAThread]
    static void Main()
    {
        // Initialize logger first so we can capture all debug output
        FileLogger.Log("Application starting...");

        RPCManager.Initialize();

        AppSettings settings = AppSettings.Load();

        settings.Save();

        Gamemode gamemode = Gamemode.PvE; // Should retrieve gamemode somehow, Found a way to get gamemode from the application log, look for "Session Mode: Pve"  

        TarkovDevApiController.GetPlayerProfile(settings.PlayerId!, gamemode).ContinueWith(task =>
        {
            if (task.Result != null)
            {
                var profile = task.Result;
                var info = profile.Info;

                FileLogger.Log($"[TarkovDevApiController] Fetched player profile: {task.Result}");

                var playerData = new PlayerData
                {
                    Nickname = info?.Nickname ?? "",
                    Experience = LevelCalculator.GetLevelForXp(info?.Experience ?? 0),
                    Mode = gamemode,
                    PlayerFaction = info?.Side switch
                    {
                        "Bear" => Faction.Bear,
                        "Usec" => Faction.USEC,
                        "Savage" => Faction.Scav,
                        _ => Faction.Unknown
                    }
                };
                RPCManager.getInstance.setPlayerData(playerData);
            }
            else
            {
                FileLogger.Log("Failed to fetch player profile.");
            }
        }).Wait();

        RPCManager.getInstance.setDiscordRpcStatus("customs");

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        AppGlobals.TAppContext = new TrayApplicationContext();

        FileLogger.Log("Application initialized, entering message loop...");
        Application.Run(AppGlobals.TAppContext);

        FileLogger.Log("Application shutting down...");
        FileLogger.Close();
    }
}