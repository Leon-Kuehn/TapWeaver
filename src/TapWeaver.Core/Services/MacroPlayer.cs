using System.Diagnostics;
using TapWeaver.Core.Input;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Services;

public class MacroPlayer
{
    private CancellationTokenSource? _cts;
    private readonly Dictionary<ushort, bool> _pressedKeys = new();
    private PlaybackStopReason _pendingStopReason = PlaybackStopReason.Completed;

    public event Action<int>? StepChanged;
    public event Action? PlaybackStarted;

    /// <summary>
    /// Raised when playback ends for any reason.
    /// The <see cref="PlaybackStopReason"/> argument indicates why it stopped.
    /// </summary>
    public event Action<PlaybackStopReason>? PlaybackStopped;

    public bool IsPlaying { get; private set; }
    
    private static readonly Dictionary<string, ushort> KeyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = 0x41, ["B"] = 0x42, ["C"] = 0x43, ["D"] = 0x44, ["E"] = 0x45,
        ["F"] = 0x46, ["G"] = 0x47, ["H"] = 0x48, ["I"] = 0x49, ["J"] = 0x4A,
        ["K"] = 0x4B, ["L"] = 0x4C, ["M"] = 0x4D, ["N"] = 0x4E, ["O"] = 0x4F,
        ["P"] = 0x50, ["Q"] = 0x51, ["R"] = 0x52, ["S"] = 0x53, ["T"] = 0x54,
        ["U"] = 0x55, ["V"] = 0x56, ["W"] = 0x57, ["X"] = 0x58, ["Y"] = 0x59,
        ["Z"] = 0x5A,
        ["0"] = 0x30, ["1"] = 0x31, ["2"] = 0x32, ["3"] = 0x33, ["4"] = 0x34,
        ["5"] = 0x35, ["6"] = 0x36, ["7"] = 0x37, ["8"] = 0x38, ["9"] = 0x39,
        ["F1"] = 0x70, ["F2"] = 0x71, ["F3"] = 0x72, ["F4"] = 0x73,
        ["F5"] = 0x74, ["F6"] = 0x75, ["F7"] = 0x76, ["F8"] = 0x77,
        ["F9"] = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,
        ["SPACE"] = 0x20, ["ENTER"] = 0x0D, ["ESCAPE"] = 0x1B, ["TAB"] = 0x09,
        ["BACKSPACE"] = 0x08, ["DELETE"] = 0x2E, ["INSERT"] = 0x2D,
        ["HOME"] = 0x24, ["END"] = 0x23, ["PAGEUP"] = 0x21, ["PAGEDOWN"] = 0x22,
        ["LEFT"] = 0x25, ["UP"] = 0x26, ["RIGHT"] = 0x27, ["DOWN"] = 0x28,
        ["SHIFT"] = 0x10, ["CTRL"] = 0x11, ["ALT"] = 0x12, ["WIN"] = 0x5B,
        ["CAPSLOCK"] = 0x14, ["NUMLOCK"] = 0x90,
    };

    public static ushort GetVkCode(string key)
    {
        if (KeyMap.TryGetValue(key.ToUpperInvariant(), out ushort vk))
            return vk;
        if (key.Length == 1)
            return (ushort)key.ToUpperInvariant()[0];
        return 0;
    }

    public void Play(Macro macro)
    {
        if (IsPlaying) return;
        _cts = new CancellationTokenSource();
        _pendingStopReason = PlaybackStopReason.Completed;
        IsPlaying = true;
        PlaybackStarted?.Invoke();
        Task.Run(() => RunPlayback(macro, _cts.Token));
    }

    /// <summary>
    /// Stops playback immediately.
    /// </summary>
    /// <param name="reason">
    /// The reason playback is being stopped.  Defaults to <see cref="PlaybackStopReason.UserStop"/>.
    /// </param>
    public void Stop(PlaybackStopReason reason = PlaybackStopReason.UserStop)
    {
        _pendingStopReason = reason;
        _cts?.Cancel();
        ReleaseAllKeys();
    }

    private async Task RunPlayback(Macro macro, CancellationToken token)
    {
        try
        {
            int iterations = 0;
            bool infinite = macro.RepeatMode == RepeatMode.Infinite;
            int maxIterations = macro.RepeatMode == RepeatMode.Once ? 1 : macro.RepeatCount;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < macro.Steps.Count && !token.IsCancellationRequested; i++)
                {
                    StepChanged?.Invoke(i);
                    await ExecuteStep(macro.Steps[i], token);
                }

                if (token.IsCancellationRequested) break;

                iterations++;
                if (!infinite && iterations >= maxIterations) break;

                if (macro.LoopDelayMs > 0)
                    await Task.Delay(macro.LoopDelayMs, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            ReleaseAllKeys();
            IsPlaying = false;
            StepChanged?.Invoke(-1);
            // If playback finished all iterations without cancellation it completed normally.
            var reason = _cts?.IsCancellationRequested == true
                ? _pendingStopReason
                : PlaybackStopReason.Completed;
            PlaybackStopped?.Invoke(reason);
        }
    }

    private async Task ExecuteStep(MacroStep step, CancellationToken token)
    {
        switch (step.Type)
        {
            case MacroStepType.KeyDown:
                if (!string.IsNullOrEmpty(step.Key))
                {
                    var vk = GetVkCode(step.Key);
                    if (vk != 0)
                    {
                        InputSimulator.SendKeyDown(vk);
                        _pressedKeys[vk] = true;
                    }
                }
                break;
            case MacroStepType.KeyUp:
                if (!string.IsNullOrEmpty(step.Key))
                {
                    var vk = GetVkCode(step.Key);
                    if (vk != 0)
                    {
                        InputSimulator.SendKeyUp(vk);
                        _pressedKeys.Remove(vk);
                    }
                }
                break;
            case MacroStepType.KeyTap:
                if (!string.IsNullOrEmpty(step.Key))
                {
                    var vk = GetVkCode(step.Key);
                    if (vk != 0)
                    {
                        InputSimulator.SendKeyDown(vk);
                        _pressedKeys[vk] = true;
                        await Task.Delay(Math.Max(step.HoldMs, 1), token);
                        InputSimulator.SendKeyUp(vk);
                        _pressedKeys.Remove(vk);
                    }
                }
                break;
            case MacroStepType.Delay:
                if (step.DelayMs > 0)
                    await Task.Delay(step.DelayMs, token);
                break;
            case MacroStepType.MouseClick:
                InputSimulator.SendMouseClick(step.Button, step.X, step.Y);
                break;
            case MacroStepType.MoveMouse:
                if (step.X.HasValue && step.Y.HasValue)
                    InputSimulator.SendMouseMove(step.X.Value, step.Y.Value);
                break;
        }
    }

    private void ReleaseAllKeys()
    {
        foreach (var key in _pressedKeys.Keys.ToList())
        {
            InputSimulator.SendKeyUp(key);
        }
        _pressedKeys.Clear();
    }
}
