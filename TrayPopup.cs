namespace TarkovRichPresence;

class TrayPopup : Form
{
    private readonly AppSettings _settings;
    private readonly Button _button;

    public TrayPopup()
    {
        _settings = AppSettings.Load();

        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;

        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(220, 70);
        BackColor = Color.FromArgb(30, 30, 30);
        Padding = new Padding(10);

        _button = new Button
        {
            Text = ExeButtonText(),
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(50, 50, 50),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f),
        };
        _button.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _button.Click += OnSelectExe;

        Controls.Add(_button);
    }

    protected override bool ShowWithoutActivation => false;

    private void OnSelectExe(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select Tarkov EXE",
            Filter = "Executable files (*.exe)|*.exe",
            CheckFileExists = true,
            FileName = _settings.ExePath ?? string.Empty,
        };
        try
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _settings.ExePath = dialog.FileName;

                _settings.PlayerId = PlayerIdFetcher.GetPlayerIdFromLog(_settings.ExeDir + "\\Logs");

                _settings.Save();

                Console.WriteLine("User selected a new Tarkov EXE. Path: " + dialog.FileName);
            }
            else
            {
                Console.WriteLine("User canceled the file selection.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error occurred while selecting Tarkov EXE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        Close();
    }

    private string ExeButtonText() =>
        _settings.ExePath is { } path
            ? Path.GetFileName(path)
            : "Select Tarkov EXE";
}
