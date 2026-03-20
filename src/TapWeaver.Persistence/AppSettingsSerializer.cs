using System.Text.Json;
using System.Text.Json.Serialization;
using TapWeaver.Core.Models;

namespace TapWeaver.Persistence;

public static class AppSettingsSerializer
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TapWeaver");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Loads application settings from %AppData%\TapWeaver\settings.json.
    /// Returns a default <see cref="AppSettings"/> instance if the file does not exist or cannot be read.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    /// <summary>
    /// Persists <paramref name="settings"/> to %AppData%\TapWeaver\settings.json.
    /// Silently ignores I/O errors.
    /// </summary>
    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, Options);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}
