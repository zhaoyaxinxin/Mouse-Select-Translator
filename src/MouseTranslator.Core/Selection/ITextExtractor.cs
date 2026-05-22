namespace MouseTranslator.Core.Selection;

public interface ITextExtractor
{
    Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken);
}
