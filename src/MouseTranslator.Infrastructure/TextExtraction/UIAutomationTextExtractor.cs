using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class UIAutomationTextExtractor : ITextExtractor
{
    private readonly IAutomationTextProvider _automationTextProvider;

    public UIAutomationTextExtractor()
        : this(new UIAutomationTextProvider())
    {
    }

    internal UIAutomationTextExtractor(IAutomationTextProvider automationTextProvider)
    {
        _automationTextProvider = automationTextProvider;
    }

    public Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = _automationTextProvider.ReadFocusedSelection();
            return Task.FromResult(result.Success
                ? TextExtractionResult.Succeeded(result.Text!, result.Source)
                : TextExtractionResult.Failed(
                    result.ErrorMessage ?? "UI Automation failed.",
                    result.Source,
                    result.FailureReason));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TextExtractionResult.Failed(
                ex.Message,
                "UIAutomation",
                TextExtractionFailureReason.UnknownError));
        }
    }
}
