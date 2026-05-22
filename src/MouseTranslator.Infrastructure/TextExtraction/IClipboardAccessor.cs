using System.Windows;

namespace MouseTranslator.Infrastructure.TextExtraction;

internal interface IClipboardAccessor
{
    IDataObject? GetDataObject();

    void SetText(string text);

    bool ContainsText();

    string GetText();

    void SetDataObject(IDataObject dataObject, bool copy);

    void Clear();
}
