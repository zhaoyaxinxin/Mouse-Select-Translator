using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.Ocr;

public sealed class LocalOcrTextExtractor : IOcrTextExtractor
{
    private readonly OcrRegionCalculator _regionCalculator;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ILocalOcrEngine _ocrEngine;
    private readonly string _language;

    public LocalOcrTextExtractor(
        OcrRegionCalculator regionCalculator,
        IScreenCaptureService screenCaptureService,
        ILocalOcrEngine ocrEngine,
        string language = "eng")
    {
        _regionCalculator = regionCalculator;
        _screenCaptureService = screenCaptureService;
        _ocrEngine = ocrEngine;
        _language = language;
    }

    public Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(TextExtractionResult.Failed(
            "OCR requires a selection region.",
            "OCR",
            TextExtractionFailureReason.NotSupported));
    }

    public async Task<TextExtractionResult> ExtractFromSelectionAsync(
        SelectionEvent selectionEvent,
        CancellationToken cancellationToken)
    {
        var region = _regionCalculator.Calculate(selectionEvent);
        var captureResult = await _screenCaptureService.CaptureAsync(
            new ScreenCaptureRequest(region),
            cancellationToken);

        if (!captureResult.Success || captureResult.ImageBytes is null)
        {
            return TextExtractionResult.Failed(
                captureResult.ErrorMessage ?? "OCR screen capture failed.",
                "OCR.Capture",
                TextExtractionFailureReason.UnknownError);
        }

        var ocrResult = await _ocrEngine.RecognizeAsync(
            new OcrRequest(captureResult.ImageBytes, captureResult.ImageFormat, _language, "OCR.Capture"),
            cancellationToken);

        if (!ocrResult.Success || string.IsNullOrWhiteSpace(ocrResult.Text))
        {
            return TextExtractionResult.Failed(
                ocrResult.ErrorMessage ?? "OCR failed.",
                ocrResult.Source,
                ocrResult.FailureReason);
        }

        return TextExtractionResult.Succeeded(ocrResult.Text, ocrResult.Source);
    }
}
