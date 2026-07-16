namespace TarkovRichPresence;
using System.IO;

class LogController
{
    const string LOG_SUBDIR = "Logs";
    private static AppSettings settings = AppGlobals.TAppContext!.getAppSettings();

    public string FullLogPath = Path.Combine(settings.ExeDir!, LOG_SUBDIR);

    public long AppLogCounter;
    public long BackendLogCounter;

    private string _currentLogFolder = "";

    public string GetLatestLogFolder()
    {
        return Directory.GetDirectories(FullLogPath)
            .OrderBy(x => x)
            .Last();
    }


    private string GetLogPath(string folder, string name)
    {
        return Directory.GetFiles(folder)
            .FirstOrDefault(x => Path.GetFileName(x).Contains(name))
            ?? "";
    }

    public void RunWatchers()
    {
        _currentLogFolder = GetLatestLogFolder();
        
        string appLog = GetLogPath(_currentLogFolder, "application");
        if (string.IsNullOrEmpty(appLog))
            return;
        string backendLog = GetLogPath(_currentLogFolder, "backend");

        AppLogCounter = new FileInfo(appLog).Length;
        BackendLogCounter = new FileInfo(backendLog).Length;

        while (true)
        {
            try{

                Thread.Sleep(250);

                // Tarkov creates a new folder every launch
                string latestFolder = GetLatestLogFolder();

                if (latestFolder != _currentLogFolder)
                {
                    _currentLogFolder = latestFolder;

                    appLog = GetLogPath(_currentLogFolder, "application");
                    backendLog = GetLogPath(_currentLogFolder, "backend");

                    AppLogCounter = 0;
                    BackendLogCounter = 0;

                    Console.WriteLine("Switched to new log folder.");
                }

                ReadNewLines(appLog, ref AppLogCounter);
                ReadNewLines(backendLog, ref BackendLogCounter);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ReadNewLines(string path, ref long position)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

        stream.Seek(position, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();

            if (line != null)
            {
                Console.WriteLine($"[{Path.GetFileName(path)}] {line}");

                // Handle the line here
            }
        }

        position = stream.Position;
    }

}