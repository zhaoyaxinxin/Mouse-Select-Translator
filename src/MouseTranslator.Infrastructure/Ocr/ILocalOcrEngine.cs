using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Ocr;

public interface ILocalOcrEngine
{
    Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken);
}
