namespace MouseTranslator.Core.Selection;

public interface IScreenCaptureService
{
    Task<ScreenCaptureResult> CaptureAsync(ScreenCaptureRequest request, CancellationToken cancellationToken);
}
