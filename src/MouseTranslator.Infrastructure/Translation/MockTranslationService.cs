using MouseTranslator.Core.Translation;

namespace MouseTranslator.Infrastructure.Translation;

public sealed class MockTranslationService : ITranslationService
{
    public Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var translated = $"[Mock Translation] {request.Text}";
        return Task.FromResult(TranslationResult.Succeeded(translated, request.SourceLanguage, request.TargetLanguage));
    }
}
