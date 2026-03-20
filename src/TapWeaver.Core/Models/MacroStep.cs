using System.Text.Json.Serialization;

namespace TapWeaver.Core.Models;

public class MacroStep
{
    public MacroStepType Type { get; set; }
    
    // Keyboard fields
    public string? Key { get; set; }
    public int HoldMs { get; set; }
    
    // Delay fields
    public int DelayMs { get; set; }
    
    // Mouse fields
    public MouseButton Button { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    
    [JsonIgnore]
    public string Description => Type switch
    {
        MacroStepType.KeyDown => $"Key Down: {Key}",
        MacroStepType.KeyUp => $"Key Up: {Key}",
        MacroStepType.KeyTap => $"Key Tap: {Key} ({HoldMs}ms)",
        MacroStepType.Delay => $"Delay: {DelayMs}ms",
        MacroStepType.MouseClick => X.HasValue ? $"Click {Button} at ({X},{Y})" : $"Click {Button}",
        MacroStepType.MoveMouse => $"Move to ({X},{Y})",
        _ => Type.ToString()
    };
    
    public MacroStep Clone() => new MacroStep
    {
        Type = Type,
        Key = Key,
        HoldMs = HoldMs,
        DelayMs = DelayMs,
        Button = Button,
        X = X,
        Y = Y
    };
}
