using System.Text.Json;

namespace TarkovRichPresence;

class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TarkovRichPresence",
        "settings.json"
    );

    private string? _playerId;

    public string? PlayerId
    {
        get { return _playerId; }
        set { _playerId = value; }
    }

    private string? _exepath;
    public string? ExePath
    {
        get { return _exepath; }
        set { if (value != null) { _exepath = value; } }
    }

    public string? ExeDir
    {
        get { return Path.GetDirectoryName(_exepath); }
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath));

                if (settings == null)
                {
                    FileLogger.Log($"ERROR: Failed to deserialize settings from {SettingsPath}, falling back to defaults.");
                    throw new Exception("Deserialization returned null");
                }

                FileLogger.Log($"[AppSettings] Loaded settings from {settings.PlayerId}, {settings.ExePath}, {settings.ExeDir} at {SettingsPath}");

                return settings;
            }

        }
        catch (Exception ex)
        {
            FileLogger.Log($"ERROR: Failed to load settings from {SettingsPath}, falling back to defaults: {ex}");
        }
        return new();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        Console.WriteLine(JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
