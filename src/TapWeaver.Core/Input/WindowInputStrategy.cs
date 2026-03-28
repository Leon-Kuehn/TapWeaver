namespace TapWeaver.Core.Input;

/// <summary>
/// Defines how keyboard input is delivered to a target window.
/// </summary>
public enum WindowInputStrategy
{
    /// <summary>
    /// Temporarily brings the target window to the foreground,
    /// sends input via global SendInput, then restores the previous foreground window.
    /// Most reliable for standard desktop applications.
    /// </summary>
    FocusSwitch,

    /// <summary>
    /// Posts WM_KEYDOWN/WM_KEYUP messages directly to the target window handle
    /// without changing focus. Works for simple desktop apps, but many modern apps,
    /// games, and Roblox ignore these messages entirely.
    /// </summary>
    PostMessage
}
