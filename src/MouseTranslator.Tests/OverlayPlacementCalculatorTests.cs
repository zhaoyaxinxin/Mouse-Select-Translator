using MouseTranslator.Core.Overlay;

namespace MouseTranslator.Tests;

public sealed class OverlayPlacementCalculatorTests
{
    private readonly OverlayPlacementCalculator _calculator = new();
    private readonly ScreenBounds _screenBounds = new(0, 0, 1000, 800);

    [Fact]
    public void Calculate_PlacesOverlayAboveCursor_WhenThereIsEnoughSpace()
    {
        var placement = _calculator.Calculate(500, 400, 200, 100, _screenBounds);

        Assert.Equal(400, placement.X);
        Assert.Equal(280, placement.Y);
    }

    [Fact]
    public void Calculate_PlacesOverlayBelowCursor_WhenTopSpaceIsInsufficient()
    {
        var placement = _calculator.Calculate(500, 50, 200, 100, _screenBounds);

        Assert.Equal(400, placement.X);
        Assert.Equal(70, placement.Y);
    }

    [Fact]
    public void Calculate_ClampsOverlayInsideScreenBounds()
    {
        var placement = _calculator.Calculate(990, 790, 200, 100, _screenBounds);

        Assert.Equal(792, placement.X);
        Assert.Equal(670, placement.Y);
    }

    [Fact]
    public void Calculate_HandlesOverlayLargerThanScreenWithoutOverflow()
    {
        var placement = _calculator.Calculate(500, 400, 1200, 900, _screenBounds);

        Assert.Equal(8, placement.X);
        Assert.Equal(8, placement.Y);
    }
}
