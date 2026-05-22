using MouseTranslator.Core.Overlay;
using MouseTranslator.Core.Selection;
using MouseTranslator.Core.Translation;

namespace MouseTranslator.Tests;

public sealed class SelectionCoordinatorTests
{
    [Fact]
    public async Task Gesture_ShowsOverlay_ForValidSelection()
    {
        var monitor = new FakeMouseSelectionMonitor();
        var overlayPresenter = new FakeOverlayPresenter();
        var translator = new FakeTranslationService();
        using var coordinator = new SelectionCoordinator(
            monitor,
            new FakeTextExtractor(TextExtractionResult.Succeeded("Graph neural networks", "Test")),
            translator,
            overlayPresenter,
            new TextValidationService(),
            new TranslationCache(),
            new SelectionOptions { ExtractionDelayMs = 1 });

        coordinator.Start();
        monitor.Raise(new SelectionEvent(0, 0, 100, 100, DateTimeOffset.UtcNow));
        await Task.Delay(30);

        Assert.NotNull(overlayPresenter.LastRequest);
        Assert.Equal("[Mock] Graph neural networks", overlayPresenter.LastRequest!.TranslatedText);
        Assert.Equal(string.Empty, overlayPresenter.LastRequest.OriginalText);
        Assert.Equal(1, translator.CallCount);
    }

    [Fact]
    public async Task DisabledCoordinator_DoesNotTranslate()
    {
        var monitor = new FakeMouseSelectionMonitor();
        var overlayPresenter = new FakeOverlayPresenter();
        var translator = new FakeTranslationService();
        using var coordinator = new SelectionCoordinator(
            monitor,
            new FakeTextExtractor(TextExtractionResult.Succeeded("Graph neural networks", "Test")),
            translator,
            overlayPresenter,
            new TextValidationService(),
            new TranslationCache(),
            new SelectionOptions { ExtractionDelayMs = 1 });

        coordinator.Start();
        coordinator.SetEnabled(false);
        monitor.Raise(new SelectionEvent(0, 0, 100, 100, DateTimeOffset.UtcNow));
        await Task.Delay(30);

        Assert.Null(overlayPresenter.LastRequest);
        Assert.Equal(0, translator.CallCount);
    }

    [Fact]
    public async Task DuplicateGestureWithinWindow_DoesNotRetranslate()
    {
        var monitor = new FakeMouseSelectionMonitor();
        var overlayPresenter = new FakeOverlayPresenter();
        var translator = new FakeTranslationService();
        using var coordinator = new SelectionCoordinator(
            monitor,
            new FakeTextExtractor(TextExtractionResult.Succeeded("Graph neural networks", "Test")),
            translator,
            overlayPresenter,
            new TextValidationService(),
            new TranslationCache(),
            new SelectionOptions
            {
                ExtractionDelayMs = 1,
                DuplicateIgnoreDuration = TimeSpan.FromSeconds(10),
            },
            clock: () => DateTimeOffset.UtcNow);

        coordinator.Start();
        monitor.Raise(new SelectionEvent(0, 0, 100, 100, DateTimeOffset.UtcNow));
        await Task.Delay(30);
        monitor.Raise(new SelectionEvent(0, 0, 120, 120, DateTimeOffset.UtcNow));
        await Task.Delay(30);

        Assert.Equal(1, translator.CallCount);
    }

    [Fact]
    public async Task Gesture_UsesOcrFallback_WhenPrimaryExtractionFails()
    {
        var monitor = new FakeMouseSelectionMonitor();
        var overlayPresenter = new FakeOverlayPresenter();
        var translator = new FakeTranslationService();
        var ocrExtractor = new FakeOcrTextExtractor(TextExtractionResult.Succeeded("scanned pdf text", "OCR.Test"));
        using var coordinator = new SelectionCoordinator(
            monitor,
            new FakeTextExtractor(TextExtractionResult.Failed("No text", "Primary", TextExtractionFailureReason.EmptySelection)),
            translator,
            overlayPresenter,
            new TextValidationService(),
            new TranslationCache(),
            new SelectionOptions { ExtractionDelayMs = 1 },
            ocrTextExtractor: ocrExtractor);

        coordinator.Start();
        monitor.Raise(new SelectionEvent(0, 0, 100, 100, DateTimeOffset.UtcNow));
        await Task.Delay(30);

        Assert.Equal(1, ocrExtractor.CallCount);
        Assert.NotNull(overlayPresenter.LastRequest);
        Assert.Equal("[Mock] scanned pdf text", overlayPresenter.LastRequest!.TranslatedText);
    }

    [Fact]
    public async Task Gesture_DoesNotUseOcrFallback_WhenPrimaryExtractionSucceeds()
    {
        var monitor = new FakeMouseSelectionMonitor();
        var overlayPresenter = new FakeOverlayPresenter();
        var translator = new FakeTranslationService();
        var ocrExtractor = new FakeOcrTextExtractor(TextExtractionResult.Succeeded("scanned pdf text", "OCR.Test"));
        using var coordinator = new SelectionCoordinator(
            monitor,
            new FakeTextExtractor(TextExtractionResult.Succeeded("browser text", "Primary")),
            translator,
            overlayPresenter,
            new TextValidationService(),
            new TranslationCache(),
            new SelectionOptions { ExtractionDelayMs = 1 },
            ocrTextExtractor: ocrExtractor);

        coordinator.Start();
        monitor.Raise(new SelectionEvent(0, 0, 100, 100, DateTimeOffset.UtcNow));
        await Task.Delay(30);

        Assert.Equal(0, ocrExtractor.CallCount);
        Assert.NotNull(overlayPresenter.LastRequest);
        Assert.Equal("[Mock] browser text", overlayPresenter.LastRequest!.TranslatedText);
    }

    private sealed class FakeMouseSelectionMonitor : IMouseSelectionMonitor
    {
        public event EventHandler<SelectionGestureEventArgs>? SelectionGestureCompleted;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Raise(SelectionEvent selectionEvent)
        {
            SelectionGestureCompleted?.Invoke(this, new SelectionGestureEventArgs(selectionEvent));
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeTextExtractor : ITextExtractor
    {
        private readonly TextExtractionResult _result;

        public FakeTextExtractor(TextExtractionResult result)
        {
            _result = result;
        }

        public Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeOcrTextExtractor : IOcrTextExtractor
    {
        private readonly TextExtractionResult _result;

        public FakeOcrTextExtractor(TextExtractionResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }

        public Task<TextExtractionResult> ExtractFromSelectionAsync(
            SelectionEvent selectionEvent,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeTranslationService : ITranslationService
    {
        public int CallCount { get; private set; }

        public Task<TranslationResult> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(TranslationResult.Succeeded($"[Mock] {request.Text}", "en", request.TargetLanguage));
        }
    }

    private sealed class FakeOverlayPresenter : IOverlayPresenter
    {
        public OverlayRequest? LastRequest { get; private set; }

        public void Show(OverlayRequest request)
        {
            LastRequest = request;
        }

        public void Hide()
        {
        }
    }
}
