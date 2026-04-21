# 🐟 Boris Bar — Windows Port

Windows system tray soundboard inspired by the Italian TV series *Boris*.
Play iconic audio clips from the show via global keyboard shortcuts, straight from the system tray.

This is a port of the original macOS app by [Andrea Ricciotti (PunxCode)](https://github.com/andrearicciotti1).
Original project: [andrearicciotti1/boris-bar](https://github.com/andrearicciotti1/boris-bar)

---

## ✨ Features

- 🐟 Fish icon in the system tray (no taskbar entry)
- 🎧 10 built-in clips with global shortcuts `Ctrl+Alt+0` … `Ctrl+Alt+9` (also `AltGr+0-9`)
- ➕ Load your own custom sounds via file picker
- ⏱ 30 s max per clip — press the same shortcut again to stop
- 🚀 Optional launch at Windows startup (toggle in the tray menu)
- 🪶 Native C# / .NET 8 — no Electron, no Python

---

## 📦 Installation

### Option A — Download (recommended)

1. Download `BorisBar-x.x.zip` from the latest [Release](../../releases/latest)
2. Extract the zip anywhere you like
3. **First launch — SmartScreen warning:** Windows may show *"Windows protected your PC"* because the app isn't signed with a paid certificate. Click **More info** → **Run anyway**
   (Alternative: right-click `BorisBar.exe` → **Properties** → tick **Unblock** → OK)
4. The fish icon appears in the system tray (bottom-right, near the clock — expand the `^` arrow if hidden)
5. Right-click the fish → pick a clip or configure the app

### Option B — Build from source (for development)

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/ManjoJr/boris-bar.git
cd boris-bar
dotnet run
```

The terminal must stay open while running. For a standalone exe see below.

---
---

## 🎵 Audio clips

Built-in clips are loaded from a `clips\` folder next to the executable.
Place your audio files there named after the clip's `Base` field (e.g. `0 - a cazzo di cane.mp3`, `7 - F4 basito.mp3`).

During development, put them in `assets\clips\` — they are copied to `bin\...\clips\` automatically at build time.

Custom sounds can also be added at runtime via **"Aggiungi suono personalizzato…"** in the tray menu.
They are stored in `%APPDATA%\BorisBar\custom\` and appear in the menu automatically.

Supported formats: `mp3`, `mp4`, `m4a`, `wav`, `aiff`, `aif`

> The *Boris* audio clips are property of RAI / Wildside / Sky and are **not** included in this repo.
> You must supply your own files. The app is intended for personal, non-commercial use.

---

## ⌨️ Default shortcuts

| Shortcut      | Clip                               |
|---------------|------------------------------------|
| `Ctrl+Alt+0`  | A cazzo di cane                    |
| `Ctrl+Alt+1`  | Cazzata                            |
| `Ctrl+Alt+2`  | La qualità                         |
| `Ctrl+Alt+3`  | Quanti cazzi                       |
| `Ctrl+Alt+4`  | Coffee break                       |
| `Ctrl+Alt+5`  | Sforzo                             |
| `Ctrl+Alt+6`  | Thank you for being so not italian |
| `Ctrl+Alt+7`  | F4                                 |
| `Ctrl+Alt+8`  | Fiano Romano                       |
| `Ctrl+Alt+9`  | Facciamoli scopare                 |

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
