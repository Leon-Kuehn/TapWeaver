# TapWeaver — User Guide

## Table of Contents
1. [Recording a Macro](#1-recording-a-macro)
2. [Creating a Macro Manually](#2-creating-a-macro-manually)
3. [Playback and Repeat Options](#3-playback-and-repeat-options)
4. [The Auto Clicker](#4-the-auto-clicker)
5. [Saving and Loading Macros](#5-saving-and-loading-macros)
6. [JSON Format Reference](#6-json-format-reference)

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

During playback the currently executing step is highlighted in blue in the grid.

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

- **Save / Save As…** — saves the current macro to a `.json` file.
- **Open…** — loads a macro from a `.json` file.
- **New** — clears the editor for a fresh macro.

---

## 6. JSON Format Reference

```json
{
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
```

You can edit this file in any text editor and reload it via **Open…**.
