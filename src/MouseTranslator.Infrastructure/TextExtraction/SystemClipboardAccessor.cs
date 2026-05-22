using System.Windows;

namespace MouseTranslator.Infrastructure.TextExtraction;

internal sealed class SystemClipboardAccessor : IClipboardAccessor
{
    public IDataObject? GetDataObject()
    {
        return Clipboard.GetDataObject();
    }

    public void SetText(string text)
    {
        Clipboard.SetText(text);
    }

    public bool ContainsText()
    {
        return Clipboard.ContainsText();
    }

    public string GetText()
    {
        return Clipboard.GetText();
    }

    public void SetDataObject(IDataObject dataObject, bool copy)
    {
        Clipboard.SetDataObject(dataObject, copy);
    }

    public void Clear()
    {
        Clipboard.Clear();
    }
}
