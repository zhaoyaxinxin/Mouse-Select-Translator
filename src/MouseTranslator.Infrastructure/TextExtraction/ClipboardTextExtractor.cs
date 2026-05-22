using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.Win32;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class ClipboardTextExtractor : ITextExtractor
{
    private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan DefaultPollingTimeout = TimeSpan.FromMilliseconds(750);

    private readonly ICopyCommandSender _copyCommandSender;
    private readonly IClipboardAccessor _clipboardAccessor;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _pollingTimeout;

    public ClipboardTextExtractor(SendInputService sendInputService)
        : this(
            sendInputService,
            new SystemClipboardAccessor(),
            static (delay, cancellationToken) => Task.Delay(delay, cancellationToken),
            DefaultPollingInterval,
            DefaultPollingTimeout)
    {
    }

    internal ClipboardTextExtractor(
        ICopyCommandSender copyCommandSender,
        IClipboardAccessor clipboardAccessor,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        TimeSpan pollingInterval,
        TimeSpan pollingTimeout)
    {
        _copyCommandSender = copyCommandSender;
        _clipboardAccessor = clipboardAccessor;
        _delayAsync = delayAsync;
        _pollingInterval = pollingInterval;
        _pollingTimeout = pollingTimeout;
    }

    public async Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        System.Windows.IDataObject? previousClipboard = null;
        var sentinel = $"MouseSelectTranslator:{Guid.NewGuid():N}";

        try
        {
            previousClipboard = _clipboardAccessor.GetDataObject();
            _clipboardAccessor.SetText(sentinel);

            await _copyCommandSender.SendCtrlCAsync(cancellationToken);
            return await WaitForClipboardTextAsync(sentinel, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return TextExtractionResult.Failed(
                ex.Message,
                "Clipboard",
                TextExtractionFailureReason.ClipboardUnavailable);
        }
        finally
        {
            TryRestoreClipboard(previousClipboard);
        }
    }

    private async Task<TextExtractionResult> WaitForClipboardTextAsync(
        string sentinel,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var sawTextPayload = false;

        while (DateTimeOffset.UtcNow - startedAt < _pollingTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_clipboardAccessor.ContainsText())
            {
                sawTextPayload = true;
                var selectedText = _clipboardAccessor.GetText();
                if (!string.Equals(selectedText, sentinel, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(selectedText))
                {
                    return TextExtractionResult.Succeeded(selectedText, "Clipboard");
                }
            }

            await _delayAsync(_pollingInterval, cancellationToken);
        }

        return sawTextPayload
            ? TextExtractionResult.Failed(
                "Clipboard selection did not change.",
                "Clipboard",
                TextExtractionFailureReason.ClipboardUnchanged)
            : TextExtractionResult.Failed(
                "Clipboard did not produce text.",
                "Clipboard",
                TextExtractionFailureReason.ClipboardNoText);
    }

    private void TryRestoreClipboard(System.Windows.IDataObject? previousClipboard)
    {
        try
        {
            if (previousClipboard is null)
            {
                _clipboardAccessor.Clear();
                return;
            }

            _clipboardAccessor.SetDataObject(previousClipboard, true);
        }
        catch
        {
        }
    }
}
