# TapWeaver

**TapWeaver** is a modern Windows desktop macro recorder, sequence editor, and high-performance auto-clicker — built with C# .NET 8 and WPF.

Think of it as a *TinyTask++*: ultra-simple to use, but with far more control.

---

## Key Features

### 🎙 Macro Recorder & Player
- Records keyboard (key down / key up) and mouse click events with precise millisecond timing
- Uses global low-level Windows hooks (`WH_KEYBOARD_LL`, `WH_MOUSE_LL`) — works even when the app is not focused
- Configurable global hotkeys to start/stop recording and playback
- Repeat modes: **Once**, **Count** (N times), or **Infinite** loop with optional loop delay
- Visual step-by-step progress highlighting during playback
- Emergency stop with automatic release of all held keys

### ✏️ Manual Sequence Editor
- Build macros entirely by hand with a clean DataGrid editor
- Step types: `KeyDown`, `KeyUp`, `KeyTap`, `Delay`, `MouseClick`, `MoveMouse`
- `KeyTap` steps simulate a real key-down + hold for the configured duration + key-up using `SendInput` — clean, precise, never blocks the system
- Inline editing of key names, durations, delays, and coordinates
- Add, duplicate, delete, and reorder steps (move up/down)
- Save and load macros as human-readable **JSON** files
- Dedicated **Sequencer Start/Stop hotkey** (default Ctrl+F8) — toggle playback from any window; the hotkey is shown directly in the Sequencer tab
- Macro editing is automatically disabled (greyed out) while a macro is running
- Status bar shows: `Idle` / `Running…` / `Stopped by user` / `Stopped by emergency hotkey`

### 🖱 Auto Clicker
- High-performance clicking using `SendInput` and a `Stopwatch`-based scheduler
- Configurable **CPS** (clicks per second) — or a randomised min/max CPS range
- Click type: Left / Right / Middle
- Position mode: follow the cursor, or click at a fixed screen coordinate (X, Y)
- Start/Stop via UI buttons or a configurable global hotkey

### 🛑 Emergency Stop & Global Safety Hotkeys
- Fixed emergency-stop hotkey **Ctrl+Alt+Pause** — always active, even when minimised
  - Immediately stops playback, the auto-clicker, and any recording
  - Releases all stuck modifier keys and mouse buttons
  - The Sequencer status bar shows **"Stopped by emergency hotkey"** when triggered
- All critical functions have configurable global hotkeys (Ctrl+F7 / F8 / F9 by default)
- Debounced hotkey handling prevents accidental rapid-fire toggles

### ⚙️ Settings & Profiles
- Always-on-top window mode (toggle in Settings or title bar)
- Hotkey assignments are user-configurable and persist between sessions
- Macros are saved as **MacroProfile** JSON files (name, description, timestamps + macro)
- Full backward compatibility with legacy plain-macro JSON files

---

## Installation

### Requirements
- Windows 10 or later (x64)
- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Build from Source
```
git clone https://github.com/Leon-Kuehn/TapWeaver.git
cd TapWeaver
dotnet build TapWeaver.slnx
dotnet run --project src/TapWeaver.UI
```

---

## Basic Usage

### Record a Macro
1. Go to the **Recorder** tab → click **Start Recording**
2. Perform your keyboard and mouse actions
3. Click **Stop & Use Macro** — the recording is loaded into the **Sequencer**

### Play a Macro
1. Go to the **Sequencer** tab, select repeat mode and count
2. Click **▶ Play**

### Create a Macro Manually
1. Go to **Sequencer** → click **New** then **+ Add Step** for each action
2. Edit Type, Key, and timing inline in the grid
3. Set **Repeat Mode** to `Infinite` and click **▶ Play**

### Run the Auto Clicker
1. Go to **Auto Clicker**, set CPS, click type, and position mode
2. Click **Start**

---

## Documentation

- [User Guide](docs/USER_GUIDE.md)
- [Developer Guide](docs/DEVELOPER_GUIDE.md)

---

## License

This project is licensed under the [MIT License](LICENSE).
