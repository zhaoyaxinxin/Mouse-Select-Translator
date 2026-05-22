namespace MouseTranslator.Core.Selection;

public interface IMouseSelectionMonitor : IDisposable
{
    event EventHandler<SelectionGestureEventArgs>? SelectionGestureCompleted;

    void Start();

    void Stop();
}
