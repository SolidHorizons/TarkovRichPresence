namespace TarkovRichPresence;
using System.IO;

static class FileLogger
{
    private static StreamWriter? _writer;
    private static readonly object _lock = new object();

    static FileLogger()
    {
        try
        {
            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TarkovRichPresence"
            );
            Directory.CreateDirectory(logDir);
            
            string logPath = Path.Combine(logDir, "debug.log");
            _writer = new StreamWriter(logPath, true) { AutoFlush = true };
            Log($"=== Logger Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize FileLogger: {ex}");
        }
    }

    public static void Log(string message)
    {
        lock (_lock)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string formatted = $"[{timestamp}] {message}";
                
                _writer?.WriteLine(formatted);
                Console.WriteLine(formatted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging: {ex}");
            }
        }
    }

    public static void Close()
    {
        lock (_lock)
        {
            _writer?.Dispose();
        }
    }
}
