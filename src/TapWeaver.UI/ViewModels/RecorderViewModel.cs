using System.Collections.ObjectModel;
using System.Windows;
using TapWeaver.Core.Models;
using TapWeaver.Core.Services;

namespace TapWeaver.UI.ViewModels;

public class RecorderViewModel : ViewModelBase
{
    private readonly MacroRecorder _recorder;
    private bool _isRecording;

    public ObservableCollection<MacroStepViewModel> RecordedSteps { get; } = new();

    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
    }

    public RelayCommand StartRecordingCommand { get; }
    public RelayCommand StopRecordingCommand { get; }
    public event Action<Macro>? RecordingComplete;

    public RecorderViewModel(MacroRecorder recorder)
    {
        _recorder = recorder;
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
