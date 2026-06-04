using System.Windows;

namespace OpenClickToDo.Services;

public sealed class ClipboardService
{
    public void CopyText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        System.Windows.Clipboard.SetText(text);
    }
}
