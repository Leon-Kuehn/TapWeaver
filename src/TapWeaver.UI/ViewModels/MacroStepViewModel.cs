using TapWeaver.Core.Models;
using TapWeaver.Core.Input;

namespace TapWeaver.UI.ViewModels;

public class MacroStepViewModel : ViewModelBase
{
    private readonly MacroStep _step;
    private static readonly IReadOnlyList<string> MouseButtonOptions = Enum
        .GetNames<MouseButton>()
        .OrderBy(static n => n, StringComparer.OrdinalIgnoreCase)
        .ToList();
    private static readonly IReadOnlyList<string> EmptyOptions = Array.Empty<string>();

    public MacroStepViewModel(MacroStep step)
    {
        _step = step;
    }

    public MacroStep Step => _step;

    public MacroStepType Type
    {
        get => _step.Type;
        set
        {
            _step.Type = value;
            EnsureInputDefaults();
            OnPropertyChanged();
            OnPropertyChanged(nameof(KeyOrButton));
            OnPropertyChanged(nameof(SelectableInputOptions));
            OnPropertyChanged(nameof(HasSelectableInput));
            OnPropertyChanged(nameof(Description));
        }
    }

    public string? Key
    {
        get => _step.Key;
        set
        {
            _step.Key = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(KeyOrButton));
            OnPropertyChanged(nameof(Description));
        }
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
        set
        {
            _step.Button = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(KeyOrButton));
            OnPropertyChanged(nameof(Description));
        }
    }

    public bool HasSelectableInput => Type is MacroStepType.KeyDown or MacroStepType.KeyUp or MacroStepType.KeyTap or MacroStepType.MouseClick;

    public IReadOnlyList<string> SelectableInputOptions => Type switch
    {
        MacroStepType.KeyDown or MacroStepType.KeyUp or MacroStepType.KeyTap => KeyboardKeyMap.AvailableKeyNames,
        MacroStepType.MouseClick => MouseButtonOptions,
        _ => EmptyOptions
    };

    public string? KeyOrButton
    {
        get => Type == MacroStepType.MouseClick ? Button.ToString() : Key;
        set
        {
            if (Type == MacroStepType.MouseClick)
            {
                if (Enum.TryParse<MouseButton>(value, true, out var parsedButton))
                    Button = parsedButton;
                return;
            }

            Key = value;
        }
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

    private void EnsureInputDefaults()
    {
        if (Type == MacroStepType.MouseClick)
        {
            if (!Enum.IsDefined(Button))
                _step.Button = MouseButton.Left;
            return;
        }

        if (Type is MacroStepType.KeyDown or MacroStepType.KeyUp or MacroStepType.KeyTap)
        {
            if (string.IsNullOrWhiteSpace(_step.Key) || !KeyboardKeyMap.TryGetVirtualKey(_step.Key, out _))
                _step.Key = KeyboardKeyMap.AvailableKeyNames.FirstOrDefault() ?? "A";
        }
    }
}
