namespace MouseTranslator.Core.Selection;

public sealed record OcrRequest(
    byte[] ImageBytes,
    string ImageFormat,
    string Language,
    string Source);
