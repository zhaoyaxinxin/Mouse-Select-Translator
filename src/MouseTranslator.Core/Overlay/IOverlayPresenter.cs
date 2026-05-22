namespace MouseTranslator.Core.Overlay;

public interface IOverlayPresenter
{
    void Show(OverlayRequest request);

    void Hide();
}
