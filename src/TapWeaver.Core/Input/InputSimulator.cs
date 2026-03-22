using System.Runtime.InteropServices;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Input;

public static class InputSimulator
{
    public static void SendKeyDown(ushort vkCode)
    {
        var scanCode = (ushort)NativeMethods.MapVirtualKey(vkCode, 0);
        var flags = scanCode == 0 ? 0u : NativeMethods.KEYEVENTF_SCANCODE;
        if (IsExtendedKey(vkCode))
            flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.INPUT_UNION
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = scanCode == 0 ? vkCode : (ushort)0,
                    wScan = scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    public static void SendKeyUp(ushort vkCode)
    {
        var scanCode = (ushort)NativeMethods.MapVirtualKey(vkCode, 0);
        var flags = (scanCode == 0 ? 0u : NativeMethods.KEYEVENTF_SCANCODE) | NativeMethods.KEYEVENTF_KEYUP;
        if (IsExtendedKey(vkCode))
            flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.INPUT_UNION
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = scanCode == 0 ? vkCode : (ushort)0,
                    wScan = scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
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

    public static void SendMouseClick(MouseButton button, int? x = null, int? y = null)
    {
        if (x.HasValue && y.HasValue)
        {
            SendMouseMove(x.Value, y.Value);
        }

        uint downFlag, upFlag;
        switch (button)
        {
            case MouseButton.Right:
                downFlag = NativeMethods.MOUSEEVENTF_RIGHTDOWN;
                upFlag = NativeMethods.MOUSEEVENTF_RIGHTUP;
                break;
            case MouseButton.Middle:
                downFlag = NativeMethods.MOUSEEVENTF_MIDDLEDOWN;
                upFlag = NativeMethods.MOUSEEVENTF_MIDDLEUP;
                break;
            default:
                downFlag = NativeMethods.MOUSEEVENTF_LEFTDOWN;
                upFlag = NativeMethods.MOUSEEVENTF_LEFTUP;
                break;
        }

        var inputs = new NativeMethods.INPUT[]
        {
            new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_MOUSE,
                u = new NativeMethods.INPUT_UNION { mi = new NativeMethods.MOUSEINPUT { dwFlags = downFlag } }
            },
            new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_MOUSE,
                u = new NativeMethods.INPUT_UNION { mi = new NativeMethods.MOUSEINPUT { dwFlags = upFlag } }
            }
        };
        NativeMethods.SendInput(2, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    public static void SendMouseMove(int x, int y)
    {
        int screenWidth = NativeMethods.GetSystemMetrics(0);
        int screenHeight = NativeMethods.GetSystemMetrics(1);
        int normalizedX = (int)((x * 65535.0) / (screenWidth - 1));
        int normalizedY = (int)((y * 65535.0) / (screenHeight - 1));

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            u = new NativeMethods.INPUT_UNION
            {
                mi = new NativeMethods.MOUSEINPUT
                {
                    dx = normalizedX,
                    dy = normalizedY,
                    dwFlags = NativeMethods.MOUSEEVENTF_MOVE | NativeMethods.MOUSEEVENTF_ABSOLUTE
                }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }
    
    public static void SendMouseButtonDown(MouseButton button)
    {
        uint downFlag = button switch
        {
            MouseButton.Right => NativeMethods.MOUSEEVENTF_RIGHTDOWN,
            MouseButton.Middle => NativeMethods.MOUSEEVENTF_MIDDLEDOWN,
            _ => NativeMethods.MOUSEEVENTF_LEFTDOWN
        };
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            u = new NativeMethods.INPUT_UNION { mi = new NativeMethods.MOUSEINPUT { dwFlags = downFlag } }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    public static void SendMouseButtonUp(MouseButton button)
    {
        uint upFlag = button switch
        {
            MouseButton.Right => NativeMethods.MOUSEEVENTF_RIGHTUP,
            MouseButton.Middle => NativeMethods.MOUSEEVENTF_MIDDLEUP,
            _ => NativeMethods.MOUSEEVENTF_LEFTUP
        };
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            u = new NativeMethods.INPUT_UNION { mi = new NativeMethods.MOUSEINPUT { dwFlags = upFlag } }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
