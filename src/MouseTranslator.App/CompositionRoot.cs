using System.Net.Http;
using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.Ocr;
using MouseTranslator.Core.Translation;
using MouseTranslator.Infrastructure.Settings;
using MouseTranslator.Infrastructure.TextExtraction;
using MouseTranslator.Infrastructure.Translation;
using MouseTranslator.Infrastructure.Win32;

namespace MouseTranslator.App;

public static class CompositionRoot
{
    public static ApplicationController Create(System.Windows.Application application)
    {
        var settingsStore = new JsonSettingsStore();
        var settings = settingsStore.LoadOrCreate();

        var selectionOptions = new SelectionOptions
        {
            DragThresholdPixels = settings.Selection.MinDragDistancePx,
            ExtractionDelayMs = settings.Selection.DelayAfterMouseUpMs,
            DuplicateIgnoreDuration = TimeSpan.FromSeconds(settings.Selection.DuplicateIgnoreSeconds),
            TargetLanguage = settings.Translation.TargetLanguage,
            OverlayAutoHideSeconds = Math.Max(10, settings.Overlay.AutoHideSeconds),
            OverlayMaxWidth = settings.Overlay.MaxWidth,
            OverlayOffsetY = settings.Overlay.OffsetY,
            OverlayDismissDistancePixels = settings.Overlay.DismissDistancePixels,
        };

        var textValidationService = new TextValidationService(
            settings.Selection.MinTextLength,
            settings.Selection.MaxTextLength);
        var mouseHook = new Win32MouseHook(selectionOptions.DragThresholdPixels);
        var sendInputService = new SendInputService();
        var browserSelectionBuffer = new BrowserSelectionBuffer();
        var browserSelectionLoopbackServer = new BrowserSelectionLoopbackServer(browserSelectionBuffer);
        var uiAutomationTextExtractor = new UIAutomationTextExtractor();
        var clipboardTextExtractor = new ClipboardTextExtractor(sendInputService);
        var browserSelectionTextExtractor = new BrowserSelectionTextExtractor(browserSelectionBuffer);
        var ocrTextExtractor = new LocalOcrTextExtractor(
            new OcrRegionCalculator(),
            new GdiScreenCaptureService(),
            new TesseractCommandLineOcrEngine());
        var compositeTextExtractor = new CompositeTextExtractor(
            new ITextExtractor[]
            {
                uiAutomationTextExtractor,
                browserSelectionTextExtractor,
                clipboardTextExtractor,
            },
            textValidationService);

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(settings.Translation.TimeoutSeconds),
        };

        ITranslationService translationService = string.Equals(
            settings.Translation.Provider,
            "Mock",
            StringComparison.OrdinalIgnoreCase)
            ? new MockTranslationService()
            : new OpenAICompatibleTranslationService(
                httpClient,
                settings.Translation.BaseUrl,
                settings.Translation.Model,
                settings.Translation.ApiKeyEnvironmentVariable);

        var overlayPresenter = new OverlayPresenter(selectionOptions);
        var foregroundWindowInfoProvider = new ForegroundWindowInfoProvider();
        var selectionCoordinator = new SelectionCoordinator(
            mouseHook,
            compositeTextExtractor,
            translationService,
            overlayPresenter,
            textValidationService,
            new TranslationCache(),
            selectionOptions,
            settings.AppBlacklist,
            foregroundWindowInfoProvider,
            ocrTextExtractor: ocrTextExtractor);

        var hotkeyManager = new GlobalHotkeyManager();
        var trayManager = new TrayManager();
        var details =
            $"Settings: {settingsStore.SettingsPath}{Environment.NewLine}" +
            $"Provider: {settings.Translation.Provider}{Environment.NewLine}" +
            $"Target language: {settings.Translation.TargetLanguage}{Environment.NewLine}" +
            $"Browser bridge: {browserSelectionLoopbackServer.SelectionEndpoint}";

        return new ApplicationController(
            application,
            selectionCoordinator,
            hotkeyManager,
            trayManager,
            overlayPresenter,
            browserSelectionLoopbackServer,
            settings.General.EnabledOnStartup,
            settings.General.StartMinimized,
            details);
    }
}
