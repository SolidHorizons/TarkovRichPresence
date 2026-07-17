namespace TarkovRichPresence;

class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
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
            Icon = SystemIcons.Application,
            Text = "Tarkov Rich Presence",
            Visible = true,
        };

        _trayIcon.Click += OnTrayIconClick;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = contextMenu;

        StartProcessWatcher();
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

        RPCManager.getInstance.setDiscordRpcStatus("mainmenu");
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
