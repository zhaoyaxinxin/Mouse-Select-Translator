namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed record BrowserSelectionPayload(
    string Text,
    string Source,
    string? Url,
    string? Title,
    bool IsPdf,
    DateTimeOffset CapturedAtUtc);
