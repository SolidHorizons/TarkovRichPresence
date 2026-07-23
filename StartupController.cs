using Microsoft.Win32;

namespace TarkovRichPresence;

static class StartupController
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "TarkovRichPresence";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(ValueName) is string existing && existing == GetExePath();
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (enabled)
        {
            key.SetValue(ValueName, GetExePath());
            FileLogger.Log("[StartupController] Enabled startup on system startup.");
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            FileLogger.Log("[StartupController] Disabled startup on system startup.");
        }
    }

    private static string GetExePath() => $"\"{Environment.ProcessPath}\"";
}
