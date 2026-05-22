using System.Windows;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Ocr;

public sealed class OcrRegionCalculator
{
    public const int DefaultPadding = 20;
    public const int MinimumWidth = 120;
    public const int MinimumHeight = 48;

    private readonly Func<OcrScreenBounds> _screenBoundsProvider;

    public OcrRegionCalculator(Func<OcrScreenBounds>? screenBoundsProvider = null)
    {
        _screenBoundsProvider = screenBoundsProvider ?? GetVirtualScreenBounds;
    }

    public OcrRegion Calculate(SelectionEvent selectionEvent)
    {
        var left = Math.Min(selectionEvent.StartX, selectionEvent.EndX);
        var top = Math.Min(selectionEvent.StartY, selectionEvent.EndY);
        var width = Math.Abs(selectionEvent.EndX - selectionEvent.StartX);
        var height = Math.Abs(selectionEvent.EndY - selectionEvent.StartY);

        ExpandToMinimumSize(ref left, ref width, MinimumWidth);
        ExpandToMinimumSize(ref top, ref height, MinimumHeight);

        left -= DefaultPadding;
        top -= DefaultPadding;
        width += DefaultPadding * 2;
        height += DefaultPadding * 2;

        return ClampToScreen(left, top, width, height);
    }

    private OcrRegion ClampToScreen(int left, int top, int width, int height)
    {
        var bounds = _screenBoundsProvider();
        var right = left + width;
        var bottom = top + height;
        var maxX = bounds.X + bounds.Width;
        var maxY = bounds.Y + bounds.Height;

        var clampedLeft = Math.Max(bounds.X, left);
        var clampedTop = Math.Max(bounds.Y, top);
        var clampedRight = Math.Min(maxX, right);
        var clampedBottom = Math.Min(maxY, bottom);

        if (clampedRight <= clampedLeft)
        {
            clampedRight = Math.Min(maxX, clampedLeft + 1);
        }

        if (clampedBottom <= clampedTop)
        {
            clampedBottom = Math.Min(maxY, clampedTop + 1);
        }

        return new OcrRegion(
            clampedLeft,
            clampedTop,
            clampedRight - clampedLeft,
            clampedBottom - clampedTop,
            DefaultPadding);
    }

    private static void ExpandToMinimumSize(ref int origin, ref int size, int minimumSize)
    {
        if (size >= minimumSize)
        {
            return;
        }

        var center = origin + (size / 2.0);
        size = minimumSize;
        origin = (int)Math.Floor(center - (size / 2.0));
    }

    private static OcrScreenBounds GetVirtualScreenBounds()
    {
        return new OcrScreenBounds(
            (int)SystemParameters.VirtualScreenLeft,
            (int)SystemParameters.VirtualScreenTop,
            (int)SystemParameters.VirtualScreenWidth,
            (int)SystemParameters.VirtualScreenHeight);
    }
}
