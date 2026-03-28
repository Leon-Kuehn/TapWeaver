using System.Diagnostics;
using System.Windows.Forms;
using TapWeaver.Core.Input;
using TapWeaver.Core.Interop;
using TapWeaver.Core.Models;

namespace TapWeaver.Core.Services;

public class MacroPlayer
{
    private sealed class PlaybackTimeline
    {
        public long Ms;
    }

    private CancellationTokenSource? _cts;
    private readonly Dictionary<ushort, bool> _pressedKeys = new();
    private readonly WindowInputSender _windowInputSender = new();
    private PlaybackStopReason _pendingStopReason = PlaybackStopReason.Completed;

    public event Action<int>? StepChanged;
    public event Action? PlaybackStarted;
    public event Action<string>? PlaybackInfo;

    /// <summary>
    /// Raised when playback ends for any reason.
    /// The <see cref="PlaybackStopReason"/> argument indicates why it stopped.
    /// </summary>
    public event Action<PlaybackStopReason>? PlaybackStopped;

    public bool IsPlaying { get; private set; }

    /// <summary>
    /// When enabled, keyboard events are posted to <see cref="TargetWindowHandle"/>.
    /// Mouse steps continue to use global SendInput behavior.
    /// </summary>
    public bool RouteInputToSelectedWindow { get; set; }

    /// <summary>
    /// Target top-level window handle for routed keyboard events.
    /// </summary>
    public IntPtr TargetWindowHandle { get; set; }

    public static ushort GetVkCode(string key)
    {
        return KeyboardKeyMap.TryGetVirtualKey(key, out var vkCode)
            ? vkCode
            : (ushort)0;
    }

    public void Play(Macro macro)
    {
        if (IsPlaying) return;

        if (RouteInputToSelectedWindow && !HasValidTargetWindow())
            DisableWindowRouting("Target window is no longer available. Routing disabled.");

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
            if (macro.Steps.Count == 0)
            {
                IsPlaying = false;
                PlaybackStopped?.Invoke(PlaybackStopReason.Completed);
                return;
            }

            bool infinite = macro.RepeatMode == RepeatMode.Infinite;
            int maxIterations = macro.RepeatMode == RepeatMode.Once ? 1 : macro.RepeatCount;
            bool highPrecisionTiming = macro.HighPrecisionTiming;
            var playbackClock = Stopwatch.StartNew();
            var timeline = new PlaybackTimeline();

            // Find the index of the first non-delay step (skip leading delays for immediate start)
            int firstActionIndex = 0;
            while (firstActionIndex < macro.Steps.Count && macro.Steps[firstActionIndex].Type == MacroStepType.Delay)
                firstActionIndex++;

            // Execute macro for the specified number of iterations
            if (infinite)
            {
                // Infinite loop
                for (int iterCount = 0; !token.IsCancellationRequested; iterCount++)
                {
                    await ExecuteIteration(macro, firstActionIndex, iterCount == 0, playbackClock, timeline, token);
                    if (macro.LoopDelayMs > 0)
                    {
                        timeline.Ms += macro.LoopDelayMs;
                        await WaitForTimelineAsync(playbackClock, timeline.Ms, highPrecisionTiming, token);
                    }
                }
            }
            else
            {
                // Finite iterations
                for (int iterCount = 0; iterCount < maxIterations && !token.IsCancellationRequested; iterCount++)
                {
                    await ExecuteIteration(macro, firstActionIndex, iterCount == 0, playbackClock, timeline, token);
                    // Only delay between iterations, not after the last one
                    if (iterCount < maxIterations - 1 && macro.LoopDelayMs > 0)
                    {
                        timeline.Ms += macro.LoopDelayMs;
                        await WaitForTimelineAsync(playbackClock, timeline.Ms, highPrecisionTiming, token);
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            // Give the OS minimal time to process all pending input events
            // This prevents the last SendInput operation from being lost
            System.Threading.Thread.Sleep(10);
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

    private async Task ExecuteIteration(Macro macro, int firstActionIndex, bool isFirstIteration, Stopwatch playbackClock, PlaybackTimeline timeline, CancellationToken token)
    {
        for (int i = 0; i < macro.Steps.Count && !token.IsCancellationRequested; i++)
        {
            // Only skip leading delays on the very first iteration of the entire playback
            if (isFirstIteration && i < firstActionIndex)
                continue;
            
            StepChanged?.Invoke(i);
            await ExecuteStep(macro.Steps[i], playbackClock, timeline, macro.HighPrecisionTiming, token);
        }
    }

    private async Task ExecuteStep(MacroStep step, Stopwatch playbackClock, PlaybackTimeline timeline, bool highPrecisionTiming, CancellationToken token)
    {
        switch (step.Type)
        {
            case MacroStepType.KeyDown:
                if (!string.IsNullOrEmpty(step.Key))
                {
                    var vk = GetVkCode(step.Key);
                    if (vk != 0)
                    {
                        SendKeyDown(vk);
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
                        SendKeyUp(vk);
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
                        SendKeyDown(vk);
                        _pressedKeys[vk] = true;
                        timeline.Ms += Math.Max(step.HoldMs, 1);
                        await WaitForTimelineAsync(playbackClock, timeline.Ms, highPrecisionTiming, token);
                        SendKeyUp(vk);
                        _pressedKeys.Remove(vk);
                    }
                }
                break;
            case MacroStepType.Delay:
                if (step.DelayMs > 0)
                {
                    timeline.Ms += step.DelayMs;
                    await WaitForTimelineAsync(playbackClock, timeline.Ms, highPrecisionTiming, token);
                }
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

    private static async Task WaitForTimelineAsync(Stopwatch playbackClock, long timelineMs, bool highPrecisionTiming, CancellationToken token)
    {
        while (true)
        {
            long remainingMs = timelineMs - playbackClock.ElapsedMilliseconds;
            if (remainingMs <= 0)
                return;

            if (highPrecisionTiming && remainingMs <= 2)
            {
                var spinner = new SpinWait();
                while (timelineMs - playbackClock.ElapsedMilliseconds > 0)
                {
                    token.ThrowIfCancellationRequested();
                    spinner.SpinOnce();
                }
                return;
            }

            // Delay in short chunks so cancellation stays responsive while preserving schedule accuracy.
            // In high precision mode we wake up slightly earlier and finish with a short spin-wait.
            int maxChunk = highPrecisionTiming ? 10 : 25;
            int earlyWakeCompensation = highPrecisionTiming ? 1 : 0;
            int delayMs = (int)Math.Min(Math.Max(remainingMs - earlyWakeCompensation, 1), maxChunk);
            await Task.Delay(delayMs, token);
        }
    }

    private void ReleaseAllKeys()
    {
        foreach (var key in _pressedKeys.Keys.ToList())
        {
            SendKeyUp(key);
        }
        _pressedKeys.Clear();
    }

    private void SendKeyDown(ushort vk)
    {
        if (TrySendKeyDownToTarget(vk))
            return;

        InputSimulator.SendKeyDown(vk);
    }

    private void SendKeyUp(ushort vk)
    {
        if (TrySendKeyUpToTarget(vk))
            return;

        InputSimulator.SendKeyUp(vk);
    }

    private bool TrySendKeyDownToTarget(ushort vk)
    {
        if (!RouteInputToSelectedWindow || TargetWindowHandle == IntPtr.Zero)
            return false;

        if (!HasValidTargetWindow())
        {
            DisableWindowRouting("Target window was closed. Routing disabled.");
            return false;
        }

        if (_windowInputSender.TrySendKeyDown(TargetWindowHandle, (Keys)vk))
            return true;

        DisableWindowRouting("Failed to send key to target window. Routing disabled.");
        return false;
    }

    private bool TrySendKeyUpToTarget(ushort vk)
    {
        if (!RouteInputToSelectedWindow || TargetWindowHandle == IntPtr.Zero)
            return false;

        if (!HasValidTargetWindow())
        {
            DisableWindowRouting("Target window was closed. Routing disabled.");
            return false;
        }

        if (_windowInputSender.TrySendKeyUp(TargetWindowHandle, (Keys)vk))
            return true;

        DisableWindowRouting("Failed to send key to target window. Routing disabled.");
        return false;
    }

    private bool HasValidTargetWindow()
        => TargetWindowHandle != IntPtr.Zero && NativeMethods.IsWindow(TargetWindowHandle);

    private void DisableWindowRouting(string info)
    {
        RouteInputToSelectedWindow = false;
        TargetWindowHandle = IntPtr.Zero;
        PlaybackInfo?.Invoke(info);
    }
}
