using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TapWeaver.Core.Interop;

namespace TapWeaver.Core.Input;

/// <summary>
/// Sends keyboard messages directly to a target window handle.
/// </summary>
public sealed class WindowInputSender
{
    /// <summary>
    /// Sends key down/up messages to a non-focused window.
    /// </summary>
    /// <remarks>
    /// This approach works well with many desktop apps, but some games use
    /// Raw Input, DirectInput, anti-cheat, or custom input loops and may ignore
    /// WM_KEYDOWN/WM_KEYUP messages entirely.
    /// </remarks>
    public void SendKey(IntPtr hWnd, Keys key, int repeatCount = 1)
    {
        if (repeatCount < 1)
            repeatCount = 1;

        for (int i = 0; i < repeatCount; i++)
        {
            SendKeyDown(hWnd, key);
            SendKeyUp(hWnd, key);
        }
    }

    public void SendKeyDown(IntPtr hWnd, Keys key)
    {
        EnsureTargetWindow(hWnd);
        ushort vkCode = (ushort)key;
        int scanCode = (int)NativeMethods.MapVirtualKey(vkCode, NativeMethods.MAPVK_VK_TO_VSC) & 0xFF;
        int lParam = BuildLParam(repeat: 1, scanCode, isExtended: IsExtendedKey(vkCode), keyUp: false);

        if (!NativeMethods.PostMessage(hWnd, NativeMethods.WM_KEYDOWN, (IntPtr)vkCode, (IntPtr)lParam))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to post WM_KEYDOWN to target window.");
    }

    public void SendKeyUp(IntPtr hWnd, Keys key)
    {
        EnsureTargetWindow(hWnd);
        ushort vkCode = (ushort)key;
        int scanCode = (int)NativeMethods.MapVirtualKey(vkCode, NativeMethods.MAPVK_VK_TO_VSC) & 0xFF;
        int lParam = BuildLParam(repeat: 1, scanCode, isExtended: IsExtendedKey(vkCode), keyUp: true);

        if (!NativeMethods.PostMessage(hWnd, NativeMethods.WM_KEYUP, (IntPtr)vkCode, (IntPtr)lParam))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to post WM_KEYUP to target window.");
    }

    public bool TrySendKey(IntPtr hWnd, Keys key, int repeatCount = 1)
    {
        try
        {
            SendKey(hWnd, key, repeatCount);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySendKeyDown(IntPtr hWnd, Keys key)
    {
        try
        {
            SendKeyDown(hWnd, key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySendKeyUp(IntPtr hWnd, Keys key)
    {
        try
        {
            SendKeyUp(hWnd, key);
            return true;
        }
        catch
        {
            return false;
        }
    }

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

        if (isExtended)
            lParam |= 1 << 24;

        if (keyUp)
        {
            lParam |= 1 << 30;
            lParam |= 1 << 31;
        }

        return lParam;
    }

    private static bool IsExtendedKey(ushort vkCode) => vkCode switch
    {
        0x21 or // PageUp
        0x22 or // PageDown
        0x23 or // End
        0x24 or // Home
        0x25 or // Left
        0x26 or // Up
        0x27 or // Right
        0x28 or // Down
        0x2D or // Insert
        0x2E or // Delete
        0x5B or // Left Win
        0x5C or // Right Win
        0x5D or // Apps
        0x6F or // Numpad /
        0x90 or // NumLock
        0xA3 or // Right Control
        0xA5 => true, // Right Alt (AltGr)
        _ => false
    };
}
