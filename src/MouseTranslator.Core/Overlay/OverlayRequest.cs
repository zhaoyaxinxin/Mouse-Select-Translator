namespace MouseTranslator.Core.Overlay;

public sealed record OverlayRequest(
    string OriginalText,
    string TranslatedText,
    double AnchorX,
    double AnchorY,
    bool IsError = false);
