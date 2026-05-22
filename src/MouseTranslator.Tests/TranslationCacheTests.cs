using MouseTranslator.Core.Translation;

namespace MouseTranslator.Tests;

public sealed class TranslationCacheTests
{
    [Fact]
    public void Store_ThenTryGet_ReturnsCachedValue()
    {
        var cache = new TranslationCache();
        var expected = TranslationResult.Succeeded("图神经网络", "en", "zh-CN");

        cache.Store("graph neural network", expected);
        var found = cache.TryGet("graph neural network", out var actual);

        Assert.True(found);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenKeyIsMissing()
    {
        var cache = new TranslationCache();

        var found = cache.TryGet("missing", out var actual);

        Assert.False(found);
        Assert.Null(actual);
    }

    [Fact]
    public void Store_EvictsOldestEntry_WhenCapacityIsExceeded()
    {
        var cache = new TranslationCache(2);

        cache.Store("one", TranslationResult.Succeeded("1", "en", "zh-CN"));
        cache.Store("two", TranslationResult.Succeeded("2", "en", "zh-CN"));
        cache.Store("three", TranslationResult.Succeeded("3", "en", "zh-CN"));

        Assert.False(cache.TryGet("one", out _));
        Assert.True(cache.TryGet("two", out _));
        Assert.True(cache.TryGet("three", out _));
    }
}
