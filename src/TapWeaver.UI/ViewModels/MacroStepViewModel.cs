using TapWeaver.Core.Models;

namespace TapWeaver.UI.ViewModels;

public class MacroStepViewModel : ViewModelBase
{
    private readonly MacroStep _step;

    public MacroStepViewModel(MacroStep step)
    {
        _step = step;
    }

    public MacroStep Step => _step;

    public MacroStepType Type
    {
        get => _step.Type;
        set { _step.Type = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public string? Key
    {
        get => _step.Key;
        set { _step.Key = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public int HoldMs
    {
        get => _step.HoldMs;
        set { _step.HoldMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public int DelayMs
    {
        get => _step.DelayMs;
        set { _step.DelayMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public MouseButton Button
    {
        get => _step.Button;
        set { _step.Button = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public int? X
    {
        get => _step.X;
        set { _step.X = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public int? Y
    {
        get => _step.Y;
        set { _step.Y = value; OnPropertyChanged(); OnPropertyChanged(nameof(Description)); }
    }

    public string Description => _step.Description;
    
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetProperty(ref _isHighlighted, value);
    }
    private bool _isHighlighted;
}
