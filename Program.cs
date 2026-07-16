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
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        AppGlobals.TAppContext = new TrayApplicationContext();
        ApplicationConfiguration.Initialize();
        Application.Run(AppGlobals.TAppContext);

    }    
}