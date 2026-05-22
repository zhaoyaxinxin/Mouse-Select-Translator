using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

internal sealed record AutomationTextReadResult(
    bool Success,
    string Source,
    string? Text,
    string? ErrorMessage,
    TextExtractionFailureReason FailureReason)
{
    public static AutomationTextReadResult Succeeded(string text, string source)
    {
        return new AutomationTextReadResult(true, source, text, null, TextExtractionFailureReason.None);
    }

    public static AutomationTextReadResult Failed(
        string errorMessage,
        string source,
        TextExtractionFailureReason failureReason)
    {
        return new AutomationTextReadResult(false, source, null, errorMessage, failureReason);
    }
}
