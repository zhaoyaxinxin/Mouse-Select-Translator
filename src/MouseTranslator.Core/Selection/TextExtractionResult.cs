namespace MouseTranslator.Core.Selection;

public sealed record TextExtractionResult(
    bool Success,
    string? Text,
    string Source,
    string? ErrorMessage,
    TextExtractionFailureReason FailureReason)
{
    public static TextExtractionResult Succeeded(string text, string source)
    {
        return new TextExtractionResult(true, text, source, null, TextExtractionFailureReason.None);
    }

    public static TextExtractionResult Failed(
        string errorMessage,
        string source = "Unknown",
        TextExtractionFailureReason failureReason = TextExtractionFailureReason.UnknownError)
    {
        return new TextExtractionResult(false, null, source, errorMessage, failureReason);
    }
}
