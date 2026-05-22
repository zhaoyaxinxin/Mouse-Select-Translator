namespace MouseTranslator.Core.Selection;

public enum TextExtractionFailureReason
{
    None = 0,
    NoFocusedElement,
    SensitiveField,
    PatternNotSupported,
    EmptySelection,
    ClipboardUnavailable,
    ClipboardNoText,
    ClipboardUnchanged,
    Timeout,
    InvalidText,
    NoBrowserSelection,
    NotSupported,
    UnknownError,
}
