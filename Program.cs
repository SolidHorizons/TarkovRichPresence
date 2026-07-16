namespace TarkovRichPresence;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        RPCManager.Initialize();

        RPCManager.getInstance.setPlayerData(39, GameEdition.EdgeOfDarkness, Gamemode.PvE, Faction.USEC);

        RPCManager.getInstance.setDiscordRpcStatus("mainmenu");

        Thread.Sleep(5000);

        RPCManager.getInstance.setDiscordRpcStatus("customs");

        Thread.Sleep(5000);

        RPCManager.getInstance.setDiscordRpcStatus("shoreline");

        Application.Run(new TrayApplicationContext());
    }
}