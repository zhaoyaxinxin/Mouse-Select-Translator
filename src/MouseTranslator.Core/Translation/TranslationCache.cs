namespace MouseTranslator.Core.Translation;

public sealed class TranslationCache
{
    private readonly int _capacity;
    private readonly Dictionary<string, LinkedListNode<CacheEntry>> _entries = new(StringComparer.Ordinal);
    private readonly LinkedList<CacheEntry> _usageOrder = new();

    public TranslationCache(int capacity = 200)
    {
        _capacity = capacity;
    }

    public int Count => _entries.Count;

    public void Store(string text, TranslationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(result);

        if (_entries.TryGetValue(text, out var existing))
        {
            existing.Value = new CacheEntry(text, result);
            _usageOrder.Remove(existing);
            _usageOrder.AddLast(existing);
            return;
        }

        if (_entries.Count >= _capacity && _usageOrder.First is not null)
        {
            var oldest = _usageOrder.First;
            _usageOrder.RemoveFirst();
            _entries.Remove(oldest!.Value.Text);
        }

        var node = new LinkedListNode<CacheEntry>(new CacheEntry(text, result));
        _usageOrder.AddLast(node);
        _entries[text] = node;
    }

    public bool TryGet(string text, out TranslationResult? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (_entries.TryGetValue(text, out var node))
        {
            _usageOrder.Remove(node);
            _usageOrder.AddLast(node);
            result = node.Value.Result;
            return true;
        }

        result = null;
        return false;
    }

    private sealed record CacheEntry(string Text, TranslationResult Result);
}
