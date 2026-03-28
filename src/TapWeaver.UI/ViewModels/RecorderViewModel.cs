using System.Collections.ObjectModel;
using System.Windows;
using TapWeaver.Core.Models;
using TapWeaver.Core.Services;
using TapWeaver.Persistence;

namespace TapWeaver.UI.ViewModels;

public class RecorderViewModel : ViewModelBase
{
    private readonly MacroRecorder _recorder;
    private readonly AppSettings _appSettings;
    private bool _isRecording;

    public ObservableCollection<MacroStepViewModel> RecordedSteps { get; } = new();

    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
    }

    public bool RecordKeyboardEvents
    {
        get => _appSettings.RecordKeyboardEvents;
        set
        {
            if (_appSettings.RecordKeyboardEvents == value) return;
            _appSettings.RecordKeyboardEvents = value;
            _recorder.CaptureKeyboardEvents = value;
            AppSettingsSerializer.Save(_appSettings);
            OnPropertyChanged();
        }
    }

    public bool RecordMouseClickEvents
    {
        get => _appSettings.RecordMouseClickEvents;
        set
        {
            if (_appSettings.RecordMouseClickEvents == value) return;
            _appSettings.RecordMouseClickEvents = value;
            _recorder.CaptureMouseClickEvents = value;
            AppSettingsSerializer.Save(_appSettings);
            OnPropertyChanged();
        }
    }

    public bool RecordMouseMoveEvents
    {
        get => _appSettings.RecordMouseMoveEvents;
        set
        {
            if (_appSettings.RecordMouseMoveEvents == value) return;
            _appSettings.RecordMouseMoveEvents = value;
            _recorder.CaptureMouseMoveEvents = value;
            AppSettingsSerializer.Save(_appSettings);
            OnPropertyChanged();
        }
    }

    public RelayCommand StartRecordingCommand { get; }
    public RelayCommand StopRecordingCommand { get; }
    public event Action<Macro>? RecordingComplete;

    public RecorderViewModel(MacroRecorder recorder, AppSettings appSettings)
    {
        _recorder = recorder;
        _appSettings = appSettings;
        _recorder.CaptureKeyboardEvents = _appSettings.RecordKeyboardEvents;
        _recorder.CaptureMouseClickEvents = _appSettings.RecordMouseClickEvents;
        _recorder.CaptureMouseMoveEvents = _appSettings.RecordMouseMoveEvents;
        _recorder.StepRecorded += OnStepRecorded;
        StartRecordingCommand = new RelayCommand(StartRecording, () => !IsRecording);
        StopRecordingCommand = new RelayCommand(StopRecording, () => IsRecording);
    }

    private void StartRecording()
    {
        RecordedSteps.Clear();
        _recorder.Start();
        IsRecording = true;
    }

    private void StopRecording()
    {
        var macro = _recorder.Stop();
        IsRecording = false;
        RecordingComplete?.Invoke(macro);
    }

    private void OnStepRecorded(MacroStep step)
    {
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            RecordedSteps.Add(new MacroStepViewModel(step));
        });
    }
}
