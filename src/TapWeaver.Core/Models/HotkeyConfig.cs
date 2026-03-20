using System.Windows.Forms;

namespace TapWeaver.Core.Models;

public class HotkeyConfig
{
    // Modifier constants for RegisterHotKey
    public const uint MOD_ALT      = 0x0001;
    public const uint MOD_CONTROL  = 0x0002;
    public const uint MOD_SHIFT    = 0x0004;
    public const uint MOD_WIN      = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    // Common virtual-key codes
    public const uint VK_SHIFT   = 0x10;
    public const uint VK_CONTROL = 0x11;
    public const uint VK_MENU    = 0x12;  // Alt
    public const uint VK_PAUSE   = 0x13;
    public const uint VK_F7      = 0x76;
    public const uint VK_F8      = 0x77;
    public const uint VK_F9      = 0x78;
    public const uint VK_LWIN    = 0x5B;
    public const uint VK_RWIN    = 0x5C;

    public uint Modifiers  { get; set; }
    public uint VirtualKey { get; set; }

    public bool IsSet => VirtualKey != 0;

    public string DisplayText
    {
        get
        {
            if (!IsSet) return "(none)";
            var parts = new List<string>();
            if ((Modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((Modifiers & MOD_ALT)     != 0) parts.Add("Alt");
            if ((Modifiers & MOD_SHIFT)   != 0) parts.Add("Shift");
            if ((Modifiers & MOD_WIN)     != 0) parts.Add("Win");
            parts.Add(((Keys)VirtualKey).ToString());
            return string.Join("+", parts);
        }
    }

    public HotkeyConfig Clone() => new() { Modifiers = Modifiers, VirtualKey = VirtualKey };
}
