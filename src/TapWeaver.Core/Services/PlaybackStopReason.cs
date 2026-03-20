namespace TapWeaver.Core.Services;

/// <summary>
/// Indicates why macro playback stopped.
/// </summary>
public enum PlaybackStopReason
{
    /// <summary>The macro completed all configured iterations normally.</summary>
    Completed,

    /// <summary>The user pressed the Stop button or the Sequencer Start/Stop hotkey.</summary>
    UserStop,

    /// <summary>The fixed emergency-stop hotkey (Ctrl+Alt+Pause) was pressed.</summary>
    EmergencyStop,
}
