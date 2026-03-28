using System.Text;
using TapWeaver.Core.Interop;

namespace TapWeaver.Core.Input;

/// <summary>
/// Enumerates currently open, visible top-level windows.
/// </summary>
public sealed class WindowSelector
{
    /// <summary>
    /// Returns a snapshot of visible top-level windows with non-empty titles.
    /// </summary>
    public IReadOnlyDictionary<IntPtr, string> GetOpenWindows()
    {
        var windows = new Dictionary<IntPtr, string>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (!NativeMethods.IsWindowVisible(hWnd))
                return true;

            int titleLength = NativeMethods.GetWindowTextLength(hWnd);
            if (titleLength <= 0)
                return true;

            var titleBuffer = new StringBuilder(titleLength + 1);
            _ = NativeMethods.GetWindowText(hWnd, titleBuffer, titleBuffer.Capacity);
            var title = titleBuffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(title))
                return true;

            windows[hWnd] = title;
            return true;
        }, IntPtr.Zero);

        // Stable ordering makes the UI list deterministic and easier to use.
        return windows
            .OrderBy(kvp => kvp.Value, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public bool IsWindowValid(IntPtr hWnd)
        => hWnd != IntPtr.Zero && NativeMethods.IsWindow(hWnd);
}
