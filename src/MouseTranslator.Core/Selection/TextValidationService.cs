using System.Text.RegularExpressions;

namespace MouseTranslator.Core.Selection;

public sealed partial class TextValidationService
{
    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly int _maxLineCount;

    public TextValidationService(int minLength = 2, int maxLength = 2000, int maxLineCount = 50)
    {
        _minLength = minLength;
        _maxLength = maxLength;
        _maxLineCount = maxLineCount;
    }

    public string Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var normalized = text.Trim();
        normalized = WhitespaceRegex().Replace(normalized, " ");
        normalized = LineEndingRegex().Replace(normalized, "\n");
        normalized = ExcessiveBlankLineRegex().Replace(normalized, "\n\n");

        return normalized;
    }

    public bool IsValid(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = Normalize(text);
        if (normalized.Length < _minLength || normalized.Length > _maxLength)
        {
            return false;
        }

        if (normalized.Split('\n').Length > _maxLineCount)
        {
            return false;
        }

        return !LooksSensitive(normalized);
    }

    public bool LooksSensitive(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.StartsWith("sk-", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith("ghp_", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith("xoxb-", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith("AKIA", StringComparison.OrdinalIgnoreCase)
            || LongTokenRegex().IsMatch(text);
    }

    [GeneratedRegex(@"[ \t]+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\r\n|\r")]
    private static partial Regex LineEndingRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveBlankLineRegex();

    [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z\d]).{33,}$")]
    private static partial Regex LongTokenRegex();
}
