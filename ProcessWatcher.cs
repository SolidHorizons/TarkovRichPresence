using System.Diagnostics;

namespace TarkovRichPresence;

// Passively polls the process list for a given process name. Uses Process.GetProcessesByName,
// which only queries process names/ids from the OS (no handle access, no injection, no reading
// of Tarkov's memory), so it stays invisible to anti-cheat while still being reliable.
class ProcessWatcher : IDisposable
{
    private readonly string _processName;
    private readonly System.Threading.Timer _timer;
    private bool _isRunning;
    private bool _disposed;

    public event Action? ProcessStarted;
    public event Action? ProcessStopped;

    public bool IsRunning => _isRunning;

    public ProcessWatcher(string processName, TimeSpan? pollInterval = null)
    {
        _processName = processName;
        var interval = pollInterval ?? TimeSpan.FromSeconds(5);
        _timer = new System.Threading.Timer(Poll, null, TimeSpan.Zero, interval);
    }

    private void Poll(object? state)
    {
        try
        {
            var processes = Process.GetProcessesByName(_processName);
            bool running = processes.Length > 0;
            foreach (var process in processes)
                process.Dispose();

            if (running == _isRunning)
                return;

            _isRunning = running;
            FileLogger.Log($"[ProcessWatcher] '{_processName}' is now {(running ? "running" : "not running")}.");

            if (running)
                ProcessStarted?.Invoke();
            else
                ProcessStopped?.Invoke();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[ProcessWatcher] Error while polling for '{_processName}': {ex}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _timer.Dispose();
    }
}
