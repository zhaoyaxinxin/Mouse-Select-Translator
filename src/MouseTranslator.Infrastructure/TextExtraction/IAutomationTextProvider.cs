namespace MouseTranslator.Infrastructure.TextExtraction;

internal interface IAutomationTextProvider
{
    AutomationTextReadResult ReadFocusedSelection();
}
