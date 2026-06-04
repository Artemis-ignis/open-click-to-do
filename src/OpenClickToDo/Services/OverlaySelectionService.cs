using OpenClickToDo.Models;

namespace OpenClickToDo.Services;

public sealed class OverlaySelectionService
{
    public string GetSelectedText(OcrTextBlock? block)
    {
        return block?.Text.Trim() ?? string.Empty;
    }
}

