using System.Windows.Forms;

namespace TapWeaver.Core.Input;

public static class KeyboardKeyMap
{
    private static readonly Dictionary<string, ushort> NameToVk = new(StringComparer.OrdinalIgnoreCase);

    static KeyboardKeyMap()
    {
        // Build from the WinForms Keys enum so mapping stays aligned with recorder key names.
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            var keyCode = key & Keys.KeyCode;
            var vk = (ushort)keyCode;
            if (vk == 0 || vk > 0xFE)
                continue;

            var name = key.ToString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (!NameToVk.ContainsKey(name))
                NameToVk[name] = vk;
        }

        AddAlias("Ctrl", 0x11);
        AddAlias("Alt", 0x12);
        AddAlias("Win", 0x5B);
        AddAlias("Esc", 0x1B);
        AddAlias("Del", 0x2E);
        AddAlias("PgUp", 0x21);
        AddAlias("PgDn", 0x22);
        AddAlias("Space", 0x20);

        AvailableKeyNames = BuildAvailableKeyNames();
    }

    public static IReadOnlyList<string> AvailableKeyNames { get; }

    public static bool TryGetVirtualKey(string keyName, out ushort vkCode)
    {
        if (string.IsNullOrWhiteSpace(keyName))
        {
            vkCode = 0;
            return false;
        }

        if (NameToVk.TryGetValue(keyName.Trim(), out vkCode))
            return true;

        var normalized = keyName.Trim();
        if (normalized.Length == 1)
        {
            vkCode = (ushort)char.ToUpperInvariant(normalized[0]);
            return true;
        }

        vkCode = 0;
        return false;
    }

    private static IReadOnlyList<string> BuildAvailableKeyNames()
    {
        var keys = NameToVk.Keys
            .Where(static k => !k.StartsWith("Mod", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return keys;
    }

    private static void AddAlias(string alias, ushort vk)
    {
        if (!NameToVk.ContainsKey(alias))
            NameToVk[alias] = vk;
    }
}
