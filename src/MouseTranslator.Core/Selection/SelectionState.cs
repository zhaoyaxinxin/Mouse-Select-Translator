namespace MouseTranslator.Core.Selection;

public enum SelectionState
{
    Disabled,
    Idle,
    MouseDown,
    Dragging,
    Processing,
    WaitingForSelection,
    ExtractingText,
    Translating,
    ShowingOverlay,
    Error,
}
