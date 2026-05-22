namespace MouseTranslator.Core.Overlay;

public sealed class OverlayPlacementCalculator
{
    public OverlayPlacement Calculate(
        double mouseX,
        double mouseY,
        double overlayWidth,
        double overlayHeight,
        ScreenBounds screenBounds,
        double offsetY = 20,
        double margin = 8)
    {
        var availableWidth = Math.Max(1, screenBounds.Right - screenBounds.Left - (margin * 2));
        var availableHeight = Math.Max(1, screenBounds.Bottom - screenBounds.Top - (margin * 2));
        overlayWidth = Math.Min(overlayWidth, availableWidth);
        overlayHeight = Math.Min(overlayHeight, availableHeight);

        var left = mouseX - (overlayWidth / 2d);
        var top = mouseY - overlayHeight - offsetY;

        if (top < screenBounds.Top + margin)
        {
            top = mouseY + offsetY;
        }

        var minLeft = screenBounds.Left + margin;
        var maxLeft = Math.Max(minLeft, screenBounds.Right - overlayWidth - margin);
        var minTop = screenBounds.Top + margin;
        var maxTop = Math.Max(minTop, screenBounds.Bottom - overlayHeight - margin);

        left = Math.Clamp(left, minLeft, maxLeft);
        top = Math.Clamp(top, minTop, maxTop);

        return new OverlayPlacement(left, top);
    }
}
