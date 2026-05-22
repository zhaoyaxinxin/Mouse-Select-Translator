using MouseTranslator.Core.Selection;

namespace MouseTranslator.Tests;

public sealed class TextValidationServiceTests
{
    private readonly TextValidationService _service = new();

    [Fact]
    public void Normalize_CollapsesWhitespaceAndBlankLines()
    {
        var normalized = _service.Normalize("  hello\t\tworld\r\n\r\n\r\nnext  ");

        Assert.Equal("hello world\n\nnext", normalized);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForTooShortText()
    {
        var isValid = _service.IsValid("a");

        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForSensitiveToken()
    {
        var isValid = _service.IsValid("sk-1234567890");

        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForNormalSentence()
    {
        var isValid = _service.IsValid("Graph neural networks aggregate information.");

        Assert.True(isValid);
    }
}
