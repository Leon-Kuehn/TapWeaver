# TapWeaver

TapWeaver is a Windows desktop automation app built with C#/.NET 8 and WPF.
It combines macro recording, sequence editing and controlled playback in a clean MVVM architecture.

## Features
- Macro recording with keyboard and mouse hooks
- Macro sequencer with editable step timeline and repeat modes
- Global hotkeys for start/stop and emergency stop
- Optional keyboard routing to a selected target window (without focus)
- Auto-clicker with configurable behavior

## Project Structure
- src/TapWeaver.UI/Views: WPF views
- src/TapWeaver.UI/ViewModels: MVVM view models
- src/TapWeaver.UI/Assets and src/TapWeaver.UI/Themes: UI resources, icons and styles
- src/TapWeaver.Core/Models: shared domain models
- src/TapWeaver.Core/Services: playback, recording, hotkeys and runtime services
- src/TapWeaver.Core/Input: input abstractions and simulators
- src/TapWeaver.Core/Interop: isolated Win32 P/Invoke declarations

## Requirements
- Windows 10 or newer (x64)
- .NET 8 SDK (for build) or .NET runtime as required by your workflow

## Clone And Build
```powershell
git clone https://github.com/Leon-Kuehn/TapWeaver.git
cd TapWeaver
dotnet build TapWeaver.slnx -c Release
```

## Run During Development
```powershell
dotnet run --project src/TapWeaver.UI
```

## Release Build Generate EXE
Use the UI project as the main publish target.

```powershell
cd src/TapWeaver.UI
dotnet publish TapWeaver.UI.csproj -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true
```

The output is written to:

```text
artifacts/win-x64/TapWeaver.exe
```

Before creating a new release, delete old output in artifacts/win-x64 to avoid version mix-ups.

## Known Limitations
Keyboard routing via posted window messages works for many standard desktop applications.
Some games (for example Roblox) may ignore these messages because they use raw input, direct input, anti-cheat protections or custom input pipelines.

## Documentation
- docs/USER_GUIDE.md
- docs/DEVELOPER_GUIDE.md

## License
MIT, see LICENSE.
