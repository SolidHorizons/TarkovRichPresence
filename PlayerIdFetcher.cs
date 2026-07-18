namespace TarkovRichPresence;

using System.Text.RegularExpressions;

class PlayerIdFetcher
{
    private static readonly Regex RE_PLAYER_ID = new Regex(@"PrepareSelectedProfileLocally.*AccountId:(?<accountid>\d+)");

    // logsRootPath is the Tarkov "Logs" folder (e.g. <ExeDir>\Logs), which contains a
    // timestamped subfolder per launch, such as "2026.07.18_20-17-23_1.0.6.5.46221".
    public static string? GetPlayerIdFromLog(string logsRootPath)
    {
        string? applicationLogPath = FindLatestApplicationLog(logsRootPath);
        if (applicationLogPath == null)
        {
            FileLogger.Log($"[PlayerIdFetcher] No application log found under: {logsRootPath}");
            return null;
        }

        using var stream = new FileStream(applicationLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            Match match = RE_PLAYER_ID.Match(line);
            if (match.Success)
                return match.Groups["accountid"].Value;
        }

        return null;
    }

    private static string? FindLatestApplicationLog(string logsRootPath)
    {
        if (!Directory.Exists(logsRootPath))
            return null;

        string? latestFolder = Directory.GetDirectories(logsRootPath)
            .OrderByDescending(x => new DirectoryInfo(x).CreationTime)
            .FirstOrDefault();

        if (latestFolder == null)
            return null;

        return Directory.GetFiles(latestFolder)
            .FirstOrDefault(x => Path.GetFileName(x).Contains("application"));
    }
}
