namespace MouseTranslator.Core.Translation;

public sealed record TranslationRequest(
    string Text,
    string? SourceLanguage,
    string TargetLanguage);
