using TapWeaver.Core.Services;

namespace TapWeaver.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RecorderViewModel Recorder { get; }
    public SequencerViewModel Sequencer { get; }
    public AutoClickerViewModel AutoClicker { get; }

    private readonly MacroRecorder _macroRecorder;
    private readonly MacroPlayer _macroPlayer;
    private readonly AutoClickerService _autoClickerService;

    public MainViewModel()
    {
        _macroRecorder = new MacroRecorder();
        _macroPlayer = new MacroPlayer();
        _autoClickerService = new AutoClickerService();

        Recorder = new RecorderViewModel(_macroRecorder);
        Sequencer = new SequencerViewModel(_macroPlayer);
        AutoClicker = new AutoClickerViewModel(_autoClickerService);

        Recorder.RecordingComplete += macro => Sequencer.LoadMacro(macro);
    }
}
