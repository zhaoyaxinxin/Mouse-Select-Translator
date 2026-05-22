using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Win32;

public sealed class Win32MouseHook : IMouseSelectionMonitor
{
    private readonly int _dragThresholdPixels;
    private readonly SynchronizationContext? _synchronizationContext;
    private readonly NativeMethods.LowLevelMouseProc _hookCallback;

    private nint _hookHandle;
    private bool _started;
    private bool _mouseDown;
    private bool _dragging;
    private NativeMethods.Point _dragStartPoint;

    public Win32MouseHook(int dragThresholdPixels)
    {
        _dragThresholdPixels = dragThresholdPixels;
        _synchronizationContext = SynchronizationContext.Current;
        _hookCallback = HookCallback;
    }

    public event EventHandler<SelectionGestureEventArgs>? SelectionGestureCompleted;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        var moduleHandle = NativeMethods.GetModuleHandle(module?.ModuleName);
        _hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _hookCallback, moduleHandle, 0);
        if (_hookHandle == nint.Zero)
        {
            throw new InvalidOperationException("Failed to install the low-level mouse hook.");
        }

        _started = true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        if (_hookHandle != nint.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = nint.Zero;
        }

        _mouseDown = false;
        _dragging = false;
        _started = false;
    }

    private nint HookCallback(int nCode, nuint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            switch ((int)wParam)
            {
                case NativeMethods.WM_LBUTTONDOWN:
                    _mouseDown = true;
                    _dragging = false;
                    _dragStartPoint = hookStruct.Pt;
                    break;

                case NativeMethods.WM_MOUSEMOVE:
                    if (_mouseDown && !_dragging && IsDrag(_dragStartPoint, hookStruct.Pt))
                    {
                        _dragging = true;
                    }

                    break;

                case NativeMethods.WM_LBUTTONUP:
                    if (_mouseDown && _dragging)
                    {
                        var selectionEvent = new SelectionEvent(
                            _dragStartPoint.X,
                            _dragStartPoint.Y,
                            hookStruct.Pt.X,
                            hookStruct.Pt.Y,
                            DateTimeOffset.UtcNow);

                        RaiseSelectionGestureCompleted(selectionEvent);
                    }

                    _mouseDown = false;
                    _dragging = false;
                    break;
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private void RaiseSelectionGestureCompleted(SelectionEvent selectionEvent)
    {
        var args = new SelectionGestureEventArgs(selectionEvent);
        if (_synchronizationContext is not null)
        {
            _synchronizationContext.Post(_ => SelectionGestureCompleted?.Invoke(this, args), null);
            return;
        }

        SelectionGestureCompleted?.Invoke(this, args);
    }

    private bool IsDrag(NativeMethods.Point start, NativeMethods.Point end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        var distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        return distance >= _dragThresholdPixels;
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
