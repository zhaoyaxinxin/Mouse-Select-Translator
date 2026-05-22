using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class NullOcrTextExtractor : IOcrTextExtractor
{
    public Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(TextExtractionResult.Failed(
            "OCR is not configured.",
            "OCR",
            TextExtractionFailureReason.NotSupported));
    }

    public Task<TextExtractionResult> ExtractFromSelectionAsync(
        SelectionEvent selectionEvent,
        CancellationToken cancellationToken)
    {
        return ExtractSelectedTextAsync(cancellationToken);
    }
}
