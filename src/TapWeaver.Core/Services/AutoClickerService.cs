using TapWeaver.Core.Input;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Services;

public class AutoClickerService
{
    private CancellationTokenSource? _cts;
    
    public bool IsRunning { get; private set; }
    public event Action? StateChanged;
    
    public double Cps { get; set; } = 10;
    public double? MinCps { get; set; }
    public double? MaxCps { get; set; }
    public MouseButton Button { get; set; } = MouseButton.Left;
    public bool UseFixedPosition { get; set; } = false;
    public int FixedX { get; set; } = 0;
    public int FixedY { get; set; } = 0;

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        IsRunning = true;
        StateChanged?.Invoke();
        Task.Run(() => ClickLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsRunning = false;
        StateChanged?.Invoke();
    }

    public void Toggle()
    {
        if (IsRunning) Stop(); else Start();
    }

    private async Task ClickLoop(CancellationToken token)
    {
        var rng = new Random();
        try
        {
            while (!token.IsCancellationRequested)
            {
                double cps = GetCurrentCps(rng);
                double intervalMs = 1000.0 / Math.Max(cps, 0.1);
                int? x = UseFixedPosition ? FixedX : null;
                int? y = UseFixedPosition ? FixedY : null;
                InputSimulator.SendMouseClick(Button, x, y);
                long waitMs = (long)intervalMs;
                if (waitMs > 0)
                    await Task.Delay((int)waitMs, token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            IsRunning = false;
            StateChanged?.Invoke();
        }
    }

    private double GetCurrentCps(Random rng)
    {
        if (MinCps.HasValue && MaxCps.HasValue)
            return MinCps.Value + rng.NextDouble() * (MaxCps.Value - MinCps.Value);
        return Cps;
    }
}
