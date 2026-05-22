using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class CompositeTextExtractor : ITextExtractor
{
    private readonly IReadOnlyList<ITextExtractor> _extractors;
    private readonly TextValidationService _textValidationService;

    public CompositeTextExtractor(
        IEnumerable<ITextExtractor> extractors,
        TextValidationService textValidationService)
    {
        _extractors = extractors.ToArray();
        _textValidationService = textValidationService;
    }

    public async Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        TextExtractionResult? lastFailure = null;

        foreach (var extractor in _extractors)
        {
            var result = await extractor.ExtractSelectedTextAsync(cancellationToken);
            if (result.Success)
            {
                if (_textValidationService.IsValid(result.Text))
                {
                    return result;
                }

                lastFailure = TextExtractionResult.Failed(
                    "Extracted text did not pass validation.",
                    result.Source,
                    TextExtractionFailureReason.InvalidText);
                continue;
            }

            lastFailure = result;
        }

        return lastFailure ?? TextExtractionResult.Failed(
            "No extractor was configured.",
            "Composite",
            TextExtractionFailureReason.NotSupported);
    }
}
