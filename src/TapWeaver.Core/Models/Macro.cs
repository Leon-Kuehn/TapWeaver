namespace TapWeaver.Core.Models;

public class Macro
{
    public string Name { get; set; } = "New Macro";
    public int Version { get; set; } = 1;
    public RepeatMode RepeatMode { get; set; } = RepeatMode.Once;
    public int RepeatCount { get; set; } = 1;
    public int LoopDelayMs { get; set; } = 0;
    public List<MacroStep> Steps { get; set; } = new();
}
