using MouseTranslator.Core.Selection;
using MouseTranslator.Infrastructure.Ocr;

namespace MouseTranslator.Tests;

public sealed class OcrRegionCalculatorTests
{
    [Fact]
    public void Calculate_UsesSelectionBoundsAndPadding()
    {
        var calculator = new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 1920, 1080));

        var region = calculator.Calculate(new SelectionEvent(100, 200, 260, 280, DateTimeOffset.UtcNow));

        Assert.Equal(new OcrRegion(80, 180, 200, 120, OcrRegionCalculator.DefaultPadding), region);
    }

    [Fact]
    public void Calculate_NormalizesReverseDrag()
    {
        var calculator = new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 1920, 1080));

        var region = calculator.Calculate(new SelectionEvent(260, 280, 100, 200, DateTimeOffset.UtcNow));

        Assert.Equal(new OcrRegion(80, 180, 200, 120, OcrRegionCalculator.DefaultPadding), region);
    }

    [Fact]
    public void Calculate_ExpandsTinySelectionToMinimumSize()
    {
        var calculator = new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 200, 100));

        var region = calculator.Calculate(new SelectionEvent(4, 6, 8, 10, DateTimeOffset.UtcNow));

        Assert.Equal(new OcrRegion(0, 0, 86, 52, OcrRegionCalculator.DefaultPadding), region);
    }

    [Fact]
    public void Calculate_ClampsSelectionNearScreenEdge()
    {
        var calculator = new OcrRegionCalculator(static () => new OcrScreenBounds(0, 0, 100, 60));

        var region = calculator.Calculate(new SelectionEvent(90, 50, 99, 59, DateTimeOffset.UtcNow));

        Assert.Equal(new OcrRegion(14, 10, 86, 50, OcrRegionCalculator.DefaultPadding), region);
    }
}
