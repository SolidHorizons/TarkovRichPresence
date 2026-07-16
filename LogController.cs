namespace TarkovRichPresence;
using System.IO;

public class LogController
{
    const string LOG_SUBDIR = "Logs";
    private static AppSettings settings = AppGlobals.TAppContext!.getAppSettings();
    private string FullLogPath = Path.Combine(settings.ExePath!, LOG_SUBDIR);


}