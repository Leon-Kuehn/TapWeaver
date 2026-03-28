using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TapWeaver.Core.Interop;

namespace TapWeaver.Core.Input;

/// <summary>
/// Sends keyboard input to a specific target window, optionally without stealing focus permanently.
/// </summary>
public sealed class WindowInputSender
{
    /// <summary>
    /// Gets or sets the strategy used to deliver input to the target window.
    /// Default is <see cref="WindowInputStrategy.FocusSwitch"/> which is the most reliable approach
    /// for standard desktop applications.
    /// </summary>
    public WindowInputStrategy Strategy { get; set; } = WindowInputStrategy.FocusSwitch;

    /// <summary>
    /// Delay in milliseconds after switching focus to the target window before sending input.
    /// Increase if the target app is slow to respond to focus changes.
    /// </summary>
    public int FocusSwitchDelayMs { get; set; } = 25;

    /// <summary>
    /// Delay in milliseconds after restoring focus back to the original window.
    /// </summary>
    public int FocusRestoreDelayMs { get; set; } = 25;

    /// <summary>
    /// Sends a key press (down + up) to the target window handle, repeated <paramref name="repeatCount"/> times.
    /// </remarks>
    public void SendKey(IntPtr hWnd, Keys key, int repeatCount = 1)
    {
        EnsureTargetWindow(hWnd);
        if (repeatCount < 1)
            repeatCount = 1;

        for (int i = 0; i < repeatCount; i++)
        {
            SendKeyDown(hWnd, key);
            SendKeyUp(hWnd, key);
        }
    }

    /// <summary>
    /// Sends a WM_KEYDOWN or equivalent to the target window.
    /// </summary>
    public void SendKeyDown(IntPtr hWnd, Keys key)
    {
        EnsureTargetWindow(hWnd);
        if (Strategy == WindowInputStrategy.FocusSwitch)
            SendKeyDownViaFocusSwitch(hWnd, key);
        else
            SendKeyDownViaPostMessage(hWnd, key);
    }

    /// <summary>
    /// Sends a WM_KEYUP or equivalent to the target window.
    /// </summary>
    public void SendKeyUp(IntPtr hWnd, Keys key)
    {
        EnsureTargetWindow(hWnd);
        if (Strategy == WindowInputStrategy.FocusSwitch)
            SendKeyUpViaFocusSwitch(hWnd, key);
        else
            SendKeyUpViaPostMessage(hWnd, key);
    }

    public bool TrySendKey(IntPtr hWnd, Keys key, int repeatCount = 1)
    {
        try { SendKey(hWnd, key, repeatCount); return true; }
        catch { return false; }
    }

    public bool TrySendKeyDown(IntPtr hWnd, Keys key)
    {
        try { SendKeyDown(hWnd, key); return true; }
        catch { return false; }
    }

    public bool TrySendKeyUp(IntPtr hWnd, Keys key)
    {
        try { SendKeyUp(hWnd, key); return true; }
        catch { return false; }
    }

    // -- FocusSwitch implementation --------------------------------------------

    private void SendKeyDownViaFocusSwitch(IntPtr hWnd, Keys key)
    {
        var (prevForeground, targetThread, currentThread) = PrepareFocusSwitch(hWnd);
        try
        {
            InputSimulator.SendKeyDown((ushort)key);
        }
        finally
        {
            RestoreFocus(prevForeground, targetThread, currentThread);
        }
    }

    private void SendKeyUpViaFocusSwitch(IntPtr hWnd, Keys key)
    {
        var (prevForeground, targetThread, currentThread) = PrepareFocusSwitch(hWnd);
        try
        {
            InputSimulator.SendKeyUp((ushort)key);
        }
        finally
        {
            RestoreFocus(prevForeground, targetThread, currentThread);
        }
    }

    private (IntPtr prevForeground, uint targetThread, uint currentThread) PrepareFocusSwitch(IntPtr hWnd)
    {
        IntPtr prevForeground = NativeMethods.GetForegroundWindow();
        uint targetThread = NativeMethods.GetWindowThreadProcessId(hWnd, out _);
        uint currentThread = NativeMethods.GetCurrentThreadId();

        NativeMethods.AttachThreadInput(currentThread, targetThread, true);
        NativeMethods.SetForegroundWindow(hWnd);
        Thread.Sleep(FocusSwitchDelayMs);

        return (prevForeground, targetThread, currentThread);
    }

    private void RestoreFocus(IntPtr prevForeground, uint targetThread, uint currentThread)
    {
        NativeMethods.AttachThreadInput(currentThread, targetThread, false);
        if (prevForeground != IntPtr.Zero)
            NativeMethods.SetForegroundWindow(prevForeground);
        Thread.Sleep(FocusRestoreDelayMs);
    }

    // -- PostMessage implementation --------------------------------------------

    private static void SendKeyDownViaPostMessage(IntPtr hWnd, Keys key)
    {
        ushort vkCode = (ushort)key;
        int scanCode = (int)NativeMethods.MapVirtualKey(vkCode, NativeMethods.MAPVK_VK_TO_VSC) & 0xFF;
        int lParam = BuildLParam(1, scanCode, IsExtendedKey(vkCode), keyUp: false);
        if (!NativeMethods.PostMessage(hWnd, NativeMethods.WM_KEYDOWN, (IntPtr)vkCode, (IntPtr)lParam))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to post WM_KEYDOWN.");
    }

    private static void SendKeyUpViaPostMessage(IntPtr hWnd, Keys key)
    {
        ushort vkCode = (ushort)key;
        int scanCode = (int)NativeMethods.MapVirtualKey(vkCode, NativeMethods.MAPVK_VK_TO_VSC) & 0xFF;
        int lParam = BuildLParam(1, scanCode, IsExtendedKey(vkCode), keyUp: true);
        if (!NativeMethods.PostMessage(hWnd, NativeMethods.WM_KEYUP, (IntPtr)vkCode, (IntPtr)lParam))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to post WM_KEYUP.");
    }

    // -- Shared helpers --------------------------------------------------------

    private static void EnsureTargetWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            throw new ArgumentException("Target window handle must not be zero.", nameof(hWnd));

        if (!NativeMethods.IsWindow(hWnd))
            throw new InvalidOperationException("Target window handle is no longer valid.");
    }

    private static int BuildLParam(int repeat, int scanCode, bool isExtended, bool keyUp)
    {
        int lParam = repeat & 0xFFFF;
        lParam |= (scanCode & 0xFF) << 16;

        if (isExtended) lParam |= 1 << 24;
        if (keyUp) { lParam |= 1 << 30; lParam |= 1 << 31; }
        return lParam;
    }

    private static bool IsExtendedKey(ushort vk) => vk switch
    {
        0x21 or 0x22 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x28 or
        0x2D or 0x2E or 0x5B or 0x5C or 0x5D or 0x6F or 0x90 or 0xA3 or 0xA5 => true,
        _ => false
    };
}
