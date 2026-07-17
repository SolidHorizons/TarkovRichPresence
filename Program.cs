namespace TarkovRichPresence;

static class AppGlobals
{
    public static TrayApplicationContext ?TAppContext {get; set;}
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

        RPCManager.Initialize();

        var playerData = new PlayerData
        {
            Level = 39,
            Edition = GameEdition.EdgeOfDarkness,
            Mode = Gamemode.PvE,
            PlayerFaction = Faction.USEC
        };
        RPCManager.getInstance.setPlayerData(playerData);

        RPCManager.getInstance.setDiscordRpcStatus("customs");

        // Initialize logger first so we can capture all debug output
        FileLogger.Log("Application starting...");
        
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