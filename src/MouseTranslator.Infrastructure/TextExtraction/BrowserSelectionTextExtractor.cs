using MouseTranslator.Core.Selection;

namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class BrowserSelectionTextExtractor : ITextExtractor
{
    private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMilliseconds(40);
    private static readonly TimeSpan DefaultPollingTimeout = TimeSpan.FromMilliseconds(600);
    private static readonly TimeSpan DefaultSelectionMaxAge = TimeSpan.FromSeconds(3);

    private readonly BrowserSelectionBuffer _buffer;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _pollingTimeout;
    private readonly TimeSpan _selectionMaxAge;

    public BrowserSelectionTextExtractor(BrowserSelectionBuffer buffer)
        : this(
            buffer,
            static (delay, cancellationToken) => Task.Delay(delay, cancellationToken),
            DefaultPollingInterval,
            DefaultPollingTimeout,
            DefaultSelectionMaxAge)
    {
    }

    internal BrowserSelectionTextExtractor(
        BrowserSelectionBuffer buffer,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        TimeSpan pollingInterval,
        TimeSpan pollingTimeout,
        TimeSpan selectionMaxAge)
    {
        _buffer = buffer;
        _delayAsync = delayAsync;
        _pollingInterval = pollingInterval;
        _pollingTimeout = pollingTimeout;
        _selectionMaxAge = selectionMaxAge;
    }

    public async Task<TextExtractionResult> ExtractSelectedTextAsync(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow - startedAt < _pollingTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_buffer.TryTakeRecent(_selectionMaxAge, out var selection))
            {
                return TextExtractionResult.Succeeded(selection!.Text, selection.Source);
            }

            await _delayAsync(_pollingInterval, cancellationToken);
        }

        return TextExtractionResult.Failed(
            "No recent browser selection was available.",
            "Edge.Extension",
            TextExtractionFailureReason.NoBrowserSelection);
    }
}
