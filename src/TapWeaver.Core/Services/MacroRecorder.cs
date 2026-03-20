using System.Diagnostics;
using System.Runtime.InteropServices;
using TapWeaver.Core.Input;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Services;

public class MacroRecorder : IDisposable
{
    private IntPtr _keyboardHook = IntPtr.Zero;
    private IntPtr _mouseHook = IntPtr.Zero;
    private NativeMethods.LowLevelProc? _keyboardProc;
    private NativeMethods.LowLevelProc? _mouseProc;
    private readonly List<MacroStep> _steps = new();
    private readonly Stopwatch _stopwatch = new();
    private long _lastEventTime = 0;
    private bool _isRecording;
    private readonly object _lock = new();

    public event Action<MacroStep>? StepRecorded;
    public bool IsRecording => _isRecording;

    public void Start()
    {
        if (_isRecording) return;
        lock (_lock)
        {
            _steps.Clear();
            _stopwatch.Restart();
            _lastEventTime = 0;
            _isRecording = true;
            InstallHooks();
        }
    }

    public Macro Stop()
    {
        if (!_isRecording) return new Macro();
        lock (_lock)
        {
            _isRecording = false;
            RemoveHooks();
            _stopwatch.Stop();
        }
        return new Macro { Steps = new List<MacroStep>(_steps) };
    }

    private void InstallHooks()
    {
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        var hMod = NativeMethods.GetModuleHandle(curModule.ModuleName);
        _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, hMod, 0);
        _mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _mouseProc, hMod, 0);
    }

    private void RemoveHooks()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }
        if (_mouseHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isRecording)
        {
            var hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();
            if (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN ||
                msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP)
            {
                AddDelayIfNeeded();
                var keyName = ((System.Windows.Forms.Keys)hookStruct.vkCode).ToString();
                var stepType = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN)
                    ? MacroStepType.KeyDown : MacroStepType.KeyUp;
                var step = new MacroStep { Type = stepType, Key = keyName };
                lock (_lock) { _steps.Add(step); }
                StepRecorded?.Invoke(step);
            }
        }
        return NativeMethods.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isRecording)
        {
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();
            MacroStepType? stepType = msg switch
            {
                NativeMethods.WM_LBUTTONDOWN => MacroStepType.MouseClick,
                NativeMethods.WM_RBUTTONDOWN => MacroStepType.MouseClick,
                NativeMethods.WM_MBUTTONDOWN => MacroStepType.MouseClick,
                _ => null
            };
            if (stepType.HasValue)
            {
                AddDelayIfNeeded();
                MouseButton button = msg switch
                {
                    NativeMethods.WM_RBUTTONDOWN => MouseButton.Right,
                    NativeMethods.WM_MBUTTONDOWN => MouseButton.Middle,
                    _ => MouseButton.Left
                };
                var step = new MacroStep
                {
                    Type = stepType.Value,
                    Button = button,
                    X = hookStruct.pt.X,
                    Y = hookStruct.pt.Y
                };
                lock (_lock) { _steps.Add(step); }
                StepRecorded?.Invoke(step);
            }
        }
        return NativeMethods.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    private void AddDelayIfNeeded()
    {
        long now = _stopwatch.ElapsedMilliseconds;
        long delta = now - _lastEventTime;
        if (_steps.Count > 0 && delta > 50)
        {
            var delay = new MacroStep { Type = MacroStepType.Delay, DelayMs = (int)delta };
            lock (_lock) { _steps.Add(delay); }
        }
        _lastEventTime = now;
    }

    public void Dispose()
    {
        RemoveHooks();
        GC.SuppressFinalize(this);
    }
}
