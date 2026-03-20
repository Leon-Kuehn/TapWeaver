using TapWeaver.Core.Input;

namespace TapWeaver.Core.Services;

public class HotkeyService : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly Dictionary<int, Action> _hotkeys = new();
    private int _nextId = 1;

    public HotkeyService(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

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

    public bool Unregister(int id)
    {
        if (_hotkeys.Remove(id))
        {
            return NativeMethods.UnregisterHotKey(_windowHandle, id);
        }
        return false;
    }

    public void HandleHotkey(int id)
    {
        if (_hotkeys.TryGetValue(id, out var action))
            action?.Invoke();
    }

    public void Dispose()
    {
        foreach (var id in _hotkeys.Keys)
            NativeMethods.UnregisterHotKey(_windowHandle, id);
        _hotkeys.Clear();
        GC.SuppressFinalize(this);
    }
}
