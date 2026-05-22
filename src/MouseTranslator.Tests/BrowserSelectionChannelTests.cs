using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.TextExtraction;

namespace MouseTranslator.Tests;

public sealed class BrowserSelectionChannelTests
{
    [Fact]
    public async Task BrowserSelectionExtractor_ReturnsRecentSelection()
    {
        var buffer = new BrowserSelectionBuffer();
        buffer.Store(new BrowserSelectionPayload(
            "edge selected text",
            "Edge.Extension",
            "https://example.com",
            "Example",
            false,
            DateTimeOffset.UtcNow));

        var extractor = new BrowserSelectionTextExtractor(
            buffer,
            static (delay, cancellationToken) => Task.CompletedTask,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromSeconds(2));

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("edge selected text", result.Text);
        Assert.Equal("Edge.Extension", result.Source);
    }

    [Fact]
    public async Task BrowserSelectionExtractor_ReturnsFailure_WhenNoSelectionArrives()
    {
        var extractor = new BrowserSelectionTextExtractor(
            new BrowserSelectionBuffer(),
            static (delay, cancellationToken) => Task.CompletedTask,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(TextExtractionFailureReason.NoBrowserSelection, result.FailureReason);
    }

    [Fact]
    public void BrowserSelectionBuffer_ConsumesLatestSelectionOnlyOnce()
    {
        var buffer = new BrowserSelectionBuffer();
        buffer.Store(new BrowserSelectionPayload(
            "edge selected text",
            "Edge.Extension",
            null,
            null,
            false,
            DateTimeOffset.UtcNow));

        var firstRead = buffer.TryTakeRecent(TimeSpan.FromSeconds(2), out var payload);
        var secondRead = buffer.TryTakeRecent(TimeSpan.FromSeconds(2), out var secondPayload);

        Assert.True(firstRead);
        Assert.NotNull(payload);
        Assert.False(secondRead);
        Assert.Null(secondPayload);
    }
}
