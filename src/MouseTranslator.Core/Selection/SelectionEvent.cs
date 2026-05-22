namespace MouseTranslator.Core.Selection;

public sealed record SelectionEvent(
    int StartX,
    int StartY,
    int EndX,
    int EndY,
    DateTimeOffset OccurredAtUtc);
