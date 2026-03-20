using System.Collections.ObjectModel;
using System.Windows;
using TapWeaver.Core.Models;
using TapWeaver.Core.Services;
using TapWeaver.Persistence;
using Microsoft.Win32;

namespace TapWeaver.UI.ViewModels;

public class SequencerViewModel : ViewModelBase
{
    private readonly MacroPlayer _player;
    private string _macroName = "New Macro";
    private string _profileDescription = "";
    private RepeatMode _repeatMode = RepeatMode.Once;
    private int _repeatCount = 1;
    private int _loopDelayMs = 0;
    private string? _currentFilePath;
    private DateTime _profileCreated = DateTime.UtcNow;
    private int _currentStepIndex = -1;
    private string _status = "Ready";

    public ObservableCollection<MacroStepViewModel> Steps { get; } = new();
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
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public bool IsPlaying => _player.IsPlaying;
    
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

    private const string DefaultStepKey = "A";
    private const int DefaultStepHoldMs = 100;

    public SequencerViewModel(MacroPlayer player)
    {
        _player = player;
        _player.StepChanged += idx => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            CurrentStepIndex = idx;
            OnPropertyChanged(nameof(IsPlaying));
        });
        _player.PlaybackStarted += () => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            Status = "Playing...";
            OnPropertyChanged(nameof(IsPlaying));
        });
        _player.PlaybackStopped += () => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            Status = "Stopped";
            OnPropertyChanged(nameof(IsPlaying));
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
    }

    public void LoadMacro(Macro macro)
    {
        MacroName = macro.Name;
        RepeatMode = macro.RepeatMode;
        RepeatCount = macro.RepeatCount;
        LoopDelayMs = macro.LoopDelayMs;
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
        Steps = Steps.Select(s => s.Step).ToList()
    };

    private MacroProfile BuildProfile() => new MacroProfile
    {
        Name        = MacroName,
        Description = ProfileDescription,
        Created     = _profileCreated,
        Macro       = BuildMacro()
    };

    private void Play() => _player.Play(BuildMacro());
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
        _profileCreated = DateTime.UtcNow;
        Steps.Clear();
        Status = "New macro";
    }
}
