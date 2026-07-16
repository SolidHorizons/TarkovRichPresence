namespace TarkovRichPresence;

class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private TrayPopup? _popup;

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
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }
}
