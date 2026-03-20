namespace TapWeaver.Core.Models;

/// <summary>
/// A named profile that wraps a <see cref="Macro"/> with metadata
/// and is persisted as a JSON file.
/// </summary>
public class MacroProfile
{
    public string   Name        { get; set; } = "New Profile";
    public string   Description { get; set; } = "";
    public DateTime Created     { get; set; } = DateTime.UtcNow;
    public DateTime Modified    { get; set; } = DateTime.UtcNow;
    public Macro    Macro       { get; set; } = new();
}
