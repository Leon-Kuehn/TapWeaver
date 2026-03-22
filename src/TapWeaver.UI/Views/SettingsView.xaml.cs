using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TapWeaver.Core.Models;
using TapWeaver.UI.ViewModels;

namespace TapWeaver.UI.Views;

public partial class SettingsView : UserControl
{
    // Tracks which hotkey textbox is currently being captured
    private TextBox? _capturingBox;

    public SettingsView()
    {
        InitializeComponent();
    }

    // ── GotFocus handlers — show capture hint ─────────────────────────────

    private void PlaybackHotkeyBox_GotFocus(object sender, RoutedEventArgs e)
        => BeginCapture(PlaybackHotkeyBox);

    private void RecordingHotkeyBox_GotFocus(object sender, RoutedEventArgs e)
        => BeginCapture(RecordingHotkeyBox);

    private void AutoClickerHotkeyBox_GotFocus(object sender, RoutedEventArgs e)
        => BeginCapture(AutoClickerHotkeyBox);

    // ── Helpers ───────────────────────────────────────────────────────────

    private void BeginCapture(TextBox box)
    {
        _capturingBox = box;
        (DataContext as SettingsViewModel)?.BeginHotkeyCapture();
    }

    private void EndCapture()
    {
        _capturingBox = null;
        (DataContext as SettingsViewModel)?.EndHotkeyCapture();
    }

    private void HotkeyBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _capturingBox))
            EndCapture();
    }

    /// <summary>
    /// Attempts to interpret a KeyEventArgs as a complete hotkey combo
    /// (at least one modifier + one non-modifier key).
    /// Returns true and fills <paramref name="modifiers"/>/<paramref name="vk"/> on success.
    /// </summary>
    private static bool TryCaptureHotkey(KeyEventArgs e, out uint modifiers, out uint vk)
    {
        e.Handled = true;
        modifiers = 0;
        vk = 0;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore pure modifier key-presses
        if (key is Key.LeftCtrl  or Key.RightCtrl  or
                   Key.LeftAlt   or Key.RightAlt   or
                   Key.LeftShift or Key.RightShift or
                   Key.LWin      or Key.RWin       or
                   Key.Escape)
            return false;

        var wpfMods = Keyboard.Modifiers;

        // Require at least one modifier so the hotkey is safe
        if (wpfMods == ModifierKeys.None)
            return false;

        if ((wpfMods & ModifierKeys.Control) != 0) modifiers |= HotkeyConfig.MOD_CONTROL;
        if ((wpfMods & ModifierKeys.Alt)     != 0) modifiers |= HotkeyConfig.MOD_ALT;
        if ((wpfMods & ModifierKeys.Shift)   != 0) modifiers |= HotkeyConfig.MOD_SHIFT;

        vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        return vk != 0;
    }

    private void PlaybackHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            EndCapture();
            return;
        }

        if (TryCaptureHotkey(e, out uint mods, out uint vk))
        {
            (DataContext as SettingsViewModel)?.SetPlaybackHotkey(mods, vk);
            EndCapture();
        }
    }

    private void RecordingHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            EndCapture();
            return;
        }

        if (TryCaptureHotkey(e, out uint mods, out uint vk))
        {
            (DataContext as SettingsViewModel)?.SetRecordingHotkey(mods, vk);
            EndCapture();
        }
    }

    private void AutoClickerHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            EndCapture();
            return;
        }

        if (TryCaptureHotkey(e, out uint mods, out uint vk))
        {
            (DataContext as SettingsViewModel)?.SetAutoClickerHotkey(mods, vk);
            EndCapture();
        }
    }
}
