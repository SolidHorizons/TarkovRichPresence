namespace TarkovRichPresence;

static class AppGlobals
{
    public static TrayApplicationContext? TAppContext { get; set; }
}

static class Program
{
    private static Mutex? _instanceMutex;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    ///

    [STAThread]
    static void Main()
    {
        // Initialize logger first so we can capture all debug output
        FileLogger.Log("Application starting...");

        _instanceMutex = new Mutex(true, "TarkovRichPresence_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            FileLogger.Log("Another instance is already running. Exiting.");
            return;
        }

        RPCManager.Initialize();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Nothing heavier than the tray icon and a passive process-name poll runs here.
        // Fetching the player profile, watching Tarkov's logs, and setting Discord RPC status
        // only start once TrayApplicationContext detects the Tarkov process is running.
        AppGlobals.TAppContext = new TrayApplicationContext();

        try
        {
            FileLogger.Log("Application initialized, entering message loop...");
            Application.Run(AppGlobals.TAppContext);
        }
        finally
        {
            FileLogger.Log("Application shutting down...");
            _instanceMutex.ReleaseMutex();
            _instanceMutex.Dispose();
            FileLogger.Close();
        }
    }
}
