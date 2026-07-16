namespace TarkovRichPresence;
using System.IO;

class LogController
{
    const string LOG_SUBDIR = "Logs";
    private string? FullLogPath;

    public long AppLogCounter;
    public long BackendLogCounter;

    private string _currentLogFolder = "";

    public string GetLatestLogFolder()
    {
        if (FullLogPath == null)
            throw new InvalidOperationException("FullLogPath not initialized");
        return Directory.GetDirectories(FullLogPath)
            .OrderByDescending(x => new DirectoryInfo(x).CreationTime)
            .First();
    }


    private string GetLogPath(string folder, string name)
    {
        return Directory.GetFiles(folder)
            .FirstOrDefault(x => Path.GetFileName(x).Contains(name))
            ?? "";
    }

    private string? appLog;
    private string? backendLog;

    private void WaitForLogFiles()
    {
        while (true)
        {
            Thread.Sleep(1000);
            _currentLogFolder = GetLatestLogFolder();
            FileLogger.Log($"[LogController] Current log folder: {_currentLogFolder}");
            
            appLog = GetLogPath(_currentLogFolder, "application");
            if (string.IsNullOrEmpty(appLog))
            {
                FileLogger.Log("ERROR: Could not find application log file, retrying...");
                Thread.Sleep(500);
                continue;
            }
            FileLogger.Log($"[LogController] Found app log: {appLog}");
            
            backendLog = GetLogPath(_currentLogFolder, "backend");
            if (string.IsNullOrEmpty(backendLog))
            {
                FileLogger.Log("ERROR: Could not find backend log file, retrying...");
                Thread.Sleep(500);
                continue;
            }
            FileLogger.Log($"[LogController] Found backend log: {backendLog}");
            break;
        }
    }

    public void RunWatchers()
    {
        try 
        {
            // Initialize on first call (lazy initialization)
            if (FullLogPath == null)
            {
                var settings = AppGlobals.TAppContext!.getAppSettings();
                if (string.IsNullOrEmpty(settings.ExeDir))
                {
                    FileLogger.Log("ERROR: ExeDir not set in AppSettings. Please configure the Tarkov exe path.");
                    return;
                }
                FullLogPath = Path.Combine(settings.ExeDir, LOG_SUBDIR);
                FileLogger.Log($"[LogController] Initialized FullLogPath: {FullLogPath}");
            }

            WaitForLogFiles();

            AppLogCounter = new FileInfo(appLog!).Length;
            BackendLogCounter = new FileInfo(backendLog!).Length;
            FileLogger.Log($"[LogController] Starting to watch logs. App size: {AppLogCounter}, Backend size: {BackendLogCounter}");

            while (true)
            {
                try
                {
                    Thread.Sleep(250);

                    // Tarkov creates a new folder every launch
                    string latestFolder = GetLatestLogFolder();

                    if (latestFolder != _currentLogFolder)
                    {
                        FileLogger.Log("Switched to new log folder, waiting for log files...");
                        
                        AppLogCounter = 0;
                        BackendLogCounter = 0;
                        
                        WaitForLogFiles();
                        
                        AppLogCounter = new FileInfo(appLog!).Length;
                        BackendLogCounter = new FileInfo(backendLog!).Length;
                        
                        FileLogger.Log($"[LogController] Ready with new logs. App size: {AppLogCounter}, Backend size: {BackendLogCounter}");
                    }

                    ReadNewLines(appLog!, ref AppLogCounter);
                    ReadNewLines(backendLog!, ref BackendLogCounter);
                }
                catch (Exception ex)
                {
                    FileLogger.Log($"[LogController] Error in loop: {ex}");
                }
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[LogController] FATAL ERROR in RunWatchers: {ex}");
        }
    }

    private void ReadNewLines(string path, ref long position)
    {
        try
        {
            FileLogger.Log($"[LogController] running ReadNewLines for {path}");
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            stream.Seek(position, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);

            int linesRead = 0;
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();

                if (line != null)
                {
                    FileLogger.Log($"[{Path.GetFileName(path)}] {line}");
                    linesRead++;
                }
            }

            position = stream.Position;
            if (linesRead > 0)
                FileLogger.Log($"[ReadNewLines] Read {linesRead} lines from {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[ReadNewLines] Error reading {path}: {ex}");
        }
    }

}