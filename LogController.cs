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

    // Returns true if the wait was cancelled before completing.
    private static bool CancellableSleep(int milliseconds, CancellationToken token)
        => token.WaitHandle.WaitOne(milliseconds);

    private void WaitForLogFiles(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (CancellableSleep(1000, token))
                return;

            _currentLogFolder = GetLatestLogFolder();
            FileLogger.Log($"[LogController] Current log folder: {_currentLogFolder}");

            appLog = GetLogPath(_currentLogFolder, "application");
            if (string.IsNullOrEmpty(appLog))
            {
                FileLogger.Log("ERROR: Could not find application log file, retrying...");
                if (CancellableSleep(500, token))
                    return;
                continue;
            }
            FileLogger.Log($"[LogController] Found app log: {appLog}");

            backendLog = GetLogPath(_currentLogFolder, "backend");
            if (string.IsNullOrEmpty(backendLog))
            {
                FileLogger.Log("ERROR: Could not find backend log file, retrying...");
                if (CancellableSleep(500, token))
                    return;
                continue;
            }
            FileLogger.Log($"[LogController] Found backend log: {backendLog}");
            break;
        }
    }

    public void RunWatchers(CancellationToken token = default)
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

            WaitForLogFiles(token);
            if (token.IsCancellationRequested)
                return;

            AppLogCounter = new FileInfo(appLog!).Length;
            BackendLogCounter = new FileInfo(backendLog!).Length;
            FileLogger.Log($"[LogController] Starting to watch logs. App size: {AppLogCounter}, Backend size: {BackendLogCounter}");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (CancellableSleep(250, token))
                        break;

                    // Tarkov creates a new folder every launch
                    string latestFolder = GetLatestLogFolder();

                    if (latestFolder != _currentLogFolder)
                    {
                        FileLogger.Log("Switched to new log folder, waiting for log files...");

                        AppLogCounter = 0;
                        BackendLogCounter = 0;

                        WaitForLogFiles(token);
                        if (token.IsCancellationRequested)
                            break;

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

            FileLogger.Log("[LogController] RunWatchers stopped.");
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

                Dictionary<string,string> lineResult = new();

                if (line != null)
                {
                    FileLogger.Log($"[{Path.GetFileName(path)}] {line}");
                    if (path.Contains("application"))
                    {
                        lineResult.Add("map", RegexController.CheckRegexOnLine(RegexController.RE_MAP, line));
                        lineResult.Add("acc", RegexController.CheckRegexOnLine(RegexController.RE_ACCOUNT_ID, line));

                    }else if (path.Contains("backend"))
                    {
                        lineResult.Add("trader", RegexController.CheckRegexOnLine(RegexController.RE_TALKING_TO_TRADER, line));
                    }
                    linesRead++;
                }

                foreach(KeyValuePair<string, string> item in lineResult)
                {
                    if(item.Value is not null) RegexController.HandleMatch(item);
                }
            }

            position = stream.Position;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[ReadNewLines] Error reading {path}: {ex}");
        }
    }

}