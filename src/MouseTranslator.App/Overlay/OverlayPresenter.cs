using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Threading;
using MouseTranslator.Core.Overlay;
using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.Win32;

namespace MouseTranslator.App;

public sealed class OverlayPresenter : IOverlayPresenter, IDisposable
{
    private const double OverlayMargin = 8d;
    private readonly SelectionOptions _selectionOptions;
    private readonly OverlayPlacementCalculator _overlayPlacementCalculator = new();
    private readonly DispatcherTimer _movementWatchTimer;
    private readonly CursorPositionService _cursorPositionService = new();

    private OverlayWindow? _overlayWindow;
    private (int X, int Y) _shownCursorPosition;
    private bool _disposed;

    public OverlayPresenter(SelectionOptions selectionOptions)
    {
        _selectionOptions = selectionOptions;
        _movementWatchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100),
        };
        _movementWatchTimer.Tick += OnMovementWatchTick;
    }

    public void Show(OverlayRequest request)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _overlayWindow ??= new OverlayWindow();
        var screen = Screen.FromPoint(new System.Drawing.Point((int)request.AnchorX, (int)request.AnchorY));
        var maxWidth = Math.Min(_selectionOptions.OverlayMaxWidth, screen.WorkingArea.Width - (OverlayMargin * 2));
        var maxHeight = Math.Max(120, screen.WorkingArea.Height - (OverlayMargin * 2));

        _overlayWindow.ShowRequest(request, new OverlayPlacement(-10000, -10000), maxWidth, maxHeight);
        var transform = GetTransformFromDevice();
        var anchor = transform.Transform(new System.Windows.Point(request.AnchorX, request.AnchorY));
        var topLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
        var bottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));
        var dipScreenBounds = new ScreenBounds(
            topLeft.X,
            topLeft.Y,
            bottomRight.X,
            bottomRight.Y);

        var placement = _overlayPlacementCalculator.Calculate(
            anchor.X,
            anchor.Y,
            _overlayWindow.ActualWidth,
            _overlayWindow.ActualHeight,
            dipScreenBounds,
            _selectionOptions.OverlayOffsetY,
            OverlayMargin);

        _overlayWindow.ShowRequest(request, placement, maxWidth, maxHeight);
        _shownCursorPosition = _cursorPositionService.GetCursorPosition();
        _movementWatchTimer.Stop();
        _movementWatchTimer.Start();
    }

    public void Hide()
    {
        if (_overlayWindow is null)
        {
            return;
        }

        _movementWatchTimer.Stop();
        _overlayWindow.Hide();
    }

    private void OnMovementWatchTick(object? sender, EventArgs e)
    {
        var cursorPosition = _cursorPositionService.GetCursorPosition();
        var deltaX = cursorPosition.X - _shownCursorPosition.X;
        var deltaY = cursorPosition.Y - _shownCursorPosition.Y;
        var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
        var dismissDistanceSquared = _selectionOptions.OverlayDismissDistancePixels * _selectionOptions.OverlayDismissDistancePixels;

        if (distanceSquared >= dismissDistanceSquared)
        {
            Hide();
        }
    }

    private Matrix GetTransformFromDevice()
    {
        var source = PresentationSource.FromVisual(_overlayWindow!);
        return source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _movementWatchTimer.Stop();
        _movementWatchTimer.Tick -= OnMovementWatchTick;
        _overlayWindow?.Close();
    }
}
