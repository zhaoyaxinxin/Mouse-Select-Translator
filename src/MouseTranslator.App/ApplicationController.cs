using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.TextExtraction;

namespace MouseTranslator.App;

public sealed class ApplicationController : IDisposable
{
    private readonly System.Windows.Application _application;
    private readonly SelectionCoordinator _selectionCoordinator;
    private readonly GlobalHotkeyManager _globalHotkeyManager;
    private readonly TrayManager _trayManager;
    private readonly OverlayPresenter _overlayPresenter;
    private readonly BrowserSelectionLoopbackServer? _browserSelectionLoopbackServer;
    private readonly bool _startMinimized;
    private readonly string _details;

    private MainWindow? _mainWindow;
    private bool _disposed;

    public ApplicationController(
        System.Windows.Application application,
        SelectionCoordinator selectionCoordinator,
        GlobalHotkeyManager globalHotkeyManager,
        TrayManager trayManager,
        OverlayPresenter overlayPresenter,
        BrowserSelectionLoopbackServer? browserSelectionLoopbackServer,
        bool enabledOnStartup,
        bool startMinimized,
        string details)
    {
        _application = application;
        _selectionCoordinator = selectionCoordinator;
        _globalHotkeyManager = globalHotkeyManager;
        _trayManager = trayManager;
        _overlayPresenter = overlayPresenter;
        _browserSelectionLoopbackServer = browserSelectionLoopbackServer;
        _startMinimized = startMinimized;
        _details = details;

        _selectionCoordinator.SetEnabled(enabledOnStartup);
        _selectionCoordinator.StateChanged += OnSelectionStateChanged;
        _globalHotkeyManager.ToggleRequested += OnToggleRequested;
        _globalHotkeyManager.HideRequested += OnHideRequested;
        _trayManager.ToggleRequested += OnToggleRequested;
        _trayManager.ExitRequested += OnExitRequested;
    }

    public void Start()
    {
        _browserSelectionLoopbackServer?.Start();
        _selectionCoordinator.Start();
        _globalHotkeyManager.Start();
        _trayManager.SetEnabled(_selectionCoordinator.Enabled);

        if (!_startMinimized)
        {
            _mainWindow = new MainWindow(_details);
            _application.MainWindow = _mainWindow;
            _mainWindow.Show();
        }
    }

    private void OnSelectionStateChanged(object? sender, SelectionState state)
    {
        _trayManager.SetEnabled(_selectionCoordinator.Enabled);
    }

    private void OnToggleRequested(object? sender, EventArgs e)
    {
        _selectionCoordinator.SetEnabled(!_selectionCoordinator.Enabled);
        _trayManager.SetEnabled(_selectionCoordinator.Enabled);
    }

    private void OnHideRequested(object? sender, EventArgs e)
    {
        _selectionCoordinator.HideOverlay();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _application.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _selectionCoordinator.StateChanged -= OnSelectionStateChanged;
        _globalHotkeyManager.ToggleRequested -= OnToggleRequested;
        _globalHotkeyManager.HideRequested -= OnHideRequested;
        _trayManager.ToggleRequested -= OnToggleRequested;
        _trayManager.ExitRequested -= OnExitRequested;
        _selectionCoordinator.Dispose();
        _globalHotkeyManager.Dispose();
        _trayManager.Dispose();
        _browserSelectionLoopbackServer?.Dispose();
        _overlayPresenter.Dispose();
    }
}
