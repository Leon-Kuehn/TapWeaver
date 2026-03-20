# TapWeaver — User Guide

## Table of Contents
1. [Recording a Macro](#1-recording-a-macro)
2. [Creating a Macro Manually](#2-creating-a-macro-manually)
3. [Playback and Repeat Options](#3-playback-and-repeat-options)
4. [The Auto Clicker](#4-the-auto-clicker)
5. [Saving and Loading Macros](#5-saving-and-loading-macros)
6. [Global Hotkeys](#6-global-hotkeys)
7. [Emergency Stop](#7-emergency-stop)
8. [Settings](#8-settings)
9. [JSON Format Reference](#9-json-format-reference)

---

## 1. Recording a Macro

1. Launch TapWeaver and switch to the **Recorder** tab.
2. Click **⏺ Start Recording** (the status indicator turns red).
3. Switch to your target application and perform the actions you want to record:
   - Key presses and releases are captured automatically.
   - Mouse clicks with screen coordinates are captured.
   - Time gaps between events are recorded as `Delay` steps.
4. Return to TapWeaver and click **⏹ Stop & Use Macro**.
5. The recorded steps are immediately loaded into the **Sequencer** tab, ready to edit or play.

> **Tip:** The recording captures events globally — you do not need to keep TapWeaver focused.

---

## 2. Creating a Macro Manually

### Example: Press A for 3 s → Delay 200 ms → Press D for 1.5 s → Press W for 1.3 s (loop forever)

1. Go to the **Sequencer** tab and click **New**.
2. Click **+ Add Step** and configure:
   - **Type:** `KeyTap`, **Key:** `A`, **Hold (ms):** `3000`
3. Click **+ Add Step** again:
   - **Type:** `Delay`, **Delay (ms):** `200`
4. Add another step:
   - **Type:** `KeyTap`, **Key:** `D`, **Hold (ms):** `1500`
5. Add another step:
   - **Type:** `KeyTap`, **Key:** `W`, **Hold (ms):** `1300`
6. Set **Repeat Mode** to `Infinite`.
7. Click **▶ Play**. Press **⏹ Stop** (or the configured stop hotkey) to halt.

### Step Types

| Type        | Parameters              | Description                                          |
|-------------|-------------------------|------------------------------------------------------|
| `KeyTap`    | Key, Hold (ms)          | Press and hold a key for the specified duration      |
| `KeyDown`   | Key                     | Press and hold a key (until a matching `KeyUp`)      |
| `KeyUp`     | Key                     | Release a previously held key                        |
| `Delay`     | Delay (ms)              | Wait for the specified number of milliseconds        |
| `MouseClick`| Button, X (opt), Y (opt)| Click a mouse button, optionally at a screen position|
| `MoveMouse` | X, Y                    | Move the mouse cursor to an absolute screen position |

### Toolbar Actions
- **+ Add Step** — insert a new step after the selected step
- **Duplicate** — clone the selected step
- **↑ / ↓** — reorder the selected step
- **🗑 Delete** — remove the selected step

---

## 3. Playback and Repeat Options

| Option         | Values              | Description                                          |
|----------------|---------------------|------------------------------------------------------|
| Repeat Mode    | Once / Count / Infinite | How many times the macro runs                    |
| Repeat Count   | integer ≥ 1         | Used when Repeat Mode = Count                        |
| Loop Delay (ms)| integer ≥ 0         | Pause between successive loop iterations             |

During playback:

- The currently executing step is **highlighted in blue** in the grid.
- The settings card and DataGrid editor are **greyed out** (editing is disabled while running).
- A **▶ Running…** indicator appears next to the Play/Stop buttons.
- The status bar in the bottom of the Sequencer tab shows one of:
  - `Idle` — no macro has been played yet, or the last run completed normally.
  - `Running…` — a macro is currently executing.
  - `Stopped by user` — the user pressed **⏹ Stop** or the Sequencer hotkey.
  - `Stopped by emergency hotkey` — the fixed **Ctrl+Alt+Pause** hotkey was used.

### How `KeyTap` steps work

A `KeyTap` step simulates a real, clean key-press:

1. Sends a **KeyDown** event for the configured key.
2. Waits for exactly the **Hold (ms)** duration (using a background timer — the UI stays responsive).
3. Sends a **KeyUp** event.

If playback is stopped early, all keys that are currently held down are automatically released — no "stuck key" issues.

---

## 4. The Auto Clicker

1. Switch to the **Auto Clicker** tab.
2. Set **CPS** (clicks per second) — e.g. `10`.
3. Choose **Click Type**: Left, Right, or Middle.
4. Choose **Position**:
   - *Follow cursor* — clicks wherever the mouse currently is.
   - *Fixed position* — enter X and Y screen coordinates.
5. Click **Start**. Click **Stop** to halt.

---

## 5. Saving and Loading Macros

- **Save / Save As…** — saves the current macro (with name and optional description) to a `.json` profile file.
- **Open…** — loads a macro profile from a `.json` file.  Legacy plain-macro files (without a profile wrapper) are loaded automatically.
- **New** — clears the editor for a fresh macro.

---

## 6. Global Hotkeys

TapWeaver registers global hotkeys that work even when the application is running in the background.

| Function                         | Default hotkey | Configurable? |
|----------------------------------|---------------|---------------|
| Sequencer Playback (Start/Stop)  | Ctrl+F8       | ✅ Yes        |
| Toggle macro recording           | Ctrl+F7       | ✅ Yes        |
| Toggle auto-clicker              | Ctrl+F9       | ✅ Yes        |
| Emergency Stop                   | Ctrl+Alt+Pause| ❌ Fixed      |

The active **Sequencer Playback** hotkey is displayed directly in the **Sequencer** tab (next to the Play/Stop buttons). Pressing it while playback is stopped will start the currently loaded macro; pressing it while playback is running will stop it.

To change a configurable hotkey:
1. Open the **Settings** tab.
2. Click the hotkey field next to the function you want to change.
3. Press your desired key combination (at least one modifier key — Ctrl, Alt, or Shift — is required).
4. The new combination is applied and saved immediately.
5. Press **Reset** to restore the factory default.

> **Note:** Hotkeys require at least one modifier key. A hotkey cannot be fully cleared; it always falls back to a safe default.

---

## 7. Emergency Stop

**Hotkey: `Ctrl + Alt + Pause`** (fixed, cannot be disabled)

The emergency-stop hotkey is always active while TapWeaver is running — even when the window is minimised or not focused. When pressed, it will:

1. Stop any running macro playback (the Sequencer status changes to **"Stopped by emergency hotkey"**).
2. Stop the auto-clicker.
3. Stop any ongoing recording.
4. Release all modifier keys (Ctrl, Alt, Shift, Win) that may have been left held down by a macro.
5. Release all mouse buttons (left, right, middle) that may be stuck in the "down" state.

You can also trigger the emergency stop manually by clicking the **🛑 Emergency Stop Now** button in the **Settings** tab.

> **Tip:** If you find yourself unable to control the computer because a macro is running too fast or looping, press **Ctrl + Alt + Pause** to immediately halt everything.

---

## 8. Settings

Open the **Settings** tab to configure:

| Setting                         | Description                                                           |
|----------------------------------|-----------------------------------------------------------------------|
| Always on top                    | Keeps the TapWeaver window above all other windows.                   |
| Sequencer Playback (Start/Stop)  | Global hotkey to start/stop sequencer playback (default Ctrl+F8).    |
| Toggle Recording                 | Global hotkey to start/stop recording (default Ctrl+F7).             |
| Toggle Auto Clicker              | Global hotkey to start/stop the auto-clicker (default Ctrl+F9).      |

Settings are persisted between sessions in `%AppData%\TapWeaver\settings.json`.

---

## 9. JSON Format Reference

```json
{
  "name": "Example Profile",
  "description": "Holds A, D, W in a loop",
  "created": "2024-01-01T00:00:00Z",
  "modified": "2024-06-01T12:00:00Z",
  "macro": {
    "name": "Example Macro",
    "version": 1,
    "repeatMode": "Infinite",
    "repeatCount": 0,
    "loopDelayMs": 0,
    "steps": [
      { "type": "KeyTap",    "key": "A", "holdMs": 3000 },
      { "type": "Delay",     "delayMs": 200 },
      { "type": "KeyTap",    "key": "D", "holdMs": 1500 },
      { "type": "Delay",     "delayMs": 100 },
      { "type": "KeyTap",    "key": "W", "holdMs": 1300 },
      { "type": "MouseClick","button": "Left", "x": 960, "y": 540 },
      { "type": "MoveMouse", "x": 100, "y": 200 }
    ]
  }
}
```

> **Tip:** Legacy files that contain only the plain `Macro` object (without a `macro` wrapper) are still
> supported — TapWeaver loads them automatically.

You can edit profile files in any text editor and reload them via **Open…**.
