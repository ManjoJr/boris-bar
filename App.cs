// Boris Bar — Windows port (C# .NET 8 WinForms)
// Global hotkeys: Alt+Win+1-9  |  Custom sounds: %APPDATA%\BorisBar\custom\
// Build: dotnet publish -c Release
// Run:   dotnet run

using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NAudio.Wave;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

record Clip(string Label, string Base, int Id, string Shortcut);

// Invisible window that owns the hotkey registrations and can safely BeginInvoke.
sealed class HostForm : Form
{
    public event Action<int>? HotKey;

    public HostForm()
    {
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        Text = "BorisBarHost";
        _ = Handle; // force Win32 handle creation now
    }

    protected override void SetVisibleCore(bool v) => base.SetVisibleCore(false);

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312) // WM_HOTKEY
            HotKey?.Invoke(m.WParam.ToInt32());
        base.WndProc(ref m);
    }
}

sealed class BorisBarApp : ApplicationContext
{
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr hIcon);

    const int MOD_ALT = 0x0001, MOD_WIN = 0x0008, MOD_NOREPEAT = 0x4000;

    static readonly Clip[] Clips =
    [
        new("F4",                                 "F4",                                 1, "Alt+Win+1"),
        new("Tutti basiti",                       "Tutti basiti",                       2, "Alt+Win+2"),
        new("A cazzo di cane",                    "a cazzo di cane",                    3, "Alt+Win+3"),
        new("Fai uno sforzo",                     "fai uno sforzo",                     4, "Alt+Win+4"),
        new("Fiano Romano",                       "Fiano Romano",                       5, "Alt+Win+5"),
        new("Però sei molto italiano",            "Però sei molto italiano",            6, "Alt+Win+6"),
        new("Thank you for being so not italian", "Thank you for being so not italian", 7, "Alt+Win+7"),
        new("Io la mollo questa serie",           "Io la mollo questa serie",           8, "Alt+Win+8"),
        new("Vuoi una pompa",                     "Vuoi una pompa",                     9, "Alt+Win+9"),
    ];

    static readonly string[] AudioExts = ["mp3", "mp4", "m4a", "wav", "aiff", "aif"];

    readonly HostForm _host;
    readonly NotifyIcon _tray;
    readonly System.Windows.Forms.Timer _stopTimer;
    IWavePlayer? _wave;
    WaveStream? _stream;
    string? _nowPlaying;

    string CustomDir
    {
        get
        {
            var d = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BorisBar", "custom");
            Directory.CreateDirectory(d);
            return d;
        }
    }

    public BorisBarApp()
    {
        _host = new HostForm();
        _host.HotKey += id =>
        {
            var c = Array.Find(Clips, c => c.Id == id);
            if (c != null) PlayBuiltIn(c.Base);
        };

        for (int i = 0; i < Clips.Length; i++)
            RegisterHotKey(_host.Handle, Clips[i].Id, MOD_WIN | MOD_ALT | MOD_NOREPEAT, (int)Keys.D1 + i);

        _stopTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _stopTimer.Tick += (_, _) => StopPlayback();

        var menu = new ContextMenuStrip();
        menu.Opening += (_, _) => BuildMenu(menu);

        _tray = new NotifyIcon
        {
            Icon = DrawFish(),
            Text = "Boris Bar",
            ContextMenuStrip = menu,
            Visible = true,
        };
    }

    // ── Menu ──────────────────────────────────────────────────────────────────

    void BuildMenu(ContextMenuStrip m)
    {
        m.Items.Clear();

        foreach (var c in Clips)
        {
            var clip = c;
            m.Items.Add(new ToolStripMenuItem(
                $"{c.Label}  ({c.Shortcut})", null,
                (_, _) => PlayBuiltIn(clip.Base)));
        }

        var customs = Directory.GetFiles(CustomDir)
            .Where(f => AudioExts.Contains(
                Path.GetExtension(f).TrimStart('.'),
                StringComparer.OrdinalIgnoreCase))
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (customs.Count > 0)
        {
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add(new ToolStripMenuItem("── Suoni personalizzati ──") { Enabled = false });
            foreach (var p in customs)
            {
                var path = p;
                m.Items.Add(new ToolStripMenuItem(
                    Path.GetFileNameWithoutExtension(p), null,
                    (_, _) => Play(path)));
            }
        }

        m.Items.Add(new ToolStripSeparator());
        m.Items.Add(new ToolStripMenuItem(
            "Aggiungi suono personalizzato…", null, (_, _) => AddSound()));
        m.Items.Add(new ToolStripMenuItem(
            "Apri cartella suoni", null,
            (_, _) => Process.Start(new ProcessStartInfo("explorer.exe", $"\"{CustomDir}\"")
                { UseShellExecute = true })));

        m.Items.Add(new ToolStripSeparator());

        var startupItem = new ToolStripMenuItem("Avvia con Windows")
        {
            Checked = IsStartupEnabled(),
            CheckOnClick = false,
        };
        startupItem.Click += (_, _) => ToggleStartup();
        m.Items.Add(startupItem);

        m.Items.Add(new ToolStripMenuItem("Esci", null, (_, _) => Quit()));
    }

    // ── Playback ──────────────────────────────────────────────────────────────

    void PlayBuiltIn(string baseName)
    {
        foreach (var ext in AudioExts)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "clips", $"{baseName}.{ext}");
            if (File.Exists(path)) { Play(path); return; }
        }
    }

    void Play(string path)
    {
        // Toggle: same clip playing → stop.
        if (_nowPlaying == path && _wave?.PlaybackState == PlaybackState.Playing)
        {
            StopPlayback();
            return;
        }
        StopPlayback();
        try
        {
            _stream = new MediaFoundationReader(path);
            _wave = new WaveOutEvent();
            _wave.Init(_stream);
            _wave.PlaybackStopped += (_, _) =>
            {
                if (_host.IsHandleCreated)
                    _host.BeginInvoke(StopPlayback);
            };
            _wave.Play();
            _nowPlaying = path;
            _stopTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Impossibile riprodurre il file:\n{ex.Message}",
                "Boris Bar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    void StopPlayback()
    {
        _stopTimer.Stop();
        _wave?.Stop();
        _wave?.Dispose();
        _wave = null;
        _stream?.Dispose();
        _stream = null;
        _nowPlaying = null;
    }

    // ── Custom sound management ───────────────────────────────────────────────

    void AddSound()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Scegli un file audio",
            Filter = "File audio|*.mp3;*.mp4;*.m4a;*.wav;*.aiff;*.aif|Tutti i file|*.*",
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var dst = Path.Combine(CustomDir, Path.GetFileName(dlg.FileName));
        if (File.Exists(dst)) File.Delete(dst);
        File.Copy(dlg.FileName, dst);
    }

    // ── Startup ───────────────────────────────────────────────────────────────

    const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    bool IsStartupEnabled()
    {
        using var k = Registry.CurrentUser.OpenSubKey(RunKey);
        return k?.GetValue("BorisBar") != null;
    }

    void ToggleStartup()
    {
        using var k = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)!;
        if (IsStartupEnabled())
            k.DeleteValue("BorisBar", throwOnMissingValue: false);
        else
            k.SetValue("BorisBar", $"\"{Application.ExecutablePath}\"");
    }

    // ── Icon (drawn from SVG geometry, scaled to 32×32) ───────────────────────

    Icon DrawFish()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var dark = new SolidBrush(Color.FromArgb(0xC9, 0x30, 0x10));
            using var red  = new SolidBrush(Color.FromArgb(0xE8, 0x40, 0x1A));
            using var lite = new SolidBrush(Color.FromArgb(110, 0xF2, 0x60, 0x35));

            // Tail (left, darker)
            Point[] tail = [new(6, 16), new(2, 10), new(2, 22)];
            g.FillPolygon(dark, tail);
            // Body ellipse
            g.FillEllipse(red, 6, 10, 18, 12);
            // Belly highlight
            g.FillEllipse(lite, 9, 14, 10, 5);
            // Eye white
            g.FillEllipse(Brushes.White, 18, 11, 6, 6);
            // Eye pupil
            g.FillEllipse(Brushes.Black, 19, 12, 4, 4);
            // Eye glint
            g.FillEllipse(Brushes.White, 21, 12, 1, 1);
        }
        var hIcon = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(hIcon).Clone();
        DestroyIcon(hIcon);
        return icon;
    }

    // ── Exit ──────────────────────────────────────────────────────────────────

    void Quit()
    {
        StopPlayback();
        foreach (var c in Clips)
            UnregisterHotKey(_host.Handle, c.Id);
        _tray.Visible = false;
        Application.Exit();
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new BorisBarApp());
    }
}
