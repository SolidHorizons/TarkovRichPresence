using System.Diagnostics;

namespace TarkovRichPresence;

class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    private TrayPopup? _popup;
    private AppSettings _settings = AppSettings.Load();
    private readonly LogController _logController = new();
    private ProcessWatcher? _processWatcher;
    private CancellationTokenSource? _sessionCts;

    public AppSettings getAppSettings()
    {
        return _settings;
    }

    public TrayApplicationContext()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? SystemIcons.Application,
            Text = "Tarkov Rich Presence",
            Visible = true,
        };

        _trayIcon.Click += OnTrayIconClick;

        _contextMenu = new ContextMenuStrip();
        var version = VersionController.getVersion();
        _contextMenu.Items.Add($"Version {version?.Split('+')[0]}").Enabled = false;

        var startupItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = StartupController.IsEnabled(),
        };
        startupItem.Click += (_, _) => StartupController.SetEnabled(startupItem.Checked);
        _contextMenu.Items.Add(startupItem);

        _contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = _contextMenu;

        StartProcessWatcher();
        _ = CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        var latestRelease = await VersionController.getLatestRelease();
        if (latestRelease == null || !VersionController.IsNewer(latestRelease.TagName, VersionController.getVersion()))
            return;

        _trayIcon.ShowBalloonTip(
            10000,
            "Update Available",
            $"Tarkov Rich Presence {latestRelease.TagName} is available. You are running {VersionController.getVersion()}.",
            ToolTipIcon.Info);

        var updateItem = new ToolStripMenuItem($"Update available: {latestRelease.TagName}");
        updateItem.Click += (_, _) => Process.Start(new ProcessStartInfo(latestRelease.HtmlUrl) { UseShellExecute = true });
        _contextMenu.Items.Insert(0, updateItem);
    }

    // Idle by default: only a lightweight timer polls for the Tarkov process name.
    // Nothing else (log watching, API calls, RPC presence) runs until Tarkov is detected.
    private void StartProcessWatcher()
    {
        if (string.IsNullOrEmpty(_settings.ExePath))
        {
            FileLogger.Log("[TrayApplicationContext] No Tarkov EXE configured yet; process detection is disabled until one is selected.");
            return;
        }

        string processName = Path.GetFileNameWithoutExtension(_settings.ExePath);
        _processWatcher = new ProcessWatcher(processName);
        _processWatcher.ProcessStarted += OnTarkovStarted;
        _processWatcher.ProcessStopped += OnTarkovStopped;
        FileLogger.Log($"[TrayApplicationContext] Watching for process '{processName}'.");
    }

    private void OnTarkovStarted()
    {
        FileLogger.Log("[TrayApplicationContext] Tarkov detected, waking up.");

        _sessionCts?.Cancel();
        _sessionCts = new CancellationTokenSource();
        var token = _sessionCts.Token;

        Task.Run(() => FetchAndApplyPlayerProfile(token), token);
        Task.Run(() => _logController.RunWatchers(token), token);
    }

    private void OnTarkovStopped()
    {
        FileLogger.Log("[TrayApplicationContext] Tarkov no longer running, going idle.");
        _sessionCts?.Cancel();
        RPCManager.getInstance.ClearPresence();
    }

    private async Task FetchAndApplyPlayerProfile(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_settings.PlayerId))
        {
            FileLogger.Log("[TrayApplicationContext] No PlayerId configured; skipping profile fetch.");
            return;
        }

        Gamemode gamemode = Gamemode.PvE; // Should retrieve gamemode somehow, Found a way to get gamemode from the application log, look for "Session Mode: Pve"

        var profile = await TarkovDevApiController.GetPlayerProfile(_settings.PlayerId!, gamemode);
        if (token.IsCancellationRequested)
            return;

        if (profile != null)
        {
            var info = profile.Info;
            FileLogger.Log($"[TarkovDevApiController] Fetched player profile: {profile}");

            var playerData = new PlayerData
            {
                Nickname = info?.Nickname ?? "",
                Experience = LevelCalculator.GetLevelForXp(info?.Experience ?? 0),
                Mode = gamemode,
                PlayerFaction = info?.Side switch
                {
                    "Bear" => Faction.Bear,
                    "Usec" => Faction.USEC,
                    "Savage" => Faction.Scav,
                    _ => Faction.Unknown
                }
            };
            RPCManager.getInstance.setPlayerData(playerData);
        }
        else
        {
            FileLogger.Log("Failed to fetch player profile.");
        }

        RPCManager.getInstance.setDiscordRpcStatus(
            "mainmenu",
            menuScreen =>
            {
                MenuScreen? screen = TarkovRPStates.GetMenuScreen(menuScreen);

                if (screen == null)
                {
                    Console.WriteLine($"Location '{menuScreen}' not found in TarkovRPStates.");
                    return null;
                }

                return RPCManager.getInstance.CreateMenuScreenPresence(screen);
            });
    }

    private void ReloadSettingsAndWatcher()
    {
        _settings = AppSettings.Load();

        if (_processWatcher != null)
        {
            _processWatcher.ProcessStarted -= OnTarkovStarted;
            _processWatcher.ProcessStopped -= OnTarkovStopped;
            _processWatcher.Dispose();
            _processWatcher = null;
        }

        StartProcessWatcher();
    }

    private void OnTrayIconClick(object? sender, EventArgs e)
    {
        if (e is MouseEventArgs { Button: MouseButtons.Left })
            ShowPopup();
    }

    private void ShowPopup()
    {
        if (_popup is { IsDisposed: false })
        {
            _popup.Close();
            return;
        }

        _popup = new TrayPopup();
        _popup.FormClosed += (_, _) => ReloadSettingsAndWatcher();

        var iconRect = GetTrayIconBounds();
        var screen = Screen.FromPoint(iconRect.Location);
        var workArea = screen.WorkingArea;

        int x = Math.Clamp(iconRect.Left, workArea.Left, workArea.Right - _popup.Width);
        int y = workArea.Bottom - _popup.Height;

        _popup.Location = new Point(x, y);
        _popup.Show();
    }

    private static Rectangle GetTrayIconBounds()
    {
        var pos = Control.MousePosition;
        return new Rectangle(pos.X - 8, pos.Y - 8, 16, 16);
    }

    private void ExitApplication()
    {
        _sessionCts?.Cancel();
        _processWatcher?.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }
}
