using System.Windows;
using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.TextExtraction;
using MouseTranslator.Infrastructure.Win32;

namespace MouseTranslator.Tests;

public sealed class TextExtractionPipelineTests
{
    [Fact]
    public async Task ClipboardExtractor_ReturnsText_WhenClipboardChangesDuringPolling()
    {
        var clipboard = new FakeClipboardAccessor();
        var copySender = new FakeCopyCommandSender();
        var extractor = new ClipboardTextExtractor(
            copySender,
            clipboard,
            (delay, cancellationToken) =>
            {
                clipboard.Text = "translated selection";
                return Task.CompletedTask;
            },
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(100));

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Clipboard", result.Source);
        Assert.Equal("translated selection", result.Text);
        Assert.Equal(1, copySender.CallCount);
    }

    [Fact]
    public async Task ClipboardExtractor_ReturnsClipboardUnchanged_WhenSentinelPersists()
    {
        var clipboard = new FakeClipboardAccessor();
        var extractor = new ClipboardTextExtractor(
            new FakeCopyCommandSender(),
            clipboard,
            static (delay, cancellationToken) => Task.CompletedTask,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.Zero);

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(TextExtractionFailureReason.ClipboardNoText, result.FailureReason);
    }

    [Fact]
    public async Task UIAutomationExtractor_UsesProviderResult()
    {
        var extractor = new UIAutomationTextExtractor(
            new FakeAutomationTextProvider(
                AutomationTextReadResult.Succeeded("textbox value", "UIAutomation.ValuePattern")));

        var result = await extractor.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("textbox value", result.Text);
        Assert.Equal("UIAutomation.ValuePattern", result.Source);
    }

    [Fact]
    public async Task CompositeExtractor_FallsBack_WhenEarlierResultIsInvalid()
    {
        var composite = new CompositeTextExtractor(
            new ITextExtractor[]
            {
                new FakeTextExtractor(TextExtractionResult.Succeeded("a", "UIAutomation")),
                new FakeTextExtractor(TextExtractionResult.Succeeded("browser selection", "Clipboard")),
            },
            new TextValidationService());

        var result = await composite.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("browser selection", result.Text);
        Assert.Equal("Clipboard", result.Source);
    }

    [Fact]
    public async Task CompositeExtractor_ReturnsValidationFailure_WhenAllExtractedTextIsInvalid()
    {
        var composite = new CompositeTextExtractor(
            new ITextExtractor[]
            {
                new FakeTextExtractor(TextExtractionResult.Succeeded("a", "UIAutomation")),
            },
            new TextValidationService());

        var result = await composite.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(TextExtractionFailureReason.InvalidText, result.FailureReason);
        Assert.Equal("UIAutomation", result.Source);
    }

    [Fact]
    public async Task CompositeExtractor_PrefersBrowserSelection_WhenBrowserTextIsAvailable()
    {
        var composite = new CompositeTextExtractor(
            new ITextExtractor[]
            {
                new FakeTextExtractor(TextExtractionResult.Failed("No UIA selection", "UIAutomation", TextExtractionFailureReason.EmptySelection)),
                new FakeTextExtractor(TextExtractionResult.Succeeded("pdf selected text", "Edge.Extension.Pdf")),
                new FakeTextExtractor(TextExtractionResult.Succeeded("clipboard text", "Clipboard")),
            },
            new TextValidationService());

        var result = await composite.ExtractSelectedTextAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("pdf selected text", result.Text);
        Assert.Equal("Edge.Extension.Pdf", result.Source);
    }

    private sealed class FakeClipboardAccessor : IClipboardAccessor
    {
        public IDataObject? DataObject { get; private set; }

        public string? Text { get; set; }

        public IDataObject? GetDataObject()
        {
            return DataObject;
        }

        public void SetText(string text)
        {
            Text = text;
        }

        public bool ContainsText()
        {
            return Text is not null;
        }

        public string GetText()
        {
            return Text ?? string.Empty;
        }

        public void SetDataObject(IDataObject dataObject, bool copy)
        {
            DataObject = dataObject;
        }

        public void Clear()
        {
            Text = null;
        }
    }

    private sealed class FakeCopyCommandSender : ICopyCommandSender
    {
        public int CallCount { get; private set; }

        public Task SendCtrlCAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAutomationTextProvider : IAutomationTextProvider
    {
        private readonly AutomationTextReadResult _result;

        public FakeAutomationTextProvider(AutomationTextReadResult result)
        {
            _result = result;
        }

        public AutomationTextReadResult ReadFocusedSelection()
        {
            return _result;
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
}
