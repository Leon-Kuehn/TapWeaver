using System.Text.Json;
using System.Text.Json.Serialization;
using TapWeaver.Core.Models;

namespace TapWeaver.Persistence;

public static class MacroSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ── Plain Macro ──────────────────────────────────────────────────────────

    public static string Serialize(Macro macro) =>
        JsonSerializer.Serialize(macro, Options);

    public static Macro? Deserialize(string json) =>
        JsonSerializer.Deserialize<Macro>(json, Options);

    public static async Task SaveAsync(Macro macro, string filePath)
    {
        var json = Serialize(macro);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<Macro?> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize(json);
    }

    // ── MacroProfile ─────────────────────────────────────────────────────────

    public static string SerializeProfile(MacroProfile profile) =>
        JsonSerializer.Serialize(profile, Options);

    public static MacroProfile? DeserializeProfile(string json) =>
        JsonSerializer.Deserialize<MacroProfile>(json, Options);

    /// <summary>
    /// Saves a <see cref="MacroProfile"/> to <paramref name="filePath"/> as JSON,
    /// updating <see cref="MacroProfile.Modified"/> to the current UTC time.
    /// </summary>
    public static async Task SaveProfileAsync(MacroProfile profile, string filePath)
    {
        profile.Modified = DateTime.UtcNow;
        var json = SerializeProfile(profile);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Loads a <see cref="MacroProfile"/> from <paramref name="filePath"/>.
    /// Supports both the new profile format and the legacy plain-<see cref="Macro"/> format.
    /// </summary>
    public static async Task<MacroProfile?> LoadProfileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);

        // Try the newer profile format first (has a "macro" property)
        try
        {
            var profile = DeserializeProfile(json);
            if (profile?.Macro != null)
                return profile;
        }
        catch { }

        // Fall back to legacy plain-Macro format
        var macro = Deserialize(json);
        if (macro != null)
            return new MacroProfile { Name = macro.Name, Macro = macro };

        return null;
    }
}
