using TapWeaver.Core.Interop;

namespace TapWeaver.Core.Services;

public class HotkeyService : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly Dictionary<int, Action>   _hotkeys   = new();
    private readonly Dictionary<int, DateTime> _lastFired = new();
    private int _nextId = 1;

    /// <summary>Minimum interval between two successive fires of the same hotkey.</summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(400);

    public HotkeyService(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    /// <summary>
    /// Registers a global hotkey and returns its ID, or -1 if registration failed.
    /// </summary>
    public int Register(uint modifiers, uint vk, Action callback)
    {
        int id = _nextId++;
        if (NativeMethods.RegisterHotKey(_windowHandle, id, modifiers, vk))
        {
            _hotkeys[id] = callback;
            return id;
        }
        return -1;
    }

    /// <summary>Unregisters a previously registered hotkey by ID.</summary>
    public bool Unregister(int id)
    {
        if (id < 0) return false;
        _lastFired.Remove(id);
        if (_hotkeys.Remove(id))
            return NativeMethods.UnregisterHotKey(_windowHandle, id);
        return false;
    }

    /// <summary>
    /// Called from the WndProc hook when a WM_HOTKEY message arrives.
    /// Applies debouncing so a single physical key-press only fires once.
    /// </summary>
    public void HandleHotkey(int id)
    {
        if (!_hotkeys.TryGetValue(id, out var action)) return;

        var now = DateTime.UtcNow;
        if (_lastFired.TryGetValue(id, out var last) && (now - last) < DebounceInterval)
            return;

        _lastFired[id] = now;
        action?.Invoke();
    }

    public void Dispose()
    {
        foreach (var id in _hotkeys.Keys)
            NativeMethods.UnregisterHotKey(_windowHandle, id);
        _hotkeys.Clear();
        _lastFired.Clear();
        GC.SuppressFinalize(this);
    }
}
