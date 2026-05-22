namespace MouseTranslator.Core.Selection;

public sealed record ScreenCaptureRequest(OcrRegion Region, string ImageFormat = "png");
