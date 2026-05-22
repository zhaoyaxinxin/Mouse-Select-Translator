namespace MouseTranslator.Infrastructure.Win32;

public sealed class CursorPositionService
{
    public (int X, int Y) GetCursorPosition()
    {
        return NativeMethods.GetCursorPos(out var point)
            ? (point.X, point.Y)
            : (0, 0);
    }
}
