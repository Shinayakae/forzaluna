# Forza Horizon 6 Luna — Open Source

The full, open-source source code for **Luna**, an all-in-one companion app for
Forza Horizon 6: live driving tools, garage control, teleportation, photo mode,
and deep tuning, in one clean Windows app.

If you would like to donate: https://ko-fi.com/ameshina

## Build

Requirements:

- Windows 10 or 11 (64-bit)
- .NET Framework 4.x developer tools (the `csc.exe` compiler ships with the
  framework, typically at
  `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`)

From this folder, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Build.ps1
```

The build produces `Release\Forza Horizon Luna Open Source.exe` together with the
runtime dependencies it needs next to it (SQLite + WebView2). Run the exe from
that `Release` folder.

## Layout

```
src\                 C# source (single WinForms application)
assets\              Icons, images, sounds, and bundled preset/teleport data
tools\packages\      Third-party reference assemblies (SQLite, WebView2)
app.manifest         Application manifest
Build.ps1            One-step build script
Release\             Build output (created by Build.ps1)
```

All UI images, sounds, and the bundled teleport/driving presets are compiled
into the exe as embedded resources. The `.txt` files under
`assets\teleport_lists\premade` and `assets\driving_presets\premade` are preset
**data**, not documentation.

## Dependencies

- [System.Data.SQLite](https://system.data.sqlite.org/) — local database work
- [Microsoft.Web.WebView2](https://developer.microsoft.com/microsoft-edge/webview2/)
  — the in-app web view

Both are included under `tools\packages` so the project builds offline.

## License

Released under the MIT License. See [LICENSE](LICENSE).

## Disclaimer

Luna is an independent, fan-made companion tool intended for single-player use.
It is not affiliated with, endorsed by, or connected to Microsoft, Xbox Game
Studios, Playground Games, or Turn 10 Studios. Forza and Forza Horizon are
trademarks of Microsoft Corporation. Use at your own risk.
