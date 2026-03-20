using TapWeaver.Core.Input;
using TapWeaver.Core.Models;
using TapWeaver.Core.Services;
using TapWeaver.Persistence;

namespace TapWeaver.UI.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    public RecorderViewModel  Recorder   { get; }
    public SequencerViewModel Sequencer  { get; }
    public AutoClickerViewModel AutoClicker { get; }
    public SettingsViewModel  Settings   { get; }

    private readonly MacroRecorder      _macroRecorder;
    private readonly MacroPlayer        _macroPlayer;
    private readonly AutoClickerService _autoClickerService;
    private readonly AppSettings        _appSettings;

    private HotkeyService? _hotkeyService;
    private int _playbackHotkeyId    = -1;
    private int _recordingHotkeyId   = -1;
    private int _autoClickerHotkeyId = -1;
    private int _emergencyStopHotkeyId = -1;

    // ── Always-on-top ────────────────────────────────────────────────────────

    public bool AlwaysOnTop
    {
        get => _appSettings.AlwaysOnTop;
        set
        {
            if (_appSettings.AlwaysOnTop == value) return;
            _appSettings.AlwaysOnTop = value;
            OnPropertyChanged();
            AppSettingsSerializer.Save(_appSettings);
        }
    }

    // ── Hotkey display strings ────────────────────────────────────────────────

    public string PlaybackHotkeyText
        => _appSettings.PlaybackToggleHotkey.DisplayText;

    public string RecordingHotkeyText
        => _appSettings.RecordingToggleHotkey.DisplayText;

    public string AutoClickerHotkeyText
        => _appSettings.AutoClickerToggleHotkey.DisplayText;

    public static string EmergencyStopHotkeyText
        => AppSettings.FixedEmergencyStop.DisplayText;

    // ── Constructor ──────────────────────────────────────────────────────────

    public MainViewModel()
    {
        _appSettings = AppSettingsSerializer.Load();

        _macroRecorder      = new MacroRecorder();
        _macroPlayer        = new MacroPlayer();
        _autoClickerService = new AutoClickerService();

        Recorder    = new RecorderViewModel(_macroRecorder);
        Sequencer   = new SequencerViewModel(_macroPlayer);
        AutoClicker = new AutoClickerViewModel(_autoClickerService);
        Settings    = new SettingsViewModel(this);

        Recorder.RecordingComplete += macro => Sequencer.LoadMacro(macro);
    }

    // ── Hotkey infrastructure ─────────────────────────────────────────────────

    /// <summary>
    /// Called by MainWindow once the HWND is available to register global hotkeys.
    /// </summary>
    public void InitializeHotkeys(IntPtr hwnd)
    {
        _hotkeyService = new HotkeyService(hwnd);
        RegisterHotkeys();
    }

    /// <summary>Dispatches a WM_HOTKEY message to the HotkeyService.</summary>
    public void HandleHotkey(int id)
    {
        _hotkeyService?.HandleHotkey(id);
    }

    /// <summary>
    /// Re-registers all hotkeys from current AppSettings.
    /// Called after the user changes a hotkey in the Settings tab.
    /// </summary>
    public void ReregisterHotkeys()
    {
        RegisterHotkeys();
        OnPropertyChanged(nameof(PlaybackHotkeyText));
        OnPropertyChanged(nameof(RecordingHotkeyText));
        OnPropertyChanged(nameof(AutoClickerHotkeyText));
    }

    private void RegisterHotkeys()
    {
        if (_hotkeyService == null) return;

        // Unregister previously registered hotkeys
        _hotkeyService.Unregister(_playbackHotkeyId);    _playbackHotkeyId    = -1;
        _hotkeyService.Unregister(_recordingHotkeyId);   _recordingHotkeyId   = -1;
        _hotkeyService.Unregister(_autoClickerHotkeyId); _autoClickerHotkeyId = -1;
        _hotkeyService.Unregister(_emergencyStopHotkeyId); _emergencyStopHotkeyId = -1;

        // Fixed emergency stop (Ctrl+Alt+Pause) — always registered
        var emergency = AppSettings.FixedEmergencyStop;
        _emergencyStopHotkeyId = _hotkeyService.Register(
            emergency.Modifiers | HotkeyConfig.MOD_NOREPEAT,
            emergency.VirtualKey,
            EmergencyStop);

        // Configurable hotkeys — require at least one modifier to be safe
        var s = _appSettings;

        if (s.PlaybackToggleHotkey.IsSet)
            _playbackHotkeyId = _hotkeyService.Register(
                s.PlaybackToggleHotkey.Modifiers | HotkeyConfig.MOD_NOREPEAT,
                s.PlaybackToggleHotkey.VirtualKey,
                TogglePlayback);

        if (s.RecordingToggleHotkey.IsSet)
            _recordingHotkeyId = _hotkeyService.Register(
                s.RecordingToggleHotkey.Modifiers | HotkeyConfig.MOD_NOREPEAT,
                s.RecordingToggleHotkey.VirtualKey,
                ToggleRecording);

        if (s.AutoClickerToggleHotkey.IsSet)
            _autoClickerHotkeyId = _hotkeyService.Register(
                s.AutoClickerToggleHotkey.Modifiers | HotkeyConfig.MOD_NOREPEAT,
                s.AutoClickerToggleHotkey.VirtualKey,
                _autoClickerService.Toggle);
    }

    // ── Hotkey actions ────────────────────────────────────────────────────────

    private void TogglePlayback()
    {
        if (_macroPlayer.IsPlaying)
            _macroPlayer.Stop();
        else
            Sequencer.PlayCommand.Execute(null);
    }

    private void ToggleRecording()
    {
        if (_macroRecorder.IsRecording)
            Recorder.StopRecordingCommand.Execute(null);
        else
            Recorder.StartRecordingCommand.Execute(null);
    }

    /// <summary>
    /// Emergency stop: immediately halts all activity and releases held keys/mouse.
    /// Triggered by the fixed Ctrl+Alt+Pause hotkey (and optionally via the UI button).
    /// </summary>
    public void EmergencyStop()
    {
        _macroPlayer.Stop(PlaybackStopReason.EmergencyStop);
        _autoClickerService.Stop();
        if (_macroRecorder.IsRecording)
            _macroRecorder.Stop();

        ReleaseAllInputs();
    }

    private static void ReleaseAllInputs()
    {
        // Release common modifier keys that could be stuck
        ushort[] keysToRelease =
        {
            (ushort)HotkeyConfig.VK_SHIFT,
            (ushort)HotkeyConfig.VK_CONTROL,
            (ushort)HotkeyConfig.VK_MENU,
            (ushort)HotkeyConfig.VK_LWIN,
            (ushort)HotkeyConfig.VK_RWIN
        };
        foreach (var vk in keysToRelease)
            InputSimulator.SendKeyUp(vk);

        // Release all mouse buttons
        InputSimulator.SendMouseButtonUp(MouseButton.Left);
        InputSimulator.SendMouseButtonUp(MouseButton.Right);
        InputSimulator.SendMouseButtonUp(MouseButton.Middle);
    }

    // ── Hotkey configuration helpers (called from SettingsViewModel) ──────────

    public void SetPlaybackHotkey(uint modifiers, uint vk)
    {
        if (vk == 0) return;
        _appSettings.PlaybackToggleHotkey.Modifiers  = modifiers;
        _appSettings.PlaybackToggleHotkey.VirtualKey = vk;
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    public void SetRecordingHotkey(uint modifiers, uint vk)
    {
        if (vk == 0) return;
        _appSettings.RecordingToggleHotkey.Modifiers  = modifiers;
        _appSettings.RecordingToggleHotkey.VirtualKey = vk;
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    public void SetAutoClickerHotkey(uint modifiers, uint vk)
    {
        if (vk == 0) return;
        _appSettings.AutoClickerToggleHotkey.Modifiers  = modifiers;
        _appSettings.AutoClickerToggleHotkey.VirtualKey = vk;
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    public void ResetPlaybackHotkeyToDefault()
    {
        _appSettings.PlaybackToggleHotkey = new HotkeyConfig
            { Modifiers = HotkeyConfig.MOD_CONTROL, VirtualKey = HotkeyConfig.VK_F8 };
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    public void ResetRecordingHotkeyToDefault()
    {
        _appSettings.RecordingToggleHotkey = new HotkeyConfig
            { Modifiers = HotkeyConfig.MOD_CONTROL, VirtualKey = HotkeyConfig.VK_F7 };
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    public void ResetAutoClickerHotkeyToDefault()
    {
        _appSettings.AutoClickerToggleHotkey = new HotkeyConfig
            { Modifiers = HotkeyConfig.MOD_CONTROL, VirtualKey = HotkeyConfig.VK_F9 };
        AppSettingsSerializer.Save(_appSettings);
        ReregisterHotkeys();
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        _hotkeyService?.Dispose();
        _macroRecorder.Dispose();
        GC.SuppressFinalize(this);
    }
}
