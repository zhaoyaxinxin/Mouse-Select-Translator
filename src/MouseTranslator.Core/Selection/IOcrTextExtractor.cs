namespace MouseTranslator.Core.Selection;

public interface IOcrTextExtractor : ITextExtractor
{
    Task<TextExtractionResult> ExtractFromSelectionAsync(
        SelectionEvent selectionEvent,
        CancellationToken cancellationToken);
}
