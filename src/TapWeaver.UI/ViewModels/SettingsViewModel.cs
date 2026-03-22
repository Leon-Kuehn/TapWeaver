namespace TapWeaver.UI.ViewModels;

/// <summary>
/// ViewModel for the Settings tab.  Most properties delegate to <see cref="MainViewModel"/>
/// so that the single source of truth for settings and hotkey registration lives there.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public SettingsViewModel(MainViewModel main)
    {
        _main = main;

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
}
