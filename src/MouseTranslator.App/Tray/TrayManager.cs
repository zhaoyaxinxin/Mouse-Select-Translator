using System.Drawing;
using System.Windows.Forms;

namespace MouseTranslator.App;

public sealed class TrayManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;

    public TrayManager()
    {
        _toggleItem = new ToolStripMenuItem("Pause");
        _toggleItem.Click += OnToggleClicked;

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExitClicked;

        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Mouse Select Translator",
            ContextMenuStrip = menu,
        };
        _notifyIcon.DoubleClick += OnToggleClicked;
    }

    public event EventHandler? ToggleRequested;

    public event EventHandler? ExitRequested;

    public void SetEnabled(bool enabled)
    {
        _toggleItem.Text = enabled ? "Pause" : "Resume";
        _notifyIcon.Text = enabled
            ? "Mouse Select Translator - Enabled"
            : "Mouse Select Translator - Paused";
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        ToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        if (_notifyIcon.ContextMenuStrip is not null)
        {
            _notifyIcon.ContextMenuStrip.Dispose();
        }

        _notifyIcon.Dispose();
    }
}
