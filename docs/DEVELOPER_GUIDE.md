# TapWeaver — Developer Guide

## Architecture Overview

The solution is split into three projects:

```
TapWeaver.slnx
├── src/TapWeaver.Core          — domain models, input simulation, engine services
├── src/TapWeaver.Persistence   — JSON serialisation / file I/O
└── src/TapWeaver.UI            — WPF presentation layer (MVVM)
```

Dependency direction: `UI → Persistence → Core`. The Core and Persistence layers have no WPF dependency.

---

## TapWeaver.Core

### Models (`Models/`)

| Class / Enum    | Purpose                                                  |
|-----------------|----------------------------------------------------------|
| `Macro`         | Top-level macro: name, repeat settings, list of steps    |
| `MacroStep`     | A single recorded or authored step                       |
| `MacroStepType` | Enum: KeyDown, KeyUp, KeyTap, Delay, MouseClick, MoveMouse |
| `RepeatMode`    | Enum: Once, Count, Infinite                              |
| `MouseButton`   | Enum: Left, Right, Middle                                |

`MacroStep` is a flat class with all possible fields; fields irrelevant to a given `Type` are ignored at runtime.

### Input Simulation (`Input/`)

| Class            | Purpose                                                   |
|------------------|-----------------------------------------------------------|
| `NativeMethods`  | P/Invoke declarations: `SendInput`, `SetWindowsHookEx`, `RegisterHotKey`, etc. |
| `InputSimulator` | Static helpers: `SendKeyDown`, `SendKeyUp`, `SendMouseClick`, `SendMouseMove` |

All input simulation uses `SendInput` (Win32), which works reliably across most applications.

### Services (`Services/`)

#### `MacroRecorder`
- Installs two global hooks via `SetWindowsHookEx`: `WH_KEYBOARD_LL` and `WH_MOUSE_LL`.
- Each relevant event is converted to a `MacroStep` and appended to an internal list.
- Time gaps > 50 ms between events are automatically inserted as `Delay` steps.
- `Stop()` uninstalls hooks and returns the completed `Macro`.
- Implements `IDisposable`; always unhooks on disposal.

#### `MacroPlayer`
- Runs on a background `Task` (never blocks the UI thread).
- Iterates steps and executes them via `InputSimulator`.
- `KeyTap` uses `Task.Delay` for the hold duration with cancellation support.
- Tracks currently pressed keys; on stop or cancellation all keys are released automatically.
- Raises `StepChanged`, `PlaybackStarted`, `PlaybackStopped` events (marshal to UI thread in ViewModels).

#### `AutoClickerService`
- Runs a tight `Task` loop; uses `Task.Delay` to schedule intervals from `1000.0 / CPS`.
- Supports randomised CPS via optional `MinCps`/`MaxCps` properties.
- Uses `SendInput` for mouse button down + up per click.

#### `HotkeyService`
- Wraps `RegisterHotKey` / `UnregisterHotKey`.
- Requires a native window handle; the `MainWindow` passes its `HWND` and routes `WM_HOTKEY` messages to `HandleHotkey`.

---

## TapWeaver.Persistence

### `MacroSerializer`
- Uses `System.Text.Json` with `JsonStringEnumConverter` so enums are stored as strings.
- `Null` values are omitted (`WhenWritingNull`) to keep JSON tidy.
- Provides `SaveAsync` / `LoadAsync` for file I/O and `Serialize` / `Deserialize` for string round-trips.

---

## TapWeaver.UI (WPF / MVVM)

### ViewModels

| ViewModel            | Owns                                              |
|----------------------|---------------------------------------------------|
| `MainViewModel`      | Instances of all three child VMs and shared services |
| `RecorderViewModel`  | `MacroRecorder`; exposes recorded steps collection |
| `SequencerViewModel` | `MacroPlayer`; exposes steps collection, file ops  |
| `AutoClickerViewModel` | `AutoClickerService`; exposes configuration     |

All ViewModels extend `ViewModelBase` (implements `INotifyPropertyChanged` with `SetProperty<T>`).
Commands are `RelayCommand` instances wired to methods in the ViewModel.

### Views

| View                 | Tab           |
|----------------------|---------------|
| `RecorderView`       | Recorder      |
| `SequencerView`      | Sequencer     |
| `AutoClickerView`    | Auto Clicker  |
| `MainWindow`         | Shell / tabs  |

### Themes (`Themes/`)
- `Colors.xaml` — colour palette as `SolidColorBrush` resources.
- `Styles.xaml` — reusable styles for buttons, text boxes, cards, tab items.

### Converters (`Converters/`)
- `InverseBoolConverter` — inverts a `bool` binding (used for position mode radio buttons).

---

## How to Add a New Step Type

1. Add the new value to `MacroStepType` in `TapWeaver.Core/Models/MacroStepType.cs`.
2. Add any new fields to `MacroStep` (or extend the model if preferred).
3. Update `MacroStep.Description` to return a meaningful string for the new type.
4. Add a `case` in `MacroPlayer.ExecuteStep` to implement the runtime behaviour.
5. The Sequencer DataGrid will automatically show the new type in the **Type** combo box.

---

## Threading Model

- The UI thread owns all `ObservableCollection` mutations; services raise events and ViewModels dispatch back via `Application.Current.Dispatcher.BeginInvoke`.
- `MacroPlayer` and `AutoClickerService` run on `ThreadPool` threads via `Task.Run`.
- `MacroRecorder` hook callbacks run on the hook's message pump thread; list mutations are protected with a `lock`.

---

## Building

```bash
dotnet build TapWeaver.slnx          # debug
dotnet publish src/TapWeaver.UI -c Release -r win-x64 --self-contained
```
