namespace MouseTranslator.Core.Selection;

public sealed record SelectionOptions
{
    public int DragThresholdPixels { get; init; } = 8;

    public int ExtractionDelayMs { get; init; } = 150;

    public TimeSpan DuplicateIgnoreDuration { get; init; } = TimeSpan.FromSeconds(3);

    public string TargetLanguage { get; init; } = "zh-CN";

    public double OverlayOffsetY { get; init; } = 20;

    public double OverlayMaxWidth { get; init; } = 420;

    public double OverlayAutoHideSeconds { get; init; } = 10;

    public double OverlayDismissDistancePixels { get; init; } = 64;
}
