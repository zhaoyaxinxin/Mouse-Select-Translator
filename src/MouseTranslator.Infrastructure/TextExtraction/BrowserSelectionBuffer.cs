namespace MouseTranslator.Infrastructure.TextExtraction;

public sealed class BrowserSelectionBuffer
{
    private readonly object _gate = new();

    private BrowserSelectionPayload? _latestSelection;
    private long _latestSequence;
    private long _consumedSequence;

    public void Store(BrowserSelectionPayload selection)
    {
        lock (_gate)
        {
            _latestSelection = selection;
            _latestSequence++;
        }
    }

    public bool TryTakeRecent(TimeSpan maxAge, out BrowserSelectionPayload? selection)
    {
        lock (_gate)
        {
            if (_latestSelection is null || _latestSequence == _consumedSequence)
            {
                selection = null;
                return false;
            }

            if (DateTimeOffset.UtcNow - _latestSelection.CapturedAtUtc > maxAge)
            {
                selection = null;
                return false;
            }

            _consumedSequence = _latestSequence;
            selection = _latestSelection;
            return true;
        }
    }
}
