using System.Windows;

namespace OpenClickToDo.Models;

public sealed class OcrTextBlock
{
    public string Text { get; init; } = string.Empty;

    public Rect Bounds { get; init; }

    public double? Confidence { get; init; }

    public string? SourceLanguage { get; init; }

    public IReadOnlyList<OcrWordBox> Words { get; init; } = Array.Empty<OcrWordBox>();
}

