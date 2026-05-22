using System.Windows.Interop;
using MouseTranslator.Infrastructure.Win32;

namespace MouseTranslator.App;

public sealed class GlobalHotkeyManager : IDisposable
{
    private const int ToggleHotkeyId = 1;
    private const int HideHotkeyId = 2;

    private HwndSource? _messageWindow;
    private bool _started;

    public event EventHandler? ToggleRequested;

    public event EventHandler? HideRequested;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        var parameters = new HwndSourceParameters("MouseTranslatorHotkeys")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0,
        };

        _messageWindow = new HwndSource(parameters);
        _messageWindow.AddHook(WndProc);
        RegisterHotkeys(_messageWindow.Handle);
        _started = true;
    }

    private void RegisterHotkeys(nint handle)
    {
        var toggleRegistered = NativeMethods.RegisterHotKey(
            handle,
            ToggleHotkeyId,
            NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT | NativeMethods.MOD_NOREPEAT,
            (uint)Keys.T);

        var hideRegistered = NativeMethods.RegisterHotKey(
            handle,
            HideHotkeyId,
            NativeMethods.MOD_NOREPEAT,
            (uint)Keys.Escape);

        if (!toggleRegistered || !hideRegistered)
        {
            throw new InvalidOperationException("Failed to register one or more global hotkeys.");
        }
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            switch (wParam.ToInt32())
            {
                case ToggleHotkeyId:
                    ToggleRequested?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;

                case HideHotkeyId:
                    HideRequested?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
            }
        }

        return 0;
    }

    public void Dispose()
    {
        if (_messageWindow is null)
        {
            return;
        }

        NativeMethods.UnregisterHotKey(_messageWindow.Handle, ToggleHotkeyId);
        NativeMethods.UnregisterHotKey(_messageWindow.Handle, HideHotkeyId);
        _messageWindow.RemoveHook(WndProc);
        _messageWindow.Dispose();
        _messageWindow = null;
        _started = false;
    }
}
