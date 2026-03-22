using TapWeaver.Core.Models;
using TapWeaver.Core.Services;

namespace TapWeaver.UI.ViewModels;

public class AutoClickerViewModel : ViewModelBase
{
    private readonly AutoClickerService _service;
    private const double HighCpsWarningThreshold = 1000.0;

    public double Cps
    {
        get => _service.Cps;
        set
        {
            _service.Cps = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowHighCpsWarning));
            OnPropertyChanged(nameof(HighCpsWarningText));
        }
    }

    public double? MinCps
    {
        get => _service.MinCps;
        set
        {
            _service.MinCps = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowHighCpsWarning));
            OnPropertyChanged(nameof(HighCpsWarningText));
        }
    }

    public double? MaxCps
    {
        get => _service.MaxCps;
        set
        {
            _service.MaxCps = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowHighCpsWarning));
            OnPropertyChanged(nameof(HighCpsWarningText));
        }
    }

    public MouseButton Button
    {
        get => _service.Button;
        set { _service.Button = value; OnPropertyChanged(); }
    }

    public bool UseFixedPosition
    {
        get => _service.UseFixedPosition;
        set { _service.UseFixedPosition = value; OnPropertyChanged(); }
    }

    public int FixedX
    {
        get => _service.FixedX;
        set { _service.FixedX = value; OnPropertyChanged(); }
    }

    public int FixedY
    {
        get => _service.FixedY;
        set { _service.FixedY = value; OnPropertyChanged(); }
    }

    public bool IsRunning => _service.IsRunning;
    public string StatusText => _service.IsRunning ? "ON" : "OFF";
    public bool ShowHighCpsWarning => IsHighRiskCps(Cps) || IsHighRiskCps(MinCps) || IsHighRiskCps(MaxCps);
    public string HighCpsWarningText =>
        $"Warning: Values >= {HighCpsWarningThreshold:0} CPS are at your own risk. Many games can break, you may get banned, and it can stress your system.";

    public RelayCommand ToggleCommand { get; }
    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand PickLocationCommand { get; }
    
    public IEnumerable<MouseButton> MouseButtons => Enum.GetValues<MouseButton>();

    public AutoClickerViewModel(AutoClickerService service)
    {
        _service = service;
        _service.StateChanged += () =>
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(StatusText));
            });
        };
        ToggleCommand = new RelayCommand(_service.Toggle);
        StartCommand = new RelayCommand(_service.Start, () => !_service.IsRunning);
        StopCommand = new RelayCommand(_service.Stop, () => _service.IsRunning);
        PickLocationCommand = new RelayCommand(PickLocation);
    }

    private void PickLocation()
    {
        var picker = new Views.CoordinatePicker();
        if (picker.ShowDialog() == true)
        {
            if (picker.SelectedX.HasValue && picker.SelectedY.HasValue)
            {
                FixedX = picker.SelectedX.Value;
                FixedY = picker.SelectedY.Value;
                UseFixedPosition = true;
            }
        }
    }

    private static bool IsHighRiskCps(double? cps)
    {
        if (!cps.HasValue)
            return false;

        return cps.Value >= HighCpsWarningThreshold;
    }
}
