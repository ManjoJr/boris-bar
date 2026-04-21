# 🐟 Boris Bar — Windows Port

Windows system tray soundboard inspired by the Italian TV series *Boris*.
Play iconic audio clips from the show via global keyboard shortcuts, straight from the system tray.

This is a port of the original macOS app by [Andrea Ricciotti (PunxCode)](https://github.com/andrearicciotti1).
Original project: [andrearicciotti1/boris-bar](https://github.com/andrearicciotti1/boris-bar)

---

## ✨ Features

- 🐟 Fish icon in the system tray (no taskbar entry)
- 🎧 9 built-in clips with global shortcuts `Alt+Win+1` … `Alt+Win+9`
- ➕ Load your own custom sounds via file picker
- ⏱ 30 s max per clip — press the same shortcut again to stop
- 🚀 Optional launch at Windows startup (toggle in the tray menu)
- 🪶 Native C# / .NET 8 — no Electron, no Python

---

## 📦 Requirements

- Windows 10 or 11 (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🚀 Run (development)

```powershell
git clone https://github.com/ManjoJr/boris-bar.git
cd boris-bar
dotnet run
```

The app appears in the system tray. The terminal must stay open while running.

---

## 📦 Build a standalone exe

```powershell
dotnet publish -c Release -o publish
```

Run `publish\BorisBar.exe` directly — no terminal needed.

---

## 🎵 Audio clips

Built-in clips are loaded from a `clips\` folder next to the executable.
Place your audio files there named after the clip label (e.g. `F4.mp3`, `Tutti basiti.mp3`).

During development, put them in `assets\clips\` — they are copied to `bin\...\clips\` automatically at build time.

Custom sounds can also be added at runtime via **"Aggiungi suono personalizzato…"** in the tray menu.
They are stored in `%APPDATA%\BorisBar\custom\` and appear in the menu automatically.

Supported formats: `mp3`, `mp4`, `m4a`, `wav`, `aiff`, `aif`

> The *Boris* audio clips are property of RAI / Wildside / Sky and are **not** included in this repo.
> You must supply your own files. The app is intended for personal, non-commercial use.

---

## ⌨️ Default shortcuts

| Shortcut     | Clip                               |
|--------------|------------------------------------|
| `Alt+Win+1`  | F4                                 |
| `Alt+Win+2`  | Tutti basiti                       |
| `Alt+Win+3`  | A cazzo di cane                    |
| `Alt+Win+4`  | Fai uno sforzo                     |
| `Alt+Win+5`  | Fiano Romano                       |
| `Alt+Win+6`  | Però sei molto italiano            |
| `Alt+Win+7`  | Thank you for being so not italian |
| `Alt+Win+8`  | Io la mollo questa serie           |
| `Alt+Win+9`  | Vuoi una pompa                     |

Shortcuts work system-wide, even when the menu is closed.
Press the same shortcut again to stop playback.

---

## 🛠 Stack

- **C#** — single-file `App.cs`, ~300 lines
- **WinForms** — `NotifyIcon`, `ContextMenuStrip`
- **NAudio** — `MediaFoundationReader` + `WaveOutEvent` for audio playback
- **Win32** — `RegisterHotKey` for system-wide shortcuts (no accessibility permissions needed)
- **Registry** — `HKCU\...\Run` for startup

---

## 📜 License

[MIT](LICENSE) for the code.

---

## 🤘 Credits

Windows port by [ManjoJr](https://github.com/ManjoJr)
Based on the original macOS app by [Andrea Ricciotti (PunxCode)](https://github.com/andrearicciotti1)
Original repo: [andrearicciotti1/boris-bar](https://github.com/andrearicciotti1/boris-bar)

---

> *"Se mi permette...a cazzo di cane."*
