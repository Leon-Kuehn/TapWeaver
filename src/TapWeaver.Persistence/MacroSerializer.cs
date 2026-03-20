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
}
