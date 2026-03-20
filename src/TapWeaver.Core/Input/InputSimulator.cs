using System.Runtime.InteropServices;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Input;

public static class InputSimulator
{
    public static void SendKeyDown(ushort vkCode)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.INPUT_UNION
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = vkCode,
                    wScan = 0,
                    dwFlags = 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    public static void SendKeyUp(ushort vkCode)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.INPUT_UNION
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = vkCode,
                    wScan = 0,
                    dwFlags = NativeMethods.KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }

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
