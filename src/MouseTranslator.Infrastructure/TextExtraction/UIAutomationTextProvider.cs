using System.Windows.Automation;
using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

internal sealed class UIAutomationTextProvider : IAutomationTextProvider
{
    public AutomationTextReadResult ReadFocusedSelection()
    {
        var element = AutomationElement.FocusedElement;
        if (element is null)
        {
            return AutomationTextReadResult.Failed(
                "No focused element.",
                "UIAutomation",
                TextExtractionFailureReason.NoFocusedElement);
        }

        var isPassword = element.GetCurrentPropertyValue(AutomationElement.IsPasswordProperty);
        if (isPassword is true)
        {
            return AutomationTextReadResult.Failed(
                "Password field skipped.",
                "UIAutomation",
                TextExtractionFailureReason.SensitiveField);
        }

        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObject))
        {
            var text = ReadTextPatternSelection((TextPattern)textPatternObject);
            return string.IsNullOrWhiteSpace(text)
                ? AutomationTextReadResult.Failed(
                    "Selection was empty.",
                    "UIAutomation",
                    TextExtractionFailureReason.EmptySelection)
                : AutomationTextReadResult.Succeeded(text, "UIAutomation");
        }

        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePatternObject))
        {
            var text = ((ValuePattern)valuePatternObject).Current.Value;
            return string.IsNullOrWhiteSpace(text)
                ? AutomationTextReadResult.Failed(
                    "ValuePattern was empty.",
                    "UIAutomation.ValuePattern",
                    TextExtractionFailureReason.EmptySelection)
                : AutomationTextReadResult.Succeeded(text, "UIAutomation.ValuePattern");
        }

        return AutomationTextReadResult.Failed(
            "No supported UI Automation pattern.",
            "UIAutomation",
            TextExtractionFailureReason.PatternNotSupported);
    }

    private static string ReadTextPatternSelection(TextPattern textPattern)
    {
        var ranges = textPattern.GetSelection();
        if (ranges is null || ranges.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, ranges.Select(static range => range.GetText(-1)));
    }
}
