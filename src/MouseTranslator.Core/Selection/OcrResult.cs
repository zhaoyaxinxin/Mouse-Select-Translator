namespace MouseTranslator.Core.Selection;

public sealed record OcrResult(
    bool Success,
    string? Text,
    string Source,
    string? ErrorMessage,
    double? Confidence,
    TextExtractionFailureReason FailureReason)
{
    public static OcrResult Succeeded(
        string text,
        string source,
        double? confidence = null)
    {
        return new OcrResult(true, text, source, null, confidence, TextExtractionFailureReason.None);
    }

    public static OcrResult Failed(
        string errorMessage,
        string source,
        TextExtractionFailureReason failureReason = TextExtractionFailureReason.UnknownError,
        double? confidence = null)
    {
        return new OcrResult(false, null, source, errorMessage, confidence, failureReason);
    }
}
