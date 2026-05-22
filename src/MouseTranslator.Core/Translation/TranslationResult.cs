namespace MouseTranslator.Core.Translation;

public sealed record TranslationResult(
    bool Success,
    string? TranslatedText,
    string? SourceLanguage,
    string? TargetLanguage,
    string? ErrorMessage)
{
    public static TranslationResult Succeeded(
        string translatedText,
        string? sourceLanguage,
        string? targetLanguage)
    {
        return new TranslationResult(true, translatedText, sourceLanguage, targetLanguage, null);
    }

    public static TranslationResult Failed(string errorMessage)
    {
        return new TranslationResult(false, null, null, null, errorMessage);
    }
}
