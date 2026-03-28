namespace TapWeaver.Core.Models;

public class AppSettings
{
    /// <summary>
    /// Fixed emergency stop hotkey: Ctrl+Alt+Pause — always active, cannot be changed.
    /// </summary>
    public static readonly HotkeyConfig FixedEmergencyStop = new()
    {
        Modifiers  = HotkeyConfig.MOD_CONTROL | HotkeyConfig.MOD_ALT,
        VirtualKey = HotkeyConfig.VK_PAUSE
    };

    public bool AlwaysOnTop { get; set; } = false;
    public bool UseDarkMode { get; set; } = true;
    public bool CompactMode { get; set; } = false;

    /// <summary>Record keyboard key down/up events while recording.</summary>
    public bool RecordKeyboardEvents { get; set; } = true;

    /// <summary>Record mouse button click events while recording.</summary>
    public bool RecordMouseClickEvents { get; set; } = true;

    /// <summary>Record mouse movement (MoveMouse steps) while recording.</summary>
    public bool RecordMouseMoveEvents { get; set; } = false;

    /// <summary>Toggle macro playback on/off. Default: Ctrl+F8.</summary>
    public HotkeyConfig PlaybackToggleHotkey { get; set; } = new()
    {
        Modifiers  = HotkeyConfig.MOD_CONTROL,
        VirtualKey = HotkeyConfig.VK_F8
    };

    /// <summary>Toggle macro recording on/off. Default: Ctrl+F7.</summary>
    public HotkeyConfig RecordingToggleHotkey { get; set; } = new()
    {
        Modifiers  = HotkeyConfig.MOD_CONTROL,
        VirtualKey = HotkeyConfig.VK_F7
    };

    /// <summary>Toggle auto-clicker on/off. Default: Ctrl+F9.</summary>
    public HotkeyConfig AutoClickerToggleHotkey { get; set; } = new()
    {
        Modifiers  = HotkeyConfig.MOD_CONTROL,
        VirtualKey = HotkeyConfig.VK_F9
    };

    /// <summary>
    /// Enables keyboard message routing to a selected target window handle.
    /// </summary>
    public bool RouteInputToSelectedWindow { get; set; } = false;

    /// <summary>
    /// Last selected target window handle. Stored as Int64 for JSON portability.
    /// </summary>
    public long TargetWindowHandle { get; set; } = 0;
}
