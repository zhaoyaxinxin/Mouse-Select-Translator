namespace MouseTranslator.Core.Selection;

public sealed record ScreenCaptureResult(
    bool Success,
    byte[]? ImageBytes,
    string ImageFormat,
    string? ErrorMessage)
{
    public static ScreenCaptureResult Succeeded(byte[] imageBytes, string imageFormat)
    {
        return new ScreenCaptureResult(true, imageBytes, imageFormat, null);
    }

    public static ScreenCaptureResult Failed(string errorMessage, string imageFormat = "png")
    {
        return new ScreenCaptureResult(false, null, imageFormat, errorMessage);
    }
}
