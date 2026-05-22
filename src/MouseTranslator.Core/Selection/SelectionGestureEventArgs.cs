namespace MouseTranslator.Core.Selection;

public sealed class SelectionGestureEventArgs : EventArgs
{
    public SelectionGestureEventArgs(SelectionEvent selectionEvent)
    {
        SelectionEvent = selectionEvent;
    }

    public SelectionEvent SelectionEvent { get; }
}
