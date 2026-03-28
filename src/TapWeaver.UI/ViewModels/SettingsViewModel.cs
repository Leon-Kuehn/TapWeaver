namespace TapWeaver.UI.ViewModels;

/// <summary>
/// ViewModel for the Settings tab.  Most properties delegate to <see cref="MainViewModel"/>
/// so that the single source of truth for settings and hotkey registration lives there.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly RecorderViewModel _recorder;

    public SettingsViewModel(MainViewModel main)
    {
        _main = main;
        _recorder = _main.Recorder;

        // Propagate hotkey text changes when MainViewModel notifies them
        _main.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.PlaybackHotkeyText):
                    OnPropertyChanged(nameof(PlaybackHotkeyText)); break;
                case nameof(MainViewModel.RecordingHotkeyText):
                    OnPropertyChanged(nameof(RecordingHotkeyText)); break;
                case nameof(MainViewModel.AutoClickerHotkeyText):
                    OnPropertyChanged(nameof(AutoClickerHotkeyText)); break;
                case nameof(MainViewModel.AlwaysOnTop):
                    OnPropertyChanged(nameof(AlwaysOnTop)); break;
                case nameof(MainViewModel.UseDarkMode):
                    OnPropertyChanged(nameof(UseDarkMode)); break;
                case nameof(MainViewModel.CompactMode):
                    OnPropertyChanged(nameof(CompactMode)); break;
            }
        };

        _recorder.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(RecorderViewModel.RecordKeyboardEvents):
                    OnPropertyChanged(nameof(RecordKeyboardEvents)); break;
                case nameof(RecorderViewModel.RecordMouseClickEvents):
                    OnPropertyChanged(nameof(RecordMouseClickEvents)); break;
                case nameof(RecorderViewModel.RecordMouseMoveEvents):
                    OnPropertyChanged(nameof(RecordMouseMoveEvents)); break;
            }
        };

        ResetPlaybackHotkeyCommand    = new RelayCommand(_main.ResetPlaybackHotkeyToDefault);
        ResetRecordingHotkeyCommand   = new RelayCommand(_main.ResetRecordingHotkeyToDefault);
        ResetAutoClickerHotkeyCommand = new RelayCommand(_main.ResetAutoClickerHotkeyToDefault);
        EmergencyStopCommand          = new RelayCommand(_main.EmergencyStop);
    }

    // ── General ──────────────────────────────────────────────────────────────

    public bool AlwaysOnTop
    {
        get => _main.AlwaysOnTop;
        set => _main.AlwaysOnTop = value;
    }

    public bool UseDarkMode
    {
        get => _main.UseDarkMode;
        set => _main.UseDarkMode = value;
    }

    public bool CompactMode
    {
        get => _main.CompactMode;
        set => _main.CompactMode = value;
    }

    public bool RecordKeyboardEvents
    {
        get => _recorder.RecordKeyboardEvents;
        set => _recorder.RecordKeyboardEvents = value;
    }

    public bool RecordMouseClickEvents
    {
        get => _recorder.RecordMouseClickEvents;
        set => _recorder.RecordMouseClickEvents = value;
    }

    public bool RecordMouseMoveEvents
    {
        get => _recorder.RecordMouseMoveEvents;
        set => _recorder.RecordMouseMoveEvents = value;
    }

    // ── Hotkey display ────────────────────────────────────────────────────────

    public string PlaybackHotkeyText    => _main.PlaybackHotkeyText;
    public string RecordingHotkeyText   => _main.RecordingHotkeyText;
    public string AutoClickerHotkeyText => _main.AutoClickerHotkeyText;
    public string EmergencyStopHotkeyText => MainViewModel.EmergencyStopHotkeyText;

    // ── Hotkey capture relay ──────────────────────────────────────────────────

    public void SetPlaybackHotkey(uint modifiers, uint vk)
        => _main.SetPlaybackHotkey(modifiers, vk);

    public void SetRecordingHotkey(uint modifiers, uint vk)
        => _main.SetRecordingHotkey(modifiers, vk);

    public void SetAutoClickerHotkey(uint modifiers, uint vk)
        => _main.SetAutoClickerHotkey(modifiers, vk);

    // ── Commands ──────────────────────────────────────────────────────────────

    public RelayCommand ResetPlaybackHotkeyCommand    { get; }
    public RelayCommand ResetRecordingHotkeyCommand   { get; }
    public RelayCommand ResetAutoClickerHotkeyCommand { get; }

    /// <summary>Manual emergency-stop button (same as the hotkey).</summary>
    public RelayCommand EmergencyStopCommand { get; }

    public void BeginHotkeyCapture()
        => _main.SetHotkeyCaptureActive(true);

    public void EndHotkeyCapture()
        => _main.SetHotkeyCaptureActive(false);
}
