using System.Collections.ObjectModel;
using System.Windows;
using TapWeaver.Core.Input;
using TapWeaver.Core.Models;
using TapWeaver.Core.Services;
using TapWeaver.Persistence;
using Microsoft.Win32;

namespace TapWeaver.UI.ViewModels;

/// <summary>
/// Sequencer view model for editing, saving and executing macro step timelines.
/// </summary>
public class SequencerViewModel : ViewModelBase
{
    private readonly MacroPlayer _player;
    private readonly WindowSelector _windowSelector = new();
    private string _macroName = "New Macro";
    private string _profileDescription = "";
    private RepeatMode _repeatMode = RepeatMode.Once;
    private int _repeatCount = 1;
    private int _loopDelayMs = 0;
    private bool _highPrecisionTiming = true;
    private string? _currentFilePath;
    private DateTime _profileCreated = DateTime.UtcNow;
    private int _currentStepIndex = -1;
    private string _status = "Idle";
    private string _windowRoutingInfo = "Keyboard input routing disabled.";
    private bool _routeInputToSelectedWindow;
    private IntPtr _targetWindowHandle = IntPtr.Zero;
    private WindowOption? _selectedWindow;

    public ObservableCollection<MacroStepViewModel> Steps { get; } = new();
    public ObservableCollection<WindowOption> OpenWindows { get; } = new();
    public MacroStepViewModel? SelectedStep
    {
        get => _selectedStep;
        set => SetProperty(ref _selectedStep, value);
    }
    private MacroStepViewModel? _selectedStep;

    public string MacroName { get => _macroName; set => SetProperty(ref _macroName, value); }
    public string ProfileDescription { get => _profileDescription; set => SetProperty(ref _profileDescription, value); }
    public RepeatMode RepeatMode { get => _repeatMode; set => SetProperty(ref _repeatMode, value); }
    public int RepeatCount { get => _repeatCount; set => SetProperty(ref _repeatCount, value); }
    public int LoopDelayMs { get => _loopDelayMs; set => SetProperty(ref _loopDelayMs, value); }
    public bool HighPrecisionTiming { get => _highPrecisionTiming; set => SetProperty(ref _highPrecisionTiming, value); }
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public string WindowRoutingInfo { get => _windowRoutingInfo; set => SetProperty(ref _windowRoutingInfo, value); }
    public bool IsPlaying => _player.IsPlaying;

    public bool RouteInputToSelectedWindow
    {
        get => _routeInputToSelectedWindow;
        set
        {
            if (_routeInputToSelectedWindow == value)
                return;

            if (value && _targetWindowHandle == IntPtr.Zero)
            {
                WindowRoutingInfo = "Select a target window before enabling routing.";
                OnPropertyChanged();
                return;
            }

            if (value && !_windowSelector.IsWindowValid(_targetWindowHandle))
            {
                InvalidateTargetWindow("Target window no longer exists. Routing disabled.");
                return;
            }

            _routeInputToSelectedWindow = value;
            OnPropertyChanged();
            SyncWindowRoutingToPlayer();

            WindowRoutingInfo = value
                ? $"Routing keyboard input to: {_selectedWindow?.Title ?? "selected window"}"
                : "Keyboard input routing disabled.";
        }
    }

    public IntPtr TargetWindowHandle
    {
        get => _targetWindowHandle;
        private set
        {
            if (!SetProperty(ref _targetWindowHandle, value))
                return;

            SyncWindowRoutingToPlayer();
        }
    }

    public WindowOption? SelectedWindow
    {
        get => _selectedWindow;
        set
        {
            if (!SetProperty(ref _selectedWindow, value))
                return;

            TargetWindowHandle = value?.Handle ?? IntPtr.Zero;

            if (TargetWindowHandle == IntPtr.Zero && RouteInputToSelectedWindow)
            {
                _routeInputToSelectedWindow = false;
                OnPropertyChanged(nameof(RouteInputToSelectedWindow));
                WindowRoutingInfo = "Keyboard input routing disabled.";
            }
            else if (TargetWindowHandle != IntPtr.Zero && RouteInputToSelectedWindow)
            {
                WindowRoutingInfo = $"Routing keyboard input to: {value?.Title}";
            }
        }
    }

    /// <summary>True when the sequencer is not running; controls whether the editor UI is enabled.</summary>
    public bool IsEditable => !_player.IsPlaying;
    
    public int CurrentStepIndex
    {
        get => _currentStepIndex;
        set
        {
            if (_currentStepIndex >= 0 && _currentStepIndex < Steps.Count)
                Steps[_currentStepIndex].IsHighlighted = false;
            SetProperty(ref _currentStepIndex, value);
            if (value >= 0 && value < Steps.Count)
                Steps[value].IsHighlighted = true;
        }
    }

    public IEnumerable<MacroStepType> StepTypes => Enum.GetValues<MacroStepType>();
    public IEnumerable<RepeatMode> RepeatModes => Enum.GetValues<RepeatMode>();

    public RelayCommand PlayCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand AddStepCommand { get; }
    public RelayCommand DeleteStepCommand { get; }
    public RelayCommand DuplicateStepCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand RefreshWindowsCommand { get; }

    private static readonly string DefaultStepKey = KeyboardKeyMap.AvailableKeyNames.FirstOrDefault() ?? "A";
    private const int DefaultStepHoldMs = 100;

    public SequencerViewModel(MacroPlayer player, AppSettings settings)
    {
        _player = player;
        _routeInputToSelectedWindow = settings.RouteInputToSelectedWindow;
        _targetWindowHandle = new IntPtr(settings.TargetWindowHandle);

        _player.StepChanged += idx => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            CurrentStepIndex = idx;
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsEditable));
        });
        _player.PlaybackStarted += () => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            Status = "Running…";
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsEditable));
        });
        _player.PlaybackStopped += reason => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            Status = reason switch
            {
                PlaybackStopReason.Completed     => "Idle",
                PlaybackStopReason.UserStop      => "Stopped by user",
                PlaybackStopReason.EmergencyStop => "Stopped by emergency hotkey",
                _                                => "Idle"
            };
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsEditable));
        });
        _player.PlaybackInfo += info => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            Status = info;

            if (!_player.RouteInputToSelectedWindow && RouteInputToSelectedWindow)
            {
                _routeInputToSelectedWindow = false;
                OnPropertyChanged(nameof(RouteInputToSelectedWindow));
            }

            if (_player.TargetWindowHandle == IntPtr.Zero)
                InvalidateTargetWindow(info);
        });

        PlayCommand = new RelayCommand(Play, () => !_player.IsPlaying && Steps.Count > 0);
        StopCommand = new RelayCommand(Stop, () => _player.IsPlaying);
        AddStepCommand = new RelayCommand(AddStep);
        DeleteStepCommand = new RelayCommand(DeleteStep, () => SelectedStep != null);
        DuplicateStepCommand = new RelayCommand(DuplicateStep, () => SelectedStep != null);
        MoveUpCommand = new RelayCommand(MoveUp, () => SelectedStep != null && Steps.IndexOf(SelectedStep) > 0);
        MoveDownCommand = new RelayCommand(MoveDown, () => SelectedStep != null && Steps.IndexOf(SelectedStep) < Steps.Count - 1);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        OpenCommand = new RelayCommand(Open);
        NewCommand = new RelayCommand(New);
        RefreshWindowsCommand = new RelayCommand(RefreshWindowList);

        RefreshWindowList();

        if (_targetWindowHandle != IntPtr.Zero)
        {
            TryRestoreSelection(_targetWindowHandle);
            if (_selectedWindow == null)
                InvalidateTargetWindow("Previously selected target window is no longer available.");
            else
                SyncWindowRoutingToPlayer();
        }
        else
        {
            SyncWindowRoutingToPlayer();
        }

        if (_routeInputToSelectedWindow && _targetWindowHandle == IntPtr.Zero)
        {
            _routeInputToSelectedWindow = false;
            WindowRoutingInfo = "Select a target window before enabling routing.";
        }
    }

    public void LoadMacro(Macro macro)
    {
        MacroName = macro.Name;
        RepeatMode = macro.RepeatMode;
        RepeatCount = macro.RepeatCount;
        LoopDelayMs = macro.LoopDelayMs;
        HighPrecisionTiming = macro.HighPrecisionTiming;
        Steps.Clear();
        foreach (var step in macro.Steps)
            Steps.Add(new MacroStepViewModel(step));
        Status = $"Loaded {macro.Steps.Count} steps";
    }

    public void LoadProfile(MacroProfile profile)
    {
        ProfileDescription = profile.Description;
        _profileCreated = profile.Created;
        LoadMacro(profile.Macro);
    }

    private Macro BuildMacro() => new Macro
    {
        Name = MacroName,
        RepeatMode = RepeatMode,
        RepeatCount = RepeatCount,
        LoopDelayMs = LoopDelayMs,
        HighPrecisionTiming = HighPrecisionTiming,
        Steps = Steps.Select(s => s.Step).ToList()
    };

    private MacroProfile BuildProfile() => new MacroProfile
    {
        Name        = MacroName,
        Description = ProfileDescription,
        Created     = _profileCreated,
        Macro       = BuildMacro()
    };

    private void Play()
    {
        if (RouteInputToSelectedWindow && !_windowSelector.IsWindowValid(TargetWindowHandle))
        {
            InvalidateTargetWindow("Target window no longer exists. Routing disabled.");
            return;
        }

        SyncWindowRoutingToPlayer();
        _player.Play(BuildMacro());
    }
    private void Stop() => _player.Stop();

    private void AddStep()
    {
        var step = new MacroStep { Type = MacroStepType.KeyTap, Key = DefaultStepKey, HoldMs = DefaultStepHoldMs };
        var vm = new MacroStepViewModel(step);
        if (SelectedStep != null)
        {
            int idx = Steps.IndexOf(SelectedStep);
            Steps.Insert(idx + 1, vm);
        }
        else
        {
            Steps.Add(vm);
        }
        SelectedStep = vm;
    }

    private void DeleteStep()
    {
        if (SelectedStep == null) return;
        int idx = Steps.IndexOf(SelectedStep);
        Steps.Remove(SelectedStep);
        if (Steps.Count > 0)
            SelectedStep = Steps[Math.Min(idx, Steps.Count - 1)];
    }

    private void DuplicateStep()
    {
        if (SelectedStep == null) return;
        int idx = Steps.IndexOf(SelectedStep);
        var clone = new MacroStepViewModel(SelectedStep.Step.Clone());
        Steps.Insert(idx + 1, clone);
        SelectedStep = clone;
    }

    private void MoveUp()
    {
        if (SelectedStep == null) return;
        int idx = Steps.IndexOf(SelectedStep);
        if (idx > 0) Steps.Move(idx, idx - 1);
    }

    private void MoveDown()
    {
        if (SelectedStep == null) return;
        int idx = Steps.IndexOf(SelectedStep);
        if (idx < Steps.Count - 1) Steps.Move(idx, idx + 1);
    }

    private void Save()
    {
        if (_currentFilePath == null) { SaveAs(); return; }
        var path = _currentFilePath;
        MacroSerializer.SaveProfileAsync(BuildProfile(), path).ContinueWith(t =>
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
                Status = t.IsFaulted ? $"Save failed: {t.Exception?.GetBaseException().Message}" : "Saved");
        });
    }

    private void SaveAs()
    {
        var dlg = new SaveFileDialog { Filter = "Macro files (*.json)|*.json|All files (*.*)|*.*", DefaultExt = ".json" };
        if (dlg.ShowDialog() == true)
        {
            _currentFilePath = dlg.FileName;
            var path = _currentFilePath;
            var name = System.IO.Path.GetFileName(path);
            MacroSerializer.SaveProfileAsync(BuildProfile(), path).ContinueWith(t =>
            {
                Application.Current?.Dispatcher.BeginInvoke(() =>
                    Status = t.IsFaulted ? $"Save failed: {t.Exception?.GetBaseException().Message}" : $"Saved to {name}");
            });
        }
    }

    private void Open()
    {
        var dlg = new OpenFileDialog { Filter = "Macro files (*.json)|*.json|All files (*.*)|*.*" };
        if (dlg.ShowDialog() == true)
        {
            _currentFilePath = dlg.FileName;
            var task = MacroSerializer.LoadProfileAsync(_currentFilePath);
            task.ContinueWith(t =>
            {
                if (t.Result != null)
                    Application.Current?.Dispatcher.Invoke(() => LoadProfile(t.Result));
            });
        }
    }

    private void New()
    {
        _currentFilePath = null;
        MacroName = "New Macro";
        ProfileDescription = "";
        RepeatMode = RepeatMode.Once;
        RepeatCount = 1;
        LoopDelayMs = 0;
        HighPrecisionTiming = true;
        _profileCreated = DateTime.UtcNow;
        Steps.Clear();
        Status = "New macro – add steps and press Play";
    }

    private void RefreshWindowList()
    {
        IntPtr previousSelection = TargetWindowHandle;
        var windows = _windowSelector.GetOpenWindows();

        OpenWindows.Clear();
        foreach (var entry in windows)
            OpenWindows.Add(new WindowOption(entry.Key, entry.Value));

        if (OpenWindows.Count == 0)
        {
            SelectedWindow = null;
            if (RouteInputToSelectedWindow)
                InvalidateTargetWindow("No visible windows found. Routing disabled.");
            else
                WindowRoutingInfo = "No visible windows found.";

            return;
        }

        if (previousSelection != IntPtr.Zero && TryRestoreSelection(previousSelection))
        {
            if (RouteInputToSelectedWindow)
                WindowRoutingInfo = $"Routing keyboard input to: {SelectedWindow?.Title}";
            return;
        }

        if (RouteInputToSelectedWindow)
            InvalidateTargetWindow("Selected target window is no longer open. Routing disabled.");
        else
            WindowRoutingInfo = "Select a target window to route keyboard input.";
    }

    private bool TryRestoreSelection(IntPtr handle)
    {
        var option = OpenWindows.FirstOrDefault(x => x.Handle == handle);
        if (option == null)
            return false;

        SelectedWindow = option;
        return true;
    }

    private void InvalidateTargetWindow(string message)
    {
        _routeInputToSelectedWindow = false;
        _targetWindowHandle = IntPtr.Zero;
        _selectedWindow = null;

        OnPropertyChanged(nameof(RouteInputToSelectedWindow));
        OnPropertyChanged(nameof(TargetWindowHandle));
        OnPropertyChanged(nameof(SelectedWindow));

        WindowRoutingInfo = message;
        SyncWindowRoutingToPlayer();
    }

    private void SyncWindowRoutingToPlayer()
    {
        _player.RouteInputToSelectedWindow = _routeInputToSelectedWindow && _targetWindowHandle != IntPtr.Zero;
        _player.TargetWindowHandle = _targetWindowHandle;
    }

    public sealed class WindowOption
    {
        public WindowOption(IntPtr handle, string title)
        {
            Handle = handle;
            Title = title;
        }

        public IntPtr Handle { get; }
        public string Title { get; }
        public string DisplayName => $"{Title} (0x{Handle.ToInt64():X})";
    }
}
