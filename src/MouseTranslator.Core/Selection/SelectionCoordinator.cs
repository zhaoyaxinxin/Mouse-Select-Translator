using MouseTranslator.Core.Overlay;
using MouseTranslator.Core.Translation;

namespace MouseTranslator.Core.Selection;

public sealed class SelectionCoordinator : IDisposable
{
    private readonly IMouseSelectionMonitor _mouseSelectionMonitor;
    private readonly ITextExtractor _textExtractor;
    private readonly ITranslationService _translationService;
    private readonly IOverlayPresenter _overlayPresenter;
    private readonly TextValidationService _textValidationService;
    private readonly TranslationCache _translationCache;
    private readonly SelectionOptions _options;
    private readonly IOcrTextExtractor? _ocrTextExtractor;
    private readonly IForegroundWindowInfoProvider? _foregroundWindowInfoProvider;
    private readonly HashSet<string> _blacklistedProcesses;
    private readonly Func<DateTimeOffset> _clock;
    private readonly object _gate = new();

    private CancellationTokenSource? _pendingOperation;
    private string? _lastShownText;
    private DateTimeOffset _lastShownAt;
    private bool _disposed;

    public SelectionCoordinator(
        IMouseSelectionMonitor mouseSelectionMonitor,
        ITextExtractor textExtractor,
        ITranslationService translationService,
        IOverlayPresenter overlayPresenter,
        TextValidationService textValidationService,
        TranslationCache translationCache,
        SelectionOptions options,
        IEnumerable<string>? blacklistedProcesses = null,
        IForegroundWindowInfoProvider? foregroundWindowInfoProvider = null,
        Func<DateTimeOffset>? clock = null,
        IOcrTextExtractor? ocrTextExtractor = null)
    {
        _mouseSelectionMonitor = mouseSelectionMonitor;
        _textExtractor = textExtractor;
        _translationService = translationService;
        _overlayPresenter = overlayPresenter;
        _textValidationService = textValidationService;
        _translationCache = translationCache;
        _options = options;
        _ocrTextExtractor = ocrTextExtractor;
        _foregroundWindowInfoProvider = foregroundWindowInfoProvider;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _blacklistedProcesses = new HashSet<string>(
            blacklistedProcesses ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        State = SelectionState.Idle;
        Enabled = true;
        _mouseSelectionMonitor.SelectionGestureCompleted += OnSelectionGestureCompleted;
    }

    public SelectionState State { get; private set; }

    public bool Enabled { get; private set; }

    public event EventHandler<SelectionState>? StateChanged;

    public void Start()
    {
        ThrowIfDisposed();
        _mouseSelectionMonitor.Start();
        SetState(Enabled ? SelectionState.Idle : SelectionState.Disabled);
    }

    public void Stop()
    {
        if (_disposed)
        {
            return;
        }

        CancelPendingOperation();
        _mouseSelectionMonitor.Stop();
        _overlayPresenter.Hide();
        SetState(Enabled ? SelectionState.Idle : SelectionState.Disabled);
    }

    public void SetEnabled(bool enabled)
    {
        ThrowIfDisposed();

        if (Enabled == enabled)
        {
            return;
        }

        Enabled = enabled;
        if (!enabled)
        {
            CancelPendingOperation();
            _overlayPresenter.Hide();
            SetState(SelectionState.Disabled);
            return;
        }

        SetState(SelectionState.Idle);
    }

    public void HideOverlay()
    {
        ThrowIfDisposed();
        _overlayPresenter.Hide();
        if (Enabled && State == SelectionState.ShowingOverlay)
        {
            SetState(SelectionState.Idle);
        }
    }

    private async void OnSelectionGestureCompleted(object? sender, SelectionGestureEventArgs e)
    {
        if (!Enabled || IsBlacklisted())
        {
            return;
        }

        var selectionEvent = e.SelectionEvent;
        var operation = ReplacePendingOperation();

        try
        {
            SetState(SelectionState.WaitingForSelection);
            await Task.Delay(_options.ExtractionDelayMs, operation.Token);

            SetState(SelectionState.ExtractingText);
            var extractionResult = await _textExtractor.ExtractSelectedTextAsync(operation.Token);
            if (!extractionResult.Success || !_textValidationService.IsValid(extractionResult.Text))
            {
                extractionResult = await TryOcrFallbackAsync(selectionEvent, operation.Token);
            }

            if (!extractionResult.Success || !_textValidationService.IsValid(extractionResult.Text))
            {
                SetState(SelectionState.Idle);
                return;
            }

            var normalizedText = _textValidationService.Normalize(extractionResult.Text!);
            var now = _clock();
            if (ShouldSuppressDuplicate(normalizedText, now))
            {
                SetState(SelectionState.Idle);
                return;
            }

            TranslationResult translationResult;
            if (_translationCache.TryGet(normalizedText, out var cached))
            {
                translationResult = cached!;
            }
            else
            {
                SetState(SelectionState.Translating);
                translationResult = await _translationService.TranslateAsync(
                    new TranslationRequest(normalizedText, null, _options.TargetLanguage),
                    operation.Token);

                if (translationResult.Success)
                {
                    _translationCache.Store(normalizedText, translationResult);
                }
            }

            if (!translationResult.Success || string.IsNullOrWhiteSpace(translationResult.TranslatedText))
            {
                _overlayPresenter.Show(new OverlayRequest(
                    string.Empty,
                    translationResult.ErrorMessage ?? "Translation failed.",
                    selectionEvent.EndX,
                    selectionEvent.EndY,
                    true));
                SetState(SelectionState.ShowingOverlay);
                return;
            }

            _lastShownText = normalizedText;
            _lastShownAt = now;

            _overlayPresenter.Show(new OverlayRequest(
                string.Empty,
                translationResult.TranslatedText,
                selectionEvent.EndX,
                selectionEvent.EndY));
            SetState(SelectionState.ShowingOverlay);
        }
        catch (OperationCanceledException)
        {
            SetState(Enabled ? SelectionState.Idle : SelectionState.Disabled);
        }
        catch
        {
            SetState(SelectionState.Error);
            _overlayPresenter.Show(new OverlayRequest(
                string.Empty,
                "Unexpected error.",
                selectionEvent.EndX,
                selectionEvent.EndY,
                true));
        }
        finally
        {
            if (ReferenceEquals(operation, _pendingOperation))
            {
                lock (_gate)
                {
                    if (ReferenceEquals(operation, _pendingOperation))
                    {
                        _pendingOperation?.Dispose();
                        _pendingOperation = null;
                    }
                }
            }

            if (Enabled && State != SelectionState.ShowingOverlay && State != SelectionState.Error)
            {
                SetState(SelectionState.Idle);
            }
        }
    }

    private CancellationTokenSource ReplacePendingOperation()
    {
        lock (_gate)
        {
            _pendingOperation?.Cancel();
            _pendingOperation?.Dispose();
            _pendingOperation = new CancellationTokenSource();
            return _pendingOperation;
        }
    }

    private void CancelPendingOperation()
    {
        lock (_gate)
        {
            _pendingOperation?.Cancel();
            _pendingOperation?.Dispose();
            _pendingOperation = null;
        }
    }

    private bool ShouldSuppressDuplicate(string text, DateTimeOffset now)
    {
        return _lastShownText is not null
            && string.Equals(_lastShownText, text, StringComparison.Ordinal)
            && now - _lastShownAt < _options.DuplicateIgnoreDuration;
    }

    private bool IsBlacklisted()
    {
        if (_blacklistedProcesses.Count == 0 || _foregroundWindowInfoProvider is null)
        {
            return false;
        }

        var processName = _foregroundWindowInfoProvider.GetForegroundProcessName();
        return processName is not null && _blacklistedProcesses.Contains(processName);
    }

    private async Task<TextExtractionResult> TryOcrFallbackAsync(
        SelectionEvent selectionEvent,
        CancellationToken cancellationToken)
    {
        if (_ocrTextExtractor is null)
        {
            return TextExtractionResult.Failed(
                "OCR extractor is not configured.",
                "OCR",
                TextExtractionFailureReason.NotSupported);
        }

        return await _ocrTextExtractor.ExtractFromSelectionAsync(selectionEvent, cancellationToken);
    }

    private void SetState(SelectionState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, state);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _mouseSelectionMonitor.SelectionGestureCompleted -= OnSelectionGestureCompleted;
        CancelPendingOperation();
        _mouseSelectionMonitor.Dispose();
        _overlayPresenter.Hide();
    }
}
