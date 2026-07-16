using System.Text.Json;

namespace TarkovRichPresence;

class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TarkovRichPresence",
        "settings.json"
    );

    private string ?_exepath;
    public string? ExePath { 
        get { return _exepath; } 
        set { if(value != null){_exepath = value;}}
    }

    public string? ExeDir {
        get { return Path.GetDirectoryName(_exepath);}
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new();
        }
        catch { }
        return new();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
