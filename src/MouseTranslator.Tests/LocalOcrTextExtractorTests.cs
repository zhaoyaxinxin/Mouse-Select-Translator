using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.Ocr;

namespace MouseTranslator.Tests;

public sealed class LocalOcrTextExtractorTests
{
    [Fact]
    public async Task ExtractFromSelectionAsync_ReturnsRecognizedText()
    {
        var screenCaptureService = new FakeScreenCaptureService(ScreenCaptureResult.Succeeded(new byte[] { 1, 2, 3 }, "png"));
        var ocrEngine = new FakeOcrEngine(OcrResult.Succeeded("scanned pdf text", "OCR.Test"));
        var extractor = new LocalOcrTextExtractor(
            new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 400, 300)),
            screenCaptureService,
            ocrEngine);

        var result = await extractor.ExtractFromSelectionAsync(
            new SelectionEvent(10, 20, 90, 60, DateTimeOffset.UtcNow),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("scanned pdf text", result.Text);
        Assert.Equal("OCR.Test", result.Source);
        Assert.NotNull(screenCaptureService.LastRequest);
        Assert.Equal("eng", ocrEngine.LastRequest!.Language);
    }

    [Fact]
    public async Task ExtractFromSelectionAsync_ReturnsFailure_WhenScreenCaptureFails()
    {
        var extractor = new LocalOcrTextExtractor(
            new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 400, 300)),
            new FakeScreenCaptureService(ScreenCaptureResult.Failed("capture failed")),
            new FakeOcrEngine(OcrResult.Succeeded("unused", "OCR.Test")));

        var result = await extractor.ExtractFromSelectionAsync(
            new SelectionEvent(10, 20, 90, 60, DateTimeOffset.UtcNow),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("OCR.Capture", result.Source);
    }

    [Fact]
    public async Task ExtractSelectedTextAsync_ReturnsNotSupportedWithoutSelectionContext()
    {
        var extractor = new LocalOcrTextExtractor(
            new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 400, 300)),
            new FakeScreenCaptureService(ScreenCaptureResult.Succeeded(new byte[] { 1 }, "png")),
            new FakeOcrEngine(OcrResult.Succeeded("unused", "OCR.Test")));

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(TextExtractionFailureReason.NotSupported, result.FailureReason);
    }

    [Fact]
    public async Task GdiScreenCaptureService_ReturnsFailure_ForUnsupportedImageFormat()
    {
        var service = new GdiScreenCaptureService();

        var result = await service.CaptureAsync(
            new ScreenCaptureRequest(new OcrRegion(0, 0, 20, 20, 0), "jpeg"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("jpeg", result.ImageFormat);
    }

    private sealed class FakeScreenCaptureService : IScreenCaptureService
    {
        private readonly ScreenCaptureResult _result;

        public FakeScreenCaptureService(ScreenCaptureResult result)
        {
            _result = result;
        }

        public ScreenCaptureRequest? LastRequest { get; private set; }

        public Task<ScreenCaptureResult> CaptureAsync(ScreenCaptureRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeOcrEngine : ILocalOcrEngine
    {
        private readonly OcrResult _result;

        public FakeOcrEngine(OcrResult result)
        {
            _result = result;
        }

        public OcrRequest? LastRequest { get; private set; }

        public Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_result);
        }
    }
}
